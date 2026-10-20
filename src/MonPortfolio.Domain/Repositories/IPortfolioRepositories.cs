using MonPortfolio.Domain.Entities;

namespace MonPortfolio.Domain.Repositories;

public interface IProjectRepository : IRepository<Project>;

public interface ISkillRepository : IRepository<Skill>;

public interface IExperienceRepository : IRepository<Experience>;

public interface ISocialLinkRepository : IRepository<SocialLink>;

public interface ICommunityActivityRepository : IRepository<CommunityActivity>;

public interface IContactMessageRepository : IRepository<ContactMessage>
{
    Task<int> CountUnreadAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContactMessage>> ListRecentAsync(int count, CancellationToken cancellationToken = default);
}
