using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Domain.Tests.Entities;

public class CertificationTests
{
    private static Certification Create(
        LocalizedText? name = null,
        CertificationKind kind = CertificationKind.Certification,
        string? issuer = "Microsoft",
        DateOnly? issuedOn = null,
        string? credentialId = null,
        string? credentialUrl = null,
        string? imageUrl = null,
        int displayOrder = 0) =>
        Certification.Create(
            name ?? new LocalizedText("Azure Fundamentals"), kind, issuer, issuedOn, credentialId, credentialUrl, imageUrl,
            displayOrder, Make.Now);

    [Fact]
    public void Create_trims_values_and_keeps_optional_fields_empty()
    {
        var certification = Create(
            name: new LocalizedText(" Architecte logiciel ", " Software architect "),
            kind: CertificationKind.CompletionCertificate,
            issuer: "  Udemy ", credentialId: "   ");

        Assert.Equal(new LocalizedText("Architecte logiciel", "Software architect"), certification.Name);
        Assert.Equal(CertificationKind.CompletionCertificate, certification.Kind);
        Assert.Equal("Udemy", certification.Issuer);
        Assert.Null(certification.CredentialId);
        Assert.Null(certification.CredentialUrl);
        Assert.Null(certification.ImageUrl);
        Assert.Equal(Make.Now, certification.CreatedAtUtc);
    }

    [Fact]
    public void Issuer_is_optional() => Assert.Null(Create(issuer: null).Issuer);

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Name_is_required(string? name) =>
        Assert.Throws<DomainException>(() => Create(name: new LocalizedText(name!)));

    [Fact]
    public void Issue_date_cannot_be_in_the_future()
    {
        var today = DateOnly.FromDateTime(Make.Now);

        Assert.Equal(today, Create(issuedOn: today).IssuedOn);
        Assert.Throws<DomainException>(() => Create(issuedOn: today.AddDays(1)));
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("/certificats/az-900")]
    [InlineData("learn.microsoft.com/credentials")]
    public void Credential_link_must_be_a_full_web_url(string url) =>
        Assert.Throws<DomainException>(() => Create(credentialUrl: url));

    [Fact]
    public void Image_accepts_uploaded_files_and_rejects_unsafe_urls()
    {
        Assert.Equal("/uploads/certifications/badge.png", Create(imageUrl: "/uploads/certifications/badge.png").ImageUrl);
        Assert.Throws<DomainException>(() => Create(imageUrl: "javascript:alert(1)"));
    }

    [Fact]
    public void Failed_update_leaves_the_certification_unchanged()
    {
        var certification = Create(issuer: "Microsoft");

        Assert.Throws<DomainException>(() => certification.Update(
            new LocalizedText("AZ-204"), CertificationKind.Certification, "Pluralsight", null, null,
            "pas une url", null, 0, Make.Later));

        Assert.Equal("Microsoft", certification.Issuer);
        Assert.Null(certification.UpdatedAtUtc);
    }
}
