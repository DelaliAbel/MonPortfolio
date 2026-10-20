using System.Globalization;
using MonPortfolio.Application;
using MonPortfolio.Application.Security;
using MonPortfolio.Domain.Common;
using MonPortfolio.Infrastructure;
using MonPortfolio.Infrastructure.Persistence.Seeding;
using MonPortfolio.Infrastructure.Persistence.Transfer;
using MonPortfolio.Web.Components;
using MonPortfolio.Web.Diagnostics;
using MonPortfolio.Web.Endpoints;
using MonPortfolio.Web.Localization;
using MonPortfolio.Web.Options;
using MonPortfolio.Web.Security;

var french = CultureInfo.GetCultureInfo("fr-FR");
CultureInfo.DefaultThreadCurrentCulture = french;
CultureInfo.DefaultThreadCurrentUICulture = french;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Contenu éditorial (textes, distinctions, liens) : modifiable sans recompiler.
    // Version anglaise : profile.json complété par les traductions de profile.en.json (mêmes clés, mêmes listes).
    builder.Configuration.AddJsonFile("profile.json", optional: false, reloadOnChange: true);
    builder.Services.Configure<ProfileOptions>(builder.Configuration.GetSection(ProfileOptions.SectionName));
    var englishProfile = new ConfigurationBuilder()
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("profile.json", optional: false, reloadOnChange: true)
        .AddJsonFile("profile.en.json", optional: true, reloadOnChange: true)
        .Build();
    builder.Services.Configure<ProfileOptions>(Languages.English, englishProfile.GetSection(ProfileOptions.SectionName));

    // Textes de l'interface : clé = texte français, traductions dans Resources/SharedResource.en.resx.
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

    // Couches de la Clean Architecture (Program.cs est la racine de composition).
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(Policies.AdminOnly, policy => policy.RequireAuthenticatedUser().RequireRole(Roles.Admin));
    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddFormPostRateLimiting();

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    var app = builder.Build();

    // Commande ponctuelle : copie du contenu d'une autre base (fichier SQLite ou chaîne SQL Server) vers
    // ConnectionStrings:Portfolio, puis arrêt (le site ne démarre pas). Voir DEPLOIEMENT-MONSTERASP.md.
    //   dotnet run --project src/MonPortfolio.Web -- --Import:From "App_Data/portfolio.db" [--Import:Replace true]
    if (app.Configuration["Import:From"] is { Length: > 0 } importSource)
        return await app.Services.ImportContentAsync(importSource, app.Configuration.GetValue<bool>("Import:Replace"));

    await app.Services.InitializeDatabaseAsync();

    app.UseSiteLanguage();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    app.UseHttpsRedirection();
    app.UseSecurityHeaders(app.Environment);
    app.UseUploadedImages();
    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapStaticAssets();
    app.MapAuthEndpoints();
    app.MapRazorComponents<App>()
        // frame-ancestors est déjà fixé à 'none' par UseSecurityHeaders : on évite un second en-tête CSP.
        .AddInteractiveServerRenderMode(options => options.ContentSecurityFrameAncestorsPolicy = null);

    await app.RunAsync();
    return 0;
}
// HostAbortedException : arrêt volontaire par les outils EF (dotnet ef), à laisser passer.
// Commande d'import : l'échec doit remonter en code de sortie, pas en page web.
catch (Exception exception) when (exception is not HostAbortedException && !IsImportCommand(args))
{
    // Hébergement mutualisé : plutôt qu'une page 500.30 muette, on sert un diagnostic lisible.
    return await StartupFailurePage.RunAsync(args, exception);
}

static bool IsImportCommand(string[] args) =>
    args.Any(arg => arg.Contains("Import:From", StringComparison.OrdinalIgnoreCase));
