using System.Text.Encodings.Web;
using MonPortfolio.Infrastructure.Diagnostics;

namespace MonPortfolio.Web.Diagnostics;

/// <summary>
/// Mode secours : si le démarrage échoue (configuration, base, droits disque…), on démarre un serveur minimal
/// qui répond 503 avec la cause et la correction, au lieu de la page IIS 500.30 sans détail.
/// Aucun secret n'est affiché ; la pile d'appels n'apparaît qu'en développement.
/// </summary>
public static class StartupFailurePage
{
    public static async Task<int> RunAsync(string[] args, Exception exception)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.Logger.LogCritical(exception, "Le site n'a pas pu démarrer.");
        var html = Render(StartupDiagnosis.From(exception), app.Environment.IsDevelopment() ? exception.ToString() : null);

        app.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.Headers.CacheControl = "no-store";
            context.Response.Headers.RetryAfter = "120";
            context.Response.Headers.XContentTypeOptions = "nosniff";
            context.Response.Headers.XFrameOptions = "DENY";
            await context.Response.WriteAsync(html);
        });

        await app.RunAsync();
        return 1;
    }

    private static string Render(StartupDiagnosis diagnosis, string? developerDetails)
    {
        var encode = HtmlEncoder.Default;
        var details = developerDetails is null
            ? string.Empty
            : $"<h2>Pile d'appels (développement uniquement)</h2><pre>{encode.Encode(developerDetails)}</pre>";

        return $$"""
            <!DOCTYPE html>
            <html lang="fr">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <meta name="robots" content="noindex" />
              <title>Site momentanément indisponible</title>
              <style>
                body { margin: 0; min-height: 100vh; display: grid; place-items: center; padding: 1.5rem;
                       background: #090d16; color: #cbd5e1; font: 16px/1.6 "Segoe UI", system-ui, sans-serif; }
                main { max-width: 720px; width: 100%; padding: 2rem; border-radius: 18px; background: #0f172a;
                       border: 1px solid rgba(212, 175, 55, .45); box-shadow: 0 30px 80px -40px rgba(245, 158, 11, .45); }
                h1 { margin: 0 0 .25rem; color: #f8fafc; font-size: 1.6rem; }
                h2 { margin: 1.5rem 0 .5rem; color: #fbbf24; font-size: .8rem; letter-spacing: .12em; text-transform: uppercase; }
                p { margin: 0; }
                .cause { color: #f8fafc; font-size: 1.15rem; font-weight: 600; }
                code, pre { display: block; padding: .8rem 1rem; border-radius: 10px; background: #090d16;
                            border: 1px solid rgba(148, 163, 184, .2); color: #fde68a; font: 13px/1.5 Consolas, monospace;
                            white-space: pre-wrap; word-break: break-word; }
                small { display: block; margin-top: 1.5rem; color: #64748b; }
              </style>
            </head>
            <body>
              <main>
                <h1>Site momentanément indisponible</h1>
                <p>Le portfolio n'a pas pu démarrer. Merci de réessayer dans quelques minutes.</p>
                <h2>Diagnostic</h2>
                <p class="cause">{{encode.Encode(diagnosis.Title)}}</p>
                <h2>Correction</h2>
                <p>{{encode.Encode(diagnosis.Advice)}}</p>
                <h2>Détail technique</h2>
                <code>{{encode.Encode(diagnosis.Technical)}}</code>
                {{details}}
                <small>Cette page ne s'affiche qu'en cas d'échec du démarrage. Elle ne contient ni mot de passe ni chaîne de connexion.</small>
              </main>
            </body>
            </html>
            """;
    }
}
