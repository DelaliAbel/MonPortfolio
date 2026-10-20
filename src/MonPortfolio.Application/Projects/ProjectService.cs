using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Repositories;

namespace MonPortfolio.Application.Projects;

public interface IProjectService : ICrudService<ProjectDto, ProjectInput>;

internal sealed class ProjectService(IProjectRepository repository, TimeProvider clock, IImageStorage? images = null)
    : CrudService<Project, ProjectDto, ProjectInput>(repository, clock, "Projet", images), IProjectService
{
    protected override ProjectDto ToDto(Project project) => project.ToDto();

    protected override string? ImageOf(Project project) => project.ImageUrl;

    protected override Project CreateEntity(ProjectInput input, DateTime utcNow) =>
        Project.Create(
            input.Title,
            input.Summary,
            input.Description,
            input.Category,
            TextList.SplitTags(input.Technologies),
            input.ImageUrl,
            input.RepositoryUrl,
            input.LiveUrl,
            input.IsFeatured,
            input.DisplayOrder,
            utcNow);

    protected override void UpdateEntity(Project project, ProjectInput input, DateTime utcNow) =>
        project.Update(
            input.Title,
            input.Summary,
            input.Description,
            input.Category,
            TextList.SplitTags(input.Technologies),
            input.ImageUrl,
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
        project.RepositoryUrl,
        project.LiveUrl,
        project.IsFeatured,
        project.DisplayOrder);
}
