using MonPortfolio.Domain.Common;

namespace MonPortfolio.Domain.Entities;

/// <summary>Message laissé par un visiteur via le formulaire de contact.</summary>
public sealed class ContactMessage : Entity
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 254;
    public const int SubjectMaxLength = 150;
    public const int BodyMinLength = 10;
    public const int BodyMaxLength = 4000;

    private ContactMessage() { } // EF Core

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public DateTime ReceivedAtUtc { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }

    public static ContactMessage Create(string name, string email, string subject, string body, DateTime utcNow)
    {
        var text = Guard.Required(body, "Message", BodyMaxLength);
        if (text.Length < BodyMinLength)
            throw new DomainException($"Le message doit contenir au moins {BodyMinLength} caractères.");

        return new ContactMessage
        {
            Name = Guard.Required(name, "Nom", NameMaxLength),
            Email = Guard.Email(email, "E-mail", EmailMaxLength),
            Subject = Guard.Required(subject, "Sujet", SubjectMaxLength),
            Body = text,
            ReceivedAtUtc = utcNow
        };
    }

    public void MarkAsRead(DateTime utcNow)
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAtUtc = utcNow;
    }

    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAtUtc = null;
    }
}
