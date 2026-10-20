using MonPortfolio.Application.SocialLinks;
using MonPortfolio.Application.Tests.Fakes;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Tests.SocialLinks;

public class SocialLinkServiceTests
{
    private readonly InMemorySocialLinkRepository repository = new();
    private readonly SocialLinkService service;

    public SocialLinkServiceTests() => service = new SocialLinkService(repository, new FakeTimeProvider());

    private static SocialLinkInput ValidInput() => new()
    {
        Platform = SocialPlatform.LinkedIn,
        Url = "https://www.linkedin.com/in/exemple",
        DisplayOrder = 1
    };

    [Fact]
    public async Task CreateAsync_persists_link_and_uses_platform_name_by_default()
    {
        var result = await service.CreateAsync(ValidInput());

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(await service.GetAllAsync());
        Assert.Equal("LinkedIn", dto.DisplayName);
        Assert.Equal("https://www.linkedin.com/in/exemple", dto.Url);
    }

    [Fact]
    public async Task Label_overrides_platform_name()
    {
        var input = ValidInput();
        input.Platform = SocialPlatform.MicrosoftLearn;
        input.Label = "Profil Microsoft Learn";

        await service.CreateAsync(input);

        Assert.Equal("Profil Microsoft Learn", Assert.Single(await service.GetAllAsync()).DisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("linkedin.com/in/exemple")]
    [InlineData("javascript:alert(1)")]
    [InlineData("/in/exemple")]
    public async Task CreateAsync_rejects_invalid_url(string url)
    {
        var input = ValidInput();
        input.Url = url;

        var result = await service.CreateAsync(input);

        Assert.True(result.IsFailure);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task Other_platform_without_label_is_rejected_by_input_validation()
    {
        var input = ValidInput();
        input.Platform = SocialPlatform.Other;
        input.Label = " ";

        var result = await service.CreateAsync(input);

        Assert.True(result.IsFailure);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task UpdateAsync_and_DeleteAsync_work_end_to_end()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;
        var input = ValidInput();
        input.Platform = SocialPlatform.GitHub;
        input.Url = "https://github.com/exemple";

        Assert.True((await service.UpdateAsync(id, input)).IsSuccess);
        Assert.Equal(SocialPlatform.GitHub, Assert.Single(await service.GetAllAsync()).Platform);

        Assert.True((await service.DeleteAsync(id)).IsSuccess);
        Assert.Empty(repository.Items);
    }
}
