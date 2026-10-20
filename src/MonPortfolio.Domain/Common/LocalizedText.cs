using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MonPortfolio.Domain.Common;

/// <summary>Langues du site : le français fait foi, l'anglais est une traduction facultative.</summary>
public static class Languages
{
    public const string French = "fr";
    public const string English = "en";

    /// <summary>Langue de l'interface courante (CultureInfo.CurrentUICulture), ramenée à « fr » ou « en ».</summary>
    public static string Current =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == English ? English : French;
}

/// <summary>
/// Texte bilingue. Le français est obligatoire ; la traduction anglaise est facultative et,
/// absente, se rabat sur le français : la version anglaise du site n'affiche jamais de trou.
/// </summary>
public sealed record LocalizedText
{
    public LocalizedText(string fr, string? en = null)
    {
        Fr = fr;
        En = string.IsNullOrWhiteSpace(en) ? null : en;
    }

    public string Fr { get; }
    public string? En { get; }

    public bool HasEnglish => En is not null;

    /// <summary>Texte dans la langue demandée, avec repli sur le français.</summary>
    public string In(string? language) => language == Languages.English && En is not null ? En : Fr;

    /// <summary>Texte dans la langue de l'interface courante.</summary>
    public override string ToString() => In(Languages.Current);

    /// <summary>Un texte saisi sans traduction est un texte français.</summary>
    [return: NotNullIfNotNull(nameof(fr))]
    public static implicit operator LocalizedText?(string? fr) => fr is null ? null : new LocalizedText(fr);
}

/// <summary>Liste bilingue (réalisations d'une étape du parcours…), avec le même repli sur le français.</summary>
public sealed record LocalizedList
{
    public LocalizedList(IReadOnlyList<string> fr, IReadOnlyList<string>? en = null)
    {
        Fr = fr;
        En = en ?? [];
    }

    public static LocalizedList Empty { get; } = new([]);

    public IReadOnlyList<string> Fr { get; }
    public IReadOnlyList<string> En { get; }

    public bool HasEnglish => En.Count > 0;

    public IReadOnlyList<string> In(string? language) => language == Languages.English && En.Count > 0 ? En : Fr;

    /// <summary>Éléments dans la langue de l'interface courante.</summary>
    public IReadOnlyList<string> Current => In(Languages.Current);

    public static LocalizedList Of(IEnumerable<string>? fr, IEnumerable<string>? en = null) =>
        new((fr ?? []).ToList(), (en ?? []).ToList());

    public bool Equals(LocalizedList? other) =>
        other is not null && Fr.SequenceEqual(other.Fr) && En.SequenceEqual(other.En);

    public override int GetHashCode() => HashCode.Combine(Fr.Count, En.Count);
}
