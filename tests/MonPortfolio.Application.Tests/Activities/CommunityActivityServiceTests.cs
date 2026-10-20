using MonPortfolio.Application.Activities;
using MonPortfolio.Application.Tests.Fakes;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Tests.Activities;

/// <summary>Activités communautaires, dont le nettoyage des images remplacées ou orphelines.</summary>
public class CommunityActivityServiceTests
{
    private const string FirstImage = "/uploads/activities/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.jpg";
    private const string SecondImage = "/uploads/activities/bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb.png";

    private readonly InMemoryCommunityActivityRepository repository = new();
    private readonly FakeImageStorage images = new();
    private readonly CommunityActivityService service;

    public CommunityActivityServiceTests() =>
        service = new CommunityActivityService(repository, new FakeTimeProvider(), images);

    private static CommunityActivityInput ValidInput(string? imageUrl = FirstImage) => new()
    {
        Title = "Introduction à .NET Aspire",
        Kind = CommunityActivityKind.Talk,
        Date = new DateOnly(2026, 3, 12),
        Organizer = ".NET Niger",
        PublicationUrl = "https://www.linkedin.com/posts/exemple",
        ImageUrl = imageUrl
    };

    [Fact]
    public async Task CreateAsync_persists_activity_with_publication_link()
    {
        var result = await service.CreateAsync(ValidInput());

        Assert.True(result.IsSuccess);
        var dto = await service.GetByIdAsync(result.Value);
        Assert.NotNull(dto);
        Assert.Equal("https://www.linkedin.com/posts/exemple", dto.PublicationUrl);
        Assert.Equal("Conférence", dto.KindLabel);
        Assert.Empty(images.Deleted);
    }

    [Theory]
    [InlineData("linkedin.com/posts/x")]
    [InlineData("/posts/local")]
    [InlineData("javascript:alert(1)")]
    public async Task CreateAsync_rejects_publication_that_is_not_a_web_url(string url)
    {
        var input = ValidInput();
        input.PublicationUrl = url;

        var result = await service.CreateAsync(input);

        Assert.True(result.IsFailure);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task UpdateAsync_deletes_previous_image_when_replaced()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;

        var result = await service.UpdateAsync(id, ValidInput(SecondImage));

        Assert.True(result.IsSuccess);
        Assert.Equal([FirstImage], images.Deleted);
    }

    [Fact]
    public async Task UpdateAsync_deletes_previous_image_when_removed()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;

        await service.UpdateAsync(id, ValidInput(imageUrl: null));

        Assert.Equal([FirstImage], images.Deleted);
    }

    [Fact]
    public async Task UpdateAsync_keeps_image_when_unchanged()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;
        var input = ValidInput();
        input.Title = "Titre corrigé";

        await service.UpdateAsync(id, input);

        Assert.Empty(images.Deleted);
    }

    [Fact]
    public async Task Failed_update_does_not_delete_any_image()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;
        var input = ValidInput(SecondImage);
        input.Title = "";

        var result = await service.UpdateAsync(id, input);

        Assert.True(result.IsFailure);
        Assert.Empty(images.Deleted);
    }

    [Fact]
    public async Task DeleteAsync_removes_activity_and_its_image()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;

        var result = await service.DeleteAsync(id);

        Assert.True(result.IsSuccess);
        Assert.Empty(repository.Items);
        Assert.Equal([FirstImage], images.Deleted);
    }
}
