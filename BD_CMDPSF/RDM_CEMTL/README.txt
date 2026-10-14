RegistreMedecinsMVC5_EDMX_LINQ_Advanced_AligneScript_Plus
=========================================================

Contenu ajouté / ajusté
-----------------------
1. Layout partagé MVC 5 : Views/Shared/_Layout.cshtml
2. Activation automatique du layout : Views/_ViewStart.cshtml
3. Feuille de style commune : Content/registre-medecins.css
4. Tableau de bord enrichi : HomeController + Views/Home/Index.cshtml
5. Menu latéral par grands blocs du registre CEMTL
6. Filtres + pagination serveur sur :
   - Medecins
   - CotisationsAnnuelles
7. Restrictions d'écriture maintenues pour Admin / SuperUtilisateur

Hypothèses EDMX
---------------
Le contexte EDMX utilisé dans les contrôleurs reste :
    HopitalMedecinsDbEntities

Les entités supposées dans l'EDMX :
- Medecin
- CotisationAnnuelle
- Installation
- Departement
- Service
- InstallationService
- Comite
- MedecinComite
- NominationAdministrative
- EntenteFDC
- FitTest
- InstanceDecision
- JournalImport
- ref.* exposées comme entités EDMX : TitreMedecin, TypePermis, StatutMedecin, CategoriePratique, Genre

À vérifier dans Visual Studio
-----------------------------
1. Régénérer l'EDMX après la mise à jour de la base.
2. Vérifier les noms exacts des propriétés de navigation EDMX.
3. Vérifier que les rôles ASP.NET Identity utilisés sont bien :
   - Admin
   - SuperUtilisateur
4. Vérifier que le layout ne conflit pas avec un layout déjà présent dans le projet.

Modules enrichis
----------------
- Home : indicateurs synthèse
- Medecins : pagination serveur, filtres, export, DataTables rapide
- CotisationsAnnuelles : filtres par année / état / impayés + pagination serveur

Remarque
--------
Les autres modules CRUD restent compatibles avec le layout et conservent l'approche EDMX + LINQ.
Ils peuvent être enrichis ensuite selon le même patron si vous voulez homogénéiser toute l'application.


Mise à jour : départements et services (2026-10-01)
---------------------------------------------------
- Les référentiels sont maintenant dbo.Departement et dbo.Service, avec Service.DepartementId comme relation hiérarchique.
- Les privilèges médicaux utilisent MedecinPrivilege.ServiceId. La propriété applicative NecessiteService reste mappée sur la colonne historique PrivilegeReference.NecessiteUnite du script SQL.
- dbo.InstallationService est mappée afin de représenter les services offerts par installation. Lorsqu'une installation possède des associations actives, le formulaire de privilège limite et valide les services disponibles.
- Les services d'un médecin sont dérivés de ses privilèges. Aucune nouvelle table d'affectation médecin-service n'est inventée, car elle n'existe pas dans script_new.sql.
- La table historique dbo.MedecinProfilCEMTL et la vue dbo.vw_MedecinsRegistreCourant sont volontairement ignorées par l'application.


Mise à jour : responsables organisationnels (2026-10-01)
--------------------------------------------------------
- Exécuter Database/Scripts/RDM_migration_departements_services_excel_20261001.sql sur une copie de la base avant de déployer cette version de l'application.
- Le script ajoute Service.RegroupementSource et crée dbo.AffectationOrganisationnelle.
- Le script modifie uniquement le schéma; il n'insère et ne modifie aucune donnée métier.
- Les chefs de département, chefs de service, secrétaires et adjoints de site sont historisés dans cette table.
- Une affectation peut être reliée à dbo.Medecin et dbo.Installation, mais conserve aussi le nom, le site et le courriel provenant de la source.
- Les valeurs temporaires du fichier Excel sont identifiées par EstAValider.
- Le module Référentiels > Affectations organisationnelles permet de consulter et corriger ces données.
- Les affectations provenant d'une source importée ne peuvent pas être supprimées dans l'application; elles doivent être désactivées pour conserver la traçabilité.
- Les pages Département et Service affichent maintenant leurs responsables, leur état de validation et le regroupement Ophtalmologie-CUO.

Ordre de déploiement :
1. Sauvegarder la base.
2. Exécuter le script SQL en environnement de test et examiner les trois résultats de contrôle à la fin du script.
3. Valider les affectations marquées « À valider ».
4. Déployer l'application.


Mise à jour : la vue Details de Medecins n'affiche plus les IDs techniques des clés étrangères; elle affiche désormais les libellés correspondants (titre, type de permis, statut, catégorie, genre).


