namespace MonPortfolio.Application.Security;

public static class Roles
{
    public const string Admin = "Admin";
}

public static class Policies
{
    /// <summary>Politique exigée par toutes les pages de l'espace /admin.</summary>
    public const string AdminOnly = "AdminOnly";
}
