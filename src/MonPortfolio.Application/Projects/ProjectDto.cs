using MonPortfolio.Domain.Common;

namespace MonPortfolio.Application.Projects;

public sealed record ProjectDto(
    Guid Id,
    LocalizedText Title,
    LocalizedText Summary,
    LocalizedText? Description,
    LocalizedText Category,
    IReadOnlyList<string> Technologies,
    string? ImageUrl,
    string? MobileImageUrl,
    string? RepositoryUrl,
    string? LiveUrl,
    bool IsFeatured,
    int DisplayOrder)
{
    /// <summary>Textes traduits en anglais (la catégorie, souvent identique dans les deux langues, n'est pas exigée).</summary>
    public bool IsTranslated => Title.HasEnglish && Summary.HasEnglish && Description?.HasEnglish != false;
}
