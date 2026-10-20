using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Domain.Entities;

/// <summary>Compétence technique, rattachée à une catégorie et notée de 1 à 100.</summary>
public sealed class Skill : AuditableEntity
{
    public const int NameMaxLength = 60;
    public const int MinLevel = 1;
    public const int MaxLevel = 100;

    private Skill() { } // EF Core

    public LocalizedText Name { get; private set; } = new(string.Empty);
    public SkillCategory Category { get; private set; }
    public int Level { get; private set; }
    public int DisplayOrder { get; private set; }

    public static Skill Create(LocalizedText name, SkillCategory category, int level, int displayOrder, DateTime utcNow)
    {
        var skill = new Skill();
        skill.Apply(name, category, level, displayOrder);
        skill.MarkCreated(utcNow);
        return skill;
    }

    public void Update(LocalizedText name, SkillCategory category, int level, int displayOrder, DateTime utcNow)
    {
        Apply(name, category, level, displayOrder);
        MarkUpdated(utcNow);
    }

    private void Apply(LocalizedText name, SkillCategory category, int level, int displayOrder)
    {
        Name = Guard.RequiredText(name, "Nom", NameMaxLength);
        Category = Guard.Defined(category, "Catégorie");
        Level = Guard.InRange(level, MinLevel, MaxLevel, "Niveau");
        DisplayOrder = Guard.InRange(displayOrder, 0, 999, "Ordre d'affichage");
    }
}
