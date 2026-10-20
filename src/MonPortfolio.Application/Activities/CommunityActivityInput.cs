using System.ComponentModel.DataAnnotations;
using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Activities;

public sealed class CommunityActivityInput
{
    [Required(ErrorMessage = "Le titre est obligatoire.")]
    [StringLength(CommunityActivity.TitleMaxLength, ErrorMessage = "Le titre ne doit pas dépasser {1} caractères.")]
    public string Title { get; set; } = string.Empty;

    [EnumDataType(typeof(CommunityActivityKind), ErrorMessage = "Type d'activité inconnu.")]
    public CommunityActivityKind Kind { get; set; } = CommunityActivityKind.Talk;

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [StringLength(CommunityActivity.OrganizerMaxLength, ErrorMessage = "L'organisateur ne doit pas dépasser {1} caractères.")]
    public string? Organizer { get; set; }

    [StringLength(CommunityActivity.LocationMaxLength, ErrorMessage = "Le lieu ne doit pas dépasser {1} caractères.")]
    public string? Location { get; set; }

    [StringLength(CommunityActivity.DescriptionMaxLength, ErrorMessage = "La description ne doit pas dépasser {1} caractères.")]
    public string? Description { get; set; }

    [WebUrl, Display(Name = "« Lien de la publication »")]
    [StringLength(CommunityActivity.UrlMaxLength)]
    public string? PublicationUrl { get; set; }

    [SafeUrl, Display(Name = "« Image »")]
    [StringLength(CommunityActivity.UrlMaxLength)]
    public string? ImageUrl { get; set; }

    public static CommunityActivityInput From(CommunityActivityDto dto) => new()
    {
        Title = dto.Title,
        Kind = dto.Kind,
        Date = dto.Date,
        Organizer = dto.Organizer,
        Location = dto.Location,
        Description = dto.Description,
        PublicationUrl = dto.PublicationUrl,
        ImageUrl = dto.ImageUrl
    };
}
