using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;

namespace MonPortfolio.Domain.Tests.Entities;

public class ProfileSettingsTests
{
    [Fact]
    public void Create_normalizes_photo_and_stack()
    {
        var settings = ProfileSettings.Create(" /uploads/profile/photo.webp ", [" .NET", "Java", "java", " "], Make.Now);

        Assert.Equal("/uploads/profile/photo.webp", settings.PhotoUrl);
        Assert.Equal([".NET", "Java"], settings.HeroStack);
        Assert.Equal(Make.Now, settings.CreatedAtUtc);
    }

    [Fact]
    public void Empty_values_mean_default_photo_and_hidden_stack()
    {
        var settings = ProfileSettings.Create("  ", [], Make.Now);

        Assert.Null(settings.PhotoUrl);
        Assert.Empty(settings.HeroStack);
    }

    [Fact]
    public void Stack_is_limited_to_keep_the_chip_readable()
    {
        var atLimit = Enumerable.Range(1, ProfileSettings.MaxHeroStack).Select(i => $"Tech{i}").ToList();

        Assert.Equal(ProfileSettings.MaxHeroStack, ProfileSettings.Create(null, atLimit, Make.Now).HeroStack.Count);
        Assert.Throws<DomainException>(() => ProfileSettings.Create(null, [.. atLimit, "Une de trop"], Make.Now));
    }

    [Fact]
    public void Photo_rejects_unsafe_urls() =>
        Assert.Throws<DomainException>(() => ProfileSettings.Create("javascript:alert(1)", [], Make.Now));

    [Fact]
    public void Update_replaces_values_and_stamps_update_date()
    {
        var settings = ProfileSettings.Create(null, ProfileSettings.DefaultHeroStack, Make.Now);

        settings.Update("https://example.com/photo.jpg", ["Blazor"], Make.Later);

        Assert.Equal("https://example.com/photo.jpg", settings.PhotoUrl);
        Assert.Equal(["Blazor"], settings.HeroStack);
        Assert.Equal(Make.Later, settings.UpdatedAtUtc);
    }
}
