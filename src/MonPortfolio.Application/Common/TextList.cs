namespace MonPortfolio.Application.Common;

/// <summary>Conversion entre les listes du domaine et leur saisie textuelle dans les formulaires.</summary>
public static class TextList
{
    private static readonly char[] TagSeparators = [',', ';', '\n', '\r'];
    private static readonly char[] LineSeparators = ['\n', '\r'];

    /// <summary>"C#, Blazor; Azure" → ["C#", "Blazor", "Azure"]</summary>
    public static IReadOnlyList<string> SplitTags(string? text) =>
        (text ?? string.Empty).Split(TagSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>Une entrée par ligne (les puces éventuelles en début de ligne sont retirées).</summary>
    public static IReadOnlyList<string> SplitLines(string? text) =>
        (text ?? string.Empty)
            .Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.TrimStart('•', '-', '*', ' '))
            .Where(line => line.Length > 0)
            .ToList();

    public static string JoinTags(IEnumerable<string> values) => string.Join(", ", values);

    public static string JoinLines(IEnumerable<string> values) => string.Join(Environment.NewLine, values);
}
