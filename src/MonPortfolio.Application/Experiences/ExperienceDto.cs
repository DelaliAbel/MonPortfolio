using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Experiences;

public sealed record ExperienceDto(
    Guid Id,
    string Title,
    string Organization,
    string? Location,
    ExperienceType Type,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Description,
    IReadOnlyList<string> Highlights,
    IReadOnlyList<string> Technologies,
    string? LinkUrl = null)
{
    public bool IsCurrent => EndDate is null;

    public string TypeLabel => ExperienceTypes.Label(Type);
}

public static class ExperienceTypes
{
    public static IReadOnlyList<ExperienceType> All { get; } = Enum.GetValues<ExperienceType>();

    public static string Label(ExperienceType type) => type switch
    {
        ExperienceType.Professional => "Expérience professionnelle",
        ExperienceType.Education => "Formation",
        ExperienceType.Community => "Communauté",
        _ => type.ToString()
    };
}
