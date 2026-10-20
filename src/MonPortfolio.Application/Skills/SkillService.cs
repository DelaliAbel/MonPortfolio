using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Repositories;

namespace MonPortfolio.Application.Skills;

public interface ISkillService : ICrudService<SkillDto, SkillInput>;

internal sealed class SkillService(ISkillRepository repository, TimeProvider clock)
    : CrudService<Skill, SkillDto, SkillInput>(repository, clock, "Compétence"), ISkillService
{
    protected override SkillDto ToDto(Skill skill) => skill.ToDto();

    protected override Skill CreateEntity(SkillInput input, DateTime utcNow) =>
        Skill.Create(input.Name, input.Category, input.Level, input.DisplayOrder, utcNow);

    protected override void UpdateEntity(Skill skill, SkillInput input, DateTime utcNow) =>
        skill.Update(input.Name, input.Category, input.Level, input.DisplayOrder, utcNow);
}

internal static class SkillMappings
{
    public static SkillDto ToDto(this Skill skill) =>
        new(skill.Id, skill.Name, skill.Category, skill.Level, skill.DisplayOrder);
}
