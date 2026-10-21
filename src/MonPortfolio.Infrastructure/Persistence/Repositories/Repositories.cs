using Microsoft.EntityFrameworkCore;
using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Repositories;

namespace MonPortfolio.Infrastructure.Persistence.Repositories;

/// <summary>
/// Un DbContext court par opération (via la fabrique) : c'est le modèle recommandé avec Blazor Server,
/// où un contexte « scoped » vivrait aussi longtemps que le circuit SignalR et serait partagé entre
/// des appels concurrents.
/// </summary>
internal abstract class EfRepository<TEntity>(IDbContextFactory<PortfolioDbContext> contextFactory)
    : IRepository<TEntity> where TEntity : Entity
{
    protected IDbContextFactory<PortfolioDbContext> ContextFactory => contextFactory;

    /// <summary>Ordre d'affichage naturel de l'entité.</summary>
    protected abstract IOrderedQueryable<TEntity> Order(IQueryable<TEntity> query);

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await Order(db.Set<TEntity>().AsNoTracking()).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Set<TEntity>().CountAsync(cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        db.Update(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await db.Set<TEntity>().Where(e => e.Id == entity.Id).ExecuteDeleteAsync(cancellationToken);
    }
}

internal sealed class ProjectRepository(IDbContextFactory<PortfolioDbContext> factory)
    : EfRepository<Project>(factory), IProjectRepository
{
    protected override IOrderedQueryable<Project> Order(IQueryable<Project> query) =>
        query.OrderBy(p => p.DisplayOrder).ThenByDescending(p => p.CreatedAtUtc);
}

internal sealed class SkillRepository(IDbContextFactory<PortfolioDbContext> factory)
    : EfRepository<Skill>(factory), ISkillRepository
{
    protected override IOrderedQueryable<Skill> Order(IQueryable<Skill> query) =>
        query.OrderBy(s => s.Category).ThenBy(s => s.DisplayOrder).ThenBy(s => s.Name);
}

internal sealed class ExperienceRepository(IDbContextFactory<PortfolioDbContext> factory)
    : EfRepository<Experience>(factory), IExperienceRepository
{
    protected override IOrderedQueryable<Experience> Order(IQueryable<Experience> query) =>
        query.OrderByDescending(e => e.StartDate).ThenBy(e => e.Title);
}

internal sealed class CommunityActivityRepository(IDbContextFactory<PortfolioDbContext> factory)
    : EfRepository<CommunityActivity>(factory), ICommunityActivityRepository
{
    protected override IOrderedQueryable<CommunityActivity> Order(IQueryable<CommunityActivity> query) =>
        query.OrderByDescending(a => a.Date).ThenBy(a => a.Title);
}

internal sealed class CertificationRepository(IDbContextFactory<PortfolioDbContext> factory)
    : EfRepository<Certification>(factory), ICertificationRepository
{
    // Ordre choisi d'abord, puis les plus récentes ; les titres sans date passent après.
    protected override IOrderedQueryable<Certification> Order(IQueryable<Certification> query) =>
        query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.IssuedOn == null).ThenByDescending(c => c.IssuedOn).ThenBy(c => c.CreatedAtUtc);
}

internal sealed class ProfileSettingsRepository(IDbContextFactory<PortfolioDbContext> factory)
    : EfRepository<ProfileSettings>(factory), IProfileSettingsRepository
{
    protected override IOrderedQueryable<ProfileSettings> Order(IQueryable<ProfileSettings> query) =>
        query.OrderBy(s => s.CreatedAtUtc);
}

internal sealed class SocialLinkRepository(IDbContextFactory<PortfolioDbContext> factory)
    : EfRepository<SocialLink>(factory), ISocialLinkRepository
{
    protected override IOrderedQueryable<SocialLink> Order(IQueryable<SocialLink> query) =>
        query.OrderBy(l => l.DisplayOrder).ThenBy(l => l.Platform);
}

internal sealed class ContactMessageRepository(IDbContextFactory<PortfolioDbContext> factory)
    : EfRepository<ContactMessage>(factory), IContactMessageRepository
{
    protected override IOrderedQueryable<ContactMessage> Order(IQueryable<ContactMessage> query) =>
        query.OrderByDescending(m => m.ReceivedAtUtc);

    public async Task<int> CountUnreadAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.ContactMessages.CountAsync(m => !m.IsRead, cancellationToken);
    }

    public async Task<IReadOnlyList<ContactMessage>> ListRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await Order(db.ContactMessages.AsNoTracking()).Take(count).ToListAsync(cancellationToken);
    }
}
