using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;
using MonPortfolio.Infrastructure.Persistence;
using MonPortfolio.Infrastructure.Persistence.Configurations;
using MonPortfolio.Infrastructure.Persistence.Seeding;

namespace MonPortfolio.Infrastructure.Tests.Persistence;

/// <summary>Stockage des textes bilingues (JSON dans la colonne d'origine) et traduction du contenu initial.</summary>
public sealed class LocalizedStorageTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 10, 20, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly PortfolioDbContext db;

    public LocalizedStorageTests()
    {
        connection.Open();
        db = new PortfolioDbContext(new DbContextOptionsBuilder<PortfolioDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }

    [Fact]
    public void Text_is_stored_as_json_with_readable_accents()
    {
        var json = LocalizedConverters.WriteText(new LocalizedText("Sécurité \"applicative\"", "Application security"));

        Assert.Equal("{\"fr\":\"Sécurité \\\"applicative\\\"\",\"en\":\"Application security\"}", json);
        Assert.Equal(new LocalizedText("Sécurité \"applicative\"", "Application security"), LocalizedConverters.ReadText(json));
        Assert.Equal("{\"fr\":\"Fintech\"}", LocalizedConverters.WriteText(new LocalizedText("Fintech")));
    }

    [Theory]
    [InlineData("Modules ERP métier")]
    [InlineData("{pas du JSON")]
    public void Legacy_plain_text_is_read_as_french(string stored) =>
        Assert.Equal(new LocalizedText(stored), LocalizedConverters.ReadText(stored));

    [Fact]
    public void Lists_round_trip_and_legacy_arrays_are_read_as_french()
    {
        var list = new LocalizedList(["Livraison"], ["Delivery"]);

        Assert.Equal(list, LocalizedConverters.ReadList(LocalizedConverters.WriteList(list)));
        Assert.Equal("{\"fr\":[\"Livraison\"]}", LocalizedConverters.WriteList(LocalizedList.Of(["Livraison"])));
        Assert.Equal(LocalizedList.Of(["Livraison", "Déploiement"]), LocalizedConverters.ReadList("[\"Livraison\",\"Déploiement\"]"));
    }

    [Fact]
    public async Task Entities_round_trip_through_the_database()
    {
        db.Experiences.Add(Experience.Create(
            new LocalizedText("Développeur", "Developer"), "Contoso", null, ExperienceType.Professional,
            new DateOnly(2024, 1, 1), null, null, new LocalizedList(["Livraison"], ["Delivery"]), ["C#"], null, Now));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var saved = await db.Experiences.SingleAsync();

        Assert.Equal(new LocalizedText("Développeur", "Developer"), saved.Title);
        Assert.Null(saved.Description);
        Assert.Equal(new LocalizedList(["Livraison"], ["Delivery"]), saved.Highlights);
    }

    [Fact]
    public async Task Rows_written_before_the_bilingual_format_are_still_readable()
    {
        db.Skills.Add(Skill.Create("C#", SkillCategory.Languages, 95, 0, Now));
        await db.SaveChangesAsync();
        await db.Database.ExecuteSqlRawAsync("UPDATE Skills SET Name = 'Sécurité applicative'");
        db.ChangeTracker.Clear();

        var skill = await db.Skills.SingleAsync();

        Assert.Equal(new LocalizedText("Sécurité applicative"), skill.Name);
    }

    [Fact]
    public async Task Seeded_texts_still_in_their_original_french_receive_their_english_translation()
    {
        await PortfolioSeedData.SeedAsync(db, TimeProvider.System, NullLogger.Instance, CancellationToken.None);
        // Base antérieure au bilinguisme : textes en français seul, l'un d'eux modifié depuis /admin.
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE Projects SET Title = 'Modules ERP métier', Category = 'ERP' WHERE DisplayOrder = 2");
        await db.Database.ExecuteSqlRawAsync("UPDATE Projects SET Title = 'Titre réécrit' WHERE DisplayOrder = 4");
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE Experiences SET Highlights = '[\"Mentorat de développeurs juniors et mise en place de pipelines CI/CD.\"]'");
        db.ChangeTracker.Clear();

        await PortfolioSeedData.CompleteTranslationsAsync(db, TimeProvider.System, NullLogger.Instance, CancellationToken.None);
        db.ChangeTracker.Clear();

        var projects = await db.Projects.OrderBy(p => p.DisplayOrder).ToListAsync();
        Assert.Equal(new LocalizedText("Modules ERP métier", "Business ERP modules"), projects[1].Title);
        Assert.Equal(new LocalizedText("ERP"), projects[1].Category);
        Assert.Equal(new LocalizedText("Titre réécrit"), projects[3].Title);

        var highlights = (await db.Experiences.FirstAsync()).Highlights;
        Assert.Equal(["Mentored junior developers and set up CI/CD pipelines."], highlights.En);
    }

    [Fact]
    public async Task Existing_translations_are_never_overwritten()
    {
        await PortfolioSeedData.SeedAsync(db, TimeProvider.System, NullLogger.Instance, CancellationToken.None);
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE Projects SET Title = {0} WHERE DisplayOrder = 2", "{\"fr\":\"Modules ERP métier\",\"en\":\"ERP suite\"}");
        db.ChangeTracker.Clear();

        await PortfolioSeedData.CompleteTranslationsAsync(db, TimeProvider.System, NullLogger.Instance, CancellationToken.None);
        db.ChangeTracker.Clear();

        var project = await db.Projects.SingleAsync(p => p.DisplayOrder == 2);
        Assert.Equal("ERP suite", project.Title.En);
    }
}
