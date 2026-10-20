using System.Text.RegularExpressions;

namespace MonPortfolio.Web.TechStack;

/// <param name="File">Nom du fichier dans wwwroot/logos (sans extension).</param>
/// <param name="InvertOnDark">Logo trop sombre pour le thème nuit : éclairci par filtre CSS.</param>
public sealed record BrandLogo(string File, bool InvertOnDark = false);

/// <summary>
/// Associe un nom de technologie saisi librement (compétence, chip de projet ou d'expérience)
/// à son logo de marque (Devicon, MIT). Les noms composés sont découpés :
/// « Oracle &amp; MySQL » → deux logos ; « CI/CD &amp; Git » → Git.
/// Les notions sans logo (architecture, sécurité, IA…) reçoivent une icône générique cohérente.
/// </summary>
public static partial class TechLogos
{
    private const int MaxLogos = 2;

    private static readonly BrandLogo DotNet = new("dotnetcore");

    private static readonly Dictionary<string, BrandLogo> Aliases = new(StringComparer.Ordinal)
    {
        // Langages
        ["c#"] = new("csharp"),
        ["csharp"] = new("csharp"),
        ["java"] = new("java"),
        ["dart"] = new("dart"),
        ["javascript"] = new("javascript"),
        ["js"] = new("javascript"),
        ["typescript"] = new("typescript"),
        ["ts"] = new("typescript"),
        ["python"] = new("python"),
        ["html"] = new("html5"),
        ["html5"] = new("html5"),
        ["css"] = new("css3"),
        ["css3"] = new("css3"),

        // Écosystème .NET
        [".net"] = DotNet,
        ["dotnet"] = DotNet,
        [".net core"] = DotNet,
        ["asp.net"] = DotNet,
        ["asp.net core"] = DotNet,
        ["web api"] = DotNet,
        [".net maui"] = DotNet,
        ["maui"] = DotNet,
        [".net aspire"] = DotNet,
        ["aspire"] = DotNet,
        ["blazor"] = new("blazor"),
        ["wpf"] = new("windows"),
        ["winforms"] = new("windows"),
        ["windows"] = new("windows"),
        ["ef core"] = new("efcore"),
        ["entity framework"] = new("efcore"),
        ["entity framework core"] = new("efcore"),
        ["xamarin"] = new("xamarin"),
        ["nuget"] = new("nuget", InvertOnDark: true),

        // Java, Python & mobile
        ["spring"] = new("spring"),
        ["spring boot"] = new("spring"),
        ["flutter"] = new("flutter"),
        ["django"] = new("django", InvertOnDark: true),

        // Cloud, DevOps & données
        ["azure"] = new("azure"),
        ["microsoft azure"] = new("azure"),
        ["azure devops"] = new("azuredevops"),
        ["git"] = new("git"),
        ["github"] = new("github", InvertOnDark: true),
        ["github actions"] = new("githubactions"),
        ["docker"] = new("docker"),
        ["kubernetes"] = new("kubernetes"),
        ["postgresql"] = new("postgresql"),
        ["postgres"] = new("postgresql"),
        ["oracle"] = new("oracle"),
        ["mysql"] = new("mysql", InvertOnDark: true),
        ["sqlite"] = new("sqlite"),
        ["sql server"] = new("sqlserver"),
        ["firebase"] = new("firebase"),
        ["firebird"] = new("firebird"),

        // Outils
        ["jira"] = new("jira"),
        ["figma"] = new("figma"),
        ["canva"] = new("canva"),
        ["bootstrap"] = new("bootstrap"),
        ["wordpress"] = new("wordpress", InvertOnDark: true),
        ["visual studio"] = new("visualstudio"),
        ["vs code"] = new("vscode"),
        ["vscode"] = new("vscode"),
        ["rider"] = new("rider"),
        ["swagger"] = new("swagger")
    };

    /// <summary>Logos de marque reconnus dans le nom (au plus deux), dans l'ordre d'apparition.</summary>
    public static IReadOnlyList<BrandLogo> Resolve(string? technology)
    {
        var name = Normalize(technology);
        if (name.Length == 0)
            return [];

        if (Lookup(name) is { } logo)
            return [logo];

        return Separators().Split(name)
            .Select(token => Lookup(token.Trim()))
            .OfType<BrandLogo>()
            .Distinct()
            .Take(MaxLogos)
            .ToList();
    }

    /// <summary>Icône générique (jeu d'icônes du site) pour une notion sans logo de marque.</summary>
    public static string FallbackIcon(string? technology)
    {
        var name = Normalize(technology);
        var tokens = Separators().Split(name).Select(t => t.Trim()).ToHashSet();

        if (ContainsAny(name, "sécurité", "securite", "security", "identity"))
            return "shield";
        if (tokens.Overlaps(["ia", "ai", "ml"]) || ContainsAny(name, "machine learning", "intelligence"))
            return "sparkles";
        if (ContainsAny(name, "microservice"))
            return "dashboard";
        if (ContainsAny(name, "architecture", "ddd", "tiers", "clean", "hexagonal"))
            return "layers";
        if (ContainsAny(name, "mvvm", "mvc", "low-code", "power platform"))
            return "layout";
        if (tokens.Overlaps(["ci", "cd"]) || ContainsAny(name, "devops", "pipeline"))
            return "terminal";
        if (ContainsAny(name, "agile", "scrum", "communauté", "mentorat"))
            return "users";
        if (ContainsAny(name, "base", "donnée", "database", "sql", "statisti"))
            return "database";
        if (tokens.Overlaps(["ui", "ux"]) || ContainsAny(name, "design"))
            return "sparkles";

        return "code";
    }

    private static BrandLogo? Lookup(string name) =>
        Aliases.GetValueOrDefault(name) ?? Aliases.GetValueOrDefault(VersionSuffix().Replace(name, string.Empty));

    private static string Normalize(string? value) =>
        string.Join(' ', (value ?? string.Empty).ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static bool ContainsAny(string value, params string[] fragments) =>
        fragments.Any(f => value.Contains(f, StringComparison.Ordinal));

    [GeneratedRegex(@"\s*(?:&|/|,|\+|\bet\b|\band\b)\s*")]
    private static partial Regex Separators();

    /// <summary>« .NET 10 », « Java 21 » → « .net », « java ».</summary>
    [GeneratedRegex(@"\s+v?\d+(?:\.\d+)*$")]
    private static partial Regex VersionSuffix();
}
