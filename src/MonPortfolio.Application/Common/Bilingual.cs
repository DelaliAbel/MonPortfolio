using MonPortfolio.Domain.Common;

namespace MonPortfolio.Application.Common;

/// <summary>Assemblage des textes bilingues à partir des champs FR / EN des formulaires.</summary>
internal static class Bilingual
{
    public static LocalizedText Text(string? fr, string? en) => new(fr ?? string.Empty, en);

    /// <summary>Null si les deux langues sont vides ; le domaine refuse une traduction sans texte français.</summary>
    public static LocalizedText? OptionalText(string? fr, string? en) =>
        string.IsNullOrWhiteSpace(fr) && string.IsNullOrWhiteSpace(en) ? null : Text(fr, en);

    /// <summary>Une entrée par ligne, dans chaque langue.</summary>
    public static LocalizedList Lines(string? fr, string? en) =>
        LocalizedList.Of(TextList.SplitLines(fr), TextList.SplitLines(en));
}
