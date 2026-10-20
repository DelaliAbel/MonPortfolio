using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Repositories;

namespace MonPortfolio.Application.Projects;

public interface IProjectService : ICrudService<ProjectDto, ProjectInput>;

internal sealed class ProjectService(IProjectRepository repository, TimeProvider clock, IImageStorage? images = null)
    : CrudService<Project, ProjectDto, ProjectInput>(repository, clock, "Projet", images), IProjectService
{
    protected override ProjectDto ToDto(Project project) => project.ToDto();

    protected override IEnumerable<string?> ImagesOf(Project project) => [project.ImageUrl, project.MobileImageUrl];

    protected override Project CreateEntity(ProjectInput input, DateTime utcNow) =>
        Project.Create(
            Bilingual.Text(input.Title, input.TitleEn),
            Bilingual.Text(input.Summary, input.SummaryEn),
            Bilingual.OptionalText(input.Description, input.DescriptionEn),
            Bilingual.Text(input.Category, input.CategoryEn),
            TextList.SplitTags(input.Technologies),
            input.ImageUrl,
            input.MobileImageUrl,
            input.RepositoryUrl,
            input.LiveUrl,
            input.IsFeatured,
            input.DisplayOrder,
            utcNow);

    protected override void UpdateEntity(Project project, ProjectInput input, DateTime utcNow) =>
        project.Update(
            Bilingual.Text(input.Title, input.TitleEn),
            Bilingual.Text(input.Summary, input.SummaryEn),
            Bilingual.OptionalText(input.Description, input.DescriptionEn),
            Bilingual.Text(input.Category, input.CategoryEn),
            TextList.SplitTags(input.Technologies),
            input.ImageUrl,
            input.MobileImageUrl,
            input.RepositoryUrl,
            input.LiveUrl,
            input.IsFeatured,
            input.DisplayOrder,
            utcNow);
}

internal static class ProjectMappings
{
    public static ProjectDto ToDto(this Project project) => new(
        project.Id,
        project.Title,
        project.Summary,
        project.Description,
        project.Category,
        project.Technologies,
        project.ImageUrl,
        project.MobileImageUrl,
        project.RepositoryUrl,
        project.LiveUrl,
        project.IsFeatured,
        project.DisplayOrder);
}