Authentification de la version ASP.NET Core
--------------------------------------------
La gestion des utilisateurs est alignee sur SRSAD :
- Active Directory valide uniquement le compte Windows et le mot de passe.
- Les domaines 06CEMTL et hmr.hmr.qc.ca sont essayes lorsque le domaine n'est pas saisi.
- Les acces RDM sont ensuite lus dans dbo.AspNetUsers, dbo.AspNetUserRoles et dbo.AspNetRoles.
- Les roles RDM sont Admin, SuperUtilisateur et Utilisateur.
- Le compte Authentication:BootstrapAdmin est protege contre la modification de roles, le retrait d'acces et la suppression.
- L'ecran Administration > Gestion des utilisateurs est reserve au role Admin.


=== IIS - UTILISATION EN HTTP ===
Pour autoriser temporairement la connexion RDM en HTTP :

  "Security": {
    "RequireHttps": false
  }

Avec RequireHttps=false :
- RDM ne redirige pas HTTP vers HTTPS.
- Les cookies .RDM.Auth et .RDM.Session sont envoyés en HTTP (Secure non forcé).
- IIS : Anonymous Authentication = Enabled ; Windows Authentication = Disabled.

Pour revenir à HTTPS, mettre RequireHttps=true et configurer un binding HTTPS/certificat dans IIS.
ATTENTION : le formulaire RDM contient un mot de passe Active Directory. En HTTP, ces identifiants ne sont pas chiffrés sur le réseau. HTTPS est recommandé en production.

BootstrapAdmin :
- Authentication:BootstrapAdmin est authentifié par AD comme tous les autres comptes.
- Après validation AD, RDM garantit que ce compte existe dans AspNetUsers et possède le rôle Admin.
- Cette garantie reste active même si d'autres administrateurs existent déjà.
- Le compte est conservé sous l'identité canonique configurée, par exemple 06CEMTL\T2-MERA8300.
- Ses rôles restent protégés dans Administration > Utilisateurs.

Organisation médicale
--------------------
La version actuelle utilise exclusivement dbo.Departement, dbo.Service,
dbo.AffectationOrganisationnelle et dbo.MedecinAffectationClinique pour
la structure organisationnelle et les appartenances cliniques.

Mise à jour RDM(19) - Tableau de bord et édition du dossier médecin (2026-10-03)
-------------------------------------------------------------------------------
- Le tableau de bord a été simplifié pour ne présenter que les indicateurs opérationnels essentiels : médecins actifs, privilèges à échéance, cotisations non réglées et affectations à valider.
- La liste "À traiter en priorité" est limitée aux 12 alertes les plus prioritaires, toutes catégories confondues.
- Les accès rapides du tableau de bord sont limités au registre des médecins, à l'organisation et aux privilèges gérés par MedecinPrivilege.
- Medecins/Edit/{id} charge maintenant le même dossier métier que DetailsModal : organisation clinique, responsabilités organisationnelles, privilèges, cotisations, Fit Tests, ententes FDC, comités, documents et alertes.
- Les coordonnées, installations et nominations restent directement modifiables dans le même formulaire.
- Retour/Annuler de Medecins/Edit/{id} ouvre le dossier du médecin au lieu de produire une URL du type Medecins?class=btn%20btn-default.
- Après sauvegarde d'un médecin, le dossier du médecin sauvegardé est rouvert automatiquement.
- appsettings.json est volontairement inchangé dans cette livraison.

Mise à jour : source unique des privilèges
------------------------------------------
- Les privilèges sont gérés uniquement avec dbo.MedecinPrivilege.
- Les anciennes tables de dossiers, décisions et historique de privilèges ne sont ni mappées ni interrogées par l'application.

Mise à jour : exportations administratives
-------------------------------------------
- Les administrateurs disposent des exportations PDF, Word et Excel uniquement sur les pages d'index, hors accueil.
- Les colonnes d'actions ne sont jamais exportées.
- Les recherches et filtres actifs sont appliqués aux données exportées, y compris pour les tableaux paginés côté serveur.
- Les actions d'exportation sont protégées côté serveur par le rôle Admin et par le jeton antifalsification.

Mise à jour : référentiel des types de courriel
------------------------------------------------
- Exécuter Database/Scripts/RDM_20261005_type_courriel.sql avant de déployer cette version de l'application.
- Le script crée ref.TypeCourriel et initialise Personnel, Universitaire et Institutionnel.
- dbo.MedecinCourriel.TypeCourrielId devient la référence normalisée; la colonne texte historique est conservée pour assurer la compatibilité et éviter toute perte de données.
- Les anciennes valeurs qui ne correspondent pas au nouveau référentiel restent affichées et peuvent être remplacées lors d'une modification.
- Le référentiel est administrable dans Référentiels > Types de courriel.
