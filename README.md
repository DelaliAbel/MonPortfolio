# MonPortfolio — Délali Abel AGBÉDÉKPUI

Portfolio professionnel **Full-Stack Senior · Microsoft Gold Ambassador**, avec espace d'administration sécurisé.
.NET 10 · Blazor Web App · EF Core (SQL Server) · ASP.NET Core Identity · Clean Architecture.

## Démarrage

```bash
dotnet run --project src/MonPortfolio.Web --launch-profile http
```

Ouvrir http://localhost:5097. Au premier lancement, l'application :

1. applique les migrations sur SQL Server (en développement : LocalDB, base `MonPortfolio`) ;
2. crée le compte administrateur défini dans `AdminAccount`. Le mot de passe n'est jamais versionné : en développement, il vient des *user-secrets*
   (`dotnet user-secrets set "AdminAccount:Password" "<mot-de-passe>" --project src/MonPortfolio.Web`), en production de `AdminAccount__Password` ;
3. importe le contenu initial issu du CV (parcours, compétences, projets) **uniquement si les tables sont vides**.

Administration : http://localhost:5097/admin

## Architecture

```
src/
├── MonPortfolio.Domain          Entités (Project, Skill, Experience, ContactMessage), invariants, ports (IRepository…)
├── MonPortfolio.Application     Cas d'utilisation (services CRUD, contact, vitrine), DTO, validation, Result
├── MonPortfolio.Infrastructure  EF Core + SQL Server, dépôts, Identity (AuthService), migrations, seed, import
└── MonPortfolio.Web             Blazor : vitrine (SSR) + /admin (Interactive Server), racine de composition
```

Dépendances orientées vers le centre : `Web → Application → Domain` et `Infrastructure → Application/Domain`.
Le Web ne référence l'Infrastructure que dans `Program.cs`, pour l'injection de dépendances.
Le Domain n'a aucune dépendance. L'Application n'utilise que `Microsoft.Extensions.DependencyInjection.Abstractions`.

| Choix | Pourquoi |
|---|---|
| Vitrine en **SSR statique** | Rapide, indexable (SEO), pas de WebSocket pour les visiteurs |
| Admin en **Interactive Server** sans prérendu | CRUD fluide, aucun double chargement |
| `IDbContextFactory` + un contexte par opération | Recommandé avec Blazor Server : pas de DbContext partagé par un circuit |
| Connexion et déconnexion par **requêtes HTTP** | Le cookie d'authentification ne peut être écrit que dans une réponse HTTP |
| Contenu éditorial dans `profile.json` | Textes, distinctions, langues : modifiables sans recompiler (rechargés à chaud) |

## Sécurité

- ASP.NET Core Identity : rôle `Admin`, mots de passe de 12 caractères minimum, verrouillage après 5 échecs (15 min).
- Cookie `HttpOnly` + `SameSite=Strict` + `Secure` hors développement ; révocation des sessions sous 5 min.
- Antiforgery sur tous les formulaires (connexion, contact, déconnexion).
- Limitation de débit : 10 POST/min par IP (anti force brute et anti spam), plus un pot de miel sur le formulaire de contact.
- En-têtes : CSP stricte (aucun script inline), `X-Frame-Options: DENY`, `nosniff`, `Referrer-Policy`, `Permissions-Policy`.
- URL des projets filtrées (`http(s)` ou chemin local uniquement : pas de `javascript:`), liens `mailto` encodés.
- Protection contre les redirections ouvertes après connexion, et réponse au même rythme que le compte existe ou non.

## Production

Les secrets ne sont jamais dans le dépôt. En production, ils passent par des variables d'environnement :
`ConnectionStrings__Portfolio` et `AdminAccount__Password` (12 caractères minimum, avec majuscule, minuscule, chiffre et symbole).

- Hébergement **MonsterASP.NET** (IIS + SQL Server) : suivez [DEPLOIEMENT-MONSTERASP.md](DEPLOIEMENT-MONSTERASP.md).
- La chaîne `ConnectionStrings:Portfolio` est vide dans `appsettings.json` : en production, fournissez-la par la variable `ConnectionStrings__Portfolio`.
- Les clés Data Protection (cookies, antiforgery) sont stockées en base : les sessions survivent aux redéploiements.
- Derrière un reverse proxy, ajoutez `UseForwardedHeaders` pour que la limitation de débit voie la vraie IP du client.

## Contenus administrables (/admin)

