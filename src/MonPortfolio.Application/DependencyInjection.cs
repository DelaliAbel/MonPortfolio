using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MonPortfolio.Application.Activities;
using MonPortfolio.Application.Certifications;
using MonPortfolio.Application.Contact;
using MonPortfolio.Application.Experiences;
using MonPortfolio.Application.Portfolio;
using MonPortfolio.Application.Profile;
using MonPortfolio.Application.Projects;
using MonPortfolio.Application.Skills;
using MonPortfolio.Application.SocialLinks;

namespace MonPortfolio.Application;

public static class DependencyInjection
{
    /// <summary>Enregistre les cas d'utilisation. Seule dépendance externe : l'abstraction de DI.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<IExperienceService, ExperienceService>();
        services.AddScoped<ICommunityActivityService, CommunityActivityService>();
        services.AddScoped<ICertificationService, CertificationService>();
        services.AddScoped<IProfileSettingsService, ProfileSettingsService>();
        services.AddScoped<ISocialLinkService, SocialLinkService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IPortfolioService, PortfolioService>();

        return services;
    }
}
