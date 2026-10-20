using System.Threading.RateLimiting;

namespace MonPortfolio.Web.Security;

public static class SecurityExtensions
{
    /// <summary>
    /// Limite les POST (connexion admin, formulaire de contact) par adresse IP :
    /// freine la force brute et le spam. Le trafic SignalR de Blazor (/_blazor) n'est pas concerné.
    /// Derrière un reverse proxy, activez UseForwardedHeaders pour obtenir la vraie IP cliente.
    /// </summary>
    public static IServiceCollection AddFormPostRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                if (!HttpMethods.IsPost(context.Request.Method) || context.Request.Path.StartsWithSegments("/_blazor"))
                    return RateLimitPartition.GetNoLimiter("unlimited");

                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
            });
            options.OnRejected = async (rejected, cancellationToken) =>
            {
                rejected.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
                await rejected.HttpContext.Response.WriteAsync(
                    "Trop de tentatives. Merci de réessayer dans une minute.\nToo many attempts. Please try again in a minute.",
                    cancellationToken);
            };
        });

        return services;
    }

    /// <summary>En-têtes de sécurité HTTP, dont une Content-Security-Policy stricte (aucun script inline).</summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, IHostEnvironment environment)
    {
        // En développement, Visual Studio / dotnet watch injectent un WebSocket de rechargement à chaud.
        var connectSources = environment.IsDevelopment() ? "'self' ws://localhost:* wss://localhost:*" : "'self'";

        var contentSecurityPolicy = string.Join("; ",
            "default-src 'self'",
            "script-src 'self'",
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
            "font-src 'self' https://fonts.gstatic.com",
            "img-src 'self' data: https:",
            $"connect-src {connectSources}",
            "object-src 'none'",
            "base-uri 'self'",
            "form-action 'self'",
            "frame-ancestors 'none'");

        return app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers.ContentSecurityPolicy = contentSecurityPolicy;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
            headers["Cross-Origin-Opener-Policy"] = "same-origin";
            await next();
        });
    }
}
