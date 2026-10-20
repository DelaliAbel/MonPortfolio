using System.ComponentModel.DataAnnotations;

namespace MonPortfolio.Application.Common;

/// <summary>
/// Valide les modèles d'entrée côté serveur : l'UI valide aussi, mais un cas d'utilisation
/// ne fait jamais confiance à son appelant.
/// </summary>
public static class InputValidator
{
    public static IReadOnlyList<string> Validate(object input)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return results.Select(r => r.ErrorMessage ?? "Valeur invalide.").ToList();
    }
}
