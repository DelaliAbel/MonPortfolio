using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Skills;

public sealed record SkillDto(Guid Id, LocalizedText Name, SkillCategory Category, int Level, int DisplayOrder)
{
    public string CategoryLabel => SkillCategories.Label(Category);
}

public sealed record SkillGroupDto(SkillCategory Category, IReadOnlyList<SkillDto> Skills)
{
    public string Label => SkillCategories.Label(Category);
}

/// <summary>Libellés d'affichage des catégories (le domaine reste agnostique de la présentation).</summary>
public static class SkillCategories
{
    public static IReadOnlyList<SkillCategory> All { get; } = Enum.GetValues<SkillCategory>();

    /// <summary>Libellé dans la langue de l'interface courante.</summary>
    public static string Label(SkillCategory category) => Languages.Current == Languages.English
        ? category switch
        {
            SkillCategory.Languages => "Languages",
            SkillCategory.DotNet => ".NET ecosystem",
            SkillCategory.JavaAndMobile => "Java, Python & Mobile",
            SkillCategory.Architecture => "Architecture",
            SkillCategory.CloudDevOpsData => "Cloud, DevOps & Data",
            SkillCategory.SecurityAiTools => "Security, AI & Tools",
            _ => category.ToString()
        }
        : category switch
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
