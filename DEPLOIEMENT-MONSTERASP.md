# Déployer MonPortfolio sur MonsterASP.NET

MonsterASP.NET héberge sous IIS (Windows) avec une base **SQL Server** : le projet utilise donc SQL Server partout,
LocalDB en développement et la base MonsterASP en production. Le schéma est créé automatiquement au premier démarrage
(migrations EF Core appliquées par l'application elle-même).

> Offre gratuite (à vérifier sur monsterasp.net, les conditions peuvent changer) : 1 site, 1 base MSSQL de 1 Go,
> .NET 10 pris en charge, sous-domaine `*.runasp.net`, **HTTPS activé sur demande (ticket au support)**,
> **aucune sauvegarde automatique**.

---

## 1. Créer le site et la base (panneau MonsterASP)

1. Créez le site web (choisissez .NET 10 si on vous le demande).
2. Créez une base **MSSQL**, puis ouvrez-la et notez les **deux** chaînes de connexion :
   - la chaîne **locale** : celle que le site utilisera (le site et la base sont sur le même réseau) ;
   - la chaîne **distante**, après avoir activé **Remote access** (Databases → votre base → Users → Remote access).
     Serveur de la forme `dbXXXX.public.databaseasp.net`. Elle ne sert qu'à envoyer vos données depuis votre PC.
   - Les deux doivent contenir `Encrypt=True;TrustServerCertificate=True`.
3. **Demandez l'activation HTTPS** pour votre sous-domaine (ticket au support sur l'offre gratuite).
   C'est indispensable : en production, le cookie d'administration est marqué `Secure` et n'est jamais envoyé en HTTP.
   Tant que le HTTPS n'est pas actif, la vitrine fonctionne mais la connexion à `/admin` est impossible.

## 2. Définir les variables d'environnement (panneau → Scripting → Environment variables)

| Nom | Valeur |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__Portfolio` | la chaîne **locale** de l'étape 1 |
| `AdminAccount__Password` | un mot de passe fort : 12 caractères minimum, avec majuscule, minuscule, chiffre et symbole |

Cliquez ensuite sur **Restart application**. Ces secrets restent dans le panneau, jamais dans les fichiers publiés.
Le compte administrateur (`delaliabel@gmail.com`, modifiable avec `AdminAccount__Email`) est créé au premier démarrage
avec ce mot de passe. Si le mot de passe ne respecte pas la politique, le compte n'est pas créé : l'erreur apparaît dans les journaux.

## 3. Vos données : deux options

### Option A — ne rien faire

Au premier démarrage sur une base vide, l'application crée les tables puis **injecte automatiquement le contenu
issu du CV** : parcours, compétences et projets. C'est le contenu « seedé » d'origine.

### Option B — reprendre exactement votre contenu local (recommandé)

Vos ajouts locaux (projets, activité, lien LinkedIn, messages…) sont aujourd'hui dans la base LocalDB `MonPortfolio`.
Depuis la racine du dépôt, dans PowerShell :

```powershell
# Cible : la base MonsterASP (chaîne DISTANTE). La variable d'environnement ne vaut que pour cette fenêtre.
$env:ConnectionStrings__Portfolio = "Server=dbXXXX.public.databaseasp.net;Database=dbXXXX;User Id=dbXXXX;Password=VOTRE_MOT_DE_PASSE;Encrypt=True;TrustServerCertificate=True"

dotnet run --project src/MonPortfolio.Web -c Release --launch-profile http -- --Import:From "Server=(localdb)\MSSQLLocalDB;Database=MonPortfolio;Trusted_Connection=True;TrustServerCertificate=True"

