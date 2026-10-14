# Intégration CMDPSF – livraison initiale

## Réalisé
- Conserve `dbo.Medecin` / `MedecinId` et toutes les tables associées.
- EF Core: `Profession`, relation ref.Profession et champs professionnels via une classe partielle `Medecin.Cmdpsf.cs`.
- Nouveau contrôleur `CmdpsfController` pour rechercher, créer et éditer les dentistes, pharmaciens et sages-femmes.
- Vues Razor `Views/Cmdpsf/Index.cshtml` et `Fiche.cshtml`; entrée de navigation CMDPSF.
- Droits d'écriture Admin/SuperUtilisateur, lecture par utilisateur authentifié.
- Blocage du formulaire médical historique en modification pour les autres professions.

## Prérequis SQL
Exécuter et vérifier **au préalable** `RDM_CEMTL_CMDPSF_Migration_Table_Medecin_CORRIGE.sql` sur une copie TEST. Vérifier que `ref.Profession` et les six colonnes de `dbo.Medecin` existent. Ne pas utiliser l'ancien script qui crée `dbo.Professionnel`.

## À valider impérativement
Cette livraison n'a pas été compilée : SDK .NET 8 non disponible dans cet environnement. Exécuter `dotnet restore` puis `dotnet build` dans le répertoire contenant Solution.csproj. Tester toutes les opérations sur RDM_CEMTL_TEST avant production.

## Limites actuelles
- Le registre affiche jusqu'à 500 fiches, pas encore de pagination serveur.
- Les écrans médicaux spécialisés (cotisations, privilèges, comités, documents, nominations, coordonnées) n'ont pas été adaptés de bout en bout aux règles métiers de chaque profession. Leurs relations SQL restent réutilisables, mais il faut encore tester et ajuster leurs validations et libellés.
- Le nouveau formulaire CMDPSF réalise le CRUD des champs généraux uniquement; il ne remplace pas tous les onglets du médecin.
- Les écritures effectuées par les nouveaux écrans ne sont pas encore reliées au service AuditService historique: vérifier la politique d'audit avant toute exploitation réelle.
- Les contrôles de date de permis ne remplacent pas la validation auprès des ordres professionnels.
- Vérifier que les noms de rôles Admin et SuperUtilisateur correspondent au paramétrage déployé.
