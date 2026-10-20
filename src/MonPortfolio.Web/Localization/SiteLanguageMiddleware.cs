using System.Globalization;
using MonPortfolio.Domain.Common;

namespace MonPortfolio.Web.Localization;

public static class SiteLanguageMiddleware
{
    /// <summary>
    /// Fixe la culture de la requête d'après l'URL (« /en… » en anglais, le reste en français, administration comprise).
    /// À placer en tête du pipeline : les pages d'erreur ré-exécutées (404, 500) héritent ainsi de la langue d'origine.
    /// </summary>
    public static IApplicationBuilder UseSiteLanguage(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        var request = context.Request;

        if (HttpMethods.IsGet(request.Method))
        {
            // Sélecteur FR | EN : le choix est mémorisé, puis on redirige vers l'adresse propre de la langue.
            var chosen = request.Query[SiteLanguage.SwitchParameter].ToString();
            if (chosen is Languages.French or Languages.English)
            {
                context.Response.Cookies.Append(SiteLanguage.CookieName, chosen, new CookieOptions
                {
                    Path = "/",
                    MaxAge = TimeSpan.FromDays(365),
                    SameSite = SameSiteMode.Lax,
                    HttpOnly = true,
                    Secure = request.IsHttps,
                    IsEssential = true // simple préférence d'affichage, sans suivi
                });
                context.Response.Redirect(SiteLanguage.HomeOf(chosen));
                return;
            }

            // Retour sur l'accueil français d'un visiteur qui a choisi l'anglais. Les robots, sans cookie,
            // voient toujours la version demandée : chaque langue reste indexée à sa propre adresse.
            if (request.Path == "/" && request.QueryString == QueryString.Empty
                && request.Cookies[SiteLanguage.CookieName] == Languages.English)
            {
                context.Response.Redirect(SiteLanguage.EnglishPrefix);
                return;
            }
        }

        var culture = SiteLanguage.IsEnglishPath(request.Path) ? SiteLanguage.English : SiteLanguage.French;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        await next(context);
    });
}
