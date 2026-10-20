using System.ComponentModel.DataAnnotations;

namespace MonPortfolio.Application.Common;

/// <summary>
/// URL http(s) absolue ou chemin local ("/images/x.png"). Contrairement à <see cref="UrlAttribute"/>,
/// accepte les chemins relatifs et refuse explicitement les schémas exécutables (javascript:, data:).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SafeUrlAttribute() : ValidationAttribute("Le champ {0} doit être une URL http(s) ou un chemin commençant par « / ».")
{
    public override bool IsValid(object? value)
    {
        if (value is not string url || string.IsNullOrWhiteSpace(url))
            return true;

        url = url.Trim();
        if (url.StartsWith('/'))
            return !url.StartsWith("//") && !url.StartsWith("/\\");

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }
}
