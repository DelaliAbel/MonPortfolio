using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Domain.Tests.Entities;

public class CommunityActivityTests
{
    [Fact]
    public void Create_keeps_values_and_stamps_creation_date()
    {
        var activity = Make.Activity(
            title: " Meetup Blazor ",
            kind: CommunityActivityKind.Meetup,
            date: new DateOnly(2026, 5, 2),
            organizer: "  ",
            location: "Niamey",
            publicationUrl: "https://www.linkedin.com/posts/exemple",
            imageUrl: "/uploads/activities/0123456789abcdef0123456789abcdef.jpg");

        Assert.Equal("Meetup Blazor", activity.Title);
        Assert.Equal(CommunityActivityKind.Meetup, activity.Kind);
        Assert.Equal(new DateOnly(2026, 5, 2), activity.Date);
        Assert.Null(activity.Organizer);
        Assert.Equal("Niamey", activity.Location);
        Assert.Equal("https://www.linkedin.com/posts/exemple", activity.PublicationUrl);
        Assert.Equal("/uploads/activities/0123456789abcdef0123456789abcdef.jpg", activity.ImageUrl);
        Assert.Equal(Make.Now, activity.CreatedAtUtc);
    }

    [Fact]
    public void Image_and_publication_are_optional()
    {
        var activity = Make.Activity(publicationUrl: null, imageUrl: null);

        Assert.Null(activity.PublicationUrl);
        Assert.Null(activity.ImageUrl);
    }

    [Theory]
    [InlineData("/posts/local")]
    [InlineData("javascript:alert(1)")]
    [InlineData("linkedin.com/posts/x")]
    public void Publication_must_be_a_full_web_url(string url) =>
        Assert.Throws<DomainException>(() => Make.Activity(publicationUrl: url));

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:image/png;base64,AAAA")]
    [InlineData("//evil.example.com/x.png")]
    public void Image_must_be_a_safe_url(string url) =>
        Assert.Throws<DomainException>(() => Make.Activity(imageUrl: url));

    [Fact]
    public void Title_is_required_and_bounded()
    {
        Assert.Throws<DomainException>(() => Make.Activity(title: " "));
        Assert.Throws<DomainException>(() => Make.Activity(title: Make.Text(CommunityActivity.TitleMaxLength + 1)));
    }

    [Fact]
    public void Unknown_kind_is_rejected() =>
        Assert.Throws<DomainException>(() => Make.Activity(kind: (CommunityActivityKind)42));

    [Fact]
    public void Update_replaces_values_and_stamps_update_date()
    {
        var activity = Make.Activity();

        activity.Update("Atelier MAUI", CommunityActivityKind.Workshop, new DateOnly(2026, 6, 1), "Microsoft Learn",
            "En ligne", "Description", "https://example.com/replay", null, Make.Later);

        Assert.Equal("Atelier MAUI", activity.Title);
        Assert.Equal(CommunityActivityKind.Workshop, activity.Kind);
        Assert.Equal("https://example.com/replay", activity.PublicationUrl);
        Assert.Equal(Make.Later, activity.UpdatedAtUtc);
    }
}
