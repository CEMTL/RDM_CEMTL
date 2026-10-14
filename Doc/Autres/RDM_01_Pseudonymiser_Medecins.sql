-- Base RDM_CEMTL | SQL Server | Exécuter uniquement sur une COPIE de test.
-- PSEUDONYMISATION REVERSIBLE : les copies privacy.* contiennent des valeurs réelles.
-- ATTENTION : ne pas exposer la base ni les sauvegardes à des tiers.
-- Coupez l'application, les imports et les écritures avant exécution.
-- Aucune suppression de cle primaire/FK; les autres champs ne sont pas modifies.
-- Portée : medecins et certaines tables rattachees seulement. AUCUNE modification
-- des utilisateurs (AspNetUsers, UtilisateurMetier), roles, identifiants ou droits.
-- Non exhaustif : AuditLog, contenus binaires DocumentMedecin, autres champs
-- libres, sauvegardes SQL et journaux peuvent toujours contenir des identifiants.
-- Ne jamais utiliser cette copie comme jeu de donnees anonymise publiable.
USE [RDM_CEMTL];
GO
SET XACT_ABORT ON;
GO
IF SCHEMA_ID(N'privacy') IS NULL EXEC(N'CREATE SCHEMA privacy AUTHORIZATION dbo');
GO
IF OBJECT_ID(N'privacy.Execution',N'U') IS NOT NULL
    THROW 51000, 'Une sauvegarde privacy existe deja : restaurer avant une nouvelle execution.',1;
GO
IF EXISTS (SELECT 1 FROM sys.tables WHERE schema_id = SCHEMA_ID('privacy') AND name LIKE N'Sauve[_]%')
    THROW 51003, 'Des sauvegardes privacy existent deja. Ne pas melanger les versions.',1;
