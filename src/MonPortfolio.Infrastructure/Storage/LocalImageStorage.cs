using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonPortfolio.Application.Common;

namespace MonPortfolio.Infrastructure.Storage;

public sealed class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    /// <summary>Préfixe public sous lequel les images sont servies.</summary>
    public const string RequestPath = "/uploads";

    /// <summary>Dossier physique (hors wwwroot), relatif à la racine du contenu s'il n'est pas absolu.</summary>
    public string RootPath { get; set; } = "App_Data/uploads";
}

/// <summary>
/// Stockage disque des images téléversées, hors de wwwroot : un déploiement n'écrase pas les fichiers
/// et ils se sauvegardent avec la base. Sécurité : le type est déduit de la signature binaire
/// (JPEG, PNG, WebP — jamais SVG, qui peut embarquer du script), le nom est généré, la taille bornée.
/// </summary>
internal sealed partial class LocalImageStorage(IOptions<ImageStorageOptions> options, ILogger<LocalImageStorage> logger)
    : IImageStorage
{
    private const string PublicPrefix = ImageStorageOptions.RequestPath + "/";
    private readonly string root = options.Value.RootPath;

    public async Task<Result<string>> SaveAsync(Stream content, ImageFolder folder, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await content.ReadAsync(chunk, cancellationToken)) > 0)
        {
            buffer.Write(chunk, 0, read);
            if (buffer.Length > ImageRules.MaxBytes)
                return Result<string>.Failure("L'image dépasse la taille maximale de 2 Mo.");
        }

        var bytes = buffer.ToArray();
        if (DetectExtension(bytes) is not { } extension)
            return Result<string>.Failure("Format non pris en charge : utilisez une image JPG, PNG ou WebP.");

        var folderName = FolderName(folder);
        var directory = Path.Combine(root, folderName);
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.CreateVersion7():N}{extension}";
        await File.WriteAllBytesAsync(Path.Combine(directory, fileName), bytes, cancellationToken);

        logger.LogInformation("Image téléversée : {Folder}/{File} ({Size} octets).", folderName, fileName, bytes.Length);
        return $"{PublicPrefix}{folderName}/{fileName}";
    }

    public Task DeleteAsync(string? publicUrl, CancellationToken cancellationToken = default)
    {
        if (!TryResolve(publicUrl, out var path))
            return Task.CompletedTask;

        try
        {
            File.Delete(path);
        }
        catch (IOException ex)
        {
            // Un fichier orphelin n'est pas bloquant : on journalise sans faire échouer l'opération métier.
            logger.LogWarning(ex, "Suppression impossible de l'image {Path}.", publicUrl);
        }

        return Task.CompletedTask;
    }

    public bool IsManaged(string? publicUrl) => TryResolve(publicUrl, out _);

    /// <summary>N'accepte que les noms générés par ce stockage : aucune traversée de répertoire possible.</summary>
    private bool TryResolve(string? publicUrl, out string path)
    {
        path = string.Empty;
        if (publicUrl is null || !publicUrl.StartsWith(PublicPrefix, StringComparison.Ordinal))
            return false;

        var relative = publicUrl[PublicPrefix.Length..];
        if (!ManagedFile().IsMatch(relative))
            return false;

        path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        return true;
    }

    private static string FolderName(ImageFolder folder) => folder switch
    {
        ImageFolder.Projects => "projects",
        ImageFolder.Activities => "activities",
        _ => throw new ArgumentOutOfRangeException(nameof(folder))
    };

    private static string? DetectExtension(ReadOnlySpan<byte> bytes)
    {
        if (bytes.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF]))
            return ".jpg";
        if (bytes.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
            return ".png";
        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8))
            return ".webp";
        return null;
    }

    [GeneratedRegex(@"^(projects|activities)/[0-9a-f]{32}\.(jpg|png|webp)$")]
    private static partial Regex ManagedFile();
}
