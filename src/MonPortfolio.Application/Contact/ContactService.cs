using MonPortfolio.Application.Common;
using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;
using MonPortfolio.Domain.Repositories;

namespace MonPortfolio.Application.Contact;

public sealed record ContactMessageDto(
    Guid Id,
    string Name,
    string Email,
    string Subject,
    string Body,
    DateTime ReceivedAtUtc,
    bool IsRead);

public interface IContactService
{
    /// <summary>Enregistre un message envoyé depuis le formulaire public.</summary>
    Task<Result> SubmitAsync(ContactInput input, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContactMessageDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> MarkAsUnreadAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<int> CountUnreadAsync(CancellationToken cancellationToken = default);
}

internal sealed class ContactService(IContactMessageRepository repository, TimeProvider clock) : IContactService
{
    private DateTime UtcNow => clock.GetUtcNow().UtcDateTime;

    public async Task<Result> SubmitAsync(ContactInput input, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(input.Website))
            return Result.Success(); // pot de miel déclenché : on ne stocke rien

        var errors = InputValidator.Validate(input);
        if (errors.Count > 0)
            return Result.Failure(errors);

        try
        {
            var message = ContactMessage.Create(input.Name, input.Email, input.Subject, input.Message, UtcNow);
            await repository.AddAsync(message, cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<IReadOnlyList<ContactMessageDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await repository.ListAsync(cancellationToken)).Select(m => m.ToDto()).ToList();

    public Task<Result> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        ChangeAsync(id, m => m.MarkAsRead(UtcNow), cancellationToken);

    public Task<Result> MarkAsUnreadAsync(Guid id, CancellationToken cancellationToken = default) =>
        ChangeAsync(id, m => m.MarkAsUnread(), cancellationToken);

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await repository.GetByIdAsync(id, cancellationToken);
        if (message is null)
            return Result.NotFound("Message");

        await repository.DeleteAsync(message, cancellationToken);
        return Result.Success();
    }

    public Task<int> CountUnreadAsync(CancellationToken cancellationToken = default) =>
        repository.CountUnreadAsync(cancellationToken);

    private async Task<Result> ChangeAsync(Guid id, Action<ContactMessage> change, CancellationToken cancellationToken)
    {
        var message = await repository.GetByIdAsync(id, cancellationToken);
        if (message is null)
            return Result.NotFound("Message");

        change(message);
        await repository.UpdateAsync(message, cancellationToken);
        return Result.Success();
    }
}

internal static class ContactMappings
{
    public static ContactMessageDto ToDto(this ContactMessage message) => new(
        message.Id,
        message.Name,
        message.Email,
        message.Subject,
        message.Body,
        message.ReceivedAtUtc,
        message.IsRead);
}
