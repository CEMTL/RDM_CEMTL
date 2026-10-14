-- Base RDM_CEMTL | SQL Server | Exécuter uniquement sur une COPIE de test.
-- PSEUDONYMISATION REVERSIBLE : les copies privacy.* contiennent des valeurs réelles.
-- ATTENTION : ne pas exposer la base ni les sauvegardes à des tiers.
-- Coupez l'application, les imports et les écritures avant exécution.
-- Aucune suppression de cle primaire/FK; les autres champs ne sont pas modifies.
-- Restaure uniquement les champs medecins modifies par le script associe.
-- AUCUNE modification des utilisateurs, roles, identifiants ou droits.
-- Ne pas utiliser avec les sauvegardes de la precedente version.
-- Ne jamais utiliser cette copie comme jeu de donnees anonymise publiable.
USE [RDM_CEMTL];
GO
SET XACT_ABORT ON;
GO
IF OBJECT_ID(N'privacy.Execution',N'U') IS NULL
    THROW 51001, 'Aucune execution a restaurer : tables privacy absentes.',1;
GO
BEGIN TRY
 BEGIN TRANSACTION;
 IF COL_LENGTH('privacy.Execution','VersionPortee') IS NULL
    THROW 51004,'Sauvegarde provenant d une autre version : restauration refusee.',1;
 IF NOT EXISTS (SELECT 1 FROM privacy.Execution WHERE VersionPortee = 'MEDECINS_SEULEMENT_V2')
    THROW 51005,'La sauvegarde ne correspond pas au script medecins seulement.',1;
 IF (SELECT COUNT(*) FROM privacy.Execution) <> 1 OR (SELECT TOP(1) Etat FROM privacy.Execution) <> N'MASQUE'
    THROW 51002,'Etat de sauvegarde inattendu.',1;

 IF OBJECT_ID(N'privacy.Sauve_Medecin',N'U') IS NULL THROW 51011, 'Sauvegarde Medecin absente.',1;
 IF (SELECT COUNT_BIG(*) FROM [dbo].[Medecin]) <> (SELECT COUNT_BIG(*) FROM [privacy].[Sauve_Medecin]) THROW 51012, 'Nombre de lignes modifie : Medecin.',1;
 UPDATE T SET T.[Permis] = B.[Permis], T.[Nom] = B.[Nom], T.[Prenom] = B.[Prenom], T.[NAM] = B.[NAM], T.[DateNaissance] = B.[DateNaissance], T.[Commentaires] = B.[Commentaires]
 FROM [dbo].[Medecin] T INNER JOIN [privacy].[Sauve_Medecin] B ON T.[MedecinId] = B.[MedecinId];

 IF OBJECT_ID(N'privacy.Sauve_MedecinAdresse',N'U') IS NULL THROW 51011, 'Sauvegarde MedecinAdresse absente.',1;
 IF (SELECT COUNT_BIG(*) FROM [dbo].[MedecinAdresse]) <> (SELECT COUNT_BIG(*) FROM [privacy].[Sauve_MedecinAdresse]) THROW 51012, 'Nombre de lignes modifie : MedecinAdresse.',1;
 UPDATE T SET T.[AdresseLigne1] = B.[AdresseLigne1], T.[Ville] = B.[Ville], T.[Province] = B.[Province], T.[CodePostal] = B.[CodePostal]
 FROM [dbo].[MedecinAdresse] T INNER JOIN [privacy].[Sauve_MedecinAdresse] B ON T.[MedecinAdresseId] = B.[MedecinAdresseId];

 IF OBJECT_ID(N'privacy.Sauve_MedecinCourriel',N'U') IS NULL THROW 51011, 'Sauvegarde MedecinCourriel absente.',1;
 IF (SELECT COUNT_BIG(*) FROM [dbo].[MedecinCourriel]) <> (SELECT COUNT_BIG(*) FROM [privacy].[Sauve_MedecinCourriel]) THROW 51012, 'Nombre de lignes modifie : MedecinCourriel.',1;
 UPDATE T SET T.[AdresseCourriel] = B.[AdresseCourriel]
 FROM [dbo].[MedecinCourriel] T INNER JOIN [privacy].[Sauve_MedecinCourriel] B ON T.[MedecinCourrielId] = B.[MedecinCourrielId];

 IF OBJECT_ID(N'privacy.Sauve_MedecinTelephone',N'U') IS NULL THROW 51011, 'Sauvegarde MedecinTelephone absente.',1;
 IF (SELECT COUNT_BIG(*) FROM [dbo].[MedecinTelephone]) <> (SELECT COUNT_BIG(*) FROM [privacy].[Sauve_MedecinTelephone]) THROW 51012, 'Nombre de lignes modifie : MedecinTelephone.',1;
 UPDATE T SET T.[NumeroTelephone] = B.[NumeroTelephone]
 FROM [dbo].[MedecinTelephone] T INNER JOIN [privacy].[Sauve_MedecinTelephone] B ON T.[MedecinTelephoneId] = B.[MedecinTelephoneId];

 IF OBJECT_ID(N'privacy.Sauve_AffectationOrganisationnelle',N'U') IS NULL THROW 51011, 'Sauvegarde AffectationOrganisationnelle absente.',1;
 IF (SELECT COUNT_BIG(*) FROM [dbo].[AffectationOrganisationnelle] WHERE MedecinId IS NOT NULL) <> (SELECT COUNT_BIG(*) FROM [privacy].[Sauve_AffectationOrganisationnelle]) THROW 51012, 'Nombre de lignes modifie : AffectationOrganisationnelle.',1;
 UPDATE T SET T.[NomAffiche] = B.[NomAffiche], T.[Courriel] = B.[Courriel], T.[CourrielSource] = B.[CourrielSource], T.[Telephone] = B.[Telephone], T.[PosteTelephone] = B.[PosteTelephone]
 FROM [dbo].[AffectationOrganisationnelle] T INNER JOIN [privacy].[Sauve_AffectationOrganisationnelle] B ON T.[AffectationOrganisationnelleId] = B.[AffectationOrganisationnelleId];



 IF OBJECT_ID(N'privacy.Sauve_NominationAdministrative',N'U') IS NULL THROW 51011, 'Sauvegarde NominationAdministrative absente.',1;
 IF (SELECT COUNT_BIG(*) FROM [dbo].[NominationAdministrative]) <> (SELECT COUNT_BIG(*) FROM [privacy].[Sauve_NominationAdministrative]) THROW 51012, 'Nombre de lignes modifie : NominationAdministrative.',1;
 UPDATE T SET T.[LibelleNomination] = B.[LibelleNomination]
 FROM [dbo].[NominationAdministrative] T INNER JOIN [privacy].[Sauve_NominationAdministrative] B ON T.[NominationAdministrativeId] = B.[NominationAdministrativeId];

 IF OBJECT_ID(N'privacy.Sauve_FitTest',N'U') IS NULL THROW 51011, 'Sauvegarde FitTest absente.',1;
 IF (SELECT COUNT_BIG(*) FROM [dbo].[FitTest]) <> (SELECT COUNT_BIG(*) FROM [privacy].[Sauve_FitTest]) THROW 51012, 'Nombre de lignes modifie : FitTest.',1;
 UPDATE T SET T.[Commentaire] = B.[Commentaire]
 FROM [dbo].[FitTest] T INNER JOIN [privacy].[Sauve_FitTest] B ON T.[FitTestId] = B.[FitTestId];

 IF OBJECT_ID(N'privacy.Sauve_MedecinAffectationClinique',N'U') IS NULL THROW 51011, 'Sauvegarde MedecinAffectationClinique absente.',1;
 IF (SELECT COUNT_BIG(*) FROM [dbo].[MedecinAffectationClinique]) <> (SELECT COUNT_BIG(*) FROM [privacy].[Sauve_MedecinAffectationClinique]) THROW 51012, 'Nombre de lignes modifie : MedecinAffectationClinique.',1;
 UPDATE T SET T.[Commentaire] = B.[Commentaire]
 FROM [dbo].[MedecinAffectationClinique] T INNER JOIN [privacy].[Sauve_MedecinAffectationClinique] B ON T.[MedecinAffectationCliniqueId] = B.[MedecinAffectationCliniqueId];

 IF OBJECT_ID(N'privacy.Sauve_MedecinComite',N'U') IS NULL THROW 51011, 'Sauvegarde MedecinComite absente.',1;
 IF (SELECT COUNT_BIG(*) FROM [dbo].[MedecinComite]) <> (SELECT COUNT_BIG(*) FROM [privacy].[Sauve_MedecinComite]) THROW 51012, 'Nombre de lignes modifie : MedecinComite.',1;
 UPDATE T SET T.[Commentaire] = B.[Commentaire]
 FROM [dbo].[MedecinComite] T INNER JOIN [privacy].[Sauve_MedecinComite] B ON T.[MedecinComiteId] = B.[MedecinComiteId];

 IF OBJECT_ID(N'privacy.Sauve_MedecinPrivilege',N'U') IS NULL THROW 51011, 'Sauvegarde MedecinPrivilege absente.',1;
 IF (SELECT COUNT_BIG(*) FROM [dbo].[MedecinPrivilege]) <> (SELECT COUNT_BIG(*) FROM [privacy].[Sauve_MedecinPrivilege]) THROW 51012, 'Nombre de lignes modifie : MedecinPrivilege.',1;
 UPDATE T SET T.[Commentaire] = B.[Commentaire], T.[Obligations] = B.[Obligations]
 FROM [dbo].[MedecinPrivilege] T INNER JOIN [privacy].[Sauve_MedecinPrivilege] B ON T.[MedecinPrivilegeId] = B.[MedecinPrivilegeId];

 IF OBJECT_ID(N'privacy.Sauve_CotisationAnnuelle',N'U') IS NULL THROW 51011, 'Sauvegarde CotisationAnnuelle absente.',1;
 IF (SELECT COUNT_BIG(*) FROM [dbo].[CotisationAnnuelle]) <> (SELECT COUNT_BIG(*) FROM [privacy].[Sauve_CotisationAnnuelle]) THROW 51012, 'Nombre de lignes modifie : CotisationAnnuelle.',1;
 UPDATE T SET T.[DescriptionSource] = B.[DescriptionSource], T.[MotifReduction] = B.[MotifReduction], T.[PenaliteTexte] = B.[PenaliteTexte], T.[NoDepot] = B.[NoDepot], T.[RecuImposition] = B.[RecuImposition]
 FROM [dbo].[CotisationAnnuelle] T INNER JOIN [privacy].[Sauve_CotisationAnnuelle] B ON T.[CotisationAnnuelleId] = B.[CotisationAnnuelleId];

 IF OBJECT_ID(N'privacy.Sauve_DocumentMedecin',N'U') IS NULL THROW 51011, 'Sauvegarde DocumentMedecin absente.',1;
 IF (SELECT COUNT_BIG(*) FROM [dbo].[DocumentMedecin]) <> (SELECT COUNT_BIG(*) FROM [privacy].[Sauve_DocumentMedecin]) THROW 51012, 'Nombre de lignes modifie : DocumentMedecin.',1;
 UPDATE T SET T.[NomFichier] = B.[NomFichier], T.[Description] = B.[Description]
 FROM [dbo].[DocumentMedecin] T INNER JOIN [privacy].[Sauve_DocumentMedecin] B ON T.[DocumentMedecinId] = B.[DocumentMedecinId];


 DROP TABLE [privacy].[Sauve_DocumentMedecin];
 DROP TABLE [privacy].[Sauve_CotisationAnnuelle];
 DROP TABLE [privacy].[Sauve_MedecinPrivilege];
 DROP TABLE [privacy].[Sauve_MedecinComite];
 DROP TABLE [privacy].[Sauve_MedecinAffectationClinique];
 DROP TABLE [privacy].[Sauve_FitTest];
 DROP TABLE [privacy].[Sauve_NominationAdministrative];
 DROP TABLE [privacy].[Sauve_AffectationOrganisationnelle];
 DROP TABLE [privacy].[Sauve_MedecinTelephone];
 DROP TABLE [privacy].[Sauve_MedecinCourriel];
 DROP TABLE [privacy].[Sauve_MedecinAdresse];
 DROP TABLE [privacy].[Sauve_Medecin];
 DROP TABLE privacy.Execution;
 COMMIT TRANSACTION;
 PRINT 'Restauration terminee. Tables temporaires privacy effacees.';
END TRY
BEGIN CATCH
 IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
GO
