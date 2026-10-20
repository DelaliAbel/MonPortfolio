using MonPortfolio.Domain.Common;

namespace MonPortfolio.Web.Formatting;

/// <summary>Recherche des listes d'administration : un texte bilingue correspond si l'une de ses langues contient le terme.</summary>
public static class TextSearch
{
    public static bool Matches(this LocalizedText? text, string term) =>
        text is not null
        && (text.Fr.Contains(term, StringComparison.OrdinalIgnoreCase)
            || text.En?.Contains(term, StringComparison.OrdinalIgnoreCase) == true);
}
