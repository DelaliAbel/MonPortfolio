namespace MonPortfolio.Domain.Common;

/// <summary>Violation d'un invariant métier.</summary>
public sealed class DomainException(string message) : Exception(message);
