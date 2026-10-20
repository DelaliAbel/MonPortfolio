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

    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public IReadOnlyList<string> Technologies { get; private set; } = [];
    public string? ImageUrl { get; private set; }
    public string? RepositoryUrl { get; private set; }
    public string? LiveUrl { get; private set; }
    public bool IsFeatured { get; private set; }
    public int DisplayOrder { get; private set; }

    public static Project Create(
        string title,
        string summary,
        string? description,
        string category,
        IEnumerable<string>? technologies,
        string? imageUrl,
        string? repositoryUrl,
        string? liveUrl,
        bool isFeatured,
        int displayOrder,
        DateTime utcNow)
    {
        var project = new Project();
        project.Apply(title, summary, description, category, technologies, imageUrl, repositoryUrl, liveUrl, isFeatured, displayOrder);
        project.MarkCreated(utcNow);
        return project;
    }

    public void Update(
        string title,
        string summary,
        string? description,
        string category,
        IEnumerable<string>? technologies,
        string? imageUrl,
        string? repositoryUrl,
        string? liveUrl,
        bool isFeatured,
        int displayOrder,
        DateTime utcNow)
    {
        Apply(title, summary, description, category, technologies, imageUrl, repositoryUrl, liveUrl, isFeatured, displayOrder);
        MarkUpdated(utcNow);
    }

    private void Apply(
        string title,
        string summary,
        string? description,
        string category,
        IEnumerable<string>? technologies,
        string? imageUrl,
        string? repositoryUrl,
        string? liveUrl,
        bool isFeatured,
        int displayOrder)
    {
        Title = Guard.Required(title, "Titre", TitleMaxLength);
        Summary = Guard.Required(summary, "Résumé", SummaryMaxLength);
        Description = Guard.Optional(description, "Description", DescriptionMaxLength);
        Category = Guard.Required(category, "Catégorie", CategoryMaxLength);
        Technologies = Guard.Tags(technologies, "Technologies", MaxTechnologies, TechnologyMaxLength);
        ImageUrl = Guard.SafeUrl(imageUrl, "Image", UrlMaxLength);
        RepositoryUrl = Guard.SafeUrl(repositoryUrl, "Dépôt de code", UrlMaxLength);
        LiveUrl = Guard.SafeUrl(liveUrl, "Démo en ligne", UrlMaxLength);
        IsFeatured = isFeatured;
        DisplayOrder = Guard.InRange(displayOrder, 0, 999, "Ordre d'affichage");
    }
}
