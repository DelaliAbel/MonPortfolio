using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Repositories;

namespace MonPortfolio.Application.Experiences;

public interface IExperienceService : ICrudService<ExperienceDto, ExperienceInput>;

internal sealed class ExperienceService(IExperienceRepository repository, TimeProvider clock)
    : CrudService<Experience, ExperienceDto, ExperienceInput>(repository, clock, "Expérience"), IExperienceService
{
    protected override ExperienceDto ToDto(Experience experience) => experience.ToDto();

    protected override Experience CreateEntity(ExperienceInput input, DateTime utcNow) =>
        Experience.Create(
            Bilingual.Text(input.Title, input.TitleEn),
            input.Organization,
            input.Location,
            input.Type,
            input.StartDate,
            input.EffectiveEndDate,
            Bilingual.OptionalText(input.Description, input.DescriptionEn),
            Bilingual.Lines(input.Highlights, input.HighlightsEn),
            TextList.SplitTags(input.Technologies),
            input.LinkUrl,
            utcNow);

    protected override void UpdateEntity(Experience experience, ExperienceInput input, DateTime utcNow) =>
        experience.Update(
            Bilingual.Text(input.Title, input.TitleEn),
            input.Organization,
            input.Location,
            input.Type,
            input.StartDate,
            input.EffectiveEndDate,
            Bilingual.OptionalText(input.Description, input.DescriptionEn),
            Bilingual.Lines(input.Highlights, input.HighlightsEn),
            TextList.SplitTags(input.Technologies),
            input.LinkUrl,
            utcNow);
}

internal static class ExperienceMappings
{
    public static ExperienceDto ToDto(this Experience experience) => new(
        experience.Id,
        experience.Title,
        experience.Organization,
        experience.Location,
        experience.Type,
        experience.StartDate,
        experience.EndDate,
        experience.Description,
        experience.Highlights,
        experience.Technologies,
        experience.LinkUrl);
}
