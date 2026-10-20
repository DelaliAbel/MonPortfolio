using System.ComponentModel.DataAnnotations;
using MonPortfolio.Domain.Entities;

namespace MonPortfolio.Application.Contact;

public sealed class ContactInput
{
    [Required(ErrorMessage = "Votre nom est obligatoire.")]
    [StringLength(ContactMessage.NameMaxLength, ErrorMessage = "Le nom ne doit pas dépasser {1} caractères.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Votre e-mail est obligatoire.")]
    [EmailAddress(ErrorMessage = "Adresse e-mail invalide.")]
    [StringLength(ContactMessage.EmailMaxLength)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le sujet est obligatoire.")]
    [StringLength(ContactMessage.SubjectMaxLength, ErrorMessage = "Le sujet ne doit pas dépasser {1} caractères.")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le message est obligatoire.")]
    [StringLength(ContactMessage.BodyMaxLength, MinimumLength = ContactMessage.BodyMinLength,
        ErrorMessage = "Le message doit contenir entre {2} et {1} caractères.")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Pot de miel anti-spam : champ masqué aux humains. S'il est rempli, l'envoi est
    /// silencieusement ignoré (le robot reçoit une réponse « succès » et n'insiste pas).
    /// </summary>
    public string? Website { get; set; }
}
