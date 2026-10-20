using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.SocialLinks;

public sealed record SocialLinkDto(Guid Id, SocialPlatform Platform, string Url, string? Label, int DisplayOrder)
{
    /// <summary>Nom affiché : libellé libre s'il existe, sinon le nom du réseau.</summary>
    public string DisplayName => Label ?? SocialPlatforms.Label(Platform);
}

/// <summary>Libellés et exemples d'adresse par réseau (présentation, hors domaine).</summary>
public static class SocialPlatforms
{
    public static IReadOnlyList<SocialPlatform> All { get; } = Enum.GetValues<SocialPlatform>();

    public static string Label(SocialPlatform platform) => platform switch
    {
        SocialPlatform.LinkedIn => "LinkedIn",
        SocialPlatform.GitHub => "GitHub",
        SocialPlatform.MicrosoftLearn => "Microsoft Learn",
        SocialPlatform.Facebook => "Facebook",
        SocialPlatform.X => "X (Twitter)",
        SocialPlatform.YouTube => "YouTube",
        SocialPlatform.Instagram => "Instagram",
        SocialPlatform.StackOverflow => "Stack Overflow",
        SocialPlatform.Medium => "Medium",
        SocialPlatform.Website => Languages.Current == Languages.English ? "Website / Blog" : "Site web / Blog",
        SocialPlatform.Other => Languages.Current == Languages.English ? "Other" : "Autre",
        _ => platform.ToString()
    };

    public static string UrlExample(SocialPlatform platform) => platform switch
    {
        SocialPlatform.LinkedIn => "https://www.linkedin.com/in/votre-profil",
        SocialPlatform.GitHub => "https://github.com/votre-compte",
        SocialPlatform.MicrosoftLearn => "https://learn.microsoft.com/fr-fr/users/votre-profil",
        SocialPlatform.Facebook => "https://www.facebook.com/votre-page",
        SocialPlatform.X => "https://x.com/votre-compte",
        SocialPlatform.YouTube => "https://www.youtube.com/@votre-chaine",
        SocialPlatform.Instagram => "https://www.instagram.com/votre-compte",
        SocialPlatform.StackOverflow => "https://stackoverflow.com/users/123456/votre-nom",
        SocialPlatform.Medium => "https://medium.com/@votre-compte",
        _ => "https://…"
    };
}
