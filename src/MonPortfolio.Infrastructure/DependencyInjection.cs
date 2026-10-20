using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonPortfolio.Application.Common;
using MonPortfolio.Application.Security;
using MonPortfolio.Domain.Repositories;
using MonPortfolio.Infrastructure.Identity;
using MonPortfolio.Infrastructure.Persistence;
using MonPortfolio.Infrastructure.Persistence.Repositories;
using MonPortfolio.Infrastructure.Storage;

namespace MonPortfolio.Infrastructure;

public static class DependencyInjection
{
    public const string AdminLoginPath = "/admin/login";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        AddPersistence(services, configuration, environment);
        AddIdentity(services, configuration, environment);
        AddImageStorage(services, configuration, environment);
        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // SQL Server partout : LocalDB en développement, base MonsterASP (ou autre) en production.
        // En production, la chaîne vient d'une variable d'environnement : ConnectionStrings__Portfolio.
        var connectionString = configuration.GetConnectionString("Portfolio");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "La chaîne de connexion « ConnectionStrings:Portfolio » est absente. " +
                $"Environnement « {environment.EnvironmentName} » : définissez la variable ConnectionStrings__Portfolio.");

        // Enregistre la fabrique (dépôts) ET le DbContext scoped (stores Identity, clés Data Protection).
        services.AddDbContextFactory<PortfolioDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                // Base distante mutualisée : on réessaie les erreurs transitoires (coupure, basculement).
                sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

        // Clés de chiffrement (cookies d'authentification, antiforgery) stockées en base :
        // les sessions survivent aux redéploiements et aux recyclages du pool IIS.
        services.AddDataProtection()
            .SetApplicationName("MonPortfolio")
            .PersistKeysToDbContext<PortfolioDbContext>();

        services.AddSingleton<IProjectRepository, ProjectRepository>();
        services.AddSingleton<ISkillRepository, SkillRepository>();
        services.AddSingleton<IExperienceRepository, ExperienceRepository>();
        services.AddSingleton<ICommunityActivityRepository, CommunityActivityRepository>();
        services.AddSingleton<ISocialLinkRepository, SocialLinkRepository>();
        services.AddSingleton<IContactMessageRepository, ContactMessageRepository>();
    }

    private static void AddIdentity(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.Configure<AdminAccountOptions>(configuration.GetSection(AdminAccountOptions.SectionName));

        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<PortfolioDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "MonPortfolio.Admin";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.LoginPath = AdminLoginPath;
            options.AccessDeniedPath = AdminLoginPath;
        });

        // Révocation rapide des sessions (changement de mot de passe, verrouillage…).
        services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromMinutes(5));

        services.AddScoped<IAuthService, AuthService>();
    }

    private static void AddImageStorage(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.Configure<ImageStorageOptions>(configuration.GetSection(ImageStorageOptions.SectionName));
        services.PostConfigure<ImageStorageOptions>(options =>
        {
            if (!Path.IsPathRooted(options.RootPath))
                options.RootPath = Path.Combine(environment.ContentRootPath, options.RootPath);
        });
        services.AddSingleton<IImageStorage, LocalImageStorage>();
    }

    /// <summary>
    /// Sert les images téléversées sous /uploads. Les fichiers créés à l'exécution échappent à
    /// MapStaticAssets (limité aux ressources connues à la compilation) : on passe donc par un PhysicalFileProvider.
    /// </summary>
    public static IApplicationBuilder UseUploadedImages(this IApplicationBuilder app)
    {
        var root = app.ApplicationServices.GetRequiredService<IOptions<ImageStorageOptions>>().Value.RootPath;
        try
        {
            Directory.CreateDirectory(root);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            // Sans droit d'écriture (hébergement mutualisé), le site doit tout de même démarrer :
            // seules les images téléversées sont indisponibles.
            app.ApplicationServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(DependencyInjection))
                .LogError(ex, "Dossier d'images inaccessible ({Root}) : le téléversement est désactivé. " +
                              "Accordez les droits d'écriture sur App_Data.", root);
            return app;
        }

        return app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(root),
            RequestPath = ImageStorageOptions.RequestPath,
            ServeUnknownFileTypes = false,
            OnPrepareResponse = context =>
                // Noms uniques (GUID) : le contenu d'une URL ne change jamais, cache long possible.
                context.Context.Response.Headers.CacheControl = "public, max-age=2592000, immutable"
        });
    }
}
