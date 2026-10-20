using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;
using MonPortfolio.Infrastructure.Identity;
using MonPortfolio.Infrastructure.Persistence;
using MonPortfolio.Infrastructure.Persistence.Transfer;

namespace MonPortfolio.Infrastructure.Tests.Persistence;

/// <summary>Reprise du contenu d'une base vers une autre (SQLite en mémoire des deux côtés).</summary>
public sealed class PortfolioContentCopierTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 10, 20, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection sourceConnection = new("Data Source=:memory:");
    private readonly SqliteConnection targetConnection = new("Data Source=:memory:");
    private readonly PortfolioDbContext source;
    private readonly PortfolioDbContext target;

    public PortfolioContentCopierTests()
    {
        source = Open(sourceConnection);
        target = Open(targetConnection);
    }

    private static PortfolioDbContext Open(SqliteConnection connection)
    {
        connection.Open();
        var db = new PortfolioDbContext(new DbContextOptionsBuilder<PortfolioDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        return db;
    }

    public void Dispose()
    {
        source.Dispose();
        target.Dispose();
        sourceConnection.Dispose();
        targetConnection.Dispose();
    }

    private void SeedSource()
    {
        source.Projects.Add(Project.Create("Portfolio", "Vitrine Blazor", null, ".NET", ["Blazor"], null, null, null, null, true, 1, Now));
        source.Skills.Add(Skill.Create("C#", SkillCategory.Languages, 95, 0, Now));
        source.Experiences.Add(Experience.Create("Développeur", "Contoso", null, ExperienceType.Professional,
            new DateOnly(2024, 1, 1), null, null, new(["Livraison"]), ["C#"], "https://contoso.example.com", Now));
        source.CommunityActivities.Add(CommunityActivity.Create("Meetup Blazor", CommunityActivityKind.Meetup,
            new DateOnly(2026, 3, 12), ".NET Niger", "Niamey", null, "https://www.linkedin.com/posts/x", null, Now));
        source.Certifications.Add(Certification.Create(
            new("Développeur Blazor", "Blazor developer"), CertificationKind.CompletionCertificate, "Udemy",
            new DateOnly(2025, 5, 1), "UC-123", "https://www.udemy.com/certificate/UC-123", null, 0, Now));
        source.SocialLinks.Add(SocialLink.Create(SocialPlatform.LinkedIn, "https://www.linkedin.com/in/exemple", null, 1, Now));
        source.ContactMessages.Add(ContactMessage.Create("Jeanne", "jeanne@example.com", "Mission", "Bonjour, êtes-vous disponible ?", Now));
        source.SaveChanges();
        source.ChangeTracker.Clear();
    }

    [Fact]
    public async Task CopyAsync_copies_every_content_table_with_ids_and_dates()
    {
        SeedSource();
        var expectedLink = await source.SocialLinks.AsNoTracking().SingleAsync();

        var report = await PortfolioContentCopier.CopyAsync(source, target, replace: false);

        Assert.Equal(new ImportReport(1, 1, 1, 1, 1, 1, 1), report);
        target.ChangeTracker.Clear();
        var copiedLink = await target.SocialLinks.SingleAsync();
        Assert.Equal(expectedLink.Id, copiedLink.Id);
        Assert.Equal(expectedLink.CreatedAtUtc, copiedLink.CreatedAtUtc);
        Assert.Equal("https://contoso.example.com", (await target.Experiences.SingleAsync()).LinkUrl);
        Assert.Equal(["Blazor"], (await target.Projects.SingleAsync()).Technologies);
        Assert.Equal("Blazor developer", (await target.Certifications.SingleAsync()).Name.En);
    }

    [Fact]
    public async Task CopyAsync_refuses_to_overwrite_existing_content_without_replace()
    {
        SeedSource();
        target.Skills.Add(Skill.Create("Java", SkillCategory.Languages, 90, 0, Now));
        await target.SaveChangesAsync();

        var report = await PortfolioContentCopier.CopyAsync(source, target, replace: false);

        Assert.Null(report);
        target.ChangeTracker.Clear();
        Assert.Equal("Java", (await target.Skills.SingleAsync()).Name);
        Assert.False(await target.Projects.AnyAsync());
    }

    [Fact]
    public async Task CopyAsync_with_replace_swaps_content_but_keeps_accounts()
    {
        SeedSource();
        target.Skills.Add(Skill.Create("Java", SkillCategory.Languages, 90, 0, Now));
        target.Users.Add(new ApplicationUser { UserName = "admin@example.com", Email = "admin@example.com" });
        await target.SaveChangesAsync();

        var report = await PortfolioContentCopier.CopyAsync(source, target, replace: true);

        Assert.NotNull(report);
        target.ChangeTracker.Clear();
        Assert.Equal("C#", (await target.Skills.SingleAsync()).Name);
        Assert.Equal("admin@example.com", (await target.Users.SingleAsync()).Email);
    }

    [Fact]
    public async Task CopyAsync_never_copies_accounts()
    {
        SeedSource();
        source.Users.Add(new ApplicationUser { UserName = "dev@example.com", Email = "dev@example.com", PasswordHash = "hash-de-dev" });
        await source.SaveChangesAsync();

        await PortfolioContentCopier.CopyAsync(source, target, replace: false);

        Assert.False(await target.Users.AnyAsync());
    }
}
