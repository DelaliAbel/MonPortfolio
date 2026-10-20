using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Domain.Entities;

/// <summary>
/// Activité communautaire ponctuelle (conférence, meetup, atelier, article…),
/// avec un lien vers sa publication et une image facultative pour la carte.
/// </summary>
public sealed class CommunityActivity : AuditableEntity
{
    public const int TitleMaxLength = 150;
    public const int OrganizerMaxLength = 120;
    public const int LocationMaxLength = 80;
    public const int DescriptionMaxLength = 1000;
    public const int UrlMaxLength = 500;

    private CommunityActivity() { } // EF Core

    public LocalizedText Title { get; private set; } = new(string.Empty);
    public CommunityActivityKind Kind { get; private set; }
    public DateOnly Date { get; private set; }

    /// <summary>Communauté ou événement hôte (ex. « .NET Niger », « Microsoft Learn Student Ambassadors »).</summary>
    public string? Organizer { get; private set; }

    public string? Location { get; private set; }
    public LocalizedText? Description { get; private set; }

    /// <summary>Publication externe qui relate l'activité (post LinkedIn, événement Facebook, replay…).</summary>
    public string? PublicationUrl { get; private set; }

    /// <summary>Image de la carte : fichier téléversé (« /uploads/… ») ou URL http(s).</summary>
    public string? ImageUrl { get; private set; }

    public static CommunityActivity Create(
        LocalizedText title,
        CommunityActivityKind kind,
        DateOnly date,
        string? organizer,
        string? location,
        LocalizedText? description,
        string? publicationUrl,
        string? imageUrl,
        DateTime utcNow)
    {
        var activity = new CommunityActivity();
        activity.Apply(title, kind, date, organizer, location, description, publicationUrl, imageUrl);
        activity.MarkCreated(utcNow);
        return activity;
    }

    public void Update(
        LocalizedText title,
        CommunityActivityKind kind,
        DateOnly date,
        string? organizer,
        string? location,
        LocalizedText? description,
        string? publicationUrl,
        string? imageUrl,
        DateTime utcNow)
    {
        Apply(title, kind, date, organizer, location, description, publicationUrl, imageUrl);
        MarkUpdated(utcNow);
    }

    private void Apply(
        LocalizedText title,
        CommunityActivityKind kind,
        DateOnly date,
        string? organizer,
        string? location,
        LocalizedText? description,
        string? publicationUrl,
        string? imageUrl)
    {
        var validTitle = Guard.RequiredText(title, "Titre", TitleMaxLength);
        var validKind = Guard.Defined(kind, "Type d'activité");
        var validOrganizer = Guard.Optional(organizer, "Organisateur", OrganizerMaxLength);
        var validLocation = Guard.Optional(location, "Lieu", LocationMaxLength);
        var validDescription = Guard.OptionalText(description, "Description", DescriptionMaxLength);
        var validPublication = Guard.OptionalWebUrl(publicationUrl, "Lien de la publication", UrlMaxLength);
        var validImage = Guard.SafeUrl(imageUrl, "Image", UrlMaxLength);

        Title = validTitle;
        Kind = validKind;
        Date = date;
        Organizer = validOrganizer;
        Location = validLocation;
        Description = validDescription;
        PublicationUrl = validPublication;
        ImageUrl = validImage;
    }
}
