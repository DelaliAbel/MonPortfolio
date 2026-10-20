using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Infrastructure.Persistence.Seeding;

/// <summary>
/// Contenu initial tiré du CV 2026. Chaque jeu n'est injecté que si sa table est vide :
/// les modifications faites depuis /admin ne sont jamais écrasées.
/// Les niveaux de compétences sont des valeurs de départ à ajuster dans l'administration.
/// </summary>
internal static class PortfolioSeedData
{
    public static async Task SeedAsync(PortfolioDbContext db, TimeProvider clock, ILogger logger, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        if (!await db.Experiences.AnyAsync(cancellationToken))
        {
            db.Experiences.AddRange(Experiences(now));
            logger.LogInformation("Parcours initial importé depuis le CV.");
        }

        if (!await db.Skills.AnyAsync(cancellationToken))
        {
            db.Skills.AddRange(Skills(now));
            logger.LogInformation("Compétences initiales importées depuis le CV.");
        }

        if (!await db.Projects.AnyAsync(cancellationToken))
        {
            db.Projects.AddRange(Projects(now));
            logger.LogInformation("Projets initiaux importés depuis le CV.");
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<Experience> Experiences(DateTime now)
    {
        const string niamey = "Niamey, Niger";
        const string gabera = "Gabera Software Solutions";

        yield return Experience.Create(
            "Développeur Senior Java", "Nita Transfert d'Argent", niamey, ExperienceType.Professional,
            new DateOnly(2026, 1, 1), null,
            "Fintech : transfert d'argent à grande échelle.",
            [
                "Conception et évolution de la plateforme de transfert d'argent en architecture microservices (Spring Boot) et de l'application mobile (Flutter).",
                "Renforcement de la sécurité applicative et de la fiabilité des transactions.",
                "Encadrement technique, revue de code et définition des bonnes pratiques de développement."
            ],
            ["Java", "Spring Boot", "Flutter", "Dart", "Microservices"], null, now);

        yield return Experience.Create(
            "Développeur Full-Stack & Responsable des Interfaces Utilisateurs", gabera, niamey, ExperienceType.Professional,
            new DateOnly(2024, 9, 1), new DateOnly(2025, 12, 31),
            null,
            [
                "Définition de l'architecture logicielle des solutions web, mobile et desktop.",
                "Coordination d'une équipe de 5+ développeurs (revue de code, collaboration via Git et Teams)."
            ],
            ["C#", "ASP.NET", "Blazor", "WPF", ".NET MAUI", "Aspire", "DDD", "N-tiers", "MVVM", "MVC"], null, now);

        yield return Experience.Create(
            "Développeur Full-Stack & Responsable Technique", gabera, niamey, ExperienceType.Professional,
            new DateOnly(2023, 1, 1), new DateOnly(2024, 8, 31),
            null,
            [
                "Développement de modules pour des systèmes de type ERP.",
                "Mentorat de développeurs juniors et mise en place de pipelines CI/CD.",
                "Travail en équipe Agile avec Jira pour la gestion des sprints."
            ],
            ["C#", ".NET", "CI/CD", "Agile / Scrum", "Jira"], null, now);

        yield return Experience.Create(
            "Développeur Logiciel Junior & Responsable RH", gabera, niamey, ExperienceType.Professional,
            new DateOnly(2022, 9, 1), new DateOnly(2022, 12, 31),
            null,
            ["Digitalisation du département RH avec Power Platform ; planification des sprints et support client."],
            ["Power Platform"], null, now);

        yield return Experience.Create(
            "Stagiaire en Développement Logiciel", gabera, niamey, ExperienceType.Professional,
            new DateOnly(2021, 1, 1), new DateOnly(2022, 8, 31),
            null,
            ["Maintenance d'applications desktop et web ; amélioration du front-end."],
            [], null, now);

        yield return Experience.Create(
            "Stagiaire Base de Données", "Secrétariat Général du Gouvernement", niamey, ExperienceType.Professional,
            new DateOnly(2020, 10, 1), new DateOnly(2021, 3, 31),
            "Secteur public.",
            ["Maintenance des bases gouvernementales, rapports statistiques et audit de données."],
            [], null, now);

        yield return Experience.Create(
            "Stagiaire Informatique (Statistiques)", "Ministère des Postes et de l'Économie Numérique", niamey, ExperienceType.Professional,
            new DateOnly(2019, 11, 1), new DateOnly(2020, 3, 31),
            "Secteur public.",
            ["Applications de traitement des enquêtes TIC avec la Direction des Statistiques."],
            [], null, now);

        // Formation : seule l'année d'obtention est connue → début = fin.
        yield return Diploma("Maîtrise en Sécurité Informatique", "HETEC", 2025, now);
        yield return Diploma("Licence en Génie Logiciel", "CET-ASNI", 2020, now);
        yield return Diploma("BTS d'État en Informatique de Gestion", "IBMT", 2019, now);
        yield return Diploma("Baccalauréat série D", "Lycée Kassaï", 2017, now);
    }

    private static Experience Diploma(string title, string school, int year, DateTime now)
    {
        var date = new DateOnly(year, 7, 1);
        return Experience.Create(title, school, "Niamey, Niger", ExperienceType.Education, date, date, null, [], [], null, now);
    }

    private static IEnumerable<Skill> Skills(DateTime now)
    {
        (SkillCategory Category, string Name, int Level)[] skills =
        [
            (SkillCategory.Languages, "C#", 95),
            (SkillCategory.Languages, "Java", 90),
            (SkillCategory.Languages, "Dart", 82),
            (SkillCategory.Languages, "JavaScript", 80),
            (SkillCategory.Languages, "Python", 70),

            (SkillCategory.DotNet, "ASP.NET Core", 95),
            (SkillCategory.DotNet, "Blazor", 93),
            (SkillCategory.DotNet, "WPF", 85),
            (SkillCategory.DotNet, ".NET MAUI", 82),
            (SkillCategory.DotNet, ".NET Aspire", 80),

            (SkillCategory.JavaAndMobile, "Spring Boot", 88),
            (SkillCategory.JavaAndMobile, "Flutter", 85),
            (SkillCategory.JavaAndMobile, "Django", 65),

            (SkillCategory.Architecture, "Clean Architecture & DDD", 90),
            (SkillCategory.Architecture, "Microservices", 88),
            (SkillCategory.Architecture, "N-tiers", 92),
            (SkillCategory.Architecture, "MVVM / MVC", 90),

            (SkillCategory.CloudDevOpsData, "Microsoft Azure", 82),
            (SkillCategory.CloudDevOpsData, "CI/CD & Git", 88),
            (SkillCategory.CloudDevOpsData, "PostgreSQL", 85),
            (SkillCategory.CloudDevOpsData, "Oracle & MySQL", 80),
            (SkillCategory.CloudDevOpsData, "Power Platform", 78),
            (SkillCategory.CloudDevOpsData, "Firebase & Firebird", 72),

            (SkillCategory.SecurityAiTools, "Sécurité applicative", 88),
            (SkillCategory.SecurityAiTools, "Agile / Scrum & Jira", 85),
            (SkillCategory.SecurityAiTools, "Figma & UI Design", 78),
            (SkillCategory.SecurityAiTools, "IA / Machine Learning", 65)
        ];

        return skills.Select((s, index) => Skill.Create(s.Name, s.Category, s.Level, index, now));
    }

    private static IEnumerable<Project> Projects(DateTime now)
    {
        yield return Project.Create(
            "Plateforme de transfert d'argent",
            "Plateforme fintech en microservices et application mobile associée, conçues pour la sécurité et la fiabilité des transactions.",
            "Conception et évolution d'une plateforme de transfert d'argent découpée en microservices Spring Boot, avec une application mobile Flutter. Travail centré sur la sécurité applicative, la fiabilité transactionnelle et la qualité du code (revues, bonnes pratiques).",
            "Fintech", ["Java", "Spring Boot", "Microservices", "Flutter", "Dart"],
            null, null, null, isFeatured: true, displayOrder: 1, now);

        yield return Project.Create(
            "Modules ERP métier",
            "Modules de gestion pour des systèmes de type ERP, livrés en continu grâce à des pipelines CI/CD.",
            "Développement de modules ERP au sein d'une équipe Agile (Jira), mise en place des pipelines CI/CD et mentorat des développeurs juniors.",
            "ERP", ["C#", "ASP.NET Core", "Blazor", "CI/CD", "Agile"],
            null, null, null, isFeatured: true, displayOrder: 2, now);

        yield return Project.Create(
            "Suite applicative web, mobile & desktop",
            "Architecture et interfaces de solutions multiplateformes : Blazor pour le web, WPF pour le desktop, .NET MAUI pour le mobile.",
            "Définition de l'architecture logicielle (DDD, N-tiers, MVVM) et pilotage des interfaces utilisateurs avec une équipe de 5+ développeurs.",
            ".NET", ["Blazor", "WPF", ".NET MAUI", ".NET Aspire", "DDD", "MVVM"],
            null, null, null, isFeatured: true, displayOrder: 3, now);

        yield return Project.Create(
            "Digitalisation du département RH",
            "Automatisation des processus RH grâce à Microsoft Power Platform.",
            null,
            "Low-code", ["Power Platform"],
            null, null, null, isFeatured: false, displayOrder: 4, now);

        yield return Project.Create(
            "Traitement des enquêtes TIC",
            "Applications de traitement des enquêtes TIC développées avec la Direction des Statistiques du ministère.",
            null,
            "Secteur public", ["Bases de données", "Statistiques"],
            null, null, null, isFeatured: false, displayOrder: 5, now);

        yield return Project.Create(
            "Portfolio Gold Ambassador",
            "Ce site : Clean Architecture en 4 couches, Blazor (.NET 10), EF Core et ASP.NET Core Identity.",
            "Vitrine rendue côté serveur (SSR) pour la performance et le référencement, espace d'administration interactif et sécurisé (Identity, verrouillage, limitation de débit, en-têtes CSP).",
            ".NET", [".NET 10", "Blazor", "EF Core", "Identity", "Clean Architecture"],
            null, null, null, isFeatured: false, displayOrder: 6, now);
    }
}
