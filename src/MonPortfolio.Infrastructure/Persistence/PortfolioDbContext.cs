using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Infrastructure.Identity;

namespace MonPortfolio.Infrastructure.Persistence;

public sealed class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
    : IdentityDbContext<ApplicationUser>(options), IDataProtectionKeyContext
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<CommunityActivity> CommunityActivities => Set<CommunityActivity>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<ProfileSettings> ProfileSettings => Set<ProfileSettings>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    /// <summary>Clés ASP.NET Core Data Protection (cookies, antiforgery), partagées entre redémarrages.</summary>
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(PortfolioDbContext).Assembly);
    }
}
