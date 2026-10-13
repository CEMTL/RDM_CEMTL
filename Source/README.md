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
`appsettings.json`. Elle peut être remplacée localement avec les secrets
utilisateur :

```powershell
dotnet user-secrets set "ConnectionStrings:RdmConnection" "Server=SERVEUR;Database=RDM_CEMTL;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
dotnet user-secrets set "Authentication:BootstrapAdmin" "DOMAINE\\compte-admin"
```

Pour IIS, les variables d'environnement
`ConnectionStrings__RdmConnection` et `Authentication__BootstrapAdmin`, ou un
fournisseur de secrets approuvé par l'organisation, peuvent remplacer les valeurs
du fichier sans modifier celui-ci.

`Security:RequireHttps` vaut `true` par défaut. Le mode administrateur local
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
ASP.NET Identity de la base RDM.

## Vérifications opérationnelles

- `GET /health/live` : le processus ASP.NET Core répond;
- `GET /health/ready` : la connexion à SQL Server fonctionne;
- `dotnet build Solution.csproj --configuration Release` : compilation locale;
- le workflow GitHub `Validation .NET` compile chaque demande de tirage.

## Base de données et déploiement

Consultez `Documentation/` et `Database/Scripts/` avant chaque livraison. Exécutez
les scripts sur une copie de la base, validez leurs résultats de contrôle, puis
effectuez une sauvegarde avant le déploiement de production. Les notes historiques
détaillées demeurent dans `README.txt`.
