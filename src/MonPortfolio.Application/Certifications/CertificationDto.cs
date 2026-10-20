using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Certifications;

public sealed record CertificationDto(
    Guid Id,
    LocalizedText Name,
    CertificationKind Kind,
    string? Issuer,
    DateOnly? IssuedOn,
    string? CredentialId,
    string? CredentialUrl,
    string? ImageUrl,
    int DisplayOrder)
{
    public string KindLabel => CertificationKinds.Label(Kind);
}

public static class CertificationKinds
{
    public static IReadOnlyList<CertificationKind> All { get; } = Enum.GetValues<CertificationKind>();

    /// <summary>Libellé dans la langue de l'interface courante.</summary>
    public static string Label(CertificationKind kind) => Languages.Current == Languages.English
        ? kind switch
        {
            CertificationKind.Certification => "Certification",
            CertificationKind.CompletionCertificate => "Certificate of completion",
            _ => kind.ToString()
        }
        : kind switch
        {
            CertificationKind.Certification => "Certification",
            CertificationKind.CompletionCertificate => "Attestation de formation",
            _ => kind.ToString()
        };

    /// <summary>Organismes courants, proposés à la saisie (un autre nom reste possible).</summary>
    public static IReadOnlyList<string> KnownIssuers { get; } =
    [
        "Microsoft", "Udemy", "LinkedIn Learning", "freeCodeCamp", "Pluralsight", "Coursera", "edX",
        "Google", "Amazon Web Services", "Oracle", "Cisco", "Scrum.org", "OpenClassrooms", "DataCamp"
    ];
}
