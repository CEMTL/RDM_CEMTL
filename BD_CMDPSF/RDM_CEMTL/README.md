# RDM CEMTL

Application intranet ASP.NET Core 8 pour la gestion du registre du Conseil des
médecins, dentistes, pharmaciens et sages-femmes du CEMTL.

## Prérequis

- SDK .NET 8;
- SQL Server avec le schéma fourni dans `Database/`;
- accès Active Directory aux domaines configurés;
- IIS avec le module d'hébergement ASP.NET Core pour le déploiement Windows.

## Configuration

La chaîne de connexion utilisée par l'application se trouve dans
`appsettings.json` et ce fichier doit accompagner la publication IIS. Elle peut
être remplacée localement avec les secrets utilisateur :

```powershell
dotnet user-secrets set "ConnectionStrings:RdmConnection" "Server=SERVEUR;Database=bd_cmdpsf;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
dotnet user-secrets set "Authentication:BootstrapAdmin" "DOMAINE\\compte-admin"
```

Pour IIS, les variables d'environnement
`ConnectionStrings__RdmConnection` et `Authentication__BootstrapAdmin`, ou un
fournisseur de secrets approuvé par l'organisation, peuvent remplacer les valeurs
du fichier sans modifier celui-ci.

`Security:RequireHttps` permet d'activer la redirection HTTPS. Le mode administrateur local
(`Authentication:UseActiveDirectory=false`) est limité à l'environnement
`Development`. L'exception `AllowLocalAdminOutsideDevelopment` doit uniquement
servir à une intervention temporaire et contrôlée.

## Exécution locale

```powershell
dotnet restore
dotnet run
```

L'application utilise la culture `fr-CA`. Les pages métier exigent une identité
ayant un rôle `Admin`, `SuperUtilisateur` ou `Utilisateur` dans les tables
ASP.NET Identity de la base bd_cmdpsf.

## Vérifications opérationnelles

- `GET /health/live` : le processus ASP.NET Core répond;
- `GET /health/ready` : la connexion à SQL Server fonctionne;
- `dotnet build Solution.csproj --configuration Release` : compilation locale;
- le workflow GitHub `Validation .NET` compile chaque demande de tirage.

## Base de données et déploiement

Publication IIS recommandée :

```powershell
dotnet publish Solution.csproj --configuration Release --output .\publish
```

- installer le module d'hébergement ASP.NET Core 8 sur le serveur;
- pointer l'application IIS vers le dossier `publish` et utiliser un pool
  « Aucun code managé »;
- activer l'authentification anonyme IIS, puisque l'application utilise son
  propre formulaire Active Directory;
- confirmer que `appsettings.json`, `web.config` et `RDM.Core.dll` sont présents;
- vérifier `/health/live`, puis `/health/ready`, avant d'ouvrir le tableau de bord.

Le paramètre `Encrypt` de la chaîne SQL est explicite afin que le comportement du
pilote soit identique dans Visual Studio et sous IIS.

Consultez `Documentation/` et `Database/Scripts/` avant chaque livraison. Exécutez
les scripts sur une copie de la base, validez leurs résultats de contrôle, puis
effectuez une sauvegarde avant le déploiement de production. Les notes historiques
détaillées demeurent dans `README.txt`.

## Migration vers BD_CMDPSF (octobre 2026)

Cette édition cible **bd_cmdpsf**, créée à partir de
`Database/bd_cmdpsf_CREATION.sql`. Le script SQL fourni définit
**`dbo.Professionnel`** (pas `dbo.Professionnelle` ni `dbo.Medecin`).

Les anciennes classes C# `Medecin`, `MedecinAdresse`, `StatutMedecin`,
`DocumentMedecin`, etc. restent en place pour préserver les contrôleurs,
les formulaires, les noms de paramètres et les URL existants.
Leurs correspondances **Entity Framework Core** sont configurées dans
`Models/HopitalMedecinsDbEntities.cs` pour cibler les tables réelles
`Professionnel`, `ProfessionnelAdresse`, `StatutProfessionnel`,
`DocumentProfessionnel`, etc. et les colonnes `ProfessionnelId`,
`StatutProfessionnelId`, `DocumentProfessionnelId`.

Le filtre « StatutMedecinId » utilisé dans l'interface se traduit donc en SQL
par `StatutProfessionnelId`. Les compteurs et les liens du tableau de bord
(comprenant les médecins spécialistes et omnipraticiens) continuent de
fonctionner en incluant tous les statuts. Les professions sont établies
à partir de `ref.Profession.CodeProfession`.

**Important :** la création de la base ne copie aucune donnée métier de
RDM_CEMTL; elle ne remplit que les professions de référence. Migrer
les données historiques séparément, après sauvegarde et vérification
des clés étrangères. Ne pas exécuter directement les anciens scripts
`Database/Scripts/RDM_*` contre bd_cmdpsf; ils sont conservés pour référence
historique.

Vérifications préalables recommandées :

```sql
USE bd_cmdpsf;
SELECT COUNT(*) AS NombreProfessionnels FROM dbo.Professionnel;
SELECT CodeProfession, Libelle FROM ref.Profession;
SELECT COUNT(*) AS NombreStatuts FROM ref.StatutProfessionnel;
```

Pour vérifier les objets SQL attendus, exécuter en lecture seule
`Database/VALIDATION_BD_CMDPSF.sql`. Une base créée à partir du script
peut être vide : les indicateurs de l'accueil peuvent donc valoir zéro.

Sous IIS, configurer `ConnectionStrings__RdmConnection` selon les
identifiants et politiques de l'organisation et s'assurer que les tables
`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` sont correctement alimentées.

### Correctif du 14 octobre 2026 : colonnes de clés primaires

Pour la base `bd_cmdpsf`, Entity Framework Core mappe également les
**sept clés primaires** renommées de `Medecin*Id` vers `Professionnel*Id` :

- MedecinAdresseId → ProfessionnelAdresseId
- MedecinAffectationCliniqueId → ProfessionnelAffectationCliniqueId
- MedecinComiteId → ProfessionnelComiteId
- MedecinCourrielId → ProfessionnelCourrielId
- MedecinInstallationId → ProfessionnelInstallationId
- MedecinPrivilegeId → ProfessionnelPrivilegeId
- MedecinTelephoneId → ProfessionnelTelephoneId

Ce correctif est exclusivement applicatif (Fluent API dans
`Models/HopitalMedecinsDbEntities.cs`) ; **aucune commande ALTER TABLE**
n'est nécessaire. Les propriétés C#, les actions MVC et les liens existants
ne changent pas. Voir `Database/VALIDATION_BD_CMDPSF.sql` pour contrôler
les colonnes physiques attendues dans votre base.
