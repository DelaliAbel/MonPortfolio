using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MonPortfolio.Domain.Common;

namespace MonPortfolio.Infrastructure.Persistence.Configurations;

/// <summary>
/// Stockage des textes bilingues dans leur colonne d'origine, au format JSON : {"fr":"…","en":"…"}.
/// La lecture tolère l'ancien format (texte ou tableau JSON brut, considéré comme du français) :
/// une base antérieure au bilinguisme (import, sauvegarde) reste lisible telle quelle.
/// </summary>
internal static class LocalizedConverters
{
    private static readonly JsonSerializerOptions Json = new()
    {
        // Colonne de base de données, jamais insérée dans du HTML : les accents restent lisibles.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static readonly ValueConverter<LocalizedText, string> Text =
        new(text => WriteText(text), json => ReadText(json));

    /// <summary>Le même, pour les textes facultatifs : EF ne convertit jamais null, la colonne reste NULL.</summary>
    public static readonly ValueConverter<LocalizedText?, string> OptionalText = Text!;

    public static readonly ValueConverter<LocalizedList, string> List =
        new(list => WriteList(list), json => ReadList(json));

    internal static string WriteText(LocalizedText text) =>
        JsonSerializer.Serialize(new TextRecord(text.Fr, text.En), Json);

    internal static LocalizedText ReadText(string value)
    {
        if (value.StartsWith('{') && TryRead<TextRecord>(value) is { Fr: not null } record)
            return new LocalizedText(record.Fr, record.En);

        return new LocalizedText(value);
    }

    internal static string WriteList(LocalizedList list) =>
        JsonSerializer.Serialize(new ListRecord(list.Fr, list.En.Count > 0 ? list.En : null), Json);

    internal static LocalizedList ReadList(string value)
    {
        if (value.StartsWith('{') && TryRead<ListRecord>(value) is { Fr: not null } record)
            return new LocalizedList(record.Fr, record.En);

        return value.StartsWith('[') && TryRead<List<string>>(value) is { } french
            ? new LocalizedList(french)
            : LocalizedList.Empty;
    }

    private static T? TryRead<T>(string json) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, Json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record TextRecord(
        [property: JsonPropertyName("fr")] string? Fr,
        [property: JsonPropertyName("en")] string? En);

    private sealed record ListRecord(
        [property: JsonPropertyName("fr")] IReadOnlyList<string>? Fr,
        [property: JsonPropertyName("en")] IReadOnlyList<string>? En);
}
