using Microsoft.AspNetCore.Antiforgery;
using MonPortfolio.Application.Security;

namespace MonPortfolio.Web.Endpoints;

public static class AuthEndpoints
{
    /// <summary>
    /// La déconnexion doit modifier le cookie : elle passe donc par une requête HTTP classique
    /// (POST + jeton antiforgery) et non par le circuit SignalR d'un composant interactif.
    /// </summary>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/admin/logout", async (HttpContext context, IAntiforgery antiforgery, IAuthService auth) =>
            {
                if (!await antiforgery.IsRequestValidAsync(context))
                    return Results.BadRequest();

                await auth.SignOutAsync();
                return Results.LocalRedirect("/admin/login?loggedOut=true");
            })
            .RequireAuthorization(Policies.AdminOnly)
            .DisableAntiforgery(); // validé explicitement ci-dessus

        return endpoints;
    }
}
