using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using MonPortfolio.Application.Common;
using MonPortfolio.Infrastructure.Storage;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace MonPortfolio.Infrastructure.Tests.Storage;

/// <summary>Le stockage ne croit ni le nom ni le type annoncés : seule la signature binaire compte.</summary>
public sealed class LocalImageStorageTests : IDisposable
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];
    private static readonly byte[] Webp = [.. "RIFF"u8, 0x24, 0x00, 0x00, 0x00, .. "WEBPVP8 "u8];

    private readonly string root = Path.Combine(Path.GetTempPath(), "monportfolio-tests", Guid.NewGuid().ToString("N"));
    private readonly LocalImageStorage storage;

    public LocalImageStorageTests() =>
        storage = new LocalImageStorage(
            OptionsFactory.Create(new ImageStorageOptions { RootPath = root }),
            NullLogger<LocalImageStorage>.Instance);

    public void Dispose()
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    public static TheoryData<byte[], string> ValidImages => new()
    {
        { Jpeg, ".jpg" },
        { Png, ".png" },
        { Webp, ".webp" }
    };

    [Theory]
    [MemberData(nameof(ValidImages))]
    public async Task SaveAsync_stores_image_under_generated_name(byte[] content, string extension)
    {
        var result = await storage.SaveAsync(new MemoryStream(content), ImageFolder.Activities);

        Assert.True(result.IsSuccess);
        Assert.Matches(@"^/uploads/activities/[0-9a-f]{32}\" + extension + "$", result.Value);
        var file = Path.Combine(root, "activities", Path.GetFileName(result.Value!));
        Assert.Equal(content, await File.ReadAllBytesAsync(file));
        Assert.True(storage.IsManaged(result.Value));
    }

    [Fact]
    public async Task SaveAsync_uses_the_folder_of_the_content_type()
    {
        var result = await storage.SaveAsync(new MemoryStream(Png), ImageFolder.Projects);

        Assert.StartsWith("/uploads/projects/", result.Value);
    }

    [Theory]
    [InlineData("<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>")]
    [InlineData("<html><body>pas une image</body></html>")]
    [InlineData("GIF89a............")]
    [InlineData("")]
    public async Task SaveAsync_rejects_content_that_is_not_jpeg_png_or_webp(string content)
    {
        var result = await storage.SaveAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)), ImageFolder.Projects);

        Assert.True(result.IsFailure);
        Assert.False(Directory.Exists(Path.Combine(root, "projects")) && Directory.EnumerateFiles(Path.Combine(root, "projects")).Any());
    }

    [Fact]
    public async Task SaveAsync_rejects_files_over_the_size_limit()
    {
        var tooBig = new byte[ImageRules.MaxBytes + 1];
        Jpeg.CopyTo(tooBig, 0);

        var result = await storage.SaveAsync(new MemoryStream(tooBig), ImageFolder.Projects);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task SaveAsync_reports_a_failure_instead_of_throwing_when_the_disk_refuses_writes()
    {
        // La racine pointe sur un fichier : toute création de dossier en dessous échoue (comme un dossier en lecture seule).
        Directory.CreateDirectory(root);
        var blocked = Path.Combine(root, "bloque");
        await File.WriteAllTextAsync(blocked, "pas un dossier");
        var readOnlyStorage = new LocalImageStorage(
            OptionsFactory.Create(new ImageStorageOptions { RootPath = blocked }),
            NullLogger<LocalImageStorage>.Instance);

        var result = await readOnlyStorage.SaveAsync(new MemoryStream(Jpeg), ImageFolder.Projects);

        Assert.True(result.IsFailure);
        Assert.Contains("App_Data", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_removes_a_managed_image()
    {
        var url = (await storage.SaveAsync(new MemoryStream(Jpeg), ImageFolder.Projects)).Value!;
        var file = Path.Combine(root, "projects", Path.GetFileName(url));

        await storage.DeleteAsync(url);

        Assert.False(File.Exists(file));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("https://cdn.example.com/photo.jpg")]
    [InlineData("/images/profile.jpg")]
    [InlineData("/uploads/../portfolio.db")]
    [InlineData("/uploads/projects/../../portfolio.db")]
    [InlineData("/uploads/projects/notes.txt")]
    [InlineData("/uploads/other/0123456789abcdef0123456789abcdef.jpg")]
    public async Task Urls_not_created_by_the_storage_are_never_touched(string? url)
    {
        var sentinel = Path.Combine(root, "portfolio.db");
        Directory.CreateDirectory(root);
        await File.WriteAllTextAsync(sentinel, "ne pas supprimer");

        Assert.False(storage.IsManaged(url));
        await storage.DeleteAsync(url);

        Assert.True(File.Exists(sentinel));
    }
}
