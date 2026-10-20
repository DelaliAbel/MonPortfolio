using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Domain.Tests.Entities;

public class SocialLinkTests
{
    [Fact]
    public void Create_keeps_values_and_stamps_creation_date()
    {
        var link = Make.SocialLink(SocialPlatform.MicrosoftLearn, " https://learn.microsoft.com/fr-fr/users/exemple ", "  ", 3);

        Assert.Equal(SocialPlatform.MicrosoftLearn, link.Platform);
        Assert.Equal("https://learn.microsoft.com/fr-fr/users/exemple", link.Url);
        Assert.Null(link.Label);
        Assert.Equal(3, link.DisplayOrder);
        Assert.Equal(Make.Now, link.CreatedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("linkedin.com/in/exemple")]
    [InlineData("/profil")]
    [InlineData("javascript:alert(1)")]
    [InlineData("mailto:moi@example.com")]
    public void Url_must_be_a_full_web_url(string url) =>
        Assert.Throws<DomainException>(() => Make.SocialLink(url: url));

    [Fact]
    public void Url_longer_than_limit_is_rejected() =>
        Assert.Throws<DomainException>(() =>
            Make.SocialLink(url: "https://example.com/" + Make.Text(SocialLink.UrlMaxLength)));

    [Fact]
    public void Other_platform_requires_a_label()
    {
        Assert.Throws<DomainException>(() => Make.SocialLink(SocialPlatform.Other, label: null));
        Assert.Equal("Blog technique", Make.SocialLink(SocialPlatform.Other, label: "Blog technique").Label);
    }

    [Fact]
    public void Unknown_platform_is_rejected() =>
        Assert.Throws<DomainException>(() => Make.SocialLink((SocialPlatform)99));

    [Fact]
    public void Update_replaces_values_and_stamps_update_date()
    {
        var link = Make.SocialLink();

        link.Update(SocialPlatform.GitHub, "https://github.com/exemple", "@exemple", 1, Make.Later);

        Assert.Equal(SocialPlatform.GitHub, link.Platform);
        Assert.Equal("https://github.com/exemple", link.Url);
        Assert.Equal("@exemple", link.Label);
        Assert.Equal(Make.Later, link.UpdatedAtUtc);
    }

    [Fact]
    public void Failed_update_leaves_link_unchanged()
    {
        var link = Make.SocialLink();

        Assert.Throws<DomainException>(() => link.Update(SocialPlatform.Other, "https://example.com", null, 0, Make.Later));

        Assert.Equal(SocialPlatform.LinkedIn, link.Platform);
        Assert.Null(link.UpdatedAtUtc);
    }
}
