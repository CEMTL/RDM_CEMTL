# Registre CMDPSF — reprise des colonnes du registre Médecins

- `Views/Cmdpsf/Index.cshtml` : mêmes colonnes que `Views/Medecins/Index.cshtml`, avec Profession en tête ; recherche, tri et pagination DataTables côté serveur ; nouveaux boutons ; actions consulter, modifier et archiver pour Admin.
- `Controllers/CmdpsfController.cs` : Index allégé et action POST `GetRegistre` (projection SQL sur coordonnées, nominations, installations et référentiels). `MedecinId` et `dbo.Medecin` inchangés.
- Aucun SQL supplémentaire. Le projet nécessite la migration existante `ref.Profession` / `Medecin.ProfessionId`.
- La suppression reste logique, sans effacement de données.
- Validation requise dans Visual Studio : pas de SDK dotnet disponible dans le conteneur ; tester au moins le registre, filtres, tri, pagination, et rôles Admin/SuperUtilisateur.
- Les valeurs de statut, catégorie, type de permis et nomination non renseignées pour les nouvelles professions s'affichent vides jusqu'à leur saisie via les modules correspondants.
