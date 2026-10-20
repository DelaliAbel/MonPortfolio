namespace MonPortfolio.Application.Projects;

public sealed record ProjectDto(
    Guid Id,
    string Title,
    string Summary,
    string? Description,
    string Category,
    IReadOnlyList<string> Technologies,
    string? ImageUrl,
    string? RepositoryUrl,
    string? LiveUrl,
    bool IsFeatured,
    int DisplayOrder);
