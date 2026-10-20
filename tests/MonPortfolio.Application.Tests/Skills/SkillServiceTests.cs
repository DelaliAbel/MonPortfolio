using MonPortfolio.Application.Skills;
using MonPortfolio.Application.Tests.Fakes;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Tests.Skills;

public class SkillServiceTests
{
    private readonly InMemorySkillRepository repository = new();
    private readonly FakeTimeProvider clock = new();
    private readonly SkillService service;

    public SkillServiceTests() => service = new SkillService(repository, clock);

    private static SkillInput ValidInput() => new()
    {
        Name = "Blazor",
        Category = SkillCategory.DotNet,
        Level = 90,
        DisplayOrder = 1
    };

    [Fact]
    public async Task CreateAsync_persists_skill()
    {
        var result = await service.CreateAsync(ValidInput());

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(repository.Items);
        Assert.Equal(saved.Id, result.Value);
        Assert.Equal("Blazor", saved.Name);
        Assert.Equal(SkillCategory.DotNet, saved.Category);
        Assert.Equal(90, saved.Level);
        Assert.Equal(clock.UtcDateTime, saved.CreatedAtUtc);
    }

    [Theory]
    [InlineData(Skill.MinLevel - 1)]
    [InlineData(Skill.MaxLevel + 1)]
    public async Task CreateAsync_rejects_level_out_of_bounds(int level)
    {
        var input = ValidInput();
        input.Level = level;

        var result = await service.CreateAsync(input);

        Assert.True(result.IsFailure);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task CreateAsync_rejects_unknown_category()
    {
        var input = ValidInput();
        input.Category = (SkillCategory)42;

        var result = await service.CreateAsync(input);

        Assert.True(result.IsFailure);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task CreateAsync_rejects_blank_name()
    {
        var input = ValidInput();
        input.Name = "   ";

        var result = await service.CreateAsync(input);

        Assert.True(result.IsFailure);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task UpdateAsync_applies_changes_and_stamps_update_date()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;
        clock.Advance(TimeSpan.FromDays(1));
        var input = ValidInput();
        input.Level = 95;
        input.Category = SkillCategory.Architecture;

        var result = await service.UpdateAsync(id, input);

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(repository.Items);
        Assert.Equal(95, saved.Level);
        Assert.Equal(SkillCategory.Architecture, saved.Category);
        Assert.Equal(clock.UtcDateTime, saved.UpdatedAtUtc);
    }

    [Fact]
    public async Task UpdateAsync_returns_not_found_for_unknown_id()
    {
        var result = await service.UpdateAsync(Guid.NewGuid(), ValidInput());

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task DeleteAsync_returns_not_found_for_unknown_id()
    {
        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(0, repository.DeleteCount);
    }

    [Fact]
    public async Task DeleteAsync_removes_skill()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;

        var result = await service.DeleteAsync(id);

        Assert.True(result.IsSuccess);
        Assert.Empty(repository.Items);
    }
}
