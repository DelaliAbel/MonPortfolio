using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Domain.Tests.Entities;

public class ExperienceTests
{
    private static readonly DateOnly Start = new(2022, 1, 1);

    [Fact]
    public void Create_keeps_values_and_stamps_creation_date()
    {
        var experience = Make.Experience(
            title: " Lead développeur ",
            organization: " Contoso ",
            location: "   ",
            type: ExperienceType.Community,
            startDate: Start,
            endDate: new DateOnly(2024, 6, 30),
            highlights: [" Migration vers .NET 10 ", ""],
            technologies: ["C#", "c#", "Azure"]);

        Assert.Equal("Lead développeur", experience.Title);
        Assert.Equal("Contoso", experience.Organization);
        Assert.Null(experience.Location);
        Assert.Equal(ExperienceType.Community, experience.Type);
        Assert.Equal(Start, experience.StartDate);
        Assert.Equal(new DateOnly(2024, 6, 30), experience.EndDate);
        Assert.False(experience.IsCurrent);
        Assert.Equal(["Migration vers .NET 10"], experience.Highlights.Fr);
        Assert.Equal(["C#", "Azure"], experience.Technologies);
        Assert.Equal(Make.Now, experience.CreatedAtUtc);
    }

    [Fact]
    public void Experience_without_end_date_is_current() =>
        Assert.True(Make.Experience(endDate: null).IsCurrent);

    [Fact]
    public void Create_rejects_end_date_before_start_date() =>
        Assert.Throws<DomainException>(() => Make.Experience(startDate: Start, endDate: Start.AddDays(-1)));

    [Fact]
    public void Create_accepts_end_date_equal_to_start_date() =>
        Assert.Equal(Start, Make.Experience(startDate: Start, endDate: Start).EndDate);

    [Fact]
    public void Update_rejects_end_date_before_start_date_and_keeps_previous_dates()
    {
        var experience = Make.Experience(startDate: Start, endDate: null);

        Assert.Throws<DomainException>(() => experience.Update(
            "Développeur .NET", "Contoso", null, ExperienceType.Professional,
            Start, Start.AddMonths(-1), null, null, null, null, Make.Later));

        Assert.Null(experience.EndDate);
        Assert.Null(experience.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(nameof(Experience.Title), null)]
    [InlineData(nameof(Experience.Title), "")]
    [InlineData(nameof(Experience.Title), "   ")]
    [InlineData(nameof(Experience.Organization), null)]
    [InlineData(nameof(Experience.Organization), "")]
    [InlineData(nameof(Experience.Organization), "   ")]
    public void Create_rejects_missing_required_field(string field, string? value) =>
        Assert.Throws<DomainException>(() => CreateWith(field, value));

    [Theory]
    [InlineData(nameof(Experience.Title), Experience.TitleMaxLength)]
    [InlineData(nameof(Experience.Organization), Experience.OrganizationMaxLength)]
    [InlineData(nameof(Experience.Location), Experience.LocationMaxLength)]
    [InlineData(nameof(Experience.Description), Experience.DescriptionMaxLength)]
    public void Text_fields_accept_max_length_and_reject_one_more(string field, int maxLength)
    {
        Assert.NotNull(CreateWith(field, Make.Text(maxLength)));
        Assert.Throws<DomainException>(() => CreateWith(field, Make.Text(maxLength + 1)));
    }

    [Fact]
    public void Create_rejects_undefined_type() =>
        Assert.Throws<DomainException>(() => Make.Experience(type: (ExperienceType)99));

    [Fact]
    public void Highlights_enforce_max_count_and_length()
    {
        var atLimit = Enumerable.Range(1, Experience.MaxHighlights).Select(i => $"Réalisation {i}").ToList();

        Assert.Equal(Experience.MaxHighlights, Make.Experience(highlights: atLimit).Highlights.Fr.Count);
        Assert.Throws<DomainException>(() => Make.Experience(highlights: [.. atLimit, "Une de trop"]));
        Assert.Throws<DomainException>(() => Make.Experience(highlights: [Make.Text(Experience.HighlightMaxLength + 1)]));
    }

    [Fact]
    public void Technologies_enforce_max_count_and_length()
    {
        var tooMany = Enumerable.Range(1, Experience.MaxTechnologies + 1).Select(i => $"Tech{i}").ToList();

        Assert.Throws<DomainException>(() => Make.Experience(technologies: tooMany));
        Assert.Throws<DomainException>(() => Make.Experience(technologies: [Make.Text(Experience.TechnologyMaxLength + 1)]));
    }

    [Fact]
    public void Update_replaces_values_and_stamps_update_date()
    {
        var experience = Make.Experience(endDate: null);

        experience.Update(
            "Architecte", "Fabrikam", "Lomé", ExperienceType.Professional,
            Start, new DateOnly(2025, 12, 31), "Description", new(["Livraison"]), ["Azure"], "https://fabrikam.example.com", Make.Later);

        Assert.Equal("Architecte", experience.Title);
        Assert.Equal("Fabrikam", experience.Organization);
        Assert.Equal("Lomé", experience.Location);
        Assert.Equal(new DateOnly(2025, 12, 31), experience.EndDate);
        Assert.False(experience.IsCurrent);
        Assert.Equal(["Livraison"], experience.Highlights.Fr);
        Assert.Equal(["Azure"], experience.Technologies);
        Assert.Equal("https://fabrikam.example.com", experience.LinkUrl);
        Assert.Equal(Make.Now, experience.CreatedAtUtc);
        Assert.Equal(Make.Later, experience.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Blank_link_is_stored_as_null(string? link) =>
        Assert.Null(Make.Experience(linkUrl: link).LinkUrl);

    [Theory]
    [InlineData("/chemin/local")]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://exemple.com/fichier")]
    [InlineData("www.exemple.com")]
    public void Link_must_be_a_full_web_url(string link) =>
        Assert.Throws<DomainException>(() => Make.Experience(linkUrl: link));

    [Fact]
    public void Failed_update_leaves_every_field_unchanged()
    {
        var experience = Make.Experience(title: "Développeur .NET", linkUrl: "https://contoso.example.com");

        Assert.Throws<DomainException>(() => experience.Update(
            "Architecte", "Fabrikam", null, ExperienceType.Professional,
            Start, null, null, null, null, "javascript:alert(1)", Make.Later));

        Assert.Equal("Développeur .NET", experience.Title);
        Assert.Equal("https://contoso.example.com", experience.LinkUrl);
    }

    private static Experience CreateWith(string field, string? value) => field switch
    {
        nameof(Experience.Title) => Make.Experience(title: value!),
        nameof(Experience.Organization) => Make.Experience(organization: value!),
        nameof(Experience.Location) => Make.Experience(location: value),
        nameof(Experience.Description) => Make.Experience(description: value),
        _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
    };
}