GO
BEGIN TRY
 BEGIN TRANSACTION;
 CREATE TABLE privacy.Execution (VersionPortee varchar(30) NOT NULL DEFAULT 'MEDECINS_SEULEMENT_V2', DateExecution datetime2(0) NOT NULL DEFAULT SYSDATETIME(), Etat nvarchar(20) NOT NULL);
 INSERT INTO privacy.Execution(Etat) VALUES (N'MASQUE');

 -- Medecin
 IF OBJECT_ID(N'privacy.Sauve_Medecin',N'U') IS NOT NULL THROW 51010, 'Sauvegarde Medecin deja existante.',1;
 SELECT [MedecinId], [Permis], [Nom], [Prenom], [NAM], [DateNaissance], [Commentaires] INTO [privacy].[Sauve_Medecin] FROM [dbo].[Medecin] WITH (TABLOCKX,HOLDLOCK);
 UPDATE T SET [Permis] = 'TEST' + RIGHT('0000000000' + CONVERT(varchar(10),T.MedecinId),10), [Nom] = N'MEDECIN_' + CONVERT(nvarchar(15),T.MedecinId), [Prenom] = N'TEST', [NAM] = CASE WHEN T.NAM IS NULL THEN NULL ELSE 'TEST' + RIGHT('000000000000',8) + RIGHT('00000000' + CONVERT(varchar(8),T.MedecinId),8) END, [DateNaissance] = CASE WHEN T.DateNaissance IS NULL THEN NULL ELSE CONVERT(date,'19700101') END, [Commentaires] = CASE WHEN T.Commentaires IS NULL THEN NULL ELSE N'[CONTENU MASQUE]' END
 FROM [dbo].[Medecin] AS T;

 -- MedecinAdresse
 IF OBJECT_ID(N'privacy.Sauve_MedecinAdresse',N'U') IS NOT NULL THROW 51010, 'Sauvegarde MedecinAdresse deja existante.',1;
 SELECT [MedecinAdresseId], [AdresseLigne1], [Ville], [Province], [CodePostal] INTO [privacy].[Sauve_MedecinAdresse] FROM [dbo].[MedecinAdresse] WITH (TABLOCKX,HOLDLOCK);
 UPDATE T SET [AdresseLigne1] = CASE WHEN T.AdresseLigne1 IS NULL THEN NULL ELSE N'100 rue Exemple' END, [Ville] = CASE WHEN T.Ville IS NULL THEN NULL ELSE N'VilleTest' END, [Province] = CASE WHEN T.Province IS NULL THEN NULL ELSE N'QC' END, [CodePostal] = CASE WHEN T.CodePostal IS NULL THEN NULL ELSE N'H0H 0H0' END
 FROM [dbo].[MedecinAdresse] AS T;

 -- MedecinCourriel
 IF OBJECT_ID(N'privacy.Sauve_MedecinCourriel',N'U') IS NOT NULL THROW 51010, 'Sauvegarde MedecinCourriel deja existante.',1;
 SELECT [MedecinCourrielId], [AdresseCourriel] INTO [privacy].[Sauve_MedecinCourriel] FROM [dbo].[MedecinCourriel] WITH (TABLOCKX,HOLDLOCK);
 UPDATE T SET [AdresseCourriel] = 'medecin' + CONVERT(varchar(12),T.MedecinCourrielId) + '@example.invalid'
 FROM [dbo].[MedecinCourriel] AS T;

 -- MedecinTelephone
 IF OBJECT_ID(N'privacy.Sauve_MedecinTelephone',N'U') IS NOT NULL THROW 51010, 'Sauvegarde MedecinTelephone deja existante.',1;
 SELECT [MedecinTelephoneId], [NumeroTelephone] INTO [privacy].[Sauve_MedecinTelephone] FROM [dbo].[MedecinTelephone] WITH (TABLOCKX,HOLDLOCK);
 UPDATE T SET [NumeroTelephone] = '555-0100'
 FROM [dbo].[MedecinTelephone] AS T;

 -- AffectationOrganisationnelle
 IF OBJECT_ID(N'privacy.Sauve_AffectationOrganisationnelle',N'U') IS NOT NULL THROW 51010, 'Sauvegarde AffectationOrganisationnelle deja existante.',1;
 SELECT [AffectationOrganisationnelleId], [NomAffiche], [Courriel], [CourrielSource], [Telephone], [PosteTelephone] INTO [privacy].[Sauve_AffectationOrganisationnelle] FROM [dbo].[AffectationOrganisationnelle] WITH (TABLOCKX,HOLDLOCK) WHERE MedecinId IS NOT NULL;
 UPDATE T SET [NomAffiche] = N'PERSONNE TEST ' + CONVERT(nvarchar(12),T.AffectationOrganisationnelleId), [Courriel] = CASE WHEN T.Courriel IS NULL THEN NULL ELSE 'affectation' + CONVERT(varchar(12),T.AffectationOrganisationnelleId) + '@example.invalid' END, [CourrielSource] = CASE WHEN T.CourrielSource IS NULL THEN NULL ELSE 'source' + CONVERT(varchar(12),T.AffectationOrganisationnelleId) + '@example.invalid' END, [Telephone] = CASE WHEN T.Telephone IS NULL THEN NULL ELSE N'555-0100' END, [PosteTelephone] = CASE WHEN T.PosteTelephone IS NULL THEN NULL ELSE N'0000' END
 FROM [dbo].[AffectationOrganisationnelle] AS T WHERE T.MedecinId IS NOT NULL;



 -- NominationAdministrative
 IF OBJECT_ID(N'privacy.Sauve_NominationAdministrative',N'U') IS NOT NULL THROW 51010, 'Sauvegarde NominationAdministrative deja existante.',1;
 SELECT [NominationAdministrativeId], [LibelleNomination] INTO [privacy].[Sauve_NominationAdministrative] FROM [dbo].[NominationAdministrative] WITH (TABLOCKX,HOLDLOCK);
 UPDATE T SET [LibelleNomination] = CASE WHEN T.LibelleNomination IS NULL THEN NULL ELSE N'Nomination de test' END
 FROM [dbo].[NominationAdministrative] AS T;

 -- FitTest
 IF OBJECT_ID(N'privacy.Sauve_FitTest',N'U') IS NOT NULL THROW 51010, 'Sauvegarde FitTest deja existante.',1;
 SELECT [FitTestId], [Commentaire] INTO [privacy].[Sauve_FitTest] FROM [dbo].[FitTest] WITH (TABLOCKX,HOLDLOCK);
 UPDATE T SET [Commentaire] = CASE WHEN T.Commentaire IS NULL THEN NULL ELSE N'[CONTENU MASQUE]' END
 FROM [dbo].[FitTest] AS T;

 -- MedecinAffectationClinique
 IF OBJECT_ID(N'privacy.Sauve_MedecinAffectationClinique',N'U') IS NOT NULL THROW 51010, 'Sauvegarde MedecinAffectationClinique deja existante.',1;
 SELECT [MedecinAffectationCliniqueId], [Commentaire] INTO [privacy].[Sauve_MedecinAffectationClinique] FROM [dbo].[MedecinAffectationClinique] WITH (TABLOCKX,HOLDLOCK);
 UPDATE T SET [Commentaire] = CASE WHEN T.Commentaire IS NULL THEN NULL ELSE N'[CONTENU MASQUE]' END
 FROM [dbo].[MedecinAffectationClinique] AS T;

 -- MedecinComite
 IF OBJECT_ID(N'privacy.Sauve_MedecinComite',N'U') IS NOT NULL THROW 51010, 'Sauvegarde MedecinComite deja existante.',1;
 SELECT [MedecinComiteId], [Commentaire] INTO [privacy].[Sauve_MedecinComite] FROM [dbo].[MedecinComite] WITH (TABLOCKX,HOLDLOCK);
 UPDATE T SET [Commentaire] = CASE WHEN T.Commentaire IS NULL THEN NULL ELSE N'[CONTENU MASQUE]' END
 FROM [dbo].[MedecinComite] AS T;

 -- MedecinPrivilege
 IF OBJECT_ID(N'privacy.Sauve_MedecinPrivilege',N'U') IS NOT NULL THROW 51010, 'Sauvegarde MedecinPrivilege deja existante.',1;
 SELECT [MedecinPrivilegeId], [Commentaire], [Obligations] INTO [privacy].[Sauve_MedecinPrivilege] FROM [dbo].[MedecinPrivilege] WITH (TABLOCKX,HOLDLOCK);
 UPDATE T SET [Commentaire] = CASE WHEN T.Commentaire IS NULL THEN NULL ELSE N'[CONTENU MASQUE]' END, [Obligations] = CASE WHEN T.Obligations IS NULL THEN NULL ELSE N'[CONTENU MASQUE]' END
 FROM [dbo].[MedecinPrivilege] AS T;

 -- CotisationAnnuelle
 IF OBJECT_ID(N'privacy.Sauve_CotisationAnnuelle',N'U') IS NOT NULL THROW 51010, 'Sauvegarde CotisationAnnuelle deja existante.',1;
 SELECT [CotisationAnnuelleId], [DescriptionSource], [MotifReduction], [PenaliteTexte], [NoDepot], [RecuImposition] INTO [privacy].[Sauve_CotisationAnnuelle] FROM [dbo].[CotisationAnnuelle] WITH (TABLOCKX,HOLDLOCK);
 UPDATE T SET [DescriptionSource] = CASE WHEN T.DescriptionSource IS NULL THEN NULL ELSE N'[CONTENU MASQUE]' END, [MotifReduction] = CASE WHEN T.MotifReduction IS NULL THEN NULL ELSE N'[CONTENU MASQUE]' END, [PenaliteTexte] = CASE WHEN T.PenaliteTexte IS NULL THEN NULL ELSE N'[CONTENU MASQUE]' END, [NoDepot] = CASE WHEN T.NoDepot IS NULL THEN NULL ELSE N'TEST' END, [RecuImposition] = CASE WHEN T.RecuImposition IS NULL THEN NULL ELSE N'TEST' END
 FROM [dbo].[CotisationAnnuelle] AS T;

 -- DocumentMedecin
 IF OBJECT_ID(N'privacy.Sauve_DocumentMedecin',N'U') IS NOT NULL THROW 51010, 'Sauvegarde DocumentMedecin deja existante.',1;
 SELECT [DocumentMedecinId], [NomFichier], [Description] INTO [privacy].[Sauve_DocumentMedecin] FROM [dbo].[DocumentMedecin] WITH (TABLOCKX,HOLDLOCK);
 UPDATE T SET [NomFichier] = N'document_test_' + CONVERT(nvarchar(12),T.DocumentMedecinId) + N'.bin', [Description] = CASE WHEN T.Description IS NULL THEN NULL ELSE N'[CONTENU MASQUE]' END
 FROM [dbo].[DocumentMedecin] AS T;


 COMMIT TRANSACTION;
 PRINT 'Pseudonymisation terminee. Copies dans privacy.* : PROTEGER cet acces.';
END TRY
BEGIN CATCH
 IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
GO
