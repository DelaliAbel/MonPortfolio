using Microsoft.AspNetCore.Identity;

namespace MonPortfolio.Infrastructure.Identity;

/// <summary>Compte d'administration. Détail d'infrastructure : le domaine ne connaît pas Identity.</summary>
public sealed class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
