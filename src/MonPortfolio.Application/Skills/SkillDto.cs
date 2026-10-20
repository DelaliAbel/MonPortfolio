using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Skills;

public sealed record SkillDto(Guid Id, string Name, SkillCategory Category, int Level, int DisplayOrder)
{
    public string CategoryLabel => SkillCategories.Label(Category);
}

public sealed record SkillGroupDto(SkillCategory Category, string Label, IReadOnlyList<SkillDto> Skills);

/// <summary>Libellés d'affichage des catégories (le domaine reste agnostique de la présentation).</summary>
public static class SkillCategories
{
    public static IReadOnlyList<SkillCategory> All { get; } = Enum.GetValues<SkillCategory>();

    public static string Label(SkillCategory category) => category switch
    {
        SkillCategory.Languages => "Langages",
        SkillCategory.DotNet => "Écosystème .NET",
        SkillCategory.JavaAndMobile => "Java, Python & Mobile",
        SkillCategory.Architecture => "Architecture",
        SkillCategory.CloudDevOpsData => "Cloud, DevOps & Données",
        SkillCategory.SecurityAiTools => "Sécurité, IA & Outils",
        _ => category.ToString()
    };
}
