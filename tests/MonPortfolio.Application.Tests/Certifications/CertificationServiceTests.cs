using MonPortfolio.Application.Certifications;
using MonPortfolio.Application.Tests.Fakes;
using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Tests.Certifications;

/// <summary>Certifications et attestations, dont le nettoyage de leur image.</summary>
public class CertificationServiceTests
{
    private const string Badge = "/uploads/certifications/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.png";
    private const string Certificate = "/uploads/certifications/bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb.jpg";

    private readonly InMemoryCertificationRepository repository = new();
    private readonly FakeImageStorage images = new();
    private readonly CertificationService service;

    public CertificationServiceTests() =>
        service = new CertificationService(repository, new FakeTimeProvider(), images);

    private static CertificationInput ValidInput(string? imageUrl = Badge) => new()
    {
        Name = "Développeur d'applications mobiles",
        NameEn = "Mobile app developer",
        Kind = CertificationKind.CompletionCertificate,
        Issuer = "Udemy",
        IssuedOn = new DateOnly(2025, 3, 14),
        CredentialId = "UC-1a2b3c",
        CredentialUrl = "https://www.udemy.com/certificate/UC-1a2b3c/",
        ImageUrl = imageUrl
    };

    [Fact]
    public async Task CreateAsync_stores_every_field_and_round_trips_to_the_form()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;
        var dto = await service.GetByIdAsync(id);

        Assert.NotNull(dto);
        Assert.Equal("Mobile app developer", dto.Name.In(Languages.English));
        Assert.Equal("Udemy", dto.Issuer);
        Assert.Equal(new DateOnly(2025, 3, 14), dto.IssuedOn);
        Assert.Equal("https://www.udemy.com/certificate/UC-1a2b3c/", dto.CredentialUrl);
        Assert.Equal(Badge, dto.ImageUrl);

        var form = CertificationInput.From(dto);
        Assert.Equal("Développeur d'applications mobiles", form.Name);
        Assert.Equal("Mobile app developer", form.NameEn);
        Assert.Equal("UC-1a2b3c", form.CredentialId);
        Assert.Empty(images.Deleted);
    }

    [Theory]
    [InlineData("udemy.com/certificate/x")]
    [InlineData("javascript:alert(1)")]
    public async Task CreateAsync_rejects_a_credential_link_that_is_not_a_web_url(string url)
    {
        var input = ValidInput();
        input.CredentialUrl = url;

        var result = await service.CreateAsync(input);

        Assert.True(result.IsFailure);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task UpdateAsync_deletes_the_replaced_image()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;

        await service.UpdateAsync(id, ValidInput(Certificate));

        Assert.Equal([Badge], images.Deleted);
    }

    [Fact]
    public async Task DeleteAsync_removes_the_certification_and_its_image()
    {
        var id = (await service.CreateAsync(ValidInput())).Value;

        await service.DeleteAsync(id);

        Assert.Empty(repository.Items);
        Assert.Equal([Badge], images.Deleted);
    }
}
