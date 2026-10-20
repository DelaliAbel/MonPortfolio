using System.ComponentModel.DataAnnotations;
using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Certifications;

/// <summary>Données saisies dans le formulaire d'administration d'une certification ou d'une attestation.</summary>
public sealed class CertificationInput
{
    [Required(ErrorMessage = "L'intitulé est obligatoire.")]
    [StringLength(Certification.NameMaxLength, ErrorMessage = "L'intitulé ne doit pas dépasser {1} caractères.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Traduction facultative : beaucoup d'intitulés (Azure Fundamentals…) sont déjà en anglais.</summary>
    [StringLength(Certification.NameMaxLength, ErrorMessage = "L'intitulé anglais ne doit pas dépasser {1} caractères.")]
    public string? NameEn { get; set; }

    [EnumDataType(typeof(CertificationKind), ErrorMessage = "Type inconnu.")]
    public CertificationKind Kind { get; set; } = CertificationKind.Certification;

    [StringLength(Certification.IssuerMaxLength, ErrorMessage = "L'organisme ne doit pas dépasser {1} caractères.")]
    public string? Issuer { get; set; }

    public DateOnly? IssuedOn { get; set; }

    [StringLength(Certification.CredentialIdMaxLength, ErrorMessage = "L'identifiant ne doit pas dépasser {1} caractères.")]
    public string? CredentialId { get; set; }

    [WebUrl, Display(Name = "« Lien du certificat »")]
    [StringLength(Certification.UrlMaxLength)]
    public string? CredentialUrl { get; set; }

    [SafeUrl, Display(Name = "« Image »")]
    [StringLength(Certification.UrlMaxLength)]
    public string? ImageUrl { get; set; }

    [Range(0, 999, ErrorMessage = "L'ordre d'affichage doit être compris entre {1} et {2}.")]
    public int DisplayOrder { get; set; }

    public static CertificationInput From(CertificationDto dto) => new()
    {
        Name = dto.Name.Fr,
        NameEn = dto.Name.En,
        Kind = dto.Kind,
        Issuer = dto.Issuer,
        IssuedOn = dto.IssuedOn,
        CredentialId = dto.CredentialId,
        CredentialUrl = dto.CredentialUrl,
        ImageUrl = dto.ImageUrl,
        DisplayOrder = dto.DisplayOrder
    };
}
