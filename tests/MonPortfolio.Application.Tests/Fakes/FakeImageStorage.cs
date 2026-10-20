using MonPortfolio.Application.Common;

namespace MonPortfolio.Application.Tests.Fakes;

/// <summary>Stockage d'images factice : mémorise les suppressions demandées.</summary>
internal sealed class FakeImageStorage : IImageStorage
{
    public List<string?> Deleted { get; } = [];

    public Task<Result<string>> SaveAsync(Stream content, ImageFolder folder, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<string>.Success($"/uploads/{folder.ToString().ToLowerInvariant()}/{Guid.NewGuid():N}.jpg"));

    public Task DeleteAsync(string? publicUrl, CancellationToken cancellationToken = default)
    {
        Deleted.Add(publicUrl);
        return Task.CompletedTask;
    }

    public bool IsManaged(string? publicUrl) => publicUrl?.StartsWith("/uploads/", StringComparison.Ordinal) == true;
}
