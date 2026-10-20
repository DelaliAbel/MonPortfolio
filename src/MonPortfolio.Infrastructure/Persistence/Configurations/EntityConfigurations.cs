using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonPortfolio.Domain.Entities;

namespace MonPortfolio.Infrastructure.Persistence.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Title).HasMaxLength(Project.TitleMaxLength).IsRequired();
        builder.Property(p => p.Summary).HasMaxLength(Project.SummaryMaxLength).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(Project.DescriptionMaxLength);
        builder.Property(p => p.Category).HasMaxLength(Project.CategoryMaxLength).IsRequired();
        builder.Property(p => p.ImageUrl).HasMaxLength(Project.UrlMaxLength);
        builder.Property(p => p.RepositoryUrl).HasMaxLength(Project.UrlMaxLength);
        builder.Property(p => p.LiveUrl).HasMaxLength(Project.UrlMaxLength);
        builder.PrimitiveCollection(p => p.Technologies); // colonne JSON

        builder.HasIndex(p => p.DisplayOrder);
    }
}

internal sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Name).HasMaxLength(Skill.NameMaxLength).IsRequired();
        builder.Property(s => s.Category).IsRequired();

        builder.HasIndex(s => new { s.Category, s.DisplayOrder });
    }
}

internal sealed class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> builder)
    {
        builder.ToTable("Experiences");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Title).HasMaxLength(Experience.TitleMaxLength).IsRequired();
        builder.Property(e => e.Organization).HasMaxLength(Experience.OrganizationMaxLength).IsRequired();
        builder.Property(e => e.Location).HasMaxLength(Experience.LocationMaxLength);
        builder.Property(e => e.Description).HasMaxLength(Experience.DescriptionMaxLength);
        builder.PrimitiveCollection(e => e.Highlights);
        builder.PrimitiveCollection(e => e.Technologies);
        builder.Property(e => e.LinkUrl).HasMaxLength(Experience.UrlMaxLength);
        builder.Ignore(e => e.IsCurrent);

        builder.HasIndex(e => e.StartDate);
    }
}

internal sealed class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    public void Configure(EntityTypeBuilder<SocialLink> builder)
    {
        builder.ToTable("SocialLinks");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.Platform).IsRequired();
        builder.Property(l => l.Url).HasMaxLength(SocialLink.UrlMaxLength).IsRequired();
        builder.Property(l => l.Label).HasMaxLength(SocialLink.LabelMaxLength);

        builder.HasIndex(l => l.DisplayOrder);
    }
}

internal sealed class CommunityActivityConfiguration : IEntityTypeConfiguration<CommunityActivity>
{
    public void Configure(EntityTypeBuilder<CommunityActivity> builder)
    {
        builder.ToTable("CommunityActivities");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Title).HasMaxLength(CommunityActivity.TitleMaxLength).IsRequired();
        builder.Property(a => a.Kind).IsRequired();
        builder.Property(a => a.Organizer).HasMaxLength(CommunityActivity.OrganizerMaxLength);
        builder.Property(a => a.Location).HasMaxLength(CommunityActivity.LocationMaxLength);
        builder.Property(a => a.Description).HasMaxLength(CommunityActivity.DescriptionMaxLength);
        builder.Property(a => a.PublicationUrl).HasMaxLength(CommunityActivity.UrlMaxLength);
        builder.Property(a => a.ImageUrl).HasMaxLength(CommunityActivity.UrlMaxLength);

        builder.HasIndex(a => a.Date);
    }
}

internal sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("ContactMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Name).HasMaxLength(ContactMessage.NameMaxLength).IsRequired();
        builder.Property(m => m.Email).HasMaxLength(ContactMessage.EmailMaxLength).IsRequired();
        builder.Property(m => m.Subject).HasMaxLength(ContactMessage.SubjectMaxLength).IsRequired();
        builder.Property(m => m.Body).HasMaxLength(ContactMessage.BodyMaxLength).IsRequired();

        builder.HasIndex(m => m.ReceivedAtUtc);
        builder.HasIndex(m => m.IsRead);
    }
}
