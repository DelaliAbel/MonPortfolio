using System.ComponentModel.DataAnnotations;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Skills;

public sealed class SkillInput
{
    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(Skill.NameMaxLength, ErrorMessage = "Le nom ne doit pas dépasser {1} caractères.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Traduction facultative : la plupart des noms (C#, Blazor…) sont identiques en anglais.</summary>
    [StringLength(Skill.NameMaxLength, ErrorMessage = "Le nom anglais ne doit pas dépasser {1} caractères.")]
    public string? NameEn { get; set; }

    [EnumDataType(typeof(SkillCategory), ErrorMessage = "Catégorie inconnue.")]
    public SkillCategory Category { get; set; } = SkillCategory.DotNet;

    [Range(Skill.MinLevel, Skill.MaxLevel, ErrorMessage = "Le niveau doit être compris entre {1} et {2}.")]
    public int Level { get; set; } = 80;

    [Range(0, 999, ErrorMessage = "L'ordre d'affichage doit être compris entre {1} et {2}.")]
    public int DisplayOrder { get; set; }

    public static SkillInput From(SkillDto dto) => new()
    {
        Name = dto.Name.Fr,
        NameEn = dto.Name.En,
        Category = dto.Category,
        Level = dto.Level,
        DisplayOrder = dto.DisplayOrder
    };
}
