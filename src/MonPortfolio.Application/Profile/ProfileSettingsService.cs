using System.ComponentModel.DataAnnotations;
using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Repositories;

namespace MonPortfolio.Application.Profile;

/// <summary>Photo du hero (null : celle de profile.json) et technologies affichées sur la photo.</summary>
public sealed record ProfileSettingsDto(string? PhotoUrl, IReadOnlyList<string> HeroStack)
{
    /// <summary>Réglages jamais enregistrés : photo de profile.json et stack d'origine.</summary>
    public static ProfileSettingsDto Default { get; } = new(null, ProfileSettings.DefaultHeroStack);
}

/// <summary>Formulaire de la page « Profil » de l'administration.</summary>
public sealed class ProfileSettingsInput
{
    [SafeUrl, Display(Name = "« Photo »")]
    [StringLength(ProfileSettings.UrlMaxLength)]
    public string? PhotoUrl { get; set; }

    /// <summary>Technologies séparées par des virgules.</summary>
    [StringLength(300, ErrorMessage = "La liste des technologies est trop longue.")]
    public string? HeroStack { get; set; }

    public static ProfileSettingsInput From(ProfileSettingsDto dto) => new()
    {
        PhotoUrl = dto.PhotoUrl,
        HeroStack = TextList.JoinTags(dto.HeroStack)
    };
}

public interface IProfileSettingsService
{
    Task<ProfileSettingsDto> GetAsync(CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(ProfileSettingsInput input, CancellationToken cancellationToken = default);
}

internal sealed class ProfileSettingsService(
    IProfileSettingsRepository repository,
    TimeProvider clock,
    IImageStorage? images = null) : IProfileSettingsService
{
    public async Task<ProfileSettingsDto> GetAsync(CancellationToken cancellationToken = default) =>
        (await repository.ListAsync(cancellationToken)).FirstOrDefault() is { } settings
            ? settings.ToDto()
            : ProfileSettingsDto.Default;

    /// <summary>Crée la ligne de réglages au premier enregistrement ; l'ancienne photo téléversée est supprimée si elle change.</summary>
    public async Task<Result> UpdateAsync(ProfileSettingsInput input, CancellationToken cancellationToken = default)
    {
        var errors = InputValidator.Validate(input);
        if (errors.Count > 0)
            return Result.Failure(errors);

        var utcNow = clock.GetUtcNow().UtcDateTime;
        var stack = TextList.SplitTags(input.HeroStack);
        var settings = (await repository.ListAsync(cancellationToken)).FirstOrDefault();
        var previousPhoto = settings?.PhotoUrl;

        try
        {
            if (settings is null)
            {
                await repository.AddAsync(ProfileSettings.Create(input.PhotoUrl, stack, utcNow), cancellationToken);
            }
            else
            {
                settings.Update(input.PhotoUrl, stack, utcNow);
                await repository.UpdateAsync(settings, cancellationToken);
            }
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        if (images is not null && previousPhoto is not null && previousPhoto != input.PhotoUrl?.Trim())
            await images.DeleteAsync(previousPhoto, cancellationToken);

        return Result.Success();
    }
}

internal static class ProfileSettingsMappings
{
    public static ProfileSettingsDto ToDto(this ProfileSettings settings) => new(settings.PhotoUrl, settings.HeroStack);
}
