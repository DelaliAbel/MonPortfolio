using MonPortfolio.Application.Profile;
using MonPortfolio.Application.Tests.Fakes;
using MonPortfolio.Domain.Entities;

namespace MonPortfolio.Application.Tests.Profile;

/// <summary>Photo du hero et stack sur la photo : une seule ligne de réglages, créée au premier enregistrement.</summary>
public class ProfileSettingsServiceTests
{
    private const string FirstPhoto = "/uploads/profile/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.jpg";
    private const string SecondPhoto = "/uploads/profile/bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb.webp";

    private readonly InMemoryProfileSettingsRepository repository = new();
    private readonly FakeImageStorage images = new();
    private readonly ProfileSettingsService service;

    public ProfileSettingsServiceTests() =>
        service = new ProfileSettingsService(repository, new FakeTimeProvider(), images);

    [Fact]
    public async Task GetAsync_returns_the_defaults_until_settings_are_saved()
    {
        var settings = await service.GetAsync();

        Assert.Null(settings.PhotoUrl);
        Assert.Equal(ProfileSettings.DefaultHeroStack, settings.HeroStack);
    }

    [Fact]
    public async Task First_save_creates_the_single_settings_row_then_updates_it()
    {
        await service.UpdateAsync(new ProfileSettingsInput { PhotoUrl = FirstPhoto, HeroStack = ".NET, Blazor; Azure" });
        await service.UpdateAsync(new ProfileSettingsInput { PhotoUrl = FirstPhoto, HeroStack = "Java" });

        var saved = Assert.Single(repository.Items);
        Assert.Equal(1, repository.AddCount);
        Assert.Equal(1, repository.UpdateCount);
        Assert.Equal(["Java"], saved.HeroStack);
        Assert.Equal(FirstPhoto, (await service.GetAsync()).PhotoUrl);
        Assert.Empty(images.Deleted);
    }

    [Fact]
    public async Task Replacing_or_removing_the_photo_deletes_the_previous_file()
    {
        await service.UpdateAsync(new ProfileSettingsInput { PhotoUrl = FirstPhoto });

        await service.UpdateAsync(new ProfileSettingsInput { PhotoUrl = SecondPhoto });
        await service.UpdateAsync(new ProfileSettingsInput { PhotoUrl = null });

        Assert.Equal([FirstPhoto, SecondPhoto], images.Deleted);
        Assert.Null((await service.GetAsync()).PhotoUrl);
    }

    [Fact]
    public async Task Invalid_values_are_reported_and_nothing_is_saved()
    {
        var tooMany = string.Join(", ", Enumerable.Range(1, ProfileSettings.MaxHeroStack + 1).Select(i => $"Tech{i}"));

        var unsafePhoto = await service.UpdateAsync(new ProfileSettingsInput { PhotoUrl = "javascript:alert(1)" });
        var tooLongStack = await service.UpdateAsync(new ProfileSettingsInput { HeroStack = tooMany });

        Assert.True(unsafePhoto.IsFailure);
        Assert.True(tooLongStack.IsFailure);
        Assert.Empty(repository.Items);
    }

    [Fact]
    public void Form_shows_the_stack_as_comma_separated_text() =>
        Assert.Equal(".NET, Java, Flutter, Azure", ProfileSettingsInput.From(ProfileSettingsDto.Default).HeroStack);
}
