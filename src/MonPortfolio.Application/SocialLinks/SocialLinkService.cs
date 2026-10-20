using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Repositories;

namespace MonPortfolio.Application.SocialLinks;

public interface ISocialLinkService : ICrudService<SocialLinkDto, SocialLinkInput>;

internal sealed class SocialLinkService(ISocialLinkRepository repository, TimeProvider clock)
    : CrudService<SocialLink, SocialLinkDto, SocialLinkInput>(repository, clock, "Lien"), ISocialLinkService
{
    protected override SocialLinkDto ToDto(SocialLink link) => link.ToDto();

    protected override SocialLink CreateEntity(SocialLinkInput input, DateTime utcNow) =>
        SocialLink.Create(input.Platform, input.Url, input.Label, input.DisplayOrder, utcNow);

    protected override void UpdateEntity(SocialLink link, SocialLinkInput input, DateTime utcNow) =>
        link.Update(input.Platform, input.Url, input.Label, input.DisplayOrder, utcNow);
}

internal static class SocialLinkMappings
{
    public static SocialLinkDto ToDto(this SocialLink link) =>
        new(link.Id, link.Platform, link.Url, link.Label, link.DisplayOrder);
}
