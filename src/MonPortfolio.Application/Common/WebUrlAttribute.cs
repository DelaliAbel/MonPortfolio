using System.ComponentModel.DataAnnotations;

namespace MonPortfolio.Application.Common;

/// <summary>URL publique complète en http(s) (profil social, publication). Les chemins locaux sont refusés.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class WebUrlAttribute() : ValidationAttribute("Le champ {0} doit être une URL complète commençant par https://.")
{
    public override bool IsValid(object? value) =>
        value is not string url
        || string.IsNullOrWhiteSpace(url)
        || (Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            && !string.IsNullOrEmpty(uri.Host));
}
