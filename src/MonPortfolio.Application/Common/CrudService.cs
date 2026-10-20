using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Repositories;

namespace MonPortfolio.Application.Common;

/// <summary>Contrat CRUD commun aux contenus administrables du portfolio.</summary>
public interface ICrudService<TDto, in TInput>
{
    Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateAsync(TInput input, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(Guid id, TInput input, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Orchestration commune : validation de l'entrée, délégation au domaine (qui garde ses invariants),
/// persistance via le port, traduction des violations métier en <see cref="Result"/>.
/// Si l'entité porte une image téléversée, l'ancien fichier est supprimé lorsqu'il est remplacé
/// ou que l'entité disparaît, pour ne pas laisser de fichiers orphelins.
/// </summary>
internal abstract class CrudService<TEntity, TDto, TInput>(
    IRepository<TEntity> repository,
    TimeProvider clock,
    string resourceName,
    IImageStorage? images = null) : ICrudService<TDto, TInput>
    where TEntity : Entity
    where TDto : class
    where TInput : class
{
    protected DateTime UtcNow => clock.GetUtcNow().UtcDateTime;

    protected abstract TDto ToDto(TEntity entity);

    protected abstract TEntity CreateEntity(TInput input, DateTime utcNow);

    protected abstract void UpdateEntity(TEntity entity, TInput input, DateTime utcNow);

    /// <summary>Images téléversées associées à l'entité (aucune par défaut) ; les valeurs nulles sont ignorées.</summary>
    protected virtual IEnumerable<string?> ImagesOf(TEntity entity) => [];

    public async Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await repository.ListAsync(cancellationToken)).Select(ToDto).ToList();

    public async Task<TDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await repository.GetByIdAsync(id, cancellationToken) is { } entity ? ToDto(entity) : null;

    public async Task<Result<Guid>> CreateAsync(TInput input, CancellationToken cancellationToken = default)
    {
        var errors = InputValidator.Validate(input);
        if (errors.Count > 0)
            return Result<Guid>.Failure(errors);

        try
        {
            var entity = CreateEntity(input, UtcNow);
            await repository.AddAsync(entity, cancellationToken);
            return entity.Id;
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }

    public async Task<Result> UpdateAsync(Guid id, TInput input, CancellationToken cancellationToken = default)
    {
        var errors = InputValidator.Validate(input);
        if (errors.Count > 0)
            return Result.Failure(errors);

        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.NotFound(resourceName);

        var previousImages = ImagesOf(entity).OfType<string>().ToList();
        try
        {
            UpdateEntity(entity, input, UtcNow);
            await repository.UpdateAsync(entity, cancellationToken);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        if (images is not null)
        {
            var currentImages = ImagesOf(entity).OfType<string>().ToHashSet(StringComparer.Ordinal);
            foreach (var previousImage in previousImages.Where(image => !currentImages.Contains(image)))
                await images.DeleteAsync(previousImage, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.NotFound(resourceName);

        await repository.DeleteAsync(entity, cancellationToken);

        if (images is not null)
        {
            foreach (var image in ImagesOf(entity).OfType<string>())
                await images.DeleteAsync(image, cancellationToken);
        }

        return Result.Success();
    }
}
