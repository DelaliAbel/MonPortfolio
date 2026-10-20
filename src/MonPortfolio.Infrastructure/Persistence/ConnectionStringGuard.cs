using Microsoft.Data.SqlClient;

namespace MonPortfolio.Infrastructure.Persistence;

/// <summary>
/// Contrôle la chaîne de connexion dès le démarrage, avec un message exploitable.
/// Dans un panneau d'hébergement, on colle souvent la valeur entre guillemets, avec le nom de la variable
/// ou sous forme d'extrait JSON : SqlClient ne répond alors qu'un laconique « format non conforme à l'index 0 ».
/// Le message produit ici ne contient jamais de valeur (ni mot de passe, ni serveur) : seulement le
/// premier caractère s'il est suspect et les noms de paramètres reconnus.
/// </summary>
internal static class ConnectionStringGuard
{
    public const string SettingName = "ConnectionStrings:Portfolio";
    public const string EnvironmentVariableName = "ConnectionStrings__Portfolio";

    private static readonly string[] KnownKeywords =
    [
        "Server", "Data Source", "Addr", "Address", "Database", "Initial Catalog", "User Id", "UID", "User",
        "Password", "PWD", "Encrypt", "TrustServerCertificate", "Trust Server Certificate", "MultipleActiveResultSets",
        "Trusted_Connection", "Integrated Security", "Connect Timeout", "Connection Timeout", "Persist Security Info",
        "Application Name", "Pooling", "Max Pool Size", "Min Pool Size"
    ];

    /// <summary>Retire les espaces et une paire de guillemets englobants, erreurs de copier-coller courantes.</summary>
    public static string Normalize(string connectionString)
    {
        var value = connectionString.Trim();
        if (value.Length >= 2 && (value[0], value[^1]) is ('"', '"') or ('\'', '\''))
            value = value[1..^1].Trim();

        return value;
    }

    /// <summary>Lève une <see cref="InvalidOperationException"/> explicite si la chaîne est inutilisable.</summary>
    public static void Validate(string connectionString)
    {
        try
        {
            _ = new SqlConnectionStringBuilder(connectionString);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or KeyNotFoundException)
        {
            throw new InvalidOperationException(
                $"La chaîne « {SettingName} » est mal formée ({Describe(connectionString)}). " +
                $"La valeur de {EnvironmentVariableName} doit commencer directement par « Server= », " +
                "sans guillemets, sans le nom de la variable et sans accolades JSON.", ex);
        }
    }

    private static string Describe(string connectionString)
    {
        var parts = new List<string>();

        var first = connectionString.FirstOrDefault();
        if (first != default && !char.IsLetter(first))
            parts.Add($"premier caractère « {first} »");

        var keys = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(segment => segment.Contains('='))
            .Select(segment => segment[..segment.IndexOf('=')].Trim())
            .ToList();

        var known = keys.Where(key => KnownKeywords.Contains(key, StringComparer.OrdinalIgnoreCase)).ToList();
        var unknownCount = keys.Count - known.Count;

        parts.Add(known.Count > 0 ? $"paramètres reconnus : {string.Join(", ", known)}" : "aucun paramètre reconnu");
        if (unknownCount > 0)
            parts.Add($"{unknownCount} paramètre(s) non reconnu(s)");

        return string.Join(" ; ", parts);
    }
}
