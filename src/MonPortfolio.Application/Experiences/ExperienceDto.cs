using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Experiences;

public sealed record ExperienceDto(
    Guid Id,
    LocalizedText Title,
    string Organization,
    string? Location,
    ExperienceType Type,
    DateOnly StartDate,
    DateOnly? EndDate,
    LocalizedText? Description,
    LocalizedList Highlights,
    IReadOnlyList<string> Technologies,
    string? LinkUrl = null)
{
    public bool IsCurrent => EndDate is null;

    public string TypeLabel => ExperienceTypes.Label(Type);

    /// <summary>Textes traduits en anglais (l'organisation et le lieu, noms propres, ne sont pas traduits).</summary>
    public bool IsTranslated =>
        Title.HasEnglish && Description?.HasEnglish != false && (Highlights.Fr.Count == 0 || Highlights.HasEnglish);
}

public static class ExperienceTypes
{
    public static IReadOnlyList<ExperienceType> All { get; } = Enum.GetValues<ExperienceType>();

    /// <summary>Libellé dans la langue de l'interface courante.</summary>
    public static string Label(ExperienceType type) => Languages.Current == Languages.English
        ? type switch
        {
            ExperienceType.Professional => "Professional experience",
            ExperienceType.Education => "Education",
            ExperienceType.Community => "Community",
            _ => type.ToString()
        }
        : type switch
        {
            ExperienceType.Professional => "Expérience professionnelle",
            ExperienceType.Education => "Formation",
            ExperienceType.Community => "Communauté",
            _ => type.ToString()
        };
}
