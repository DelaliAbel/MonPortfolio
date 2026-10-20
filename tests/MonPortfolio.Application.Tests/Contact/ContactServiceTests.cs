using MonPortfolio.Application.Contact;
using MonPortfolio.Application.Tests.Fakes;

namespace MonPortfolio.Application.Tests.Contact;

public class ContactServiceTests
{
    private readonly InMemoryContactMessageRepository repository = new();
    private readonly FakeTimeProvider clock = new();
    private readonly ContactService service;

    public ContactServiceTests() => service = new ContactService(repository, clock);

    private static ContactInput ValidInput() => new()
    {
        Name = "Jeanne Dupont",
        Email = "jeanne.dupont@example.com",
        Subject = "Proposition de mission",
        Message = "Bonjour, êtes-vous disponible en novembre ?"
    };

    [Fact]
    public async Task SubmitAsync_stores_message()
    {
        var result = await service.SubmitAsync(ValidInput());

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(repository.Items);
        Assert.Equal("jeanne.dupont@example.com", saved.Email);
        Assert.Equal(clock.UtcDateTime, saved.ReceivedAtUtc);
        Assert.False(saved.IsRead);
    }

    [Fact]
    public async Task SubmitAsync_with_honeypot_filled_succeeds_without_storing()
    {
        var input = ValidInput();
        input.Website = "https://spam.example.com";

        var result = await service.SubmitAsync(input);

        Assert.True(result.IsSuccess);
        Assert.Empty(repository.Items);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task SubmitAsync_with_honeypot_filled_succeeds_even_when_input_is_invalid()
    {
        var result = await service.SubmitAsync(new ContactInput { Website = "x" });

        Assert.True(result.IsSuccess);
        Assert.Equal(0, repository.AddCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SubmitAsync_with_blank_honeypot_is_processed_normally(string website)
    {
        var input = ValidInput();
        input.Website = website;

        var result = await service.SubmitAsync(input);

        Assert.True(result.IsSuccess);
        Assert.Single(repository.Items);
    }

    [Fact]
    public async Task SubmitAsync_returns_failure_when_annotations_are_violated()
    {
        var input = ValidInput();
        input.Email = "pas-un-email";
        input.Message = "Court";

        var result = await service.SubmitAsync(input);

        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task SubmitAsync_translates_domain_exception_into_failure()
    {
        var input = ValidInput();
        input.Message = "    Salut !    "; // assez long pour l'annotation, trop court une fois trimé

        var result = await service.SubmitAsync(input);

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task MarkAsReadAsync_and_MarkAsUnreadAsync_toggle_read_state()
    {
        await service.SubmitAsync(ValidInput());
        var id = Assert.Single(repository.Items).Id;

        Assert.True((await service.MarkAsReadAsync(id)).IsSuccess);
        Assert.Equal(0, await service.CountUnreadAsync());
        Assert.True(Assert.Single(await service.GetAllAsync()).IsRead);

        Assert.True((await service.MarkAsUnreadAsync(id)).IsSuccess);
        Assert.Equal(1, await service.CountUnreadAsync());
        Assert.Equal(2, repository.UpdateCount);
    }

    [Fact]
    public async Task Operations_on_unknown_message_return_not_found()
    {
        var unknown = Guid.NewGuid();

        Assert.True((await service.MarkAsReadAsync(unknown)).IsFailure);
        Assert.True((await service.MarkAsUnreadAsync(unknown)).IsFailure);
        Assert.True((await service.DeleteAsync(unknown)).IsFailure);
        Assert.Equal(0, repository.UpdateCount);
        Assert.Equal(0, repository.DeleteCount);
    }
}
