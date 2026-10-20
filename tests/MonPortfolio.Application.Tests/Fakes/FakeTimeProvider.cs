namespace MonPortfolio.Application.Tests.Fakes;

/// <summary>Horloge figée, avançable à la main.</summary>
internal sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public static readonly DateTimeOffset Start = new(2026, 10, 19, 8, 30, 0, TimeSpan.Zero);

    public FakeTimeProvider() : this(Start) { }

    public DateTimeOffset UtcNow { get; private set; } = utcNow;

    /// <summary>Instant courant tel que les services le transmettent au domaine.</summary>
    public DateTime UtcDateTime => UtcNow.UtcDateTime;

    public override DateTimeOffset GetUtcNow() => UtcNow;

    public void Advance(TimeSpan delta) => UtcNow += delta;
}
