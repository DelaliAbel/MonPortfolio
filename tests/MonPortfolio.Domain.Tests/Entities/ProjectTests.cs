using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;

namespace MonPortfolio.Domain.Tests.Entities;

public class ProjectTests
{
    [Fact]
    public void Create_trims_text_fields_and_stamps_creation_date()
    {
        var project = Make.Project(title: "  Portfolio  ", summary: " Site vitrine ", category: " Web ", description: "   ");

        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.Equal("Portfolio", project.Title);
        Assert.Equal("Site vitrine", project.Summary);
        Assert.Equal("Web", project.Category);
        Assert.Null(project.Description);
        Assert.Equal(Make.Now, project.CreatedAtUtc);
        Assert.Null(project.UpdatedAtUtc);
    }

    [Fact]
    public void Create_keeps_trimmed_english_translations()
    {
        var project = Make.Project(title: " Plateforme ", titleEn: " Platform ", description: "Détails", descriptionEn: "  ");

        Assert.Equal(new LocalizedText("Plateforme", "Platform"), project.Title);
        Assert.Equal(new LocalizedText("Détails"), project.Description);
    }

    [Fact]
    public void English_translation_obeys_the_same_max_length() =>
        Assert.Throws<DomainException>(() => Make.Project(titleEn: Make.Text(Project.TitleMaxLength + 1)));

    [Fact]
    public void Optional_text_rejects_a_translation_without_french() =>
        Assert.Throws<DomainException>(() => Make.Project(description: "  ", descriptionEn: "English only"));

    [Theory]
    [InlineData(nameof(Project.Title), null)]
    [InlineData(nameof(Project.Title), "")]
    [InlineData(nameof(Project.Title), "   ")]
    [InlineData(nameof(Project.Summary), null)]
    [InlineData(nameof(Project.Summary), "")]
    [InlineData(nameof(Project.Summary), "   ")]
    [InlineData(nameof(Project.Category), null)]
    [InlineData(nameof(Project.Category), "")]
    [InlineData(nameof(Project.Category), "   ")]
    public void Create_rejects_missing_required_field(string field, string? value) =>
        Assert.Throws<DomainException>(() => CreateWith(field, value));

    [Theory]
    [InlineData(nameof(Project.Title), Project.TitleMaxLength)]
    [InlineData(nameof(Project.Summary), Project.SummaryMaxLength)]
    [InlineData(nameof(Project.Description), Project.DescriptionMaxLength)]
    [InlineData(nameof(Project.Category), Project.CategoryMaxLength)]
    public void Text_fields_accept_max_length_and_reject_one_more(string field, int maxLength)
    {
        Assert.NotNull(CreateWith(field, Make.Text(maxLength)));
        Assert.NotNull(CreateWith(field, $"  {Make.Text(maxLength)}  ")); // mesuré après trim
        Assert.Throws<DomainException>(() => CreateWith(field, Make.Text(maxLength + 1)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public void Create_accepts_display_order_within_bounds(int displayOrder) =>
        Assert.Equal(displayOrder, Make.Project(displayOrder: displayOrder).DisplayOrder);

    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void Create_rejects_display_order_out_of_bounds(int displayOrder) =>
        Assert.Throws<DomainException>(() => Make.Project(displayOrder: displayOrder));

    [Theory]
    [InlineData(nameof(Project.ImageUrl))]
    [InlineData(nameof(Project.MobileImageUrl))]
    [InlineData(nameof(Project.RepositoryUrl))]
    [InlineData(nameof(Project.LiveUrl))]
    public void Every_url_field_rejects_unsafe_scheme(string field) =>
        Assert.Throws<DomainException>(() => CreateWith(field, "javascript:alert(1)"));

    [Fact]
    public void Update_replaces_values_and_stamps_update_date()
    {
        var project = Make.Project();
        var id = project.Id;

        project.Update(
            "Nouveau titre", "Nouveau résumé", "Description", "Mobile", ["Flutter"],
            "/images/app.png", "/images/app-mobile.png", "https://github.com/exemple/app", "https://app.example.com",
            isFeatured: true, displayOrder: 3, Make.Later);

        Assert.Equal(id, project.Id);
        Assert.Equal("Nouveau titre", project.Title);
        Assert.Equal("Nouveau résumé", project.Summary);
        Assert.Equal("Description", project.Description);
        Assert.Equal("Mobile", project.Category);
        Assert.Equal(["Flutter"], project.Technologies);
        Assert.Equal("/images/app.png", project.ImageUrl);
        Assert.Equal("/images/app-mobile.png", project.MobileImageUrl);
        Assert.Equal("https://github.com/exemple/app", project.RepositoryUrl);
        Assert.Equal("https://app.example.com", project.LiveUrl);
        Assert.True(project.IsFeatured);
        Assert.Equal(3, project.DisplayOrder);
        Assert.Equal(Make.Now, project.CreatedAtUtc);
        Assert.Equal(Make.Later, project.UpdatedAtUtc);
    }

    [Fact]
    public void Update_enforces_the_same_invariants_as_create()
    {
        var project = Make.Project();

        Assert.Throws<DomainException>(() => project.Update(
            "   ", "Résumé", null, "Web", null, null, null, null, null, false, 0, Make.Later));
    }

    private static Project CreateWith(string field, string? value) => field switch
    {
        nameof(Project.Title) => Make.Project(title: value!),
        nameof(Project.Summary) => Make.Project(summary: value!),
        nameof(Project.Description) => Make.Project(description: value),
        nameof(Project.Category) => Make.Project(category: value!),
        nameof(Project.ImageUrl) => Make.Project(imageUrl: value),
        nameof(Project.MobileImageUrl) => Make.Project(mobileImageUrl: value),
        nameof(Project.RepositoryUrl) => Make.Project(repositoryUrl: value),
        nameof(Project.LiveUrl) => Make.Project(liveUrl: value),
        _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
    };
}
