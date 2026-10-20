using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Repositories;

namespace MonPortfolio.Application.Activities;

public interface ICommunityActivityService : ICrudService<CommunityActivityDto, CommunityActivityInput>;

internal sealed class CommunityActivityService(
    ICommunityActivityRepository repository,
    TimeProvider clock,
    IImageStorage? images = null)
    : CrudService<CommunityActivity, CommunityActivityDto, CommunityActivityInput>(repository, clock, "Activité", images),
      ICommunityActivityService
{
    protected override CommunityActivityDto ToDto(CommunityActivity activity) => activity.ToDto();

    protected override IEnumerable<string?> ImagesOf(CommunityActivity activity) => [activity.ImageUrl];

    protected override CommunityActivity CreateEntity(CommunityActivityInput input, DateTime utcNow) =>
        CommunityActivity.Create(
            input.Title,
            input.Kind,
            input.Date,
            input.Organizer,
            input.Location,
            input.Description,
            input.PublicationUrl,
            input.ImageUrl,
            utcNow);

    protected override void UpdateEntity(CommunityActivity activity, CommunityActivityInput input, DateTime utcNow) =>
        activity.Update(
            input.Title,
            input.Kind,
            input.Date,
            input.Organizer,
            input.Location,
            input.Description,
            input.PublicationUrl,
            input.ImageUrl,
            utcNow);
}

internal static class CommunityActivityMappings
{
    public static CommunityActivityDto ToDto(this CommunityActivity activity) => new(
        activity.Id,
        activity.Title,
        activity.Kind,
        activity.Date,
        activity.Organizer,
        activity.Location,
        activity.Description,
        activity.PublicationUrl,
        activity.ImageUrl);
}
