using MonPortfolio.Infrastructure.Persistence;

namespace MonPortfolio.Infrastructure.Tests.Persistence;

/// <summary>Erreurs de copier-coller courantes dans un panneau d'hébergement.</summary>
public class ConnectionStringGuardTests
{
    private const string Secret = "S3cr3t!Valeur";
    private const string Valid =
        "Server=db1234.databaseasp.net;Database=db1234;User Id=db1234;Password=" + Secret + ";Encrypt=True;TrustServerCertificate=True";

    [Theory]
    [InlineData("\"" + Valid + "\"")]
    [InlineData("'" + Valid + "'")]
    [InlineData("  " + Valid + "  ")]
    [InlineData(" \" " + Valid + " \" ")]
    public void Normalize_removes_surrounding_quotes_and_spaces(string pasted)
    {
        var normalized = ConnectionStringGuard.Normalize(pasted);

        Assert.Equal(Valid, normalized);
        ConnectionStringGuard.Validate(normalized);
    }

    [Fact]
    public void Valid_connection_string_is_accepted() =>
        ConnectionStringGuard.Validate(Valid);

    [Theory]
    [InlineData("{\"ConnectionStrings\": {\"Portfolio\": \"" + Valid + "\"}}")]
    [InlineData("\"DefaultConnection\": \"" + Valid + "\"")]
    [InlineData("ConnectionStrings__Portfolio=" + Valid)]
    [InlineData("=" + Valid)]
    public void Malformed_value_is_reported_without_leaking_any_value(string pasted)
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConnectionStringGuard.Validate(ConnectionStringGuard.Normalize(pasted)));

        Assert.Contains("mal formée", exception.Message);
        Assert.Contains("Server=", exception.Message);
        Assert.DoesNotContain(Secret, exception.Message);
        Assert.DoesNotContain("db1234", exception.Message);
    }

    [Fact]
    public void Message_points_at_a_suspicious_first_character()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConnectionStringGuard.Validate("{\"Portfolio\": \"" + Valid + "\"}"));

        Assert.Contains("premier caractère « { »", exception.Message);
    }
}
