namespace MonPortfolio.Application.Common;

/// <summary>Dossiers de destination des images téléversées depuis l'administration.</summary>
public enum ImageFolder
{
    Projects,
    Activities,
    Certifications
}

public static class ImageRules
{
    public const long MaxBytes = 2 * 1024 * 1024;

    /// <summary>Valeur de l'attribut HTML « accept » ; le type réel est revérifié côté serveur.</summary>
    public const string AcceptAttribute = "image/jpeg,image/png,image/webp";

    public const string Description = "JPG, PNG ou WebP · 2 Mo max";
}

/// <summary>
/// Port de stockage des images. L'implémentation contrôle le contenu réel du fichier
/// (signature binaire, taille) : le nom et le type MIME envoyés par le navigateur ne sont jamais crus.
/// </summary>
public interface IImageStorage
{
    /// <summary>Enregistre l'image et renvoie son URL publique (« /uploads/… »).</summary>
    Task<Result<string>> SaveAsync(Stream content, ImageFolder folder, CancellationToken cancellationToken = default);

    /// <summary>Supprime une image téléversée par l'application ; les URL externes sont ignorées.</summary>
    Task DeleteAsync(string? publicUrl, CancellationToken cancellationToken = default);

    /// <summary>Indique si l'URL désigne un fichier géré par ce stockage.</summary>
    bool IsManaged(string? publicUrl);
}
