namespace MonPortfolio.Infrastructure.Identity;

/// <summary>
/// Compte administrateur créé au premier démarrage. Le mot de passe ne doit jamais être commité
/// pour la production : utilisez les user-secrets ou la variable d'environnement AdminAccount__Password.
/// </summary>
public sealed class AdminAccountOptions
{
    public const string SectionName = "AdminAccount";

    public string? Email { get; set; }
    public string? Password { get; set; }
    public string DisplayName { get; set; } = "Administrateur";
}
