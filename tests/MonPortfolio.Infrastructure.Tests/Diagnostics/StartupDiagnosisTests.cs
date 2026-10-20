using MonPortfolio.Infrastructure.Diagnostics;

namespace MonPortfolio.Infrastructure.Tests.Diagnostics;

public class StartupDiagnosisTests
{
    [Fact]
    public void Missing_connection_string_points_to_the_environment_variable()
    {
        var diagnosis = StartupDiagnosis.From(new InvalidOperationException(
            "La chaîne de connexion « ConnectionStrings:Portfolio » est absente."));

        Assert.Equal("Chaîne de connexion absente", diagnosis.Title);
        Assert.Contains("ConnectionStrings__Portfolio", diagnosis.Advice);
        Assert.StartsWith("InvalidOperationException : ", diagnosis.Technical);
    }

    [Fact]
    public void Denied_write_access_is_recognised_even_when_wrapped()
    {
        var wrapped = new AggregateException(
            new InvalidOperationException("Démarrage interrompu",
                new UnauthorizedAccessException(@"Access to the path 'D:\Sites\site1\wwwroot\App_Data\uploads' is denied.")));

        var diagnosis = StartupDiagnosis.From(wrapped);

        Assert.Equal("Écriture refusée sur le disque", diagnosis.Title);
        Assert.Contains("App_Data", diagnosis.Advice);
    }

    [Theory]
    [InlineData("Format of the initialization string does not conform to specification starting at index 0.")]
    [InlineData("Keyword not supported: 'serveur'.")]
    public void Malformed_connection_string_is_explained(string message) =>
        Assert.Equal("Chaîne de connexion mal formée", StartupDiagnosis.From(new ArgumentException(message)).Title);

    [Fact]
    public void Missing_profile_file_is_explained() =>
        Assert.Equal("Fichier profile.json manquant",
            StartupDiagnosis.From(new FileNotFoundException("The configuration file 'profile.json' was not found and is not optional.")).Title);

    [Fact]
    public void Unknown_error_falls_back_to_the_innermost_exception()
    {
        var diagnosis = StartupDiagnosis.From(new Exception("Enveloppe", new TimeoutException("Délai dépassé")));

        Assert.Equal("Erreur inattendue au démarrage", diagnosis.Title);
        Assert.Equal("TimeoutException : Délai dépassé", diagnosis.Technical);
    }

    [Fact]
    public void Technical_detail_is_kept_short_and_on_one_line()
    {
        var diagnosis = StartupDiagnosis.From(new Exception("ligne 1\nligne 2 " + new string('x', 1000)));

        Assert.DoesNotContain('\n', diagnosis.Technical);
        Assert.True(diagnosis.Technical.Length < 450);
        Assert.EndsWith("…", diagnosis.Technical);
    }
}
