using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Infrastructure.Persistence.Seeding;

/// <summary>
/// Contenu initial tiré du CV 2026, en français avec sa traduction anglaise. Chaque jeu n'est injecté que si
/// sa table est vide : les modifications faites depuis /admin ne sont jamais écrasées.
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

        if (!await db.Certifications.AnyAsync(cancellationToken))
        {
            db.Certifications.AddRange(Certifications(now));
            logger.LogInformation("Certifications initiales importées depuis le CV.");
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Bases créées avant le bilinguisme : ajoute la traduction anglaise aux contenus dont le texte français
    /// est resté identique au contenu initial et qui n'ont pas encore de traduction. Un texte modifié depuis
    /// /admin n'est jamais touché ; il se traduit dans l'administration.
    /// </summary>
    public static async Task CompleteTranslationsAsync(PortfolioDbContext db, TimeProvider clock, ILogger logger, CancellationToken cancellationToken)
    {
        var translations = Translations(clock.GetUtcNow().UtcDateTime);
        var entities = new List<object>();
        entities.AddRange(await db.Projects.ToListAsync(cancellationToken));
        entities.AddRange(await db.Experiences.ToListAsync(cancellationToken));
        entities.AddRange(await db.Skills.ToListAsync(cancellationToken));

        var completed = 0;
        foreach (var property in entities.SelectMany(entity => db.Entry(entity).Properties))
        {
            switch (property.CurrentValue)
            {
                case LocalizedText { En: null } text when translations.TryGetValue(text.Fr, out var english):
                    property.CurrentValue = new LocalizedText(text.Fr, english);
                    completed++;
                    break;

                case LocalizedList { HasEnglish: false, Fr.Count: > 0 } list when list.Fr.All(translations.ContainsKey):
                    property.CurrentValue = new LocalizedList(list.Fr, list.Fr.Select(item => translations[item]).ToList());
                    completed++;
                    break;
            }
        }

        if (completed == 0)
            return;

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("{Count} texte(s) du contenu initial traduit(s) en anglais.", completed);
    }

    /// <summary>Correspondances français → anglais de tout le contenu initial.</summary>
    private static Dictionary<string, string> Translations(DateTime now)
    {
        var translations = new Dictionary<string, string>(StringComparer.Ordinal);

        void Add(LocalizedText? text)
        {
            if (text?.En is { } english)
                translations.TryAdd(text.Fr, english);
        }

        foreach (var project in Projects(now))
        {
            Add(project.Title);
            Add(project.Summary);
            Add(project.Description);
            Add(project.Category);
        }

        foreach (var experience in Experiences(now))
        {
            Add(experience.Title);
            Add(experience.Description);
            foreach (var (fr, en) in experience.Highlights.Fr.Zip(experience.Highlights.En))
                translations.TryAdd(fr, en);
        }

        foreach (var skill in Skills(now))
            Add(skill.Name);

        return translations;
    }

    private static IEnumerable<Experience> Experiences(DateTime now)
    {
        const string niamey = "Niamey, Niger";
        const string gabera = "Gabera Software Solutions";
        LocalizedText publicSector = new("Secteur public.", "Public sector.");

        yield return Experience.Create(
            new("Développeur Senior Java", "Senior Java Developer"), "Nita Transfert d'Argent", niamey, ExperienceType.Professional,
            new DateOnly(2026, 1, 1), null,
            new("Fintech : transfert d'argent à grande échelle.", "Fintech: large-scale money transfer."),
            new(
                [
                    "Conception et évolution de la plateforme de transfert d'argent en architecture microservices (Spring Boot) et de l'application mobile (Flutter).",
                    "Renforcement de la sécurité applicative et de la fiabilité des transactions.",
                    "Encadrement technique, revue de code et définition des bonnes pratiques de développement."
                ],
                [
                    "Designed and evolved the money transfer platform on a microservices architecture (Spring Boot), along with its mobile app (Flutter).",
                    "Strengthened application security and transaction reliability.",
                    "Technical leadership, code reviews and definition of development best practices."
                ]),
            ["Java", "Spring Boot", "Flutter", "Dart", "Microservices"], null, now);

        yield return Experience.Create(
            new("Développeur Full-Stack & Responsable des Interfaces Utilisateurs", "Full-Stack Developer & User Interface Lead"),
            gabera, niamey, ExperienceType.Professional,
            new DateOnly(2024, 9, 1), new DateOnly(2025, 12, 31),
            null,
            new(
                [
                    "Définition de l'architecture logicielle des solutions web, mobile et desktop.",
                    "Coordination d'une équipe de 5+ développeurs (revue de code, collaboration via Git et Teams)."
                ],
                [
                    "Defined the software architecture of web, mobile and desktop solutions.",
                    "Coordinated a team of 5+ developers (code reviews, collaboration through Git and Teams)."
                ]),
            ["C#", "ASP.NET", "Blazor", "WPF", ".NET MAUI", "Aspire", "DDD", "N-tiers", "MVVM", "MVC"], null, now);

        yield return Experience.Create(
            new("Développeur Full-Stack & Responsable Technique", "Full-Stack Developer & Technical Lead"),
            gabera, niamey, ExperienceType.Professional,
            new DateOnly(2023, 1, 1), new DateOnly(2024, 8, 31),
            null,
            new(
                [
                    "Développement de modules pour des systèmes de type ERP.",
                    "Mentorat de développeurs juniors et mise en place de pipelines CI/CD.",
                    "Travail en équipe Agile avec Jira pour la gestion des sprints."
                ],
                [
                    "Developed modules for ERP-type systems.",
                    "Mentored junior developers and set up CI/CD pipelines.",
                    "Worked in an Agile team, managing sprints with Jira."
                ]),
            ["C#", ".NET", "CI/CD", "Agile / Scrum", "Jira"], null, now);

        yield return Experience.Create(
            new("Développeur Logiciel Junior & Responsable RH", "Junior Software Developer & HR Manager"),
            gabera, niamey, ExperienceType.Professional,
            new DateOnly(2022, 9, 1), new DateOnly(2022, 12, 31),
            null,
            new(
                ["Digitalisation du département RH avec Power Platform ; planification des sprints et support client."],
                ["Digitized the HR department with Power Platform; sprint planning and customer support."]),
            ["Power Platform"], null, now);

        yield return Experience.Create(
            new("Stagiaire en Développement Logiciel", "Software Development Intern"),
            gabera, niamey, ExperienceType.Professional,
            new DateOnly(2021, 1, 1), new DateOnly(2022, 8, 31),
            null,
            new(
                ["Maintenance d'applications desktop et web ; amélioration du front-end."],
                ["Maintained desktop and web applications; improved the front end."]),
            [], null, now);

        yield return Experience.Create(
            new("Stagiaire Base de Données", "Database Intern"), "Secrétariat Général du Gouvernement", niamey, ExperienceType.Professional,
            new DateOnly(2020, 10, 1), new DateOnly(2021, 3, 31),
            publicSector,
            new(
                ["Maintenance des bases gouvernementales, rapports statistiques et audit de données."],
                ["Maintained government databases, produced statistical reports and audited data."]),
            [], null, now);

        yield return Experience.Create(
            new("Stagiaire Informatique (Statistiques)", "IT Intern (Statistics)"), "Ministère des Postes et de l'Économie Numérique", niamey,
            ExperienceType.Professional,
            new DateOnly(2019, 11, 1), new DateOnly(2020, 3, 31),
            publicSector,
            new(
                ["Applications de traitement des enquêtes TIC avec la Direction des Statistiques."],
                ["Built ICT survey processing applications with the Statistics Department."]),
            [], null, now);

        // Formation : seule l'année d'obtention est connue → début = fin.
        yield return Diploma(new("Maîtrise en Sécurité Informatique", "Master's Degree in Computer Security"), "HETEC", 2025, now);
        yield return Diploma(new("Licence en Génie Logiciel", "Bachelor's Degree in Software Engineering"), "CET-ASNI", 2020, now);
        yield return Diploma(new("BTS d'État en Informatique de Gestion", "Higher National Diploma (BTS) in Business Computing"), "IBMT", 2019, now);
        yield return Diploma(new("Baccalauréat série D", "Baccalaureate, Science (Series D)"), "Lycée Kassaï", 2017, now);
    }

    private static Experience Diploma(LocalizedText title, string school, int year, DateTime now)
    {
        var date = new DateOnly(year, 7, 1);
        return Experience.Create(title, school, "Niamey, Niger", ExperienceType.Education, date, date, null, null, [], null, now);
    }

    private static IEnumerable<Skill> Skills(DateTime now)
    {
        (SkillCategory Category, LocalizedText Name, int Level)[] skills =
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
            (SkillCategory.Architecture, new("N-tiers", "N-tier"), 92),
            (SkillCategory.Architecture, "MVVM / MVC", 90),

            (SkillCategory.CloudDevOpsData, "Microsoft Azure", 82),
            (SkillCategory.CloudDevOpsData, "CI/CD & Git", 88),
            (SkillCategory.CloudDevOpsData, "PostgreSQL", 85),
            (SkillCategory.CloudDevOpsData, "Oracle & MySQL", 80),
            (SkillCategory.CloudDevOpsData, "Power Platform", 78),
            (SkillCategory.CloudDevOpsData, "Firebase & Firebird", 72),

            (SkillCategory.SecurityAiTools, new("Sécurité applicative", "Application security"), 88),
            (SkillCategory.SecurityAiTools, "Agile / Scrum & Jira", 85),
            (SkillCategory.SecurityAiTools, "Figma & UI Design", 78),
            (SkillCategory.SecurityAiTools, new("IA / Machine Learning", "AI / Machine Learning"), 65)
        ];

        return skills.Select((s, index) => Skill.Create(s.Name, s.Category, s.Level, index, now));
    }

    /// <summary>
    /// Intitulés du CV. Le CV ne précise ni l'organisme de chaque titre, ni sa date : ils se complètent,
    /// avec le lien de vérification et l'image, dans /admin/certifications.
    /// </summary>
    private static IEnumerable<Certification> Certifications(DateTime now)
    {
        LocalizedText[] names =
        [
            "C# Fundamentals",
            new("Architecte logiciel", "Software architect"),
            "Microsoft Azure",
            "DevOps",
            new("Spécialiste sécurité IT", "IT security specialist"),
            new("Spécialiste IA & ML", "AI & ML specialist"),
            new("Développeur d'applications mobiles", "Mobile app developer"),
            "JavaScript",
            "Figma",
            "Bootstrap"
        ];

        return names.Select((name, index) =>
            Certification.Create(name, CertificationKind.Certification, null, null, null, null, null, index, now));
    }

    private static IEnumerable<Project> Projects(DateTime now)
    {
        yield return Project.Create(
            new("Plateforme de transfert d'argent", "Money transfer platform"),
            new("Plateforme fintech en microservices et application mobile associée, conçues pour la sécurité et la fiabilité des transactions.",
                "Microservices fintech platform and companion mobile app, built for secure and reliable transactions."),
            new("Conception et évolution d'une plateforme de transfert d'argent découpée en microservices Spring Boot, avec une application mobile Flutter. Travail centré sur la sécurité applicative, la fiabilité transactionnelle et la qualité du code (revues, bonnes pratiques).",
                "Design and evolution of a money transfer platform split into Spring Boot microservices, with a Flutter mobile app. The work focused on application security, transactional reliability and code quality (reviews, best practices)."),
            "Fintech", ["Java", "Spring Boot", "Microservices", "Flutter", "Dart"],
            null, null, null, null, isFeatured: true, displayOrder: 1, now);

        yield return Project.Create(
            new("Modules ERP métier", "Business ERP modules"),
            new("Modules de gestion pour des systèmes de type ERP, livrés en continu grâce à des pipelines CI/CD.",
                "Management modules for ERP-type systems, delivered continuously through CI/CD pipelines."),
            new("Développement de modules ERP au sein d'une équipe Agile (Jira), mise en place des pipelines CI/CD et mentorat des développeurs juniors.",
                "Developed ERP modules within an Agile team (Jira), set up the CI/CD pipelines and mentored junior developers."),
            "ERP", ["C#", "ASP.NET Core", "Blazor", "CI/CD", "Agile"],
            null, null, null, null, isFeatured: true, displayOrder: 2, now);

        yield return Project.Create(
            new("Suite applicative web, mobile & desktop", "Web, mobile & desktop application suite"),
            new("Architecture et interfaces de solutions multiplateformes : Blazor pour le web, WPF pour le desktop, .NET MAUI pour le mobile.",
                "Architecture and interfaces of cross-platform solutions: Blazor for the web, WPF for desktop, .NET MAUI for mobile."),
            new("Définition de l'architecture logicielle (DDD, N-tiers, MVVM) et pilotage des interfaces utilisateurs avec une équipe de 5+ développeurs.",
                "Defined the software architecture (DDD, N-tier, MVVM) and led the user interfaces with a team of 5+ developers."),
            ".NET", ["Blazor", "WPF", ".NET MAUI", ".NET Aspire", "DDD", "MVVM"],
            null, null, null, null, isFeatured: true, displayOrder: 3, now);

        yield return Project.Create(
            new("Digitalisation du département RH", "HR department digitization"),
            new("Automatisation des processus RH grâce à Microsoft Power Platform.",
                "Automated HR processes with Microsoft Power Platform."),
            null,
            "Low-code", ["Power Platform"],
            null, null, null, null, isFeatured: false, displayOrder: 4, now);

        yield return Project.Create(
            new("Traitement des enquêtes TIC", "ICT survey processing"),
            new("Applications de traitement des enquêtes TIC développées avec la Direction des Statistiques du ministère.",
                "ICT survey processing applications developed with the ministry's Statistics Department."),
            null,
            new("Secteur public", "Public sector"), ["Bases de données", "Statistiques"],
            null, null, null, null, isFeatured: false, displayOrder: 5, now);

        yield return Project.Create(
            "Portfolio Gold Ambassador",
            new("Ce site : Clean Architecture en 4 couches, Blazor (.NET 10), EF Core et ASP.NET Core Identity.",
                "This website: 4-layer Clean Architecture, Blazor (.NET 10), EF Core and ASP.NET Core Identity."),
            new("Vitrine rendue côté serveur (SSR) pour la performance et le référencement, espace d'administration interactif et sécurisé (Identity, verrouillage, limitation de débit, en-têtes CSP).",
                "Server-side rendered (SSR) showcase for performance and SEO, with a secure interactive admin area (Identity, lockout, rate limiting, CSP headers)."),
            ".NET", [".NET 10", "Blazor", "EF Core", "Identity", "Clean Architecture"],
            null, null, null, null, isFeatured: false, displayOrder: 6, now);
    }
}
