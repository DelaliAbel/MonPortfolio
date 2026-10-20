using MonPortfolio.Application.Projects;
using MonPortfolio.Application.Tests.Fakes;

namespace MonPortfolio.Application.Tests.Projects;

/// <summary>Nettoyage des deux captures d'un projet (écran et mobile), gérées indépendamment.</summary>
public class ProjectImagesTests
{
    private const string Screen = "/uploads/projects/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.jpg";
    private const string Mobile = "/uploads/projects/bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb.png";
    private const string NewMobile = "/uploads/projects/cccccccccccccccccccccccccccccccc.webp";

    private readonly InMemoryProjectRepository repository = new();
    private readonly FakeImageStorage images = new();
    private readonly ProjectService service;

    public ProjectImagesTests() => service = new ProjectService(repository, new FakeTimeProvider(), images);

    private static ProjectInput ValidInput(string? screen = Screen, string? mobile = Mobile) => new()
    {
        Title = "Application de transfert",
        Summary = "Plateforme web et application mobile",
        Category = "Fintech",
        ImageUrl = screen,
        MobileImageUrl = mobile
    };

    [Fact]
    public async Task CreateAsync_keeps_both_captures()
    {
        var result = await service.CreateAsync(ValidInput());

        var dto = await service.GetByIdAsync(result.Value);
        Assert.NotNull(dto);
        Assert.Equal(Screen, dto.ImageUrl);
        Assert.Equal(Mobile, dto.MobileImageUrl);
        Assert.Empty(images.Deleted);
    }

    [Fact]
    public async Task UpdateAsync_deletes_only_the_replaced_capture()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;

        await service.UpdateAsync(id, ValidInput(mobile: NewMobile));

        Assert.Equal([Mobile], images.Deleted);
    }

    [Fact]
    public async Task UpdateAsync_deletes_removed_captures()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;

        await service.UpdateAsync(id, ValidInput(screen: null, mobile: null));

        Assert.Equal([Screen, Mobile], images.Deleted);
    }

    [Fact]
    public async Task UpdateAsync_keeps_a_capture_moved_to_the_other_slot()
    {
        var id = (await service.CreateAsync(ValidInput(mobile: null))).Value;

        await service.UpdateAsync(id, ValidInput(screen: null, mobile: Screen));

        Assert.Empty(images.Deleted);
    }

    [Fact]
    public async Task DeleteAsync_removes_both_captures()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;

        await service.DeleteAsync(id);

        Assert.Equal([Screen, Mobile], images.Deleted);
    }

    [Fact]
    public async Task DeleteAsync_ignores_missing_captures()
    {
        var id = (await service.CreateAsync(ValidInput(screen: null, mobile: null))).Value;

        await service.DeleteAsync(id);

        Assert.Empty(images.Deleted);
    }
}
