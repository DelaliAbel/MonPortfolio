using System.ComponentModel.DataAnnotations;
using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Experiences;

public sealed class ExperienceInput : IValidatableObject
{
    [Required(ErrorMessage = "L'intitulé est obligatoire.")]
    [StringLength(Experience.TitleMaxLength, ErrorMessage = "L'intitulé ne doit pas dépasser {1} caractères.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'organisation est obligatoire.")]
    [StringLength(Experience.OrganizationMaxLength, ErrorMessage = "L'organisation ne doit pas dépasser {1} caractères.")]
    public string Organization { get; set; } = string.Empty;

    [StringLength(Experience.LocationMaxLength, ErrorMessage = "Le lieu ne doit pas dépasser {1} caractères.")]
    public string? Location { get; set; }

    [EnumDataType(typeof(ExperienceType), ErrorMessage = "Type inconnu.")]
    public ExperienceType Type { get; set; } = ExperienceType.Professional;

    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public DateOnly? EndDate { get; set; }

    /// <summary>Poste en cours : la date de fin est ignorée.</summary>
    public bool IsCurrent { get; set; }

    [StringLength(Experience.DescriptionMaxLength, ErrorMessage = "La description ne doit pas dépasser {1} caractères.")]
    public string? Description { get; set; }

    /// <summary>Réalisations clés, une par ligne.</summary>
    [StringLength(2500, ErrorMessage = "Les réalisations sont trop longues.")]
    public string? Highlights { get; set; }

    /// <summary>Technologies séparées par des virgules.</summary>
    [StringLength(600, ErrorMessage = "La liste des technologies est trop longue.")]
    public string? Technologies { get; set; }

    /// <summary>Lien externe facultatif : site de l'organisation, publication, attestation…</summary>
    [WebUrl, Display(Name = "« Lien »")]
    [StringLength(Experience.UrlMaxLength)]
    public string? LinkUrl { get; set; }

    public DateOnly? EffectiveEndDate => IsCurrent ? null : EndDate;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsCurrent && EndDate is null)
            yield return new ValidationResult(
                "Indiquez une date de fin ou cochez « En cours ».", [nameof(EndDate)]);

        if (EffectiveEndDate is { } end && end < StartDate)
            yield return new ValidationResult(
                "La date de fin ne peut pas précéder la date de début.", [nameof(EndDate)]);
    }

    public static ExperienceInput From(ExperienceDto dto) => new()
    {
        Title = dto.Title,
        Organization = dto.Organization,
        Location = dto.Location,
        Type = dto.Type,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        IsCurrent = dto.IsCurrent,
        Description = dto.Description,
        Highlights = TextList.JoinLines(dto.Highlights),
        Technologies = TextList.JoinTags(dto.Technologies),
        LinkUrl = dto.LinkUrl
    };
}
