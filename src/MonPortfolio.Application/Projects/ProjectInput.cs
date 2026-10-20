using System.ComponentModel.DataAnnotations;
using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Entities;

namespace MonPortfolio.Application.Projects;

/// <summary>
/// Données saisies dans le formulaire d'administration d'un projet.
/// Chaque texte a sa traduction anglaise facultative (suffixe « En ») : absente, le site anglais affiche le français.
/// </summary>
public sealed class ProjectInput
{
    [Required(ErrorMessage = "Le titre est obligatoire.")]
    [StringLength(Project.TitleMaxLength, ErrorMessage = "Le titre ne doit pas dépasser {1} caractères.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(Project.TitleMaxLength, ErrorMessage = "Le titre anglais ne doit pas dépasser {1} caractères.")]
    public string? TitleEn { get; set; }

    [Required(ErrorMessage = "Le résumé est obligatoire.")]
    [StringLength(Project.SummaryMaxLength, ErrorMessage = "Le résumé ne doit pas dépasser {1} caractères.")]
    public string Summary { get; set; } = string.Empty;

    [StringLength(Project.SummaryMaxLength, ErrorMessage = "Le résumé anglais ne doit pas dépasser {1} caractères.")]
    public string? SummaryEn { get; set; }

    [StringLength(Project.DescriptionMaxLength, ErrorMessage = "La description ne doit pas dépasser {1} caractères.")]
    public string? Description { get; set; }

    [StringLength(Project.DescriptionMaxLength, ErrorMessage = "La description anglaise ne doit pas dépasser {1} caractères.")]
    public string? DescriptionEn { get; set; }

    [Required(ErrorMessage = "La catégorie est obligatoire.")]
    [StringLength(Project.CategoryMaxLength, ErrorMessage = "La catégorie ne doit pas dépasser {1} caractères.")]
    public string Category { get; set; } = string.Empty;

    [StringLength(Project.CategoryMaxLength, ErrorMessage = "La catégorie anglaise ne doit pas dépasser {1} caractères.")]
    public string? CategoryEn { get; set; }

    /// <summary>Technologies séparées par des virgules.</summary>
    [StringLength(600, ErrorMessage = "La liste des technologies est trop longue.")]
    public string? Technologies { get; set; }

    [SafeUrl, Display(Name = "« Capture écran »")]
    [StringLength(Project.UrlMaxLength)]
    public string? ImageUrl { get; set; }

    [SafeUrl, Display(Name = "« Capture mobile »")]
    [StringLength(Project.UrlMaxLength)]
    public string? MobileImageUrl { get; set; }

    [SafeUrl, Display(Name = "« Dépôt de code »")]
    [StringLength(Project.UrlMaxLength)]
    public string? RepositoryUrl { get; set; }

    [SafeUrl, Display(Name = "« Démo en ligne »")]
    [StringLength(Project.UrlMaxLength)]
    public string? LiveUrl { get; set; }

    public bool IsFeatured { get; set; }

    [Range(0, 999, ErrorMessage = "L'ordre d'affichage doit être compris entre {1} et {2}.")]
    public int DisplayOrder { get; set; }

    public static ProjectInput From(ProjectDto dto) => new()
    {
        Title = dto.Title.Fr,
        TitleEn = dto.Title.En,
        Summary = dto.Summary.Fr,
        SummaryEn = dto.Summary.En,
        Description = dto.Description?.Fr,
        DescriptionEn = dto.Description?.En,
        Category = dto.Category.Fr,
        CategoryEn = dto.Category.En,
        Technologies = TextList.JoinTags(dto.Technologies),
        ImageUrl = dto.ImageUrl,
        MobileImageUrl = dto.MobileImageUrl,
        RepositoryUrl = dto.RepositoryUrl,
        LiveUrl = dto.LiveUrl,
        IsFeatured = dto.IsFeatured,
        DisplayOrder = dto.DisplayOrder
    };
}
