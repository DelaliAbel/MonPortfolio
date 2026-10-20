using MonPortfolio.Application.Projects;
using MonPortfolio.Application.Tests.Fakes;
using MonPortfolio.Domain.Entities;

namespace MonPortfolio.Application.Tests.Projects;

/// <summary>Comportements communs de CrudService, éprouvés à travers le service des projets.</summary>
public class ProjectServiceTests
{
    private readonly InMemoryProjectRepository repository = new();
    private readonly FakeTimeProvider clock = new();
    private readonly ProjectService service;

    public ProjectServiceTests() => service = new ProjectService(repository, clock);

    private static ProjectInput ValidInput() => new()
    {
        Title = "Portfolio",
        Summary = "Site vitrine en Blazor",
        Category = "Web",
        Technologies = "C#, Blazor; EF Core",
        ImageUrl = "/images/portfolio.png",
        RepositoryUrl = "https://github.com/exemple/portfolio",
        DisplayOrder = 1
    };

    /// <summary>Passe la validation DataAnnotations (600 caractères max) mais dépasse le plafond du domaine.</summary>
    private static string TooManyTechnologies() =>
        string.Join(", ", Enumerable.Range(1, Project.MaxTechnologies + 1).Select(i => $"Tech{i}"));

    [Fact]
    public async Task CreateAsync_persists_project_and_returns_its_id()
    {
        var result = await service.CreateAsync(ValidInput());

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(repository.Items);
        Assert.Equal(saved.Id, result.Value);
        Assert.Equal("Portfolio", saved.Title);
        Assert.Equal(["C#", "Blazor", "EF Core"], saved.Technologies);
        Assert.Equal(clock.UtcDateTime, saved.CreatedAtUtc);
    }

    [Theory]
    [InlineData("titre manquant")]
    [InlineData("résumé trop long")]
    [InlineData("ordre hors bornes")]
    [InlineData("url javascript")]
    [InlineData("url protocole relatif")]
    public async Task CreateAsync_returns_failure_when_annotations_are_violated(string violation)
    {
        var input = ValidInput();
        switch (violation)
        {
            case "titre manquant": input.Title = "  "; break;
            case "résumé trop long": input.Summary = new string('a', Project.SummaryMaxLength + 1); break;
            case "ordre hors bornes": input.DisplayOrder = 1000; break;
            case "url javascript": input.LiveUrl = "javascript:alert(1)"; break;
            case "url protocole relatif": input.ImageUrl = "//evil.example.com/x.png"; break;
        }

        var result = await service.CreateAsync(input);

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(Guid.Empty, result.Value);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task CreateAsync_reports_every_invalid_field()
    {
        var input = ValidInput();
        input.Title = "";
        input.Category = "";

        var result = await service.CreateAsync(input);

        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public async Task CreateAsync_translates_domain_exception_into_failure()
    {
        var input = ValidInput();
        input.Technologies = TooManyTechnologies();

        var result = await service.CreateAsync(input);

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task GetAllAsync_and_GetByIdAsync_return_dtos()
    {
        var first = (await service.CreateAsync(ValidInput())).Value;
        var secondInput = ValidInput();
        secondInput.Title = "Application mobile";
        await service.CreateAsync(secondInput);

        var all = await service.GetAllAsync();
        var dto = await service.GetByIdAsync(first);

        Assert.Equal(["Portfolio", "Application mobile"], all.Select(p => p.Title));
        Assert.NotNull(dto);
        Assert.Equal(first, dto.Id);
        Assert.Equal("https://github.com/exemple/portfolio", dto.RepositoryUrl);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_for_unknown_id() =>
        Assert.Null(await service.GetByIdAsync(Guid.NewGuid()));

    [Fact]
    public async Task UpdateAsync_applies_changes_and_stamps_update_date()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;
        clock.Advance(TimeSpan.FromHours(1));
        var input = ValidInput();
        input.Title = "Portfolio v2";
        input.IsFeatured = true;

        var result = await service.UpdateAsync(id, input);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, repository.UpdateCount);
        var saved = Assert.Single(repository.Items);
        Assert.Equal("Portfolio v2", saved.Title);
        Assert.True(saved.IsFeatured);
        Assert.Equal(FakeTimeProvider.Start.UtcDateTime, saved.CreatedAtUtc);
        Assert.Equal(clock.UtcDateTime, saved.UpdatedAtUtc);
    }

    [Fact]
    public async Task UpdateAsync_returns_not_found_for_unknown_id()
    {
        await service.CreateAsync(ValidInput());

        var result = await service.UpdateAsync(Guid.NewGuid(), ValidInput());

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task UpdateAsync_returns_failure_when_annotations_are_violated()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;
        var input = ValidInput();
        input.RepositoryUrl = "data:text/html,<script>alert(1)</script>";

        var result = await service.UpdateAsync(id, input);

        Assert.True(result.IsFailure);
        Assert.Equal(0, repository.UpdateCount);
        Assert.Equal("https://github.com/exemple/portfolio", Assert.Single(repository.Items).RepositoryUrl);
    }

    [Fact]
    public async Task UpdateAsync_translates_domain_exception_into_failure()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;
        var input = ValidInput();
        input.Technologies = TooManyTechnologies();

        var result = await service.UpdateAsync(id, input);

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task DeleteAsync_removes_project()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;

        var result = await service.DeleteAsync(id);

        Assert.True(result.IsSuccess);
        Assert.Empty(repository.Items);
    }

    [Fact]
    public async Task DeleteAsync_returns_not_found_for_unknown_id()
    {
        await service.CreateAsync(ValidInput());

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(0, repository.DeleteCount);
        Assert.Single(repository.Items);
    }
}
