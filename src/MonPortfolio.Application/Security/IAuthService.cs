namespace MonPortfolio.Application.Security;

public enum SignInStatus
{
    Succeeded,
    InvalidCredentials,
    LockedOut,
    NotAllowed
}

/// <summary>
/// Port d'authentification de l'administrateur. L'implémentation (ASP.NET Core Identity)
/// est fournie par l'Infrastructure : l'UI ne connaît ni Identity ni le stockage des comptes.
/// </summary>
public interface IAuthService
{
    Task<SignInStatus> SignInAsync(string email, string password, bool rememberMe);

    Task SignOutAsync();
}