Remove-Item Env:ConnectionStrings__Portfolio
```

- La commande crée les tables sur MonsterASP si besoin, copie le contenu en une seule transaction, puis s'arrête.
  Le site ne démarre pas.
- Si le site a déjà démarré une fois, la base contient déjà le contenu du CV et l'import refuse de créer des doublons :
  ajoutez `--Import:Replace true` pour remplacer ce contenu.
- **Les comptes ne sont jamais copiés** : l'administrateur de production garde le mot de passe défini à l'étape 2.
- Après plusieurs tentatives de connexion échouées, MonsterASP bloque votre IP pendant 24 h : vérifiez la chaîne avant de lancer.
- Une fois l'import fait, vous pouvez désactiver **Remote access**.

## 4. Publier depuis Visual Studio (Web Deploy)

1. Panneau → votre site → **Deploy** → passez **WebDeploy** sur *Enabled*, puis téléchargez le fichier `.publishSettings`.
2. Visual Studio : clic droit sur **MonPortfolio.Web** → **Publier** → **Importer le profil** → choisissez ce fichier.
3. **Afficher tous les paramètres** :
   - Configuration : `Release` ; framework cible : `net10.0` ;
   - Mode de déploiement : **Dépendant du framework** ;
   - Runtime cible : **win-x64**. Le paquet passe d'environ 114 Mo à 23 Mo.
   - **Bases de données / Migrations Entity Framework : laissez vide.** La chaîne vient de la variable
     d'environnement, et l'application applique elle-même ses migrations au démarrage.
4. **Publier.**

Le projet est déjà configuré pour la publication :

- `App_Data` n'est jamais publié ni effacé sur le serveur : les images téléversées sont préservées.
- Les fichiers présents uniquement sur le serveur ne sont pas supprimés.
- `appsettings.Development.json` (chaîne LocalDB de développement) n'est jamais publié. Le mot de passe de développement,
  lui, n'est même pas dans le dépôt : il est stocké dans les *user-secrets* de votre machine.

## 5. Vérifier

1. `https://VOTRE-SITE.runasp.net` : la vitrine s'affiche avec votre contenu.
2. `/admin` : connectez-vous avec `delaliabel@gmail.com` et le mot de passe de l'étape 2.
3. Ajoutez puis supprimez une image test dans **Activités**. Cela confirme que le dossier `App_Data/uploads` est accessible en écriture.
   Si ce n'est pas le cas, demandez au support les droits d'écriture sur le dossier `App_Data` du site.
4. Dans la console du navigateur (F12), vérifiez qu'aucune erreur n'apparaît. Si les WebSockets ne sont pas autorisés,
   Blazor bascule automatiquement en *long polling* : l'administration reste utilisable.

## 6. Images téléversées

Les images sont stockées dans `App_Data/uploads` sur le serveur et servies sous `/uploads`. Si vous aviez des images en local,
copiez le contenu de `src/MonPortfolio.Web/App_Data/uploads` vers `/wwwroot/App_Data/uploads` du site, par FTP, en gardant
la même arborescence (`projects/…`, `activities/…`). Aujourd'hui, votre dossier local est vide.

## 7. Sauvegardes (rien d'automatique sur l'offre gratuite)

La même commande d'import fonctionne dans l'autre sens. Pour rapatrier le contenu de production vers votre LocalDB :

```powershell
dotnet run --project src/MonPortfolio.Web -c Release --launch-profile http -- --Import:From "Server=dbXXXX.public.databaseasp.net;Database=dbXXXX;User Id=dbXXXX;Password=VOTRE_MOT_DE_PASSE;Encrypt=True;TrustServerCertificate=True" --Import:Replace true
```

(La cible est alors la base de développement définie dans `appsettings.Development.json`.) Pensez aussi à télécharger
`App_Data/uploads` par FTP.

## Dépannage

| Symptôme | Cause probable |
|---|---|
| Erreur 500.30 au démarrage | `ConnectionStrings__Portfolio` absente ou incorrecte. L'application l'indique explicitement dans ses journaux ; activez `stdoutLogEnabled="true"` dans `web.config` le temps du diagnostic. |
| Impossible de se connecter à `/admin` | HTTPS pas encore actif (cookie `Secure`), ou mot de passe différent de `AdminAccount__Password`. |
| Compte administrateur absent | Mot de passe trop faible : il doit faire 12 caractères minimum, avec majuscule, minuscule, chiffre et symbole. |
| « Trop de tentatives… » | Limitation de débit : 10 envois de formulaire par minute et par IP. Patientez une minute. |
| Erreur lors du téléversement d'une image | Droits d'écriture manquants sur `App_Data` (voir l'étape 5). |
| Avertissement « No XML encryptor configured » | Normal : les clés de chiffrement des cookies sont stockées en base, dont l'accès est déjà protégé. |
