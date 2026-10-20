using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;

namespace MonPortfolio.Domain.Tests.Common;

/// <summary>
/// <see cref="Guard"/> est interne au domaine : on l'éprouve à travers les entités qui l'appliquent.
/// </summary>
public class GuardTests
{
    [Theory]
    [InlineData("https://github.com/exemple/portfolio")]
    [InlineData("http://example.com/demo?x=1#y")]
    [InlineData("HTTPS://EXAMPLE.COM")]
    [InlineData("/images/portfolio.png")]
    [InlineData("/")]
    public void SafeUrl_accepts_web_urls_and_local_paths(string url) =>
        Assert.Equal(url, Make.Project(imageUrl: url).ImageUrl);

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("JavaScript:alert(document.cookie)")]
    [InlineData("  javascript:alert(1)  ")]
    [InlineData("vbscript:msgbox(1)")]
    [InlineData("data:text/html;base64,PHNjcmlwdD5hbGVydCgxKTwvc2NyaXB0Pg==")]
    [InlineData("//evil.example.com/x.png")]
    [InlineData("/\\evil.example.com/x.png")]
    [InlineData("\\\\serveur\\partage\\x.png")]
    [InlineData("ftp://example.com/x.png")]
    [InlineData("mailto:moi@example.com")]
    [InlineData("file:///C:/Windows/win.ini")]
    [InlineData("images/portfolio.png")]
    public void SafeUrl_rejects_other_schemes_and_protocol_relative_urls(string url) =>
        Assert.Throws<DomainException>(() => Make.Project(imageUrl: url));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SafeUrl_treats_blank_as_no_url(string? url) =>
        Assert.Null(Make.Project(imageUrl: url).ImageUrl);

    [Fact]
    public void SafeUrl_trims_the_value() =>
        Assert.Equal("https://example.com", Make.Project(liveUrl: "  https://example.com  ").LiveUrl);

    [Fact]
    public void SafeUrl_enforces_max_length()
    {
        const string prefix = "https://example.com/";
        var atLimit = prefix + Make.Text(Project.UrlMaxLength - prefix.Length);

        Assert.Equal(atLimit, Make.Project(repositoryUrl: atLimit).RepositoryUrl);
        Assert.Throws<DomainException>(() => Make.Project(repositoryUrl: atLimit + "a"));
    }

    [Fact]
    public void Tags_are_trimmed_and_blank_entries_dropped() =>
        Assert.Equal(["C#", "Blazor"], Make.Project(technologies: [" C# ", "", "   ", "Blazor  "]).Technologies);

    [Fact]
    public void Tags_are_deduplicated_case_insensitively_keeping_first_occurrence_and_order() =>
        Assert.Equal(
            ["C#", "Blazor", "Azure"],
            Make.Project(technologies: ["C#", "Blazor", "c#", " BLAZOR ", "Azure"]).Technologies);

    [Fact]
    public void Tags_default_to_empty_list() =>
        Assert.Empty(Make.Project(technologies: null).Technologies);

    [Fact]
    public void Tags_enforce_max_count()
    {
        var atLimit = Enumerable.Range(1, Project.MaxTechnologies).Select(i => $"Tech{i}").ToList();

        Assert.Equal(Project.MaxTechnologies, Make.Project(technologies: atLimit).Technologies.Count);
        Assert.Throws<DomainException>(() => Make.Project(technologies: [.. atLimit, "Une de trop"]));
    }

    [Fact]
    public void Tags_count_is_checked_after_deduplication()
    {
        var atLimit = Enumerable.Range(1, Project.MaxTechnologies).Select(i => $"Tech{i}").ToList();

        var project = Make.Project(technologies: [.. atLimit, "TECH1"]);

        Assert.Equal(Project.MaxTechnologies, project.Technologies.Count);
    }

    [Fact]
    public void Tags_enforce_max_length_of_each_entry()
    {
        Assert.Single(Make.Project(technologies: [Make.Text(Project.TechnologyMaxLength)]).Technologies);
        Assert.Throws<DomainException>(() => Make.Project(technologies: [Make.Text(Project.TechnologyMaxLength + 1)]));
    }
}
