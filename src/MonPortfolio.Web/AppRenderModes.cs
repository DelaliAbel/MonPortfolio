using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MonPortfolio.Web;

/// <summary>
/// Stratégie de rendu :
/// - vitrine publique et connexion : rendu statique côté serveur (SSR) → rapide, indexable, sans WebSocket ;
/// - administration : Interactive Server, sans prérendu (inutile pour un back-office, évite le double chargement).
/// </summary>
public static class AppRenderModes
{
    public static readonly IComponentRenderMode InteractiveAdmin = new InteractiveServerRenderMode(prerender: false);
}
