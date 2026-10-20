namespace MonPortfolio.Domain.Common;

/// <summary>
/// Racine de toutes les entités : identité stable générée par le domaine
/// (GUID v7, ordonnable dans le temps) et non par la base de données.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected init; } = Guid.CreateVersion7();
}

/// <summary>
/// Entité horodatée. Les dates sont fournies par la couche Application
/// (via <see cref="TimeProvider"/>) pour garder le domaine déterministe et testable.
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    protected void MarkCreated(DateTime utcNow) => CreatedAtUtc = utcNow;

    protected void MarkUpdated(DateTime utcNow) => UpdatedAtUtc = utcNow;
}
