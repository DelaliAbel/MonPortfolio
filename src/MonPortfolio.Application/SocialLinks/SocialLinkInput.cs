using System.ComponentModel.DataAnnotations;
using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.SocialLinks;

public sealed class SocialLinkInput : IValidatableObject
{
    [EnumDataType(typeof(SocialPlatform), ErrorMessage = "Réseau inconnu.")]
    public SocialPlatform Platform { get; set; } = SocialPlatform.LinkedIn;

    [Required(ErrorMessage = "L'adresse du profil est obligatoire.")]
    [WebUrl, Display(Name = "« Adresse du profil »")]
    [StringLength(SocialLink.UrlMaxLength, ErrorMessage = "L'adresse ne doit pas dépasser {1} caractères.")]
    public string Url { get; set; } = string.Empty;

    [StringLength(SocialLink.LabelMaxLength, ErrorMessage = "Le libellé ne doit pas dépasser {1} caractères.")]
    public string? Label { get; set; }

    [Range(0, 999, ErrorMessage = "L'ordre d'affichage doit être compris entre {1} et {2}.")]
    public int DisplayOrder { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Platform == SocialPlatform.Other && string.IsNullOrWhiteSpace(Label))
            yield return new ValidationResult("Indiquez un libellé pour un lien de type « Autre ».", [nameof(Label)]);
    }

    public static SocialLinkInput From(SocialLinkDto dto) => new()
    {
        Platform = dto.Platform,
        Url = dto.Url,
        Label = dto.Label,
        DisplayOrder = dto.DisplayOrder
    };
}
