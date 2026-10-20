using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Domain.Entities;

/// <summary>Étape du parcours : poste occupé, diplôme ou engagement communautaire.</summary>
public sealed class Experience : AuditableEntity
{
    public const int TitleMaxLength = 120;
    public const int OrganizationMaxLength = 120;
    public const int LocationMaxLength = 80;
    public const int DescriptionMaxLength = 1000;
    public const int HighlightMaxLength = 300;
    public const int MaxHighlights = 8;
    public const int TechnologyMaxLength = 40;
    public const int MaxTechnologies = 15;
    public const int UrlMaxLength = 500;

    private Experience() { } // EF Core

    public string Title { get; private set; } = string.Empty;
    public string Organization { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public ExperienceType Type { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyList<string> Highlights { get; private set; } = [];
    public IReadOnlyList<string> Technologies { get; private set; } = [];

    /// <summary>Lien externe : site de l'organisation, publication, attestation…</summary>
    public string? LinkUrl { get; private set; }

    /// <summary>Poste toujours en cours (pas de date de fin).</summary>
    public bool IsCurrent => EndDate is null;

    public static Experience Create(
        string title,
        string organization,
        string? location,
        ExperienceType type,
        DateOnly startDate,
        DateOnly? endDate,
        string? description,
        IEnumerable<string>? highlights,
        IEnumerable<string>? technologies,
        string? linkUrl,
        DateTime utcNow)
    {
        var experience = new Experience();
        experience.Apply(title, organization, location, type, startDate, endDate, description, highlights, technologies, linkUrl);
        experience.MarkCreated(utcNow);
        return experience;
    }

    public void Update(
        string title,
        string organization,
        string? location,
        ExperienceType type,
        DateOnly startDate,
        DateOnly? endDate,
        string? description,
        IEnumerable<string>? highlights,
        IEnumerable<string>? technologies,
        string? linkUrl,
        DateTime utcNow)
    {
        Apply(title, organization, location, type, startDate, endDate, description, highlights, technologies, linkUrl);
        MarkUpdated(utcNow);
    }

    private void Apply(
        string title,
        string organization,
        string? location,
        ExperienceType type,
        DateOnly startDate,
        DateOnly? endDate,
        string? description,
        IEnumerable<string>? highlights,
        IEnumerable<string>? technologies,
        string? linkUrl)
    {
        if (endDate is { } end && end < startDate)
            throw new DomainException("La date de fin ne peut pas précéder la date de début.");

        // Toutes les valeurs sont validées avant la première affectation : un échec ne laisse rien à moitié modifié.
        var validTitle = Guard.Required(title, "Intitulé", TitleMaxLength);
        var validOrganization = Guard.Required(organization, "Organisation", OrganizationMaxLength);
        var validLocation = Guard.Optional(location, "Lieu", LocationMaxLength);
        var validType = Guard.Defined(type, "Type");
        var validDescription = Guard.Optional(description, "Description", DescriptionMaxLength);
        var validHighlights = Guard.Tags(highlights, "Réalisations", MaxHighlights, HighlightMaxLength);
        var validTechnologies = Guard.Tags(technologies, "Technologies", MaxTechnologies, TechnologyMaxLength);
        var validLink = Guard.OptionalWebUrl(linkUrl, "Lien", UrlMaxLength);

        Title = validTitle;
        Organization = validOrganization;
        Location = validLocation;
        Type = validType;
        StartDate = startDate;
        EndDate = endDate;
        Description = validDescription;
        Highlights = validHighlights;
        Technologies = validTechnologies;
        LinkUrl = validLink;
    }
}
