using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Entities;

namespace MonPortfolio.Domain.Tests.Entities;

public class ContactMessageTests
{
    [Fact]
    public void Create_keeps_trimmed_values_and_starts_unread()
    {
        var message = Make.ContactMessage(
            name: " Jeanne Dupont ",
            email: " jeanne.dupont@example.com ",
            subject: " Mission ",
            body: "  Bonjour, êtes-vous disponible ?  ");

        Assert.Equal("Jeanne Dupont", message.Name);
        Assert.Equal("jeanne.dupont@example.com", message.Email);
        Assert.Equal("Mission", message.Subject);
        Assert.Equal("Bonjour, êtes-vous disponible ?", message.Body);
        Assert.Equal(Make.Now, message.ReceivedAtUtc);
        Assert.False(message.IsRead);
        Assert.Null(message.ReadAtUtc);
    }

    [Theory]
    [InlineData("pas-un-email")]
    [InlineData("jeanne@")]
    [InlineData("@example.com")]
    [InlineData("jeanne@@example.com")]
    [InlineData("Jeanne <jeanne@example.com>")]
    public void Create_rejects_invalid_email(string email) =>
        Assert.Throws<DomainException>(() => Make.ContactMessage(email: email));

    [Fact]
    public void Create_rejects_email_longer_than_max_length()
    {
        const string domain = "@example.com";
        var tooLong = Make.Text(ContactMessage.EmailMaxLength + 1 - domain.Length) + domain;

        Assert.Throws<DomainException>(() => Make.ContactMessage(email: tooLong));
    }

    [Theory]
    [InlineData("Trop court")] // 10 caractères : la limite
    [InlineData("  Trop court  ")]
    public void Create_accepts_body_of_min_length(string body) =>
        Assert.Equal(ContactMessage.BodyMinLength, Make.ContactMessage(body: body).Body.Length);

    [Theory]
    [InlineData("Salut !")]
    [InlineData("   Salut !   ")] // longueur mesurée après trim
    public void Create_rejects_body_shorter_than_min_length(string body) =>
        Assert.Throws<DomainException>(() => Make.ContactMessage(body: body));

    [Fact]
    public void Create_rejects_body_longer_than_max_length() =>
        Assert.Throws<DomainException>(() => Make.ContactMessage(body: Make.Text(ContactMessage.BodyMaxLength + 1)));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_requires_every_field(string? blank)
    {
        Assert.Throws<DomainException>(() => Make.ContactMessage(name: blank!));
        Assert.Throws<DomainException>(() => Make.ContactMessage(email: blank!));
        Assert.Throws<DomainException>(() => Make.ContactMessage(subject: blank!));
        Assert.Throws<DomainException>(() => Make.ContactMessage(body: blank!));
    }

    [Fact]
    public void Name_and_subject_enforce_max_length()
    {
        Assert.Throws<DomainException>(() => Make.ContactMessage(name: Make.Text(ContactMessage.NameMaxLength + 1)));
        Assert.Throws<DomainException>(() => Make.ContactMessage(subject: Make.Text(ContactMessage.SubjectMaxLength + 1)));
    }

    [Fact]
    public void MarkAsRead_records_read_date()
    {
        var message = Make.ContactMessage();

        message.MarkAsRead(Make.Later);

        Assert.True(message.IsRead);
        Assert.Equal(Make.Later, message.ReadAtUtc);
    }

    [Fact]
    public void MarkAsRead_twice_keeps_first_read_date()
    {
        var message = Make.ContactMessage();
        message.MarkAsRead(Make.Later);

        message.MarkAsRead(Make.Later.AddDays(1));

        Assert.True(message.IsRead);
        Assert.Equal(Make.Later, message.ReadAtUtc);
    }

    [Fact]
    public void MarkAsUnread_clears_read_state()
    {
        var message = Make.ContactMessage();
        message.MarkAsRead(Make.Later);

        message.MarkAsUnread();

        Assert.False(message.IsRead);
        Assert.Null(message.ReadAtUtc);
    }
}
