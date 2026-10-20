using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Domain.Entities;

/// <summary>
/// Certification ou attestation de fin de formation, avec son organisme, sa date,
/// un lien de vérification et une image (badge ou certificat) facultatifs.
/// </summary>
public sealed class Certification : AuditableEntity
{
    public const int NameMaxLength = 150;
    public const int IssuerMaxLength = 80;
    public const int CredentialIdMaxLength = 100;
    public const int UrlMaxLength = 500;

    private Certification() { } // EF Core

    public LocalizedText Name { get; private set; } = new(string.Empty);
    public CertificationKind Kind { get; private set; }

    /// <summary>Organisme : Microsoft, Udemy, LinkedIn Learning, freeCodeCamp, Pluralsight…</summary>
    public string? Issuer { get; private set; }

    public DateOnly? IssuedOn { get; private set; }

    /// <summary>Identifiant du titre, tel qu'il figure sur le certificat.</summary>
    public string? CredentialId { get; private set; }

    /// <summary>Page de vérification ou du certificat en ligne (Microsoft Learn, Credly, Udemy…).</summary>
    public string? CredentialUrl { get; private set; }

    /// <summary>Badge ou capture du certificat : fichier téléversé (« /uploads/… ») ou URL http(s).</summary>
    public string? ImageUrl { get; private set; }

    public int DisplayOrder { get; private set; }

    public static Certification Create(
        LocalizedText name,
        CertificationKind kind,
        string? issuer,
        DateOnly? issuedOn,
        string? credentialId,
        string? credentialUrl,
        string? imageUrl,
        int displayOrder,
        DateTime utcNow)
    {
        var certification = new Certification();
        certification.Apply(name, kind, issuer, issuedOn, credentialId, credentialUrl, imageUrl, displayOrder, utcNow);
        certification.MarkCreated(utcNow);
        return certification;
    }

    public void Update(
        LocalizedText name,
        CertificationKind kind,
        string? issuer,
        DateOnly? issuedOn,
        string? credentialId,
        string? credentialUrl,
        string? imageUrl,
        int displayOrder,
        DateTime utcNow)
    {
        Apply(name, kind, issuer, issuedOn, credentialId, credentialUrl, imageUrl, displayOrder, utcNow);
        MarkUpdated(utcNow);
    }

    private void Apply(
        LocalizedText name,
        CertificationKind kind,
        string? issuer,
        DateOnly? issuedOn,
        string? credentialId,
        string? credentialUrl,
        string? imageUrl,
        int displayOrder,
        DateTime utcNow)
    {
        if (issuedOn > DateOnly.FromDateTime(utcNow))
            throw new DomainException("La date d'obtention ne peut pas être dans le futur.");

        // Toutes les valeurs sont validées avant la première affectation : un échec ne laisse rien à moitié modifié.
        var validName = Guard.RequiredText(name, "Intitulé", NameMaxLength);
        var validKind = Guard.Defined(kind, "Type");
        var validIssuer = Guard.Optional(issuer, "Organisme", IssuerMaxLength);
        var validCredentialId = Guard.Optional(credentialId, "Identifiant", CredentialIdMaxLength);
        var validCredentialUrl = Guard.OptionalWebUrl(credentialUrl, "Lien du certificat", UrlMaxLength);
        var validImage = Guard.SafeUrl(imageUrl, "Image", UrlMaxLength);
        var validOrder = Guard.InRange(displayOrder, 0, 999, "Ordre d'affichage");

        Name = validName;
        Kind = validKind;
        Issuer = validIssuer;
        IssuedOn = issuedOn;
        CredentialId = validCredentialId;
        CredentialUrl = validCredentialUrl;
        ImageUrl = validImage;
        DisplayOrder = validOrder;
    }
}
