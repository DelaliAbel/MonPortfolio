using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Domain.Entities;

/// <summary>Lien vers un profil public (LinkedIn, GitHub, Microsoft Learn…), affiché sur la vitrine.</summary>
public sealed class SocialLink : AuditableEntity
{
    public const int UrlMaxLength = 500;
    public const int LabelMaxLength = 60;

    private SocialLink() { } // EF Core

    public SocialPlatform Platform { get; private set; }
    public string Url { get; private set; } = string.Empty;

    /// <summary>Libellé libre (ex. « @delali » ou « Blog technique »), obligatoire pour « Autre ».</summary>
    public string? Label { get; private set; }

    public int DisplayOrder { get; private set; }

    public static SocialLink Create(SocialPlatform platform, string url, string? label, int displayOrder, DateTime utcNow)
    {
        var link = new SocialLink();
        link.Apply(platform, url, label, displayOrder);
        link.MarkCreated(utcNow);
        return link;
    }

    public void Update(SocialPlatform platform, string url, string? label, int displayOrder, DateTime utcNow)
    {
        Apply(platform, url, label, displayOrder);
        MarkUpdated(utcNow);
    }

    private void Apply(SocialPlatform platform, string url, string? label, int displayOrder)
    {
        var validPlatform = Guard.Defined(platform, "Réseau");
        var validUrl = Guard.WebUrl(url, "Adresse du profil", UrlMaxLength);
        var validLabel = Guard.Optional(label, "Libellé", LabelMaxLength);
        var validOrder = Guard.InRange(displayOrder, 0, 999, "Ordre d'affichage");

        if (validPlatform == SocialPlatform.Other && validLabel is null)
            throw new DomainException("Un libellé est obligatoire pour un lien de type « Autre ».");

        Platform = validPlatform;
        Url = validUrl;
        Label = validLabel;
        DisplayOrder = validOrder;
    }
}
