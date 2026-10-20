using MonPortfolio.Application.Activities;
using MonPortfolio.Application.Contact;
using MonPortfolio.Application.Experiences;
using MonPortfolio.Application.Projects;
using MonPortfolio.Application.Skills;
using MonPortfolio.Application.SocialLinks;
using MonPortfolio.Domain.Repositories;

namespace MonPortfolio.Application.Portfolio;

/// <summary>Modèle de lecture complet de la vitrine publique.</summary>
public sealed record PortfolioDto(
    IReadOnlyList<ProjectDto> Projects,
    IReadOnlyList<SkillGroupDto> SkillGroups,
    IReadOnlyList<ExperienceDto> Experiences,
    IReadOnlyList<CommunityActivityDto> Activities,
    IReadOnlyList<SocialLinkDto> SocialLinks)
{
    public static PortfolioDto Empty { get; } = new([], [], [], [], []);
}

public sealed record DashboardStatsDto(
    int Projects,
    int Skills,
    int Experiences,
    int Activities,
    int SocialLinks,
    int Messages,
    int UnreadMessages,
    IReadOnlyList<ContactMessageDto> RecentMessages);

public interface IPortfolioService
{
    Task<PortfolioDto> GetPublicPortfolioAsync(CancellationToken cancellationToken = default);

    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}

internal sealed class PortfolioService(
    IProjectRepository projects,
    ISkillRepository skills,
    IExperienceRepository experiences,
    ICommunityActivityRepository activities,
    ISocialLinkRepository socialLinks,
    IContactMessageRepository messages) : IPortfolioService
{
    public async Task<PortfolioDto> GetPublicPortfolioAsync(CancellationToken cancellationToken = default)
    {
        // Chaque dépôt ouvre son propre contexte : les lectures peuvent s'exécuter en parallèle.
        var projectsTask = projects.ListAsync(cancellationToken);
        var skillsTask = skills.ListAsync(cancellationToken);
        var experiencesTask = experiences.ListAsync(cancellationToken);
        var activitiesTask = activities.ListAsync(cancellationToken);
        var socialLinksTask = socialLinks.ListAsync(cancellationToken);
        await Task.WhenAll(projectsTask, skillsTask, experiencesTask, activitiesTask, socialLinksTask);

        var skillGroups = (await skillsTask)
            .Select(s => s.ToDto())
            .GroupBy(s => s.Category)
            .OrderBy(g => g.Key)
            .Select(g => new SkillGroupDto(g.Key, g.ToList()))
            .ToList();

        return new PortfolioDto(
            (await projectsTask).Select(p => p.ToDto()).ToList(),
            skillGroups,
            (await experiencesTask).Select(e => e.ToDto()).ToList(),
            (await activitiesTask).Select(a => a.ToDto()).ToList(),
            (await socialLinksTask).Select(l => l.ToDto()).ToList());
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var projectCount = projects.CountAsync(cancellationToken);
        var skillCount = skills.CountAsync(cancellationToken);
        var experienceCount = experiences.CountAsync(cancellationToken);
        var activityCount = activities.CountAsync(cancellationToken);
        var socialLinkCount = socialLinks.CountAsync(cancellationToken);
        var messageCount = messages.CountAsync(cancellationToken);
        var unreadCount = messages.CountUnreadAsync(cancellationToken);
        var recent = messages.ListRecentAsync(5, cancellationToken);
        await Task.WhenAll(projectCount, skillCount, experienceCount, activityCount, socialLinkCount, messageCount, unreadCount, recent);

        return new DashboardStatsDto(
            await projectCount,
            await skillCount,
            await experienceCount,
            await activityCount,
            await socialLinkCount,
            await messageCount,
            await unreadCount,
            (await recent).Select(m => m.ToDto()).ToList());
    }
}