| Écran | Contenu |
|---|---|
| Projets | Réalisations, avec une image de couverture téléversée ou les logos de la stack |
| Compétences | Stack par catégorie, avec un niveau de maîtrise |
| Parcours | Expériences, formations et engagements, avec un lien externe facultatif |
| Activités | Conférences, meetups, ateliers, articles : lien vers la publication et image de la carte (facultative) |
| Certifications | Certifications et attestations de fin de formation (Microsoft, Udemy, LinkedIn Learning, freeCodeCamp, Pluralsight…) : organisme, date, identifiant, lien de vérification et image (badge ou certificat) facultatifs |
| Réseaux sociaux | LinkedIn, GitHub, Microsoft Learn, Facebook, X, YouTube… affichés dans le hero, la section contact et le pied de page |
| Messages | Demandes reçues via le formulaire de contact |

Les images (projets, activités, certifications) sont stockées dans `App_Data/uploads` (hors de `wwwroot`) et servies sous `/uploads`. Seuls les fichiers JPG, PNG et WebP de 2 Mo au plus sont acceptés : le type est vérifié sur le contenu réel du fichier, et le nom est généré par l'application. Une image remplacée ou supprimée est effacée du disque. **Sauvegardez la base SQL Server et le dossier `App_Data/uploads`** : ensemble, ils contiennent tout le contenu.

## Français / anglais

La vitrine existe en deux langues, chacune à sa propre adresse : `/` en français (langue par défaut) et `/en` en anglais. Les deux pages sont reliées par des balises `hreflang` et une `canonical`, ce qui permet à Google d'indexer les deux versions. Le sélecteur **FR | EN** de l'en-tête mémorise le choix dans un cookie : un visiteur qui a choisi l'anglais et revient sur `/` est renvoyé vers `/en`. L'administration reste en français.

| Contenu | Où le traduire |
|---|---|
| Projets, parcours, activités, certifications, compétences | Dans `/admin` : chaque texte a un champ **FR** (obligatoire) et un champ **EN** (facultatif). Le badge « EN à traduire » signale les éléments incomplets. |
| Profil (accroche, présentation, expertises, langues, distinctions) | `src/MonPortfolio.Web/profile.en.json`. Il complète `profile.json` clé par clé : gardez les mêmes listes, dans le même ordre. |
| Textes de l'interface (titres, boutons, formulaire) | `src/MonPortfolio.Web/Resources/SharedResource.en.resx`. La clé est le texte français du composant. |

Une traduction absente n'est jamais un trou : la version anglaise affiche le texte français. Au démarrage, les textes du contenu initial (issu du CV) encore identiques à l'original reçoivent automatiquement leur traduction anglaise. Un texte modifié depuis `/admin` n'est jamais touché.

## Logos des technologies

Les logos de `src/MonPortfolio.Web/wwwroot/logos` proviennent de [Devicon](https://devicon.dev) 2.17.0 (licence MIT, voir `LICENSE-devicon.txt`). Les marques restent la propriété de leurs détenteurs.
Les logos sont associés automatiquement au nom des compétences et technologies par `Web/TechStack/TechLogos.cs`. Les noms composés sont découpés (« Oracle & MySQL » affiche deux logos), et les notions sans marque (architecture, sécurité, IA…) reçoivent une icône générique.
Pour une nouvelle technologie : déposez son SVG dans `wwwroot/logos`, puis ajoutez un alias dans `TechLogos.Aliases`.

Les réseaux sociaux utilisent leurs **logos officiels, affichés tels quels**, dans `wwwroot/logos/social`. Ils proviennent de *SVG Logos* (Gil Barbara, licence CC0), sauf Instagram qui vient de *Skill Icons* (MIT). Les sources et licences sont dans `LICENSES.txt`. Chaque logo est affiché seul, dans ses couleurs d'origine. GitHub, X et Medium, noirs à l'origine, passent sur leur variante officielle blanche en thème sombre.

## Migrations

```bash
dotnet ef migrations add <Nom> -p src/MonPortfolio.Infrastructure -s src/MonPortfolio.Web -o Persistence/Migrations
```

## Copier le contenu d'une base à l'autre

```bash
# Source : fichier SQLite (ancienne base) ou chaîne SQL Server. Cible : ConnectionStrings:Portfolio.
dotnet run --project src/MonPortfolio.Web -- --Import:From "App_Data/portfolio.db" [--Import:Replace true]
```
