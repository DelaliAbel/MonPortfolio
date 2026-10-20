using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Domain.Tests;

/// <summary>
/// Fabriques d'entités valides par défaut : chaque test ne précise que la valeur qu'il met à l'épreuve.
/// </summary>
internal static class Make
{
    public static readonly DateTime Now = new(2026, 10, 19, 8, 30, 0, DateTimeKind.Utc);
    public static readonly DateTime Later = Now.AddHours(2);

    public static string Text(int length) => new('a', length);

    public static Project Project(
        string title = "Portfolio",
        string summary = "Site vitrine en Blazor",
        string? description = null,
        string category = "Web",
        IEnumerable<string>? technologies = null,
        string? imageUrl = null,
        string? mobileImageUrl = null,
        string? repositoryUrl = null,
        string? liveUrl = null,
        bool isFeatured = false,
        int displayOrder = 0) =>
        global::MonPortfolio.Domain.Entities.Project.Create(
            title, summary, description, category, technologies,
            imageUrl, mobileImageUrl, repositoryUrl, liveUrl, isFeatured, displayOrder, Now);

    public static Skill Skill(
        string name = "C#",
        SkillCategory category = SkillCategory.DotNet,
        int level = 90,
        int displayOrder = 0) =>
        global::MonPortfolio.Domain.Entities.Skill.Create(name, category, level, displayOrder, Now);

    public static Experience Experience(
        string title = "Développeur .NET",
        string organization = "Contoso",
        string? location = null,
        ExperienceType type = ExperienceType.Professional,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        string? description = null,
        IEnumerable<string>? highlights = null,
        IEnumerable<string>? technologies = null,
        string? linkUrl = null) =>
        global::MonPortfolio.Domain.Entities.Experience.Create(
            title, organization, location, type, startDate ?? new DateOnly(2022, 1, 1), endDate,
            description, highlights, technologies, linkUrl, Now);

    public static SocialLink SocialLink(
        SocialPlatform platform = SocialPlatform.LinkedIn,
        string url = "https://www.linkedin.com/in/exemple",
        string? label = null,
        int displayOrder = 0) =>
        global::MonPortfolio.Domain.Entities.SocialLink.Create(platform, url, label, displayOrder, Now);

    public static CommunityActivity Activity(
        string title = "Introduction à .NET Aspire",
        CommunityActivityKind kind = CommunityActivityKind.Talk,
        DateOnly? date = null,
        string? organizer = ".NET Niger",
        string? location = null,
        string? description = null,
        string? publicationUrl = null,
        string? imageUrl = null) =>
        global::MonPortfolio.Domain.Entities.CommunityActivity.Create(
            title, kind, date ?? new DateOnly(2026, 3, 12), organizer, location, description,
            publicationUrl, imageUrl, Now);

    public static ContactMessage ContactMessage(
        string name = "Jeanne Dupont",
        string email = "jeanne.dupont@example.com",
        string subject = "Proposition de mission",
        string body = "Bonjour, j'aimerais échanger avec vous.") =>
        global::MonPortfolio.Domain.Entities.ContactMessage.Create(name, email, subject, body, Now);
}
