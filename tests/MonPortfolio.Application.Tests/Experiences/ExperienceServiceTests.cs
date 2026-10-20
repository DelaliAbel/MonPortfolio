using MonPortfolio.Application.Experiences;
using MonPortfolio.Application.Tests.Fakes;
using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Tests.Experiences;

public class ExperienceServiceTests
{
    private static readonly DateOnly Start = new(2022, 1, 1);

    private readonly InMemoryExperienceRepository repository = new();
    private readonly FakeTimeProvider clock = new();
    private readonly ExperienceService service;

    public ExperienceServiceTests() => service = new ExperienceService(repository, clock);

    private static ExperienceInput ValidInput() => new()
    {
        Title = "Développeur .NET",
        Organization = "Contoso",
        Type = ExperienceType.Professional,
        StartDate = Start,
        EndDate = new DateOnly(2024, 6, 30),
        Highlights = "• Migration vers .NET 10\n- Mise en place de la CI",
        Technologies = "C#, Azure"
    };

    [Fact]
    public async Task CreateAsync_splits_english_highlights_and_round_trips_them()
    {
        var input = ValidInput();
        input.TitleEn = ".NET Developer";
        input.HighlightsEn = "Migration to .NET 10\nCI setup";

        var id = (await service.CreateAsync(input)).Value;
        var dto = await service.GetByIdAsync(id);

        Assert.NotNull(dto);
        Assert.Equal(["Migration to .NET 10", "CI setup"], dto.Highlights.In(Languages.English));
        Assert.Equal(["Migration vers .NET 10", "Mise en place de la CI"], dto.Highlights.In(Languages.French));
        Assert.True(dto.IsTranslated);
        Assert.Equal("Migration to .NET 10" + Environment.NewLine + "CI setup", ExperienceInput.From(dto).HighlightsEn);
    }

    [Fact]
    public async Task CreateAsync_persists_experience_with_split_lists()
    {
        var result = await service.CreateAsync(ValidInput());

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(repository.Items);
        Assert.Equal(saved.Id, result.Value);
        Assert.Equal(new DateOnly(2024, 6, 30), saved.EndDate);
        Assert.Equal(["Migration vers .NET 10", "Mise en place de la CI"], saved.Highlights.Fr);
        Assert.Equal(["C#", "Azure"], saved.Technologies);
        Assert.Equal(clock.UtcDateTime, saved.CreatedAtUtc);
    }

    [Fact]
    public async Task CreateAsync_ignores_end_date_of_current_position()
    {
        var input = ValidInput();
        input.IsCurrent = true;

        var result = await service.CreateAsync(input);

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(repository.Items);
        Assert.Null(saved.EndDate);
        Assert.True(saved.IsCurrent);
    }

    [Fact]
    public async Task CreateAsync_requires_end_date_unless_current()
    {
        var input = ValidInput();
        input.EndDate = null;

        var result = await service.CreateAsync(input);

        Assert.True(result.IsFailure);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task CreateAsync_rejects_end_date_before_start_date()
    {
        var input = ValidInput();
        input.EndDate = Start.AddDays(-1);

        var result = await service.CreateAsync(input);

        Assert.True(result.IsFailure);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task CreateAsync_translates_domain_exception_into_failure()
    {
        var input = ValidInput();
        input.Highlights = string.Join('\n', Enumerable.Range(1, Experience.MaxHighlights + 1).Select(i => $"Réalisation {i}"));

        var result = await service.CreateAsync(input);

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task UpdateAsync_applies_changes_and_stamps_update_date()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;
        clock.Advance(TimeSpan.FromMinutes(5));
        var input = ValidInput();
        input.Organization = "Fabrikam";
        input.IsCurrent = true;

        var result = await service.UpdateAsync(id, input);

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(repository.Items);
        Assert.Equal("Fabrikam", saved.Organization);
        Assert.True(saved.IsCurrent);
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
}
