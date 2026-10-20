namespace MonPortfolio.Domain.Enums;

/// <summary>Nature d'un titre obtenu. Les valeurs sont persistées : ne pas les renuméroter.</summary>
public enum CertificationKind
{
    /// <summary>Certification délivrée après examen (Microsoft, AWS…).</summary>
    Certification = 0,

    /// <summary>Attestation de fin de formation (Udemy, LinkedIn Learning, freeCodeCamp, Pluralsight…).</summary>
    CompletionCertificate = 1
}
