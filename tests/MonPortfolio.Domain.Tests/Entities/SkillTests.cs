using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Domain.Tests.Entities;

public class SkillTests
{
    [Fact]
    public void Create_keeps_values_trims_name_and_stamps_creation_date()
    {
        var skill = Make.Skill(name: "  Blazor  ", category: SkillCategory.DotNet, level: 85, displayOrder: 2);

        Assert.Equal("Blazor", skill.Name);
        Assert.Equal(SkillCategory.DotNet, skill.Category);
        Assert.Equal(85, skill.Level);
        Assert.Equal(2, skill.DisplayOrder);
        Assert.Equal(Make.Now, skill.CreatedAtUtc);
        Assert.Null(skill.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_requires_name(string? name) =>
        Assert.Throws<DomainException>(() => Make.Skill(name: name!));

    [Fact]
    public void Name_accepts_max_length_and_rejects_one_more()
    {
        Assert.NotNull(Make.Skill(name: Make.Text(Skill.NameMaxLength)));
        Assert.Throws<DomainException>(() => Make.Skill(name: Make.Text(Skill.NameMaxLength + 1)));
    }

    [Fact]
    public void Create_rejects_undefined_category() =>
        Assert.Throws<DomainException>(() => Make.Skill(category: (SkillCategory)42));

    [Theory]
    [InlineData(Skill.MinLevel)]
    [InlineData(Skill.MaxLevel)]
    public void Create_accepts_level_within_bounds(int level) =>
        Assert.Equal(level, Make.Skill(level: level).Level);

    [Theory]
    [InlineData(Skill.MinLevel - 1)]
    [InlineData(Skill.MaxLevel + 1)]
    public void Create_rejects_level_out_of_bounds(int level) =>
        Assert.Throws<DomainException>(() => Make.Skill(level: level));

    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void Create_rejects_display_order_out_of_bounds(int displayOrder) =>
        Assert.Throws<DomainException>(() => Make.Skill(displayOrder: displayOrder));

    [Fact]
    public void Update_replaces_values_and_stamps_update_date()
    {
        var skill = Make.Skill();

        skill.Update("Azure", SkillCategory.CloudDevOpsData, 70, 5, Make.Later);

        Assert.Equal("Azure", skill.Name);
        Assert.Equal(SkillCategory.CloudDevOpsData, skill.Category);
        Assert.Equal(70, skill.Level);
        Assert.Equal(5, skill.DisplayOrder);
        Assert.Equal(Make.Now, skill.CreatedAtUtc);
        Assert.Equal(Make.Later, skill.UpdatedAtUtc);
    }

    [Fact]
    public void Update_enforces_the_same_invariants_as_create() =>
        Assert.Throws<DomainException>(() => Make.Skill().Update("C#", SkillCategory.DotNet, 0, 0, Make.Later));
}
