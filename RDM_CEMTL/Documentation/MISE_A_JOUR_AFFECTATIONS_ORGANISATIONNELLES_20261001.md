# Mise à jour des affectations organisationnelles

## Portée

Cette version adapte RDM au schéma créé par `Database/Scripts/RDM_migration_departements_services_excel_20261001.sql`.

La table `Departement` demeure le référentiel des départements. La table `Service` demeure le référentiel des services et reçoit la propriété facultative `RegroupementSource`. Les personnes et leurs rôles sont enregistrés dans `AffectationOrganisationnelle` afin de conserver les périodes et la provenance des données.

## Fonctionnalités ajoutées

- Liste filtrable des affectations organisationnelles.
- Filtres par rôle, état actif et validation requise.
- Création et modification réservées aux rôles `Admin` et `SuperUtilisateur`.
- Validation de la cible selon le rôle :
  - `CHEF_SERVICE` exige un service;
  - les autres rôles exigent un département.
- Liaison facultative avec un médecin et une installation existants.
- Gestion des coordonnées, titres, regroupements, périodes et intérims.
- Protection contre la suppression d'une affectation importée.
- Affichage des responsables dans les pages Département et Service.
- Blocage de la suppression d'un département ou d'un service encore utilisé par une affectation.

## Déploiement

1. Sauvegarder la base de données.
2. Exécuter le script SQL sur une base de test.
3. Vérifier la création de `AffectationOrganisationnelle`, des index et des vues.
4. Déployer l'application après la réussite de la migration.
5. Saisir ensuite les services et les affectations depuis les écrans de l'application.

L'application ne crée pas automatiquement cette structure au démarrage. Le script SQL doit être exécuté avant le déploiement applicatif. Il ne charge et ne modifie aucune donnée métier.

## Choix de conception

Les noms des responsables ne sont pas ajoutés comme colonnes directes de `Departement` ou de `Service`. Une personne peut changer de rôle, avoir une période d'intérim ou être remplacée. La table d'affectations conserve cet historique et évite d'écraser l'information précédente.

`CourrielSource` conserve la valeur brute importée. `Courriel` contient uniquement une adresse validée. `MedecinId` reste facultatif pour permettre les secrétaires et les personnes qui ne peuvent pas encore être associées sans ambiguïté au registre des médecins.
