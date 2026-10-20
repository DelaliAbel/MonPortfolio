using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Web.Formatting;

/// <summary>Présentation des liens externes : icônes par réseau / type d'activité, adresses raccourcies.</summary>
public static class LinkDisplay
{
    public static string PlatformIcon(SocialPlatform platform) => platform switch
    {
        SocialPlatform.LinkedIn => "linkedin",
        SocialPlatform.GitHub => "github",
        SocialPlatform.MicrosoftLearn => "graduation",
        SocialPlatform.Facebook => "facebook",
        SocialPlatform.X => "x-brand",
        SocialPlatform.YouTube => "youtube",
        SocialPlatform.Instagram => "instagram",
        SocialPlatform.StackOverflow => "stackoverflow",
        SocialPlatform.Medium => "book-open",
        SocialPlatform.Website => "globe",
        _ => "link"
    };

    /// <summary>Clé du réseau (nom du fichier de logo, attribut data-platform).</summary>
    public static string PlatformKey(SocialPlatform platform) => platform.ToString().ToLowerInvariant();

    /// <summary>
    /// Logo officiel du réseau (wwwroot/logos/social), affiché tel quel ;
    /// null pour « Site web » et « Autre », qui n'ont pas de marque : une icône du site les remplace.
    /// </summary>
    public static string? PlatformLogo(SocialPlatform platform) => platform switch
    {
        SocialPlatform.Website or SocialPlatform.Other => null,
        _ => $"logos/social/{PlatformKey(platform)}.svg"
    };

    /// <summary>
    /// Variante officielle blanche des logos monochromes noirs (GitHub, X, Medium), affichée en thème sombre
    /// où la version noire serait invisible. Null pour les logos déjà lisibles sur fond sombre.
    /// </summary>
    public static string? PlatformLogoOnDark(SocialPlatform platform) => platform switch
    {
        SocialPlatform.GitHub or SocialPlatform.X or SocialPlatform.Medium => $"logos/social/{PlatformKey(platform)}-white.svg",
        _ => null
    };

    public static string ActivityIcon(CommunityActivityKind kind) => kind switch
    {
        CommunityActivityKind.Talk => "mic",
        CommunityActivityKind.Meetup => "users",
        CommunityActivityKind.Workshop => "terminal",
        CommunityActivityKind.Training => "graduation",
        CommunityActivityKind.Article => "book-open",
        CommunityActivityKind.Livestream => "video",
        CommunityActivityKind.Hackathon => "trophy",
        CommunityActivityKind.Mentoring => "target",
        _ => "sparkles"
    };

    /// <summary>« https://www.linkedin.com/in/delali/ » → « linkedin.com/in/delali ».</summary>
    public static string ShortUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url;

        var host = uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? uri.Host[4..] : uri.Host;
        var path = uri.AbsolutePath.TrimEnd('/');
        var text = host + path;
        return text.Length > 42 ? text[..41] + "…" : text;
    }

    /// <summary>Domaine seul : « nita.ne ».</summary>
    public static string Host(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
            ? uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? uri.Host[4..] : uri.Host
            : url;
}
