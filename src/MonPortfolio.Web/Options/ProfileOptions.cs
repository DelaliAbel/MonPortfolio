namespace MonPortfolio.Web.Options;

/// <summary>
/// Contenu éditorial de la vitrine (profil.json) : textes, liens, distinctions.
/// Rechargé à chaud — modifier le fichier suffit, sans recompiler.
/// Les contenus volumineux (projets, compétences, parcours, activités, réseaux sociaux) sont gérés en base via /admin.
/// </summary>
public sealed class ProfileOptions
{
    public const string SectionName = "Profile";

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public string Tagline { get; set; } = string.Empty;
    public string? Availability { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CvUrl { get; set; }
    public string PhotoUrl { get; set; } = "/images/profile.jpg";
    public List<string> About { get; set; } = [];
    public List<StatItem> Stats { get; set; } = [];
    public List<ExpertiseItem> Expertises { get; set; } = [];
    public List<LanguageItem> Languages { get; set; } = [];
    public AmbassadorInfo Ambassador { get; set; } = new();
    public CommunityInfo Community { get; set; } = new();
    public CertificationInfo Certifications { get; set; } = new();

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string Initials =>
        string.Concat($"{FirstName} {LastName}".Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => w[0]))
            .ToUpperInvariant();
}

public sealed class StatItem
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public sealed class ExpertiseItem
{
    public string Icon { get; set; } = "code";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class LanguageItem
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Niveau de 1 à 5.</summary>
    public int Level { get; set; }
}

public sealed class AmbassadorInfo
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<AmbassadorMilestone> Milestones { get; set; } = [];
}

public sealed class AmbassadorMilestone
{
    public string Level { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class CommunityInfo
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Pillars { get; set; } = [];
    public string? Url { get; set; }
}

public sealed class CertificationInfo
{
    public List<string> Providers { get; set; } = [];
    public List<CertificationItem> Items { get; set; } = [];
}

public sealed class CertificationItem
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "award";
}
