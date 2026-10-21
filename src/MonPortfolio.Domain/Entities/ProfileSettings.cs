using MonPortfolio.Domain.Common;

namespace MonPortfolio.Domain.Entities;

/// <summary>
/// Réglages du profil modifiables depuis l'administration : photo du hero et technologies affichées sur la photo.
/// Une seule ligne. Stockés en base (et non dans profile.json) pour survivre aux redéploiements ;
/// les textes du profil restent, eux, dans profile.json.
/// </summary>
public sealed class ProfileSettings : AuditableEntity
{
    public const int UrlMaxLength = 500;
    public const int TechnologyMaxLength = 40;
    public const int MaxHeroStack = 6;

    /// <summary>Stack affichée tant que les réglages n'ont jamais été enregistrés.</summary>
    public static IReadOnlyList<string> DefaultHeroStack { get; } = [".NET", "Java", "Flutter", "Azure"];

    private ProfileSettings() { } // EF Core

    /// <summary>Photo téléversée (« /uploads/… ») ou URL http(s) ; null : la photo par défaut de profile.json.</summary>
    public string? PhotoUrl { get; private set; }

    /// <summary>Technologies dont le logo est posé sur la photo ; vide : la pastille est masquée.</summary>
    public IReadOnlyList<string> HeroStack { get; private set; } = [];

    public static ProfileSettings Create(string? photoUrl, IEnumerable<string>? heroStack, DateTime utcNow)
    {
        var settings = new ProfileSettings();
        settings.Apply(photoUrl, heroStack);
        settings.MarkCreated(utcNow);
        return settings;
    }

    public void Update(string? photoUrl, IEnumerable<string>? heroStack, DateTime utcNow)
    {
        Apply(photoUrl, heroStack);
        MarkUpdated(utcNow);
    }

    private void Apply(string? photoUrl, IEnumerable<string>? heroStack)
    {
        var validPhoto = Guard.SafeUrl(photoUrl, "Photo", UrlMaxLength);
        var validStack = Guard.Tags(heroStack, "Stack sur la photo", MaxHeroStack, TechnologyMaxLength);

        PhotoUrl = validPhoto;
        HeroStack = validStack;
    }
}
