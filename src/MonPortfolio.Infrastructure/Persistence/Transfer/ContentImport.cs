using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MonPortfolio.Infrastructure.Persistence.Transfer;

public sealed record ImportReport(int Projects, int Skills, int Experiences, int Activities, int SocialLinks, int Messages)
{
    public override string ToString() =>
        $"{Projects} projet(s), {Skills} compétence(s), {Experiences} étape(s) de parcours, " +
        $"{Activities} activité(s), {SocialLinks} réseau(x), {Messages} message(s)";
}

/// <summary>
/// Copie ponctuelle du contenu d'une base source vers la base configurée (ConnectionStrings:Portfolio).
/// La source est soit un fichier SQLite (ancienne base locale), soit une chaîne de connexion SQL Server :
/// LocalDB → MonsterASP pour publier, ou MonsterASP → LocalDB pour sauvegarder.
/// Seul le contenu éditorial est copié (identifiants et dates conservés), jamais les comptes :
/// l'administrateur de chaque environnement garde son propre mot de passe.
/// </summary>
public static class ContentImport
{
    /// <summary>
    /// Point d'entrée de la commande :
    /// <c>dotnet run --project src/MonPortfolio.Web -- --Import:From "&lt;fichier .db ou chaîne SQL Server&gt;" [--Import:Replace true]</c>.
    /// Renvoie le code de sortie du processus.
    /// </summary>
    public static async Task<int> ImportContentAsync(
        this IServiceProvider services, string from, bool replace, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ContentImport));

        var sourceOptions = BuildSourceOptions(from, provider.GetRequiredService<IHostEnvironment>().ContentRootPath, logger);
        if (sourceOptions is null)
            return 1;

        var target = provider.GetRequiredService<PortfolioDbContext>();
        logger.LogInformation("Mise à jour du schéma de la base cible « {Database} »…", target.Database.GetDbConnection().Database);
        await target.Database.MigrateAsync(cancellationToken);

        await using var source = new PortfolioDbContext(sourceOptions);
        var report = await PortfolioContentCopier.CopyAsync(source, target, replace, cancellationToken);
        if (report is null)
        {
            logger.LogError(
                "La base cible contient déjà du contenu : import annulé pour éviter les doublons. " +
                "Relancez avec --Import:Replace true pour le remplacer (les comptes ne sont pas touchés).");
            return 2;
        }

        logger.LogInformation("Import terminé : {Report}.", report);
        return 0;
    }

    /// <summary>Une valeur contenant « = » est une chaîne SQL Server ; sinon, un chemin de fichier SQLite.</summary>
    private static DbContextOptions<PortfolioDbContext>? BuildSourceOptions(string from, string contentRoot, ILogger logger)
    {
        var options = new DbContextOptionsBuilder<PortfolioDbContext>();

        if (from.Contains('='))
        {
            logger.LogInformation("Source : base SQL Server.");
            return options.UseSqlServer(from).Options;
        }

        var path = Path.IsPathRooted(from) ? from : Path.Combine(contentRoot, from);
        if (!File.Exists(path))
        {
            logger.LogError("Base SQLite introuvable : {Path}", path);
            return null;
        }

        logger.LogInformation("Source : fichier SQLite {Path} (lecture seule).", path);
        var connection = new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadOnly };
        return options.UseSqlite(connection.ToString()).Options;
    }
}

internal static class PortfolioContentCopier
{
    /// <summary>
    /// Copie le contenu de <paramref name="source"/> vers <paramref name="target"/> dans une transaction.
    /// Renvoie null, sans rien modifier, si la cible contient déjà du contenu et que <paramref name="replace"/> est faux.
    /// </summary>
    public static async Task<ImportReport?> CopyAsync(
        PortfolioDbContext source, PortfolioDbContext target, bool replace, CancellationToken cancellationToken = default)
    {
        var projects = await source.Projects.AsNoTracking().ToListAsync(cancellationToken);
        var skills = await source.Skills.AsNoTracking().ToListAsync(cancellationToken);
        var experiences = await source.Experiences.AsNoTracking().ToListAsync(cancellationToken);
        var activities = await source.CommunityActivities.AsNoTracking().ToListAsync(cancellationToken);
        var socialLinks = await source.SocialLinks.AsNoTracking().ToListAsync(cancellationToken);
        var messages = await source.ContactMessages.AsNoTracking().ToListAsync(cancellationToken);

        if (!replace && await HasContentAsync(target, cancellationToken))
            return null;

        // La stratégie de réessai SQL Server impose d'englober la transaction dans ExecuteAsync.
        var strategy = target.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            target.ChangeTracker.Clear();
            await using var transaction = await target.Database.BeginTransactionAsync(cancellationToken);

            if (replace)
            {
                await target.Projects.ExecuteDeleteAsync(cancellationToken);
                await target.Skills.ExecuteDeleteAsync(cancellationToken);
                await target.Experiences.ExecuteDeleteAsync(cancellationToken);
                await target.CommunityActivities.ExecuteDeleteAsync(cancellationToken);
                await target.SocialLinks.ExecuteDeleteAsync(cancellationToken);
                await target.ContactMessages.ExecuteDeleteAsync(cancellationToken);
            }

            target.Projects.AddRange(projects);
            target.Skills.AddRange(skills);
            target.Experiences.AddRange(experiences);
            target.CommunityActivities.AddRange(activities);
            target.SocialLinks.AddRange(socialLinks);
            target.ContactMessages.AddRange(messages);

            await target.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

        return new ImportReport(projects.Count, skills.Count, experiences.Count, activities.Count, socialLinks.Count, messages.Count);
    }

    private static async Task<bool> HasContentAsync(PortfolioDbContext db, CancellationToken cancellationToken) =>
        await db.Projects.AnyAsync(cancellationToken)
        || await db.Skills.AnyAsync(cancellationToken)
        || await db.Experiences.AnyAsync(cancellationToken)
        || await db.CommunityActivities.AnyAsync(cancellationToken)
        || await db.SocialLinks.AnyAsync(cancellationToken)
        || await db.ContactMessages.AnyAsync(cancellationToken);
}
