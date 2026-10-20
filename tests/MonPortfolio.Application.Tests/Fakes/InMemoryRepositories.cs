using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Repositories;

namespace MonPortfolio.Application.Tests.Fakes;

/// <summary>
/// Dépôt en mémoire : stocke les entités dans une liste et compte les écritures,
/// pour vérifier qu'un échec ne persiste rien.
/// </summary>
internal class InMemoryRepository<TEntity> : IRepository<TEntity> where TEntity : Entity
{
    public List<TEntity> Items { get; } = [];
    public int AddCount { get; private set; }
    public int UpdateCount { get; private set; }
    public int DeleteCount { get; private set; }

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(e => e.Id == id));

    public Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TEntity>>(Items.ToList());

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Count);

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        AddCount++;
        Items.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        UpdateCount++;
        var index = Items.FindIndex(e => e.Id == entity.Id);
        if (index < 0)
            throw new InvalidOperationException("Mise à jour d'une entité absente du dépôt.");

        Items[index] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DeleteCount++;
        Items.RemoveAll(e => e.Id == entity.Id);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryProjectRepository : InMemoryRepository<Project>, IProjectRepository;

internal sealed class InMemorySkillRepository : InMemoryRepository<Skill>, ISkillRepository;

internal sealed class InMemoryExperienceRepository : InMemoryRepository<Experience>, IExperienceRepository;

internal sealed class InMemorySocialLinkRepository : InMemoryRepository<SocialLink>, ISocialLinkRepository;

internal sealed class InMemoryCommunityActivityRepository : InMemoryRepository<CommunityActivity>, ICommunityActivityRepository;

internal sealed class InMemoryContactMessageRepository : InMemoryRepository<ContactMessage>, IContactMessageRepository
{
    public Task<int> CountUnreadAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Count(m => !m.IsRead));

    public Task<IReadOnlyList<ContactMessage>> ListRecentAsync(int count, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContactMessage>>(
            Items.OrderByDescending(m => m.ReceivedAtUtc).Take(count).ToList());
}
