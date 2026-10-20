using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonPortfolio.Application.Security;
using MonPortfolio.Infrastructure.Identity;

namespace MonPortfolio.Infrastructure.Persistence.Seeding;

public static class DatabaseInitializer
{
    /// <summary>
    /// Applique les migrations, crée le compte administrateur et, si la base est vide,
    /// injecte le contenu initial issu du CV (modifiable ensuite depuis /admin).
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseInitializer));

        var db = provider.GetRequiredService<PortfolioDbContext>();
        await db.Database.MigrateAsync(cancellationToken);

        await SeedAdminAsync(provider, logger);
        await PortfolioSeedData.SeedAsync(db, provider.GetRequiredService<TimeProvider>(), logger, cancellationToken);
    }

    private static async Task SeedAdminAsync(IServiceProvider provider, ILogger logger)
    {
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(Roles.Admin))
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));

        var options = provider.GetRequiredService<IOptions<AdminAccountOptions>>().Value;
        if (string.IsNullOrWhiteSpace(options.Email))
        {
            logger.LogWarning("AdminAccount:Email n'est pas configuré : aucun compte administrateur créé.");
            return;
        }

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync(options.Email);

        if (admin is null)
        {
            if (string.IsNullOrWhiteSpace(options.Password))
            {
                logger.LogWarning("AdminAccount:Password n'est pas configuré : le compte {Email} n'a pas été créé.", options.Email);
                return;
            }

            admin = new ApplicationUser
            {
                UserName = options.Email,
                Email = options.Email,
                EmailConfirmed = true,
                DisplayName = options.DisplayName
            };

            var created = await userManager.CreateAsync(admin, options.Password);
            if (!created.Succeeded)
            {
                logger.LogError("Création du compte administrateur impossible : {Errors}",
                    string.Join(" ", created.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Compte administrateur {Email} créé.", options.Email);
        }

        if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
            await userManager.AddToRoleAsync(admin, Roles.Admin);
    }
}
