using MonPortfolio.Domain.Common;

namespace MonPortfolio.Domain.Entities;

/// <summary>Réalisation présentée dans la grille de projets du portfolio.</summary>
public sealed class Project : AuditableEntity
{
    public const int TitleMaxLength = 120;
    public const int SummaryMaxLength = 300;
    public const int DescriptionMaxLength = 4000;
    public const int CategoryMaxLength = 60;
    public const int UrlMaxLength = 500;
    public const int TechnologyMaxLength = 40;
    public const int MaxTechnologies = 15;

    private Project() { } // EF Core

    public LocalizedText Title { get; private set; } = new(string.Empty);
    public LocalizedText Summary { get; private set; } = new(string.Empty);
    public LocalizedText? Description { get; private set; }
    public LocalizedText Category { get; private set; } = new(string.Empty);
    public IReadOnlyList<string> Technologies { get; private set; } = [];

    /// <summary>Capture d'écran (web ou desktop), présentée dans une fenêtre inclinée.</summary>
    public string? ImageUrl { get; private set; }

    /// <summary>Capture mobile (portrait), présentée dans un cadre de téléphone.</summary>
    public string? MobileImageUrl { get; private set; }

    public string? RepositoryUrl { get; private set; }
    public string? LiveUrl { get; private set; }
    public bool IsFeatured { get; private set; }
    public int DisplayOrder { get; private set; }

    public static Project Create(
        LocalizedText title,
        LocalizedText summary,
        LocalizedText? description,
        LocalizedText category,
        IEnumerable<string>? technologies,
        string? imageUrl,
        string? mobileImageUrl,
        string? repositoryUrl,
        string? liveUrl,
        bool isFeatured,
        int displayOrder,
        DateTime utcNow)
    {
        var project = new Project();
        project.Apply(title, summary, description, category, technologies, imageUrl, mobileImageUrl, repositoryUrl, liveUrl, isFeatured, displayOrder);
        project.MarkCreated(utcNow);
        return project;
    }

    public void Update(
        LocalizedText title,
        LocalizedText summary,
        LocalizedText? description,
        LocalizedText category,
        IEnumerable<string>? technologies,
        string? imageUrl,
        string? mobileImageUrl,
        string? repositoryUrl,
        string? liveUrl,
        bool isFeatured,
        int displayOrder,
        DateTime utcNow)
    {
        Apply(title, summary, description, category, technologies, imageUrl, mobileImageUrl, repositoryUrl, liveUrl, isFeatured, displayOrder);
        MarkUpdated(utcNow);
    }

    private void Apply(
        LocalizedText title,
        LocalizedText summary,
        LocalizedText? description,
        LocalizedText category,
        IEnumerable<string>? technologies,
        string? imageUrl,
        string? mobileImageUrl,
        string? repositoryUrl,
        string? liveUrl,
        bool isFeatured,
        int displayOrder)
    {
        Title = Guard.RequiredText(title, "Titre", TitleMaxLength);
        Summary = Guard.RequiredText(summary, "Résumé", SummaryMaxLength);
        Description = Guard.OptionalText(description, "Description", DescriptionMaxLength);
        Category = Guard.RequiredText(category, "Catégorie", CategoryMaxLength);
        Technologies = Guard.Tags(technologies, "Technologies", MaxTechnologies, TechnologyMaxLength);
        ImageUrl = Guard.SafeUrl(imageUrl, "Capture écran", UrlMaxLength);
        MobileImageUrl = Guard.SafeUrl(mobileImageUrl, "Capture mobile", UrlMaxLength);
        RepositoryUrl = Guard.SafeUrl(repositoryUrl, "Dépôt de code", UrlMaxLength);
        LiveUrl = Guard.SafeUrl(liveUrl, "Démo en ligne", UrlMaxLength);
        IsFeatured = isFeatured;
        DisplayOrder = Guard.InRange(displayOrder, 0, 999, "Ordre d'affichage");
    }
}
