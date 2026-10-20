using Microsoft.Data.SqlClient;

namespace MonPortfolio.Infrastructure.Diagnostics;

/// <summary>
/// Traduit une exception de démarrage en cause compréhensible et en action à mener.
/// Sur un hébergement mutualisé, les journaux complets sont souvent inaccessibles : ce diagnostic
/// est affiché à la place de la page 500.30. Il ne contient jamais de secret (ni chaîne de connexion
/// ni mot de passe), seulement le type et le message de l'exception.
/// </summary>
public sealed record StartupDiagnosis(string Title, string Advice, string Technical)
{
    private const int MaxMessageLength = 400;

    // Erreurs réseau usuelles de SqlClient : serveur introuvable, délai dépassé, connexion refusée.
    private static readonly HashSet<int> NetworkErrors = [-2, -1, 2, 40, 53, 121, 1225, 10053, 10054, 10060, 10061, 11001];
    private static readonly HashSet<int> PermissionErrors = [229, 262, 1088, 2760, 15247];

    public static StartupDiagnosis From(Exception exception)
    {
        var chain = Flatten(exception).ToList();
        foreach (var candidate in chain)
        {
            if (Match(candidate) is { } diagnosis)
                return diagnosis;
        }

        var root = chain[^1];
        return new StartupDiagnosis(
            "Erreur inattendue au démarrage",
            "Consultez le journal d'application de l'hébergeur ; le message ci-dessous indique l'origine du problème.",
            Describe(root));
    }

    private static StartupDiagnosis? Match(Exception exception) => exception switch
    {
        InvalidOperationException when exception.Message.Contains("ConnectionStrings:Portfolio", StringComparison.Ordinal) => new(
            "Chaîne de connexion absente",
            "Créez la variable d'environnement « ConnectionStrings__Portfolio » (deux tirets bas) avec la chaîne LOCALE " +
            "de votre base, puis cliquez sur « Restart application ».",
            Describe(exception)),

        SqlException sql when IsCertificateError(sql) => new(
            "Certificat SQL Server refusé",
            "Ajoutez « ;Encrypt=True;TrustServerCertificate=True » à la fin de ConnectionStrings__Portfolio, puis redémarrez l'application.",
            Describe(exception)),

        SqlException { Number: 18456 } => new(
            "Identifiant ou mot de passe SQL refusé",
            "Vérifiez « User Id » et « Password » dans ConnectionStrings__Portfolio : recopiez-les depuis le panneau de la base.",
            Describe(exception)),

        SqlException { Number: 4060 } => new(
            "Base de données inaccessible",
            "Vérifiez le nom de la base (« Database=… ») dans ConnectionStrings__Portfolio.",
            Describe(exception)),

        SqlException sql when PermissionErrors.Contains(sql.Number) => new(
            "Droits SQL insuffisants",
            "L'utilisateur de la base ne peut pas créer les tables : demandez au support les droits « db_owner » sur votre base.",
            Describe(exception)),

        SqlException sql when NetworkErrors.Contains(sql.Number) => new(
            "Serveur SQL injoignable",
            "Vérifiez « Server=… » dans ConnectionStrings__Portfolio : utilisez la chaîne LOCALE fournie par l'hébergeur, " +
            "pas l'adresse « .public » réservée à l'accès distant.",
            Describe(exception)),

        SqlException => new(
            "Erreur SQL Server au démarrage",
            "Vérifiez ConnectionStrings__Portfolio (serveur, base, identifiants) et que la base accepte la création des tables.",
            Describe(exception)),

        UnauthorizedAccessException => new(
            "Écriture refusée sur le disque",
            "Le site n'a pas le droit d'écrire dans son dossier : demandez au support les droits d'écriture sur « App_Data ».",
            Describe(exception)),

        FileNotFoundException when exception.Message.Contains("profile.json", StringComparison.OrdinalIgnoreCase) => new(
            "Fichier profile.json manquant",
            "Republiez le site : « profile.json » doit se trouver à la racine du site.",
            Describe(exception)),

        ArgumentException when exception.Message.Contains("initialization string", StringComparison.OrdinalIgnoreCase)
                               || exception.Message.Contains("Keyword not supported", StringComparison.OrdinalIgnoreCase) => new(
            "Chaîne de connexion mal formée",
            "Recopiez la chaîne depuis le panneau, sans guillemets autour, en séparant les paramètres par « ; ».",
            Describe(exception)),

        _ => null
    };

    private static bool IsCertificateError(SqlException exception) =>
        exception.Message.Contains("certificat", StringComparison.OrdinalIgnoreCase)
        || exception.Message.Contains("SSL Provider", StringComparison.OrdinalIgnoreCase)
        || exception.Message.Contains("not trusted", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<Exception> Flatten(Exception exception)
    {
        yield return exception;

        var children = exception is AggregateException aggregate
            ? aggregate.InnerExceptions
            : exception.InnerException is { } inner ? [inner] : [];

        foreach (var child in children)
        foreach (var descendant in Flatten(child))
            yield return descendant;
    }

    private static string Describe(Exception exception)
    {
        var message = exception.Message.ReplaceLineEndings(" ");
        if (message.Length > MaxMessageLength)
            message = message[..MaxMessageLength] + "…";

        return $"{exception.GetType().Name} : {message}";
    }
}
