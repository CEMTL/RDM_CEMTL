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
   - ProfilsCEMTL
   - CotisationsAnnuelles
   - DossiersPrivilege
7. Restrictions d'écriture maintenues pour Admin / SuperUtilisateur

Hypothèses EDMX
---------------
Le contexte EDMX utilisé dans les contrôleurs reste :
    HopitalMedecinsDbEntities

Les entités supposées dans l'EDMX :
- Medecin
- MedecinProfilCEMTL
- CotisationAnnuelle
- DossierPrivilege
- Installation
- UniteOrganisationnelle
- Comite
- MedecinComite
- NominationAdministrative
- EntenteFDC
- FitTest
- InstanceDecision
- DossierPrivilegeDecision
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
- ProfilsCEMTL : filtres par département / absence / FDC + pagination serveur
- CotisationsAnnuelles : filtres par année / état / impayés + pagination serveur
- DossiersPrivilege : filtres par PEM / actif / absence + pagination serveur

Remarque
--------
Les autres modules CRUD restent compatibles avec le layout et conservent l'approche EDMX + LINQ.
Ils peuvent être enrichis ensuite selon le même patron si vous voulez homogénéiser toute l'application.


Mise à jour : la vue Details de Medecins n'affiche plus les IDs techniques des clés étrangères; elle affiche désormais les libellés correspondants (titre, type de permis, statut, catégorie, genre).
