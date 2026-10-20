using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MonPortfolio.Application.Security;

namespace MonPortfolio.Infrastructure.Identity;

internal sealed class AuthService(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<SignInStatus> SignInAsync(string email, string password, bool rememberMe)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.IsInRoleAsync(user, Roles.Admin))
        {
            // Hachage factice : temps de réponse comparable, pour ne pas révéler l'existence du compte.
            userManager.PasswordHasher.HashPassword(new ApplicationUser(), password);
            logger.LogWarning("Échec de connexion admin : compte inconnu ou non autorisé.");
            return SignInStatus.InvalidCredentials;
        }

        var result = await signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("Connexion admin réussie pour {UserId}.", user.Id);
            return SignInStatus.Succeeded;
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("Compte admin {UserId} verrouillé après plusieurs échecs.", user.Id);
            return SignInStatus.LockedOut;
        }

        return result.IsNotAllowed ? SignInStatus.NotAllowed : SignInStatus.InvalidCredentials;
    }

    public Task SignOutAsync() => signInManager.SignOutAsync();
}
