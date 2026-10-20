using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Repositories;

namespace MonPortfolio.Application.Certifications;

public interface ICertificationService : ICrudService<CertificationDto, CertificationInput>;

internal sealed class CertificationService(
    ICertificationRepository repository,
    TimeProvider clock,
    IImageStorage? images = null)
    : CrudService<Certification, CertificationDto, CertificationInput>(repository, clock, "Certification", images),
      ICertificationService
{
    protected override CertificationDto ToDto(Certification certification) => certification.ToDto();

    protected override IEnumerable<string?> ImagesOf(Certification certification) => [certification.ImageUrl];

    protected override Certification CreateEntity(CertificationInput input, DateTime utcNow) =>
        Certification.Create(
            Bilingual.Text(input.Name, input.NameEn),
            input.Kind,
            input.Issuer,
            input.IssuedOn,
            input.CredentialId,
            input.CredentialUrl,
            input.ImageUrl,
            input.DisplayOrder,
            utcNow);

    protected override void UpdateEntity(Certification certification, CertificationInput input, DateTime utcNow) =>
        certification.Update(
            Bilingual.Text(input.Name, input.NameEn),
            input.Kind,
            input.Issuer,
            input.IssuedOn,
            input.CredentialId,
            input.CredentialUrl,
            input.ImageUrl,
            input.DisplayOrder,
            utcNow);
}

internal static class CertificationMappings
{
    public static CertificationDto ToDto(this Certification certification) => new(
        certification.Id,
        certification.Name,
        certification.Kind,
        certification.Issuer,
        certification.IssuedOn,
        certification.CredentialId,
        certification.CredentialUrl,
        certification.ImageUrl,
        certification.DisplayOrder);
}
