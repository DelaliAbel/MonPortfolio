using System.Net.Mail;

namespace MonPortfolio.Domain.Common;

/// <summary>Clauses de garde partagées par les entités pour protéger leurs invariants.</summary>
internal static class Guard
{
    public static string Required(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"Le champ « {field} » est obligatoire.");

        return MaxLength(value.Trim(), field, maxLength);
    }

    public static string? Optional(string? value, string field, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? null : MaxLength(value.Trim(), field, maxLength);

    public static int InRange(int value, int min, int max, string field) =>
        value < min || value > max
            ? throw new DomainException($"Le champ « {field} » doit être compris entre {min} et {max}.")
            : value;

    public static TEnum Defined<TEnum>(TEnum value, string field) where TEnum : struct, Enum =>
        Enum.IsDefined(value) ? value : throw new DomainException($"La valeur du champ « {field} » est inconnue.");

    public static string Email(string? value, string field, int maxLength)
    {
        var email = Required(value, field, maxLength);
        return MailAddress.TryCreate(email, out var address) && address.Address == email
            ? email
            : throw new DomainException($"Le champ « {field} » doit être une adresse e-mail valide.");
    }

    /// <summary>
    /// N'accepte que des URL http(s) absolues ou des chemins locaux ("/images/...").
    /// Bloque notamment les schémas dangereux (javascript:, data:) rendus dans des href.
    /// </summary>
    public static string? SafeUrl(string? value, string field, int maxLength)
    {
        var url = Optional(value, field, maxLength);
        if (url is null)
            return null;

        var isLocalPath = url.StartsWith('/') && !url.StartsWith("//") && !url.StartsWith("/\\");
        var isWebUrl = Uri.TryCreate(url, UriKind.Absolute, out var uri)
                       && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

        return isLocalPath || isWebUrl
            ? url
            : throw new DomainException($"Le champ « {field} » doit être une URL http(s) ou un chemin local.");
    }

    /// <summary>URL publique complète en http(s) : lien vers un profil social ou une publication externe.</summary>
    public static string WebUrl(string? value, string field, int maxLength)
    {
        var url = Required(value, field, maxLength);
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
               && !string.IsNullOrEmpty(uri.Host)
            ? url
            : throw new DomainException($"Le champ « {field} » doit être une URL complète commençant par https://.");
    }

    public static string? OptionalWebUrl(string? value, string field, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? null : WebUrl(value, field, maxLength);

    /// <summary>Normalise une liste d'étiquettes : trim, sans doublons, bornée en taille.</summary>
    public static IReadOnlyList<string> Tags(IEnumerable<string>? values, string field, int maxCount, int maxLength)
    {
        var tags = (values ?? [])
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => MaxLength(v.Trim(), field, maxLength))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return tags.Count > maxCount
            ? throw new DomainException($"Le champ « {field} » accepte au plus {maxCount} éléments.")
            : tags;
    }

    private static string MaxLength(string value, string field, int maxLength) =>
        value.Length > maxLength
            ? throw new DomainException($"Le champ « {field} » ne doit pas dépasser {maxLength} caractères.")
            : value;
}
