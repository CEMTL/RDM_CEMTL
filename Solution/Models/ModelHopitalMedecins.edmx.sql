
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 04/02/2026 06:52:22
-- Generated from EDMX file: C:\Mes Projets\Medecins\v2\Solution\Models\ModelHopitalMedecins.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [RegistreMedecinsCIUSSS_EST];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_AffectationsEffectifs_Medecin]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AffectationsEffectifs] DROP CONSTRAINT [FK_AffectationsEffectifs_Medecin];
GO
IF OBJECT_ID(N'[dbo].[FK_AffectationsEffectifs_Poste]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AffectationsEffectifs] DROP CONSTRAINT [FK_AffectationsEffectifs_Poste];
GO
IF OBJECT_ID(N'[dbo].[FK_AlertesDossier_Medecin]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AlertesDossier] DROP CONSTRAINT [FK_AlertesDossier_Medecin];
GO
IF OBJECT_ID(N'[dbo].[FK_Comites_Departement]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comites] DROP CONSTRAINT [FK_Comites_Departement];
GO
IF OBJECT_ID(N'[dbo].[FK_Comites_Installation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comites] DROP CONSTRAINT [FK_Comites_Installation];
GO
IF OBJECT_ID(N'[dbo].[FK_Comites_TypeComite]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comites] DROP CONSTRAINT [FK_Comites_TypeComite];
GO
IF OBJECT_ID(N'[dbo].[FK_Conges_Medecin]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CongesMedecins] DROP CONSTRAINT [FK_Conges_Medecin];
GO
IF OBJECT_ID(N'[dbo].[FK_Conges_TypeConge]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CongesMedecins] DROP CONSTRAINT [FK_Conges_TypeConge];
GO
IF OBJECT_ID(N'[dbo].[FK_Cotisations_Medecin]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CotisationsMedecins] DROP CONSTRAINT [FK_Cotisations_Medecin];
GO
IF OBJECT_ID(N'[dbo].[FK_Cotisations_TypeCotisation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CotisationsMedecins] DROP CONSTRAINT [FK_Cotisations_TypeCotisation];
GO
IF OBJECT_ID(N'[dbo].[FK_DecisionsWorkflow_TypeDecision]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DecisionsWorkflow] DROP CONSTRAINT [FK_DecisionsWorkflow_TypeDecision];
GO
IF OBJECT_ID(N'[dbo].[FK_DepartementsCliniques_Installation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DepartementsCliniques] DROP CONSTRAINT [FK_DepartementsCliniques_Installation];
GO
IF OBJECT_ID(N'[dbo].[FK_DocumentsMedecins_Medecin]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocumentsMedecins] DROP CONSTRAINT [FK_DocumentsMedecins_Medecin];
GO
IF OBJECT_ID(N'[dbo].[FK_DocumentsMedecins_TypeDocument]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocumentsMedecins] DROP CONSTRAINT [FK_DocumentsMedecins_TypeDocument];
GO
IF OBJECT_ID(N'[dbo].[FK_MedecinInstallations_Installation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MedecinInstallations] DROP CONSTRAINT [FK_MedecinInstallations_Installation];
GO
IF OBJECT_ID(N'[dbo].[FK_MedecinInstallations_Medecin]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MedecinInstallations] DROP CONSTRAINT [FK_MedecinInstallations_Medecin];
GO
IF OBJECT_ID(N'[dbo].[FK_Medecins_Categorie]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Medecins] DROP CONSTRAINT [FK_Medecins_Categorie];
GO
IF OBJECT_ID(N'[dbo].[FK_Medecins_Specialite]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Medecins] DROP CONSTRAINT [FK_Medecins_Specialite];
GO
IF OBJECT_ID(N'[dbo].[FK_Medecins_StatutDossier]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Medecins] DROP CONSTRAINT [FK_Medecins_StatutDossier];
GO
IF OBJECT_ID(N'[dbo].[FK_MembresComites_Comite]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MembresComites] DROP CONSTRAINT [FK_MembresComites_Comite];
GO
IF OBJECT_ID(N'[dbo].[FK_MembresComites_Medecin]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MembresComites] DROP CONSTRAINT [FK_MembresComites_Medecin];
GO
IF OBJECT_ID(N'[dbo].[FK_MouvementsCarriere_Medecin]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MouvementsCarriere] DROP CONSTRAINT [FK_MouvementsCarriere_Medecin];
GO
IF OBJECT_ID(N'[dbo].[FK_MouvementsCarriere_Type]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MouvementsCarriere] DROP CONSTRAINT [FK_MouvementsCarriere_Type];
GO
IF OBJECT_ID(N'[dbo].[FK_Nominations_Departement]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[NominationsMedecins] DROP CONSTRAINT [FK_Nominations_Departement];
GO
IF OBJECT_ID(N'[dbo].[FK_Nominations_Installation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[NominationsMedecins] DROP CONSTRAINT [FK_Nominations_Installation];
GO
IF OBJECT_ID(N'[dbo].[FK_Nominations_Medecin]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[NominationsMedecins] DROP CONSTRAINT [FK_Nominations_Medecin];
GO
IF OBJECT_ID(N'[dbo].[FK_Nominations_Service]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[NominationsMedecins] DROP CONSTRAINT [FK_Nominations_Service];
GO
IF OBJECT_ID(N'[dbo].[FK_Nominations_StatutNomination]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[NominationsMedecins] DROP CONSTRAINT [FK_Nominations_StatutNomination];
GO
IF OBJECT_ID(N'[dbo].[FK_Nominations_TypeNomination]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[NominationsMedecins] DROP CONSTRAINT [FK_Nominations_TypeNomination];
GO
IF OBJECT_ID(N'[dbo].[FK_PlansEffectifs_Departement]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PlansEffectifs] DROP CONSTRAINT [FK_PlansEffectifs_Departement];
GO
IF OBJECT_ID(N'[dbo].[FK_PlansEffectifs_Installation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PlansEffectifs] DROP CONSTRAINT [FK_PlansEffectifs_Installation];
GO
IF OBJECT_ID(N'[dbo].[FK_PlansEffectifs_Regime]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PlansEffectifs] DROP CONSTRAINT [FK_PlansEffectifs_Regime];
GO
IF OBJECT_ID(N'[dbo].[FK_PlansEffectifs_Service]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PlansEffectifs] DROP CONSTRAINT [FK_PlansEffectifs_Service];
GO
IF OBJECT_ID(N'[dbo].[FK_PlansEffectifs_Specialite]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PlansEffectifs] DROP CONSTRAINT [FK_PlansEffectifs_Specialite];
GO
IF OBJECT_ID(N'[dbo].[FK_PostesEffectifs_Plan]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PostesEffectifs] DROP CONSTRAINT [FK_PostesEffectifs_Plan];
GO
IF OBJECT_ID(N'[dbo].[FK_PratiquesReduites_Medecin]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PratiquesReduites] DROP CONSTRAINT [FK_PratiquesReduites_Medecin];
GO
IF OBJECT_ID(N'[dbo].[FK_PratiquesReduites_Type]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PratiquesReduites] DROP CONSTRAINT [FK_PratiquesReduites_Type];
GO
IF OBJECT_ID(N'[dbo].[FK_PrivilegeObligations_Privilege]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PrivilegeObligations] DROP CONSTRAINT [FK_PrivilegeObligations_Privilege];
GO
IF OBJECT_ID(N'[dbo].[FK_PrivilegeObligations_TypeObligation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PrivilegeObligations] DROP CONSTRAINT [FK_PrivilegeObligations_TypeObligation];
GO
IF OBJECT_ID(N'[dbo].[FK_Privileges_Departement]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PrivilegesMedecins] DROP CONSTRAINT [FK_Privileges_Departement];
GO
IF OBJECT_ID(N'[dbo].[FK_Privileges_Installation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PrivilegesMedecins] DROP CONSTRAINT [FK_Privileges_Installation];
GO
IF OBJECT_ID(N'[dbo].[FK_Privileges_Medecin]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PrivilegesMedecins] DROP CONSTRAINT [FK_Privileges_Medecin];
GO
IF OBJECT_ID(N'[dbo].[FK_Privileges_Nomination]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PrivilegesMedecins] DROP CONSTRAINT [FK_Privileges_Nomination];
GO
IF OBJECT_ID(N'[dbo].[FK_Privileges_Service]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PrivilegesMedecins] DROP CONSTRAINT [FK_Privileges_Service];
GO
IF OBJECT_ID(N'[dbo].[FK_Privileges_StatutPrivilege]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PrivilegesMedecins] DROP CONSTRAINT [FK_Privileges_StatutPrivilege];
GO
IF OBJECT_ID(N'[dbo].[FK_Privileges_TypePrivilege]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PrivilegesMedecins] DROP CONSTRAINT [FK_Privileges_TypePrivilege];
GO
IF OBJECT_ID(N'[dbo].[FK_RefSpecialite_Categorie]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RefSpecialite] DROP CONSTRAINT [FK_RefSpecialite_Categorie];
GO
IF OBJECT_ID(N'[dbo].[FK_Remplacements_Conge]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RemplacementsTemporaires] DROP CONSTRAINT [FK_Remplacements_Conge];
GO
IF OBJECT_ID(N'[dbo].[FK_Remplacements_MedecinRemplacant]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RemplacementsTemporaires] DROP CONSTRAINT [FK_Remplacements_MedecinRemplacant];
GO
IF OBJECT_ID(N'[dbo].[FK_Remplacements_MedecinRemplace]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RemplacementsTemporaires] DROP CONSTRAINT [FK_Remplacements_MedecinRemplace];
GO
IF OBJECT_ID(N'[dbo].[FK_Remplacements_Poste]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RemplacementsTemporaires] DROP CONSTRAINT [FK_Remplacements_Poste];
GO
IF OBJECT_ID(N'[dbo].[FK_ServicesCliniques_Departement]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ServicesCliniques] DROP CONSTRAINT [FK_ServicesCliniques_Departement];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[AffectationsEffectifs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AffectationsEffectifs];
GO
IF OBJECT_ID(N'[dbo].[AlertesDossier]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AlertesDossier];
GO
IF OBJECT_ID(N'[dbo].[Comites]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Comites];
GO
IF OBJECT_ID(N'[dbo].[CongesMedecins]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CongesMedecins];
GO
IF OBJECT_ID(N'[dbo].[CotisationsMedecins]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CotisationsMedecins];
GO
IF OBJECT_ID(N'[dbo].[DecisionsWorkflow]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DecisionsWorkflow];
GO
IF OBJECT_ID(N'[dbo].[DepartementsCliniques]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DepartementsCliniques];
GO
IF OBJECT_ID(N'[dbo].[DocumentsMedecins]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DocumentsMedecins];
GO
IF OBJECT_ID(N'[dbo].[Installations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Installations];
GO
IF OBJECT_ID(N'[dbo].[JournalAudit]', 'U') IS NOT NULL
    DROP TABLE [dbo].[JournalAudit];
GO
IF OBJECT_ID(N'[dbo].[MedecinInstallations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MedecinInstallations];
GO
IF OBJECT_ID(N'[dbo].[Medecins]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Medecins];
GO
IF OBJECT_ID(N'[dbo].[MembresComites]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MembresComites];
GO
IF OBJECT_ID(N'[dbo].[MouvementsCarriere]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MouvementsCarriere];
GO
IF OBJECT_ID(N'[dbo].[NominationsMedecins]', 'U') IS NOT NULL
    DROP TABLE [dbo].[NominationsMedecins];
GO
IF OBJECT_ID(N'[dbo].[PlansEffectifs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PlansEffectifs];
GO
IF OBJECT_ID(N'[dbo].[PostesEffectifs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PostesEffectifs];
GO
IF OBJECT_ID(N'[dbo].[PratiquesReduites]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PratiquesReduites];
GO
IF OBJECT_ID(N'[dbo].[PrivilegeObligations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PrivilegeObligations];
GO
IF OBJECT_ID(N'[dbo].[PrivilegesMedecins]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PrivilegesMedecins];
GO
IF OBJECT_ID(N'[dbo].[RefCategorieMedecin]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefCategorieMedecin];
GO
IF OBJECT_ID(N'[dbo].[RefRegimeEffectif]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefRegimeEffectif];
GO
IF OBJECT_ID(N'[dbo].[RefSpecialite]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefSpecialite];
GO
IF OBJECT_ID(N'[dbo].[RefStatutDossierMedecin]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefStatutDossierMedecin];
GO
IF OBJECT_ID(N'[dbo].[RefStatutNomination]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefStatutNomination];
GO
IF OBJECT_ID(N'[dbo].[RefStatutPrivilege]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefStatutPrivilege];
GO
IF OBJECT_ID(N'[dbo].[RefTypeComite]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefTypeComite];
GO
IF OBJECT_ID(N'[dbo].[RefTypeConge]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefTypeConge];
GO
IF OBJECT_ID(N'[dbo].[RefTypeCotisation]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefTypeCotisation];
GO
IF OBJECT_ID(N'[dbo].[RefTypeDecision]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefTypeDecision];
GO
IF OBJECT_ID(N'[dbo].[RefTypeDocument]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefTypeDocument];
GO
IF OBJECT_ID(N'[dbo].[RefTypeMouvementCarriere]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefTypeMouvementCarriere];
GO
IF OBJECT_ID(N'[dbo].[RefTypeNomination]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefTypeNomination];
GO
IF OBJECT_ID(N'[dbo].[RefTypeObligation]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefTypeObligation];
GO
IF OBJECT_ID(N'[dbo].[RefTypePratiqueReduite]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefTypePratiqueReduite];
GO
IF OBJECT_ID(N'[dbo].[RefTypePrivilege]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RefTypePrivilege];
GO
IF OBJECT_ID(N'[dbo].[RemplacementsTemporaires]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RemplacementsTemporaires];
GO
IF OBJECT_ID(N'[dbo].[ServicesCliniques]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ServicesCliniques];
GO
IF OBJECT_ID(N'[HopitalMedecinsDbModelStoreContainer].[v_AlertesRenouvellement]', 'U') IS NOT NULL
    DROP TABLE [HopitalMedecinsDbModelStoreContainer].[v_AlertesRenouvellement];
GO
IF OBJECT_ID(N'[HopitalMedecinsDbModelStoreContainer].[v_NominationsActives]', 'U') IS NOT NULL
    DROP TABLE [HopitalMedecinsDbModelStoreContainer].[v_NominationsActives];
GO
IF OBJECT_ID(N'[HopitalMedecinsDbModelStoreContainer].[v_OccupationPEM]', 'U') IS NOT NULL
    DROP TABLE [HopitalMedecinsDbModelStoreContainer].[v_OccupationPEM];
GO
IF OBJECT_ID(N'[HopitalMedecinsDbModelStoreContainer].[v_PrivilegesActifs]', 'U') IS NOT NULL
    DROP TABLE [HopitalMedecinsDbModelStoreContainer].[v_PrivilegesActifs];
GO
IF OBJECT_ID(N'[HopitalMedecinsDbModelStoreContainer].[v_RegistreMedecins]', 'U') IS NOT NULL
    DROP TABLE [HopitalMedecinsDbModelStoreContainer].[v_RegistreMedecins];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'AffectationsEffectifs'
CREATE TABLE [dbo].[AffectationsEffectifs] (
    [AffectationEffectifId] int IDENTITY(1,1) NOT NULL,
    [PosteEffectifId] int  NOT NULL,
    [MedecinId] int  NOT NULL,
    [DateDebut] datetime  NOT NULL,
    [DateFin] datetime  NULL,
    [PourcentageOccupation] decimal(5,2)  NOT NULL,
    [TypeOccupation] nvarchar(50)  NOT NULL,
    [Commentaire] nvarchar(500)  NULL
);
GO

-- Creating table 'AlertesDossier'
CREATE TABLE [dbo].[AlertesDossier] (
    [AlerteDossierId] int IDENTITY(1,1) NOT NULL,
    [MedecinId] int  NOT NULL,
    [TypeAlerte] nvarchar(100)  NOT NULL,
    [DateAlerte] datetime  NOT NULL,
    [Priorite] nvarchar(20)  NOT NULL,
    [EstTraitee] bit  NOT NULL,
    [DateTraitement] datetime  NULL,
    [Commentaire] nvarchar(1000)  NULL
);
GO

-- Creating table 'Comites'
CREATE TABLE [dbo].[Comites] (
    [ComiteId] int IDENTITY(1,1) NOT NULL,
    [TypeComiteId] int  NOT NULL,
    [Code] varchar(30)  NOT NULL,
    [NomComite] nvarchar(200)  NOT NULL,
    [InstallationId] int  NULL,
    [DepartementId] int  NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'CongesMedecins'
CREATE TABLE [dbo].[CongesMedecins] (
    [CongeId] int IDENTITY(1,1) NOT NULL,
    [MedecinId] int  NOT NULL,
    [TypeCongeId] int  NOT NULL,
    [DateDemande] datetime  NOT NULL,
    [DateDebut] datetime  NOT NULL,
    [DateFinPrevue] datetime  NULL,
    [DateFinReelle] datetime  NULL,
    [StatutConge] nvarchar(30)  NOT NULL,
    [Motif] nvarchar(500)  NULL,
    [ImpactSurPriviliges] nvarchar(500)  NULL,
    [ImpactSurPEM] nvarchar(500)  NULL,
    [Commentaire] nvarchar(1000)  NULL
);
GO

-- Creating table 'CotisationsMedecins'
CREATE TABLE [dbo].[CotisationsMedecins] (
    [CotisationId] int IDENTITY(1,1) NOT NULL,
    [MedecinId] int  NOT NULL,
    [TypeCotisationId] int  NOT NULL,
    [AnneeCotisation] int  NOT NULL,
    [Montant] decimal(10,2)  NULL,
    [DateExigibilite] datetime  NULL,
    [DatePaiement] datetime  NULL,
    [StatutPaiement] nvarchar(30)  NOT NULL,
    [ReductionAppliquee] decimal(10,2)  NULL,
    [MotifReduction] nvarchar(200)  NULL,
    [NumeroReference] nvarchar(100)  NULL,
    [Commentaire] nvarchar(500)  NULL
);
GO

-- Creating table 'DecisionsWorkflow'
CREATE TABLE [dbo].[DecisionsWorkflow] (
    [DecisionWorkflowId] int IDENTITY(1,1) NOT NULL,
    [TypeDecisionId] int  NOT NULL,
    [EntiteCible] varchar(50)  NOT NULL,
    [EntiteId] int  NOT NULL,
    [DateDecision] datetime  NOT NULL,
    [InstanceDecisionnelle] nvarchar(150)  NOT NULL,
    [NumeroReference] nvarchar(100)  NULL,
    [Resultat] nvarchar(100)  NOT NULL,
    [Commentaire] nvarchar(1000)  NULL
);
GO

-- Creating table 'DepartementsCliniques'
CREATE TABLE [dbo].[DepartementsCliniques] (
    [DepartementId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(30)  NOT NULL,
    [NomDepartement] nvarchar(200)  NOT NULL,
    [InstallationId] int  NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'DocumentsMedecins'
CREATE TABLE [dbo].[DocumentsMedecins] (
    [DocumentMedecinId] int IDENTITY(1,1) NOT NULL,
    [MedecinId] int  NOT NULL,
    [TypeDocumentId] int  NOT NULL,
    [EntiteCible] varchar(50)  NULL,
    [EntiteId] int  NULL,
    [TitreDocument] nvarchar(200)  NOT NULL,
    [NomFichier] nvarchar(255)  NULL,
    [CheminStockage] nvarchar(500)  NULL,
    [DateDocument] datetime  NULL,
    [DateExpiration] datetime  NULL,
    [EstObligatoire] bit  NOT NULL,
    [Commentaire] nvarchar(1000)  NULL
);
GO

-- Creating table 'Installations'
CREATE TABLE [dbo].[Installations] (
    [InstallationId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(30)  NOT NULL,
    [NomInstallation] nvarchar(200)  NOT NULL,
    [TypeInstallation] nvarchar(100)  NULL,
    [Adresse] nvarchar(250)  NULL,
    [Ville] nvarchar(100)  NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'JournalAudit'
CREATE TABLE [dbo].[JournalAudit] (
    [JournalAuditId] bigint IDENTITY(1,1) NOT NULL,
    [DateAction] datetime  NOT NULL,
    [Utilisateur] nvarchar(100)  NOT NULL,
    [Action] nvarchar(100)  NOT NULL,
    [Entite] nvarchar(100)  NOT NULL,
    [EntiteId] nvarchar(100)  NULL,
    [Description] nvarchar(2000)  NULL,
    [AdresseIP] nvarchar(50)  NULL,
    [Succes] bit  NOT NULL
);
GO

-- Creating table 'MedecinInstallations'
CREATE TABLE [dbo].[MedecinInstallations] (
    [MedecinInstallationId] int IDENTITY(1,1) NOT NULL,
    [MedecinId] int  NOT NULL,
    [InstallationId] int  NOT NULL,
    [EstInstallationPrincipale] bit  NOT NULL,
    [DateDebut] datetime  NOT NULL,
    [DateFin] datetime  NULL,
    [Commentaire] nvarchar(500)  NULL
);
GO

-- Creating table 'Medecins'
CREATE TABLE [dbo].[Medecins] (
    [MedecinId] int IDENTITY(1,1) NOT NULL,
    [NumeroRegistreInterne] varchar(30)  NOT NULL,
    [NumeroPermisCMQ] varchar(30)  NULL,
    [RAMQ] varchar(30)  NULL,
    [NAS] varchar(15)  NULL,
    [Nom] nvarchar(100)  NOT NULL,
    [Prenom] nvarchar(100)  NOT NULL,
    [Sexe] char(1)  NULL,
    [DateNaissance] datetime  NULL,
    [CourrielProfessionnel] nvarchar(200)  NULL,
    [Telephone] nvarchar(30)  NULL,
    [AdresseProfessionnelle] nvarchar(250)  NULL,
    [CategorieMedecinId] int  NOT NULL,
    [SpecialiteId] int  NULL,
    [StatutDossierMedecinId] int  NOT NULL,
    [DateDebutRelation] datetime  NULL,
    [DateFinRelation] datetime  NULL,
    [EstChefDepartement] bit  NOT NULL,
    [EstActif] bit  NOT NULL,
    [DateCreation] datetime  NOT NULL,
    [DateModification] datetime  NULL,
    [CreePar] nvarchar(100)  NULL,
    [ModifiePar] nvarchar(100)  NULL
);
GO

-- Creating table 'MembresComites'
CREATE TABLE [dbo].[MembresComites] (
    [MembreComiteId] int IDENTITY(1,1) NOT NULL,
    [ComiteId] int  NOT NULL,
    [MedecinId] int  NOT NULL,
    [FonctionDansComite] nvarchar(100)  NULL,
    [DateDebut] datetime  NOT NULL,
    [DateFin] datetime  NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'MouvementsCarriere'
CREATE TABLE [dbo].[MouvementsCarriere] (
    [MouvementCarriereId] int IDENTITY(1,1) NOT NULL,
    [MedecinId] int  NOT NULL,
    [TypeMouvementCarriereId] int  NOT NULL,
    [DateAnnonce] datetime  NULL,
    [DateEffet] datetime  NOT NULL,
    [Motif] nvarchar(500)  NULL,
    [DetailsTransition] nvarchar(1000)  NULL,
    [RemplacementPrevu] bit  NOT NULL,
    [Commentaire] nvarchar(1000)  NULL
);
GO

-- Creating table 'NominationsMedecins'
CREATE TABLE [dbo].[NominationsMedecins] (
    [NominationId] int IDENTITY(1,1) NOT NULL,
    [MedecinId] int  NOT NULL,
    [TypeNominationId] int  NOT NULL,
    [StatutNominationId] int  NOT NULL,
    [InstallationId] int  NULL,
    [DepartementId] int  NULL,
    [ServiceCliniqueId] int  NULL,
    [DateDemande] datetime  NOT NULL,
    [DateRecommendationCMDP] datetime  NULL,
    [DateDecisionCA] datetime  NULL,
    [DateEffet] datetime  NULL,
    [DateEcheance] datetime  NULL,
    [NumeroResolution] nvarchar(100)  NULL,
    [Motif] nvarchar(500)  NULL,
    [Commentaire] nvarchar(1000)  NULL
);
GO

-- Creating table 'PlansEffectifs'
CREATE TABLE [dbo].[PlansEffectifs] (
    [PlanEffectifId] int IDENTITY(1,1) NOT NULL,
    [RegimeEffectifId] int  NOT NULL,
    [AnneeExercice] int  NOT NULL,
    [InstallationId] int  NULL,
    [DepartementId] int  NULL,
    [ServiceCliniqueId] int  NULL,
    [SpecialiteId] int  NULL,
    [NbPostesAutorises] int  NOT NULL,
    [NbPostesOccupes] int  NOT NULL,
    [DateApprobation] datetime  NULL,
    [ReferenceMinisterielle] nvarchar(150)  NULL,
    [Commentaire] nvarchar(500)  NULL
);
GO

-- Creating table 'PostesEffectifs'
CREATE TABLE [dbo].[PostesEffectifs] (
    [PosteEffectifId] int IDENTITY(1,1) NOT NULL,
    [PlanEffectifId] int  NOT NULL,
    [CodePoste] varchar(50)  NOT NULL,
    [LibellePoste] nvarchar(200)  NOT NULL,
    [StatutPoste] nvarchar(50)  NOT NULL,
    [DateOuverture] datetime  NULL,
    [DateFermeture] datetime  NULL,
    [Commentaire] nvarchar(500)  NULL
);
GO

-- Creating table 'PratiquesReduites'
CREATE TABLE [dbo].[PratiquesReduites] (
    [PratiqueReduiteId] int IDENTITY(1,1) NOT NULL,
    [MedecinId] int  NOT NULL,
    [TypePratiqueReduiteId] int  NOT NULL,
    [DateDebut] datetime  NOT NULL,
    [DateFinPrevue] datetime  NULL,
    [PourcentageActivite] decimal(5,2)  NOT NULL,
    [Motif] nvarchar(500)  NULL,
    [BaseNormative] nvarchar(300)  NULL,
    [ValidationChef] bit  NOT NULL,
    [ValidationDSP] bit  NOT NULL,
    [Commentaire] nvarchar(1000)  NULL
);
GO

-- Creating table 'PrivilegeObligations'
CREATE TABLE [dbo].[PrivilegeObligations] (
    [PrivilegeObligationId] int IDENTITY(1,1) NOT NULL,
    [PrivilegeId] int  NOT NULL,
    [TypeObligationId] int  NOT NULL,
    [LibelleObligation] nvarchar(200)  NOT NULL,
    [DescriptionObligation] nvarchar(1000)  NULL,
    [DateDebut] datetime  NULL,
    [DateFin] datetime  NULL,
    [EstObligatoire] bit  NOT NULL
);
GO

-- Creating table 'PrivilegesMedecins'
CREATE TABLE [dbo].[PrivilegesMedecins] (
    [PrivilegeId] int IDENTITY(1,1) NOT NULL,
    [NominationId] int  NULL,
    [MedecinId] int  NOT NULL,
    [TypePrivilegeId] int  NOT NULL,
    [StatutPrivilegeId] int  NOT NULL,
    [InstallationId] int  NULL,
    [DepartementId] int  NULL,
    [ServiceCliniqueId] int  NULL,
    [LibelleCourt] nvarchar(150)  NOT NULL,
    [DescriptionPrivilege] nvarchar(1000)  NULL,
    [DateDebut] datetime  NOT NULL,
    [DateFin] datetime  NULL,
    [EstUrgence] bit  NOT NULL,
    [EstTemporaire] bit  NOT NULL,
    [Commentaire] nvarchar(1000)  NULL
);
GO

-- Creating table 'RefCategorieMedecin'
CREATE TABLE [dbo].[RefCategorieMedecin] (
    [CategorieMedecinId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(30)  NOT NULL,
    [Libelle] nvarchar(100)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefRegimeEffectif'
CREATE TABLE [dbo].[RefRegimeEffectif] (
    [RegimeEffectifId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(20)  NOT NULL,
    [Libelle] nvarchar(100)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefSpecialite'
CREATE TABLE [dbo].[RefSpecialite] (
    [SpecialiteId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(30)  NOT NULL,
    [Libelle] nvarchar(150)  NOT NULL,
    [CategorieMedecinId] int  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefStatutDossierMedecin'
CREATE TABLE [dbo].[RefStatutDossierMedecin] (
    [StatutDossierMedecinId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(30)  NOT NULL,
    [Libelle] nvarchar(100)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefStatutNomination'
CREATE TABLE [dbo].[RefStatutNomination] (
    [StatutNominationId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(30)  NOT NULL,
    [Libelle] nvarchar(100)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefStatutPrivilege'
CREATE TABLE [dbo].[RefStatutPrivilege] (
    [StatutPrivilegeId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(30)  NOT NULL,
    [Libelle] nvarchar(100)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefTypeComite'
CREATE TABLE [dbo].[RefTypeComite] (
    [TypeComiteId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(30)  NOT NULL,
    [Libelle] nvarchar(150)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefTypeConge'
CREATE TABLE [dbo].[RefTypeConge] (
    [TypeCongeId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(30)  NOT NULL,
    [Libelle] nvarchar(150)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefTypeCotisation'
CREATE TABLE [dbo].[RefTypeCotisation] (
    [TypeCotisationId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(30)  NOT NULL,
    [Libelle] nvarchar(150)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefTypeDecision'
CREATE TABLE [dbo].[RefTypeDecision] (
    [TypeDecisionId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(40)  NOT NULL,
    [Libelle] nvarchar(150)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefTypeDocument'
CREATE TABLE [dbo].[RefTypeDocument] (
    [TypeDocumentId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(40)  NOT NULL,
    [Libelle] nvarchar(150)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefTypeMouvementCarriere'
CREATE TABLE [dbo].[RefTypeMouvementCarriere] (
    [TypeMouvementCarriereId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(40)  NOT NULL,
    [Libelle] nvarchar(150)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefTypeNomination'
CREATE TABLE [dbo].[RefTypeNomination] (
    [TypeNominationId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(30)  NOT NULL,
    [Libelle] nvarchar(100)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefTypeObligation'
CREATE TABLE [dbo].[RefTypeObligation] (
    [TypeObligationId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(40)  NOT NULL,
    [Libelle] nvarchar(150)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefTypePratiqueReduite'
CREATE TABLE [dbo].[RefTypePratiqueReduite] (
    [TypePratiqueReduiteId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(40)  NOT NULL,
    [Libelle] nvarchar(150)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RefTypePrivilege'
CREATE TABLE [dbo].[RefTypePrivilege] (
    [TypePrivilegeId] int IDENTITY(1,1) NOT NULL,
    [Code] varchar(40)  NOT NULL,
    [Libelle] nvarchar(150)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'RemplacementsTemporaires'
CREATE TABLE [dbo].[RemplacementsTemporaires] (
    [RemplacementTemporaireId] int IDENTITY(1,1) NOT NULL,
    [MedecinRemplaceId] int  NOT NULL,
    [MedecinRemplacantId] int  NOT NULL,
    [PosteEffectifId] int  NULL,
    [CongeId] int  NULL,
    [DateDebut] datetime  NOT NULL,
    [DateFinPrevue] datetime  NULL,
    [DateFinReelle] datetime  NULL,
    [StatutRemplacement] nvarchar(30)  NOT NULL,
    [Commentaire] nvarchar(1000)  NULL
);
GO

-- Creating table 'ServicesCliniques'
CREATE TABLE [dbo].[ServicesCliniques] (
    [ServiceCliniqueId] int IDENTITY(1,1) NOT NULL,
    [DepartementId] int  NOT NULL,
    [Code] varchar(30)  NOT NULL,
    [NomService] nvarchar(200)  NOT NULL,
    [EstActif] bit  NOT NULL
);
GO

-- Creating table 'v_AlertesRenouvellement'
CREATE TABLE [dbo].[v_AlertesRenouvellement] (
    [MedecinId] int  NOT NULL,
    [NumeroRegistreInterne] varchar(30)  NOT NULL,
    [Nom] nvarchar(100)  NOT NULL,
    [Prenom] nvarchar(100)  NOT NULL,
    [TypeAlerte] varchar(10)  NOT NULL,
    [DateReference] datetime  NULL,
    [JoursRestants] int  NULL
);
GO

-- Creating table 'v_NominationsActives'
CREATE TABLE [dbo].[v_NominationsActives] (
    [NominationId] int  NOT NULL,
    [MedecinId] int  NOT NULL,
    [Nom] nvarchar(100)  NOT NULL,
    [Prenom] nvarchar(100)  NOT NULL,
    [TypeNomination] nvarchar(100)  NOT NULL,
    [StatutNomination] nvarchar(100)  NOT NULL,
    [DateDemande] datetime  NOT NULL,
    [DateRecommendationCMDP] datetime  NULL,
    [DateDecisionCA] datetime  NULL,
    [DateEffet] datetime  NULL,
    [DateEcheance] datetime  NULL
);
GO

-- Creating table 'v_OccupationPEM'
CREATE TABLE [dbo].[v_OccupationPEM] (
    [PlanEffectifId] int  NOT NULL,
    [Regime] varchar(20)  NOT NULL,
    [AnneeExercice] int  NOT NULL,
    [NomInstallation] nvarchar(200)  NULL,
    [NomDepartement] nvarchar(200)  NULL,
    [Specialite] nvarchar(150)  NULL,
    [NbPostesAutorises] int  NOT NULL,
    [NbPostesOccupesReels] int  NULL
);
GO

-- Creating table 'v_PrivilegesActifs'
CREATE TABLE [dbo].[v_PrivilegesActifs] (
    [PrivilegeId] int  NOT NULL,
    [MedecinId] int  NOT NULL,
    [Nom] nvarchar(100)  NOT NULL,
    [Prenom] nvarchar(100)  NOT NULL,
    [TypePrivilege] nvarchar(150)  NOT NULL,
    [StatutPrivilege] nvarchar(100)  NOT NULL,
    [LibelleCourt] nvarchar(150)  NOT NULL,
    [DateDebut] datetime  NOT NULL,
    [DateFin] datetime  NULL,
    [NomInstallation] nvarchar(200)  NULL,
    [NomDepartement] nvarchar(200)  NULL,
    [NomService] nvarchar(200)  NULL
);
GO

-- Creating table 'v_RegistreMedecins'
CREATE TABLE [dbo].[v_RegistreMedecins] (
    [MedecinId] int  NOT NULL,
    [NumeroRegistreInterne] varchar(30)  NOT NULL,
    [NumeroPermisCMQ] varchar(30)  NULL,
    [Nom] nvarchar(100)  NOT NULL,
    [Prenom] nvarchar(100)  NOT NULL,
    [CategorieMedecin] nvarchar(100)  NOT NULL,
    [Specialite] nvarchar(150)  NULL,
    [StatutDossier] nvarchar(100)  NOT NULL,
    [CourrielProfessionnel] nvarchar(200)  NULL,
    [Telephone] nvarchar(30)  NULL,
    [EstActif] bit  NOT NULL,
    [DateDebutRelation] datetime  NULL,
    [DateFinRelation] datetime  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [AffectationEffectifId] in table 'AffectationsEffectifs'
ALTER TABLE [dbo].[AffectationsEffectifs]
ADD CONSTRAINT [PK_AffectationsEffectifs]
    PRIMARY KEY CLUSTERED ([AffectationEffectifId] ASC);
GO

-- Creating primary key on [AlerteDossierId] in table 'AlertesDossier'
ALTER TABLE [dbo].[AlertesDossier]
ADD CONSTRAINT [PK_AlertesDossier]
    PRIMARY KEY CLUSTERED ([AlerteDossierId] ASC);
GO

-- Creating primary key on [ComiteId] in table 'Comites'
ALTER TABLE [dbo].[Comites]
ADD CONSTRAINT [PK_Comites]
    PRIMARY KEY CLUSTERED ([ComiteId] ASC);
GO

-- Creating primary key on [CongeId] in table 'CongesMedecins'
ALTER TABLE [dbo].[CongesMedecins]
ADD CONSTRAINT [PK_CongesMedecins]
    PRIMARY KEY CLUSTERED ([CongeId] ASC);
GO

-- Creating primary key on [CotisationId] in table 'CotisationsMedecins'
ALTER TABLE [dbo].[CotisationsMedecins]
ADD CONSTRAINT [PK_CotisationsMedecins]
    PRIMARY KEY CLUSTERED ([CotisationId] ASC);
GO

-- Creating primary key on [DecisionWorkflowId] in table 'DecisionsWorkflow'
ALTER TABLE [dbo].[DecisionsWorkflow]
ADD CONSTRAINT [PK_DecisionsWorkflow]
    PRIMARY KEY CLUSTERED ([DecisionWorkflowId] ASC);
GO

-- Creating primary key on [DepartementId] in table 'DepartementsCliniques'
ALTER TABLE [dbo].[DepartementsCliniques]
ADD CONSTRAINT [PK_DepartementsCliniques]
    PRIMARY KEY CLUSTERED ([DepartementId] ASC);
GO

-- Creating primary key on [DocumentMedecinId] in table 'DocumentsMedecins'
ALTER TABLE [dbo].[DocumentsMedecins]
ADD CONSTRAINT [PK_DocumentsMedecins]
    PRIMARY KEY CLUSTERED ([DocumentMedecinId] ASC);
GO

-- Creating primary key on [InstallationId] in table 'Installations'
ALTER TABLE [dbo].[Installations]
ADD CONSTRAINT [PK_Installations]
    PRIMARY KEY CLUSTERED ([InstallationId] ASC);
GO

-- Creating primary key on [JournalAuditId] in table 'JournalAudit'
ALTER TABLE [dbo].[JournalAudit]
ADD CONSTRAINT [PK_JournalAudit]
    PRIMARY KEY CLUSTERED ([JournalAuditId] ASC);
GO

-- Creating primary key on [MedecinInstallationId] in table 'MedecinInstallations'
ALTER TABLE [dbo].[MedecinInstallations]
ADD CONSTRAINT [PK_MedecinInstallations]
    PRIMARY KEY CLUSTERED ([MedecinInstallationId] ASC);
GO

-- Creating primary key on [MedecinId] in table 'Medecins'
ALTER TABLE [dbo].[Medecins]
ADD CONSTRAINT [PK_Medecins]
    PRIMARY KEY CLUSTERED ([MedecinId] ASC);
GO

-- Creating primary key on [MembreComiteId] in table 'MembresComites'
ALTER TABLE [dbo].[MembresComites]
ADD CONSTRAINT [PK_MembresComites]
    PRIMARY KEY CLUSTERED ([MembreComiteId] ASC);
GO

-- Creating primary key on [MouvementCarriereId] in table 'MouvementsCarriere'
ALTER TABLE [dbo].[MouvementsCarriere]
ADD CONSTRAINT [PK_MouvementsCarriere]
    PRIMARY KEY CLUSTERED ([MouvementCarriereId] ASC);
GO

-- Creating primary key on [NominationId] in table 'NominationsMedecins'
ALTER TABLE [dbo].[NominationsMedecins]
ADD CONSTRAINT [PK_NominationsMedecins]
    PRIMARY KEY CLUSTERED ([NominationId] ASC);
GO

-- Creating primary key on [PlanEffectifId] in table 'PlansEffectifs'
ALTER TABLE [dbo].[PlansEffectifs]
ADD CONSTRAINT [PK_PlansEffectifs]
    PRIMARY KEY CLUSTERED ([PlanEffectifId] ASC);
GO

-- Creating primary key on [PosteEffectifId] in table 'PostesEffectifs'
ALTER TABLE [dbo].[PostesEffectifs]
ADD CONSTRAINT [PK_PostesEffectifs]
    PRIMARY KEY CLUSTERED ([PosteEffectifId] ASC);
GO

-- Creating primary key on [PratiqueReduiteId] in table 'PratiquesReduites'
ALTER TABLE [dbo].[PratiquesReduites]
ADD CONSTRAINT [PK_PratiquesReduites]
    PRIMARY KEY CLUSTERED ([PratiqueReduiteId] ASC);
GO

-- Creating primary key on [PrivilegeObligationId] in table 'PrivilegeObligations'
ALTER TABLE [dbo].[PrivilegeObligations]
ADD CONSTRAINT [PK_PrivilegeObligations]
    PRIMARY KEY CLUSTERED ([PrivilegeObligationId] ASC);
GO

-- Creating primary key on [PrivilegeId] in table 'PrivilegesMedecins'
ALTER TABLE [dbo].[PrivilegesMedecins]
ADD CONSTRAINT [PK_PrivilegesMedecins]
    PRIMARY KEY CLUSTERED ([PrivilegeId] ASC);
GO

-- Creating primary key on [CategorieMedecinId] in table 'RefCategorieMedecin'
ALTER TABLE [dbo].[RefCategorieMedecin]
ADD CONSTRAINT [PK_RefCategorieMedecin]
    PRIMARY KEY CLUSTERED ([CategorieMedecinId] ASC);
GO

-- Creating primary key on [RegimeEffectifId] in table 'RefRegimeEffectif'
ALTER TABLE [dbo].[RefRegimeEffectif]
ADD CONSTRAINT [PK_RefRegimeEffectif]
    PRIMARY KEY CLUSTERED ([RegimeEffectifId] ASC);
GO

-- Creating primary key on [SpecialiteId] in table 'RefSpecialite'
ALTER TABLE [dbo].[RefSpecialite]
ADD CONSTRAINT [PK_RefSpecialite]
    PRIMARY KEY CLUSTERED ([SpecialiteId] ASC);
GO

-- Creating primary key on [StatutDossierMedecinId] in table 'RefStatutDossierMedecin'
ALTER TABLE [dbo].[RefStatutDossierMedecin]
ADD CONSTRAINT [PK_RefStatutDossierMedecin]
    PRIMARY KEY CLUSTERED ([StatutDossierMedecinId] ASC);
GO

-- Creating primary key on [StatutNominationId] in table 'RefStatutNomination'
ALTER TABLE [dbo].[RefStatutNomination]
ADD CONSTRAINT [PK_RefStatutNomination]
    PRIMARY KEY CLUSTERED ([StatutNominationId] ASC);
GO

-- Creating primary key on [StatutPrivilegeId] in table 'RefStatutPrivilege'
ALTER TABLE [dbo].[RefStatutPrivilege]
ADD CONSTRAINT [PK_RefStatutPrivilege]
    PRIMARY KEY CLUSTERED ([StatutPrivilegeId] ASC);
GO

-- Creating primary key on [TypeComiteId] in table 'RefTypeComite'
ALTER TABLE [dbo].[RefTypeComite]
ADD CONSTRAINT [PK_RefTypeComite]
    PRIMARY KEY CLUSTERED ([TypeComiteId] ASC);
GO

-- Creating primary key on [TypeCongeId] in table 'RefTypeConge'
ALTER TABLE [dbo].[RefTypeConge]
ADD CONSTRAINT [PK_RefTypeConge]
    PRIMARY KEY CLUSTERED ([TypeCongeId] ASC);
GO

-- Creating primary key on [TypeCotisationId] in table 'RefTypeCotisation'
ALTER TABLE [dbo].[RefTypeCotisation]
ADD CONSTRAINT [PK_RefTypeCotisation]
    PRIMARY KEY CLUSTERED ([TypeCotisationId] ASC);
GO

-- Creating primary key on [TypeDecisionId] in table 'RefTypeDecision'
ALTER TABLE [dbo].[RefTypeDecision]
ADD CONSTRAINT [PK_RefTypeDecision]
    PRIMARY KEY CLUSTERED ([TypeDecisionId] ASC);
GO

-- Creating primary key on [TypeDocumentId] in table 'RefTypeDocument'
ALTER TABLE [dbo].[RefTypeDocument]
ADD CONSTRAINT [PK_RefTypeDocument]
    PRIMARY KEY CLUSTERED ([TypeDocumentId] ASC);
GO

-- Creating primary key on [TypeMouvementCarriereId] in table 'RefTypeMouvementCarriere'
ALTER TABLE [dbo].[RefTypeMouvementCarriere]
ADD CONSTRAINT [PK_RefTypeMouvementCarriere]
    PRIMARY KEY CLUSTERED ([TypeMouvementCarriereId] ASC);
GO

-- Creating primary key on [TypeNominationId] in table 'RefTypeNomination'
ALTER TABLE [dbo].[RefTypeNomination]
ADD CONSTRAINT [PK_RefTypeNomination]
    PRIMARY KEY CLUSTERED ([TypeNominationId] ASC);
GO

-- Creating primary key on [TypeObligationId] in table 'RefTypeObligation'
ALTER TABLE [dbo].[RefTypeObligation]
ADD CONSTRAINT [PK_RefTypeObligation]
    PRIMARY KEY CLUSTERED ([TypeObligationId] ASC);
GO

-- Creating primary key on [TypePratiqueReduiteId] in table 'RefTypePratiqueReduite'
ALTER TABLE [dbo].[RefTypePratiqueReduite]
ADD CONSTRAINT [PK_RefTypePratiqueReduite]
    PRIMARY KEY CLUSTERED ([TypePratiqueReduiteId] ASC);
GO

-- Creating primary key on [TypePrivilegeId] in table 'RefTypePrivilege'
ALTER TABLE [dbo].[RefTypePrivilege]
ADD CONSTRAINT [PK_RefTypePrivilege]
    PRIMARY KEY CLUSTERED ([TypePrivilegeId] ASC);
GO

-- Creating primary key on [RemplacementTemporaireId] in table 'RemplacementsTemporaires'
ALTER TABLE [dbo].[RemplacementsTemporaires]
ADD CONSTRAINT [PK_RemplacementsTemporaires]
    PRIMARY KEY CLUSTERED ([RemplacementTemporaireId] ASC);
GO

-- Creating primary key on [ServiceCliniqueId] in table 'ServicesCliniques'
ALTER TABLE [dbo].[ServicesCliniques]
ADD CONSTRAINT [PK_ServicesCliniques]
    PRIMARY KEY CLUSTERED ([ServiceCliniqueId] ASC);
GO

-- Creating primary key on [MedecinId], [NumeroRegistreInterne], [Nom], [Prenom], [TypeAlerte] in table 'v_AlertesRenouvellement'
ALTER TABLE [dbo].[v_AlertesRenouvellement]
ADD CONSTRAINT [PK_v_AlertesRenouvellement]
    PRIMARY KEY CLUSTERED ([MedecinId], [NumeroRegistreInterne], [Nom], [Prenom], [TypeAlerte] ASC);
GO

-- Creating primary key on [NominationId], [MedecinId], [Nom], [Prenom], [TypeNomination], [StatutNomination], [DateDemande] in table 'v_NominationsActives'
ALTER TABLE [dbo].[v_NominationsActives]
ADD CONSTRAINT [PK_v_NominationsActives]
    PRIMARY KEY CLUSTERED ([NominationId], [MedecinId], [Nom], [Prenom], [TypeNomination], [StatutNomination], [DateDemande] ASC);
GO

-- Creating primary key on [PlanEffectifId], [Regime], [AnneeExercice], [NbPostesAutorises] in table 'v_OccupationPEM'
ALTER TABLE [dbo].[v_OccupationPEM]
ADD CONSTRAINT [PK_v_OccupationPEM]
    PRIMARY KEY CLUSTERED ([PlanEffectifId], [Regime], [AnneeExercice], [NbPostesAutorises] ASC);
GO

-- Creating primary key on [PrivilegeId], [MedecinId], [Nom], [Prenom], [TypePrivilege], [StatutPrivilege], [LibelleCourt], [DateDebut] in table 'v_PrivilegesActifs'
ALTER TABLE [dbo].[v_PrivilegesActifs]
ADD CONSTRAINT [PK_v_PrivilegesActifs]
    PRIMARY KEY CLUSTERED ([PrivilegeId], [MedecinId], [Nom], [Prenom], [TypePrivilege], [StatutPrivilege], [LibelleCourt], [DateDebut] ASC);
GO

-- Creating primary key on [MedecinId], [NumeroRegistreInterne], [Nom], [Prenom], [CategorieMedecin], [StatutDossier], [EstActif] in table 'v_RegistreMedecins'
ALTER TABLE [dbo].[v_RegistreMedecins]
ADD CONSTRAINT [PK_v_RegistreMedecins]
    PRIMARY KEY CLUSTERED ([MedecinId], [NumeroRegistreInterne], [Nom], [Prenom], [CategorieMedecin], [StatutDossier], [EstActif] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [MedecinId] in table 'AffectationsEffectifs'
ALTER TABLE [dbo].[AffectationsEffectifs]
ADD CONSTRAINT [FK_AffectationsEffectifs_Medecin]
    FOREIGN KEY ([MedecinId])
    REFERENCES [dbo].[Medecins]
        ([MedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AffectationsEffectifs_Medecin'
CREATE INDEX [IX_FK_AffectationsEffectifs_Medecin]
ON [dbo].[AffectationsEffectifs]
    ([MedecinId]);
GO

-- Creating foreign key on [PosteEffectifId] in table 'AffectationsEffectifs'
ALTER TABLE [dbo].[AffectationsEffectifs]
ADD CONSTRAINT [FK_AffectationsEffectifs_Poste]
    FOREIGN KEY ([PosteEffectifId])
    REFERENCES [dbo].[PostesEffectifs]
        ([PosteEffectifId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AffectationsEffectifs_Poste'
CREATE INDEX [IX_FK_AffectationsEffectifs_Poste]
ON [dbo].[AffectationsEffectifs]
    ([PosteEffectifId]);
GO

-- Creating foreign key on [MedecinId] in table 'AlertesDossier'
ALTER TABLE [dbo].[AlertesDossier]
ADD CONSTRAINT [FK_AlertesDossier_Medecin]
    FOREIGN KEY ([MedecinId])
    REFERENCES [dbo].[Medecins]
        ([MedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AlertesDossier_Medecin'
CREATE INDEX [IX_FK_AlertesDossier_Medecin]
ON [dbo].[AlertesDossier]
    ([MedecinId]);
GO

-- Creating foreign key on [DepartementId] in table 'Comites'
ALTER TABLE [dbo].[Comites]
ADD CONSTRAINT [FK_Comites_Departement]
    FOREIGN KEY ([DepartementId])
    REFERENCES [dbo].[DepartementsCliniques]
        ([DepartementId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Comites_Departement'
CREATE INDEX [IX_FK_Comites_Departement]
ON [dbo].[Comites]
    ([DepartementId]);
GO

-- Creating foreign key on [InstallationId] in table 'Comites'
ALTER TABLE [dbo].[Comites]
ADD CONSTRAINT [FK_Comites_Installation]
    FOREIGN KEY ([InstallationId])
    REFERENCES [dbo].[Installations]
        ([InstallationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Comites_Installation'
CREATE INDEX [IX_FK_Comites_Installation]
ON [dbo].[Comites]
    ([InstallationId]);
GO

-- Creating foreign key on [TypeComiteId] in table 'Comites'
ALTER TABLE [dbo].[Comites]
ADD CONSTRAINT [FK_Comites_TypeComite]
    FOREIGN KEY ([TypeComiteId])
    REFERENCES [dbo].[RefTypeComite]
        ([TypeComiteId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Comites_TypeComite'
CREATE INDEX [IX_FK_Comites_TypeComite]
ON [dbo].[Comites]
    ([TypeComiteId]);
GO

-- Creating foreign key on [ComiteId] in table 'MembresComites'
ALTER TABLE [dbo].[MembresComites]
ADD CONSTRAINT [FK_MembresComites_Comite]
    FOREIGN KEY ([ComiteId])
    REFERENCES [dbo].[Comites]
        ([ComiteId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MembresComites_Comite'
CREATE INDEX [IX_FK_MembresComites_Comite]
ON [dbo].[MembresComites]
    ([ComiteId]);
GO

-- Creating foreign key on [MedecinId] in table 'CongesMedecins'
ALTER TABLE [dbo].[CongesMedecins]
ADD CONSTRAINT [FK_Conges_Medecin]
    FOREIGN KEY ([MedecinId])
    REFERENCES [dbo].[Medecins]
        ([MedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Conges_Medecin'
CREATE INDEX [IX_FK_Conges_Medecin]
ON [dbo].[CongesMedecins]
    ([MedecinId]);
GO

-- Creating foreign key on [TypeCongeId] in table 'CongesMedecins'
ALTER TABLE [dbo].[CongesMedecins]
ADD CONSTRAINT [FK_Conges_TypeConge]
    FOREIGN KEY ([TypeCongeId])
    REFERENCES [dbo].[RefTypeConge]
        ([TypeCongeId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Conges_TypeConge'
CREATE INDEX [IX_FK_Conges_TypeConge]
ON [dbo].[CongesMedecins]
    ([TypeCongeId]);
GO

-- Creating foreign key on [CongeId] in table 'RemplacementsTemporaires'
ALTER TABLE [dbo].[RemplacementsTemporaires]
ADD CONSTRAINT [FK_Remplacements_Conge]
    FOREIGN KEY ([CongeId])
    REFERENCES [dbo].[CongesMedecins]
        ([CongeId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Remplacements_Conge'
CREATE INDEX [IX_FK_Remplacements_Conge]
ON [dbo].[RemplacementsTemporaires]
    ([CongeId]);
GO

-- Creating foreign key on [MedecinId] in table 'CotisationsMedecins'
ALTER TABLE [dbo].[CotisationsMedecins]
ADD CONSTRAINT [FK_Cotisations_Medecin]
    FOREIGN KEY ([MedecinId])
    REFERENCES [dbo].[Medecins]
        ([MedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Cotisations_Medecin'
CREATE INDEX [IX_FK_Cotisations_Medecin]
ON [dbo].[CotisationsMedecins]
    ([MedecinId]);
GO

-- Creating foreign key on [TypeCotisationId] in table 'CotisationsMedecins'
ALTER TABLE [dbo].[CotisationsMedecins]
ADD CONSTRAINT [FK_Cotisations_TypeCotisation]
    FOREIGN KEY ([TypeCotisationId])
    REFERENCES [dbo].[RefTypeCotisation]
        ([TypeCotisationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Cotisations_TypeCotisation'
CREATE INDEX [IX_FK_Cotisations_TypeCotisation]
ON [dbo].[CotisationsMedecins]
    ([TypeCotisationId]);
GO

-- Creating foreign key on [TypeDecisionId] in table 'DecisionsWorkflow'
ALTER TABLE [dbo].[DecisionsWorkflow]
ADD CONSTRAINT [FK_DecisionsWorkflow_TypeDecision]
    FOREIGN KEY ([TypeDecisionId])
    REFERENCES [dbo].[RefTypeDecision]
        ([TypeDecisionId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DecisionsWorkflow_TypeDecision'
CREATE INDEX [IX_FK_DecisionsWorkflow_TypeDecision]
ON [dbo].[DecisionsWorkflow]
    ([TypeDecisionId]);
GO

-- Creating foreign key on [InstallationId] in table 'DepartementsCliniques'
ALTER TABLE [dbo].[DepartementsCliniques]
ADD CONSTRAINT [FK_DepartementsCliniques_Installation]
    FOREIGN KEY ([InstallationId])
    REFERENCES [dbo].[Installations]
        ([InstallationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DepartementsCliniques_Installation'
CREATE INDEX [IX_FK_DepartementsCliniques_Installation]
ON [dbo].[DepartementsCliniques]
    ([InstallationId]);
GO

-- Creating foreign key on [DepartementId] in table 'NominationsMedecins'
ALTER TABLE [dbo].[NominationsMedecins]
ADD CONSTRAINT [FK_Nominations_Departement]
    FOREIGN KEY ([DepartementId])
    REFERENCES [dbo].[DepartementsCliniques]
        ([DepartementId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Nominations_Departement'
CREATE INDEX [IX_FK_Nominations_Departement]
ON [dbo].[NominationsMedecins]
    ([DepartementId]);
GO

-- Creating foreign key on [DepartementId] in table 'PlansEffectifs'
ALTER TABLE [dbo].[PlansEffectifs]
ADD CONSTRAINT [FK_PlansEffectifs_Departement]
    FOREIGN KEY ([DepartementId])
    REFERENCES [dbo].[DepartementsCliniques]
        ([DepartementId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PlansEffectifs_Departement'
CREATE INDEX [IX_FK_PlansEffectifs_Departement]
ON [dbo].[PlansEffectifs]
    ([DepartementId]);
GO

-- Creating foreign key on [DepartementId] in table 'PrivilegesMedecins'
ALTER TABLE [dbo].[PrivilegesMedecins]
ADD CONSTRAINT [FK_Privileges_Departement]
    FOREIGN KEY ([DepartementId])
    REFERENCES [dbo].[DepartementsCliniques]
        ([DepartementId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Privileges_Departement'
CREATE INDEX [IX_FK_Privileges_Departement]
ON [dbo].[PrivilegesMedecins]
    ([DepartementId]);
GO

-- Creating foreign key on [DepartementId] in table 'ServicesCliniques'
ALTER TABLE [dbo].[ServicesCliniques]
ADD CONSTRAINT [FK_ServicesCliniques_Departement]
    FOREIGN KEY ([DepartementId])
    REFERENCES [dbo].[DepartementsCliniques]
        ([DepartementId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ServicesCliniques_Departement'
CREATE INDEX [IX_FK_ServicesCliniques_Departement]
ON [dbo].[ServicesCliniques]
    ([DepartementId]);
GO

-- Creating foreign key on [MedecinId] in table 'DocumentsMedecins'
ALTER TABLE [dbo].[DocumentsMedecins]
ADD CONSTRAINT [FK_DocumentsMedecins_Medecin]
    FOREIGN KEY ([MedecinId])
    REFERENCES [dbo].[Medecins]
        ([MedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DocumentsMedecins_Medecin'
CREATE INDEX [IX_FK_DocumentsMedecins_Medecin]
ON [dbo].[DocumentsMedecins]
    ([MedecinId]);
GO

-- Creating foreign key on [TypeDocumentId] in table 'DocumentsMedecins'
ALTER TABLE [dbo].[DocumentsMedecins]
ADD CONSTRAINT [FK_DocumentsMedecins_TypeDocument]
    FOREIGN KEY ([TypeDocumentId])
    REFERENCES [dbo].[RefTypeDocument]
        ([TypeDocumentId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DocumentsMedecins_TypeDocument'
CREATE INDEX [IX_FK_DocumentsMedecins_TypeDocument]
ON [dbo].[DocumentsMedecins]
    ([TypeDocumentId]);
GO

-- Creating foreign key on [InstallationId] in table 'MedecinInstallations'
ALTER TABLE [dbo].[MedecinInstallations]
ADD CONSTRAINT [FK_MedecinInstallations_Installation]
    FOREIGN KEY ([InstallationId])
    REFERENCES [dbo].[Installations]
        ([InstallationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MedecinInstallations_Installation'
CREATE INDEX [IX_FK_MedecinInstallations_Installation]
ON [dbo].[MedecinInstallations]
    ([InstallationId]);
GO

-- Creating foreign key on [InstallationId] in table 'NominationsMedecins'
ALTER TABLE [dbo].[NominationsMedecins]
ADD CONSTRAINT [FK_Nominations_Installation]
    FOREIGN KEY ([InstallationId])
    REFERENCES [dbo].[Installations]
        ([InstallationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Nominations_Installation'
CREATE INDEX [IX_FK_Nominations_Installation]
ON [dbo].[NominationsMedecins]
    ([InstallationId]);
GO

-- Creating foreign key on [InstallationId] in table 'PlansEffectifs'
ALTER TABLE [dbo].[PlansEffectifs]
ADD CONSTRAINT [FK_PlansEffectifs_Installation]
    FOREIGN KEY ([InstallationId])
    REFERENCES [dbo].[Installations]
        ([InstallationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PlansEffectifs_Installation'
CREATE INDEX [IX_FK_PlansEffectifs_Installation]
ON [dbo].[PlansEffectifs]
    ([InstallationId]);
GO

-- Creating foreign key on [InstallationId] in table 'PrivilegesMedecins'
ALTER TABLE [dbo].[PrivilegesMedecins]
ADD CONSTRAINT [FK_Privileges_Installation]
    FOREIGN KEY ([InstallationId])
    REFERENCES [dbo].[Installations]
        ([InstallationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Privileges_Installation'
CREATE INDEX [IX_FK_Privileges_Installation]
ON [dbo].[PrivilegesMedecins]
    ([InstallationId]);
GO

-- Creating foreign key on [MedecinId] in table 'MedecinInstallations'
ALTER TABLE [dbo].[MedecinInstallations]
ADD CONSTRAINT [FK_MedecinInstallations_Medecin]
    FOREIGN KEY ([MedecinId])
    REFERENCES [dbo].[Medecins]
        ([MedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MedecinInstallations_Medecin'
CREATE INDEX [IX_FK_MedecinInstallations_Medecin]
ON [dbo].[MedecinInstallations]
    ([MedecinId]);
GO

-- Creating foreign key on [CategorieMedecinId] in table 'Medecins'
ALTER TABLE [dbo].[Medecins]
ADD CONSTRAINT [FK_Medecins_Categorie]
    FOREIGN KEY ([CategorieMedecinId])
    REFERENCES [dbo].[RefCategorieMedecin]
        ([CategorieMedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Medecins_Categorie'
CREATE INDEX [IX_FK_Medecins_Categorie]
ON [dbo].[Medecins]
    ([CategorieMedecinId]);
GO

-- Creating foreign key on [SpecialiteId] in table 'Medecins'
ALTER TABLE [dbo].[Medecins]
ADD CONSTRAINT [FK_Medecins_Specialite]
    FOREIGN KEY ([SpecialiteId])
    REFERENCES [dbo].[RefSpecialite]
        ([SpecialiteId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Medecins_Specialite'
CREATE INDEX [IX_FK_Medecins_Specialite]
ON [dbo].[Medecins]
    ([SpecialiteId]);
GO

-- Creating foreign key on [StatutDossierMedecinId] in table 'Medecins'
ALTER TABLE [dbo].[Medecins]
ADD CONSTRAINT [FK_Medecins_StatutDossier]
    FOREIGN KEY ([StatutDossierMedecinId])
    REFERENCES [dbo].[RefStatutDossierMedecin]
        ([StatutDossierMedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Medecins_StatutDossier'
CREATE INDEX [IX_FK_Medecins_StatutDossier]
ON [dbo].[Medecins]
    ([StatutDossierMedecinId]);
GO

-- Creating foreign key on [MedecinId] in table 'MembresComites'
ALTER TABLE [dbo].[MembresComites]
ADD CONSTRAINT [FK_MembresComites_Medecin]
    FOREIGN KEY ([MedecinId])
    REFERENCES [dbo].[Medecins]
        ([MedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MembresComites_Medecin'
CREATE INDEX [IX_FK_MembresComites_Medecin]
ON [dbo].[MembresComites]
    ([MedecinId]);
GO

-- Creating foreign key on [MedecinId] in table 'MouvementsCarriere'
ALTER TABLE [dbo].[MouvementsCarriere]
ADD CONSTRAINT [FK_MouvementsCarriere_Medecin]
    FOREIGN KEY ([MedecinId])
    REFERENCES [dbo].[Medecins]
        ([MedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MouvementsCarriere_Medecin'
CREATE INDEX [IX_FK_MouvementsCarriere_Medecin]
ON [dbo].[MouvementsCarriere]
    ([MedecinId]);
GO

-- Creating foreign key on [MedecinId] in table 'NominationsMedecins'
ALTER TABLE [dbo].[NominationsMedecins]
ADD CONSTRAINT [FK_Nominations_Medecin]
    FOREIGN KEY ([MedecinId])
    REFERENCES [dbo].[Medecins]
        ([MedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Nominations_Medecin'
CREATE INDEX [IX_FK_Nominations_Medecin]
ON [dbo].[NominationsMedecins]
    ([MedecinId]);
GO

-- Creating foreign key on [MedecinId] in table 'PratiquesReduites'
ALTER TABLE [dbo].[PratiquesReduites]
ADD CONSTRAINT [FK_PratiquesReduites_Medecin]
    FOREIGN KEY ([MedecinId])
    REFERENCES [dbo].[Medecins]
        ([MedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PratiquesReduites_Medecin'
CREATE INDEX [IX_FK_PratiquesReduites_Medecin]
ON [dbo].[PratiquesReduites]
    ([MedecinId]);
GO

-- Creating foreign key on [MedecinId] in table 'PrivilegesMedecins'
ALTER TABLE [dbo].[PrivilegesMedecins]
ADD CONSTRAINT [FK_Privileges_Medecin]
    FOREIGN KEY ([MedecinId])
    REFERENCES [dbo].[Medecins]
        ([MedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Privileges_Medecin'
CREATE INDEX [IX_FK_Privileges_Medecin]
ON [dbo].[PrivilegesMedecins]
    ([MedecinId]);
GO

-- Creating foreign key on [MedecinRemplacantId] in table 'RemplacementsTemporaires'
ALTER TABLE [dbo].[RemplacementsTemporaires]
ADD CONSTRAINT [FK_Remplacements_MedecinRemplacant]
    FOREIGN KEY ([MedecinRemplacantId])
    REFERENCES [dbo].[Medecins]
        ([MedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Remplacements_MedecinRemplacant'
CREATE INDEX [IX_FK_Remplacements_MedecinRemplacant]
ON [dbo].[RemplacementsTemporaires]
    ([MedecinRemplacantId]);
GO

-- Creating foreign key on [MedecinRemplaceId] in table 'RemplacementsTemporaires'
ALTER TABLE [dbo].[RemplacementsTemporaires]
ADD CONSTRAINT [FK_Remplacements_MedecinRemplace]
    FOREIGN KEY ([MedecinRemplaceId])
    REFERENCES [dbo].[Medecins]
        ([MedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Remplacements_MedecinRemplace'
CREATE INDEX [IX_FK_Remplacements_MedecinRemplace]
ON [dbo].[RemplacementsTemporaires]
    ([MedecinRemplaceId]);
GO

-- Creating foreign key on [TypeMouvementCarriereId] in table 'MouvementsCarriere'
ALTER TABLE [dbo].[MouvementsCarriere]
ADD CONSTRAINT [FK_MouvementsCarriere_Type]
    FOREIGN KEY ([TypeMouvementCarriereId])
    REFERENCES [dbo].[RefTypeMouvementCarriere]
        ([TypeMouvementCarriereId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MouvementsCarriere_Type'
CREATE INDEX [IX_FK_MouvementsCarriere_Type]
ON [dbo].[MouvementsCarriere]
    ([TypeMouvementCarriereId]);
GO

-- Creating foreign key on [ServiceCliniqueId] in table 'NominationsMedecins'
ALTER TABLE [dbo].[NominationsMedecins]
ADD CONSTRAINT [FK_Nominations_Service]
    FOREIGN KEY ([ServiceCliniqueId])
    REFERENCES [dbo].[ServicesCliniques]
        ([ServiceCliniqueId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Nominations_Service'
CREATE INDEX [IX_FK_Nominations_Service]
ON [dbo].[NominationsMedecins]
    ([ServiceCliniqueId]);
GO

-- Creating foreign key on [StatutNominationId] in table 'NominationsMedecins'
ALTER TABLE [dbo].[NominationsMedecins]
ADD CONSTRAINT [FK_Nominations_StatutNomination]
    FOREIGN KEY ([StatutNominationId])
    REFERENCES [dbo].[RefStatutNomination]
        ([StatutNominationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Nominations_StatutNomination'
CREATE INDEX [IX_FK_Nominations_StatutNomination]
ON [dbo].[NominationsMedecins]
    ([StatutNominationId]);
GO

-- Creating foreign key on [TypeNominationId] in table 'NominationsMedecins'
ALTER TABLE [dbo].[NominationsMedecins]
ADD CONSTRAINT [FK_Nominations_TypeNomination]
    FOREIGN KEY ([TypeNominationId])
    REFERENCES [dbo].[RefTypeNomination]
        ([TypeNominationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Nominations_TypeNomination'
CREATE INDEX [IX_FK_Nominations_TypeNomination]
ON [dbo].[NominationsMedecins]
    ([TypeNominationId]);
GO

-- Creating foreign key on [NominationId] in table 'PrivilegesMedecins'
ALTER TABLE [dbo].[PrivilegesMedecins]
ADD CONSTRAINT [FK_Privileges_Nomination]
    FOREIGN KEY ([NominationId])
    REFERENCES [dbo].[NominationsMedecins]
        ([NominationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Privileges_Nomination'
CREATE INDEX [IX_FK_Privileges_Nomination]
ON [dbo].[PrivilegesMedecins]
    ([NominationId]);
GO

-- Creating foreign key on [RegimeEffectifId] in table 'PlansEffectifs'
ALTER TABLE [dbo].[PlansEffectifs]
ADD CONSTRAINT [FK_PlansEffectifs_Regime]
    FOREIGN KEY ([RegimeEffectifId])
    REFERENCES [dbo].[RefRegimeEffectif]
        ([RegimeEffectifId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PlansEffectifs_Regime'
CREATE INDEX [IX_FK_PlansEffectifs_Regime]
ON [dbo].[PlansEffectifs]
    ([RegimeEffectifId]);
GO

-- Creating foreign key on [ServiceCliniqueId] in table 'PlansEffectifs'
ALTER TABLE [dbo].[PlansEffectifs]
ADD CONSTRAINT [FK_PlansEffectifs_Service]
    FOREIGN KEY ([ServiceCliniqueId])
    REFERENCES [dbo].[ServicesCliniques]
        ([ServiceCliniqueId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PlansEffectifs_Service'
CREATE INDEX [IX_FK_PlansEffectifs_Service]
ON [dbo].[PlansEffectifs]
    ([ServiceCliniqueId]);
GO

-- Creating foreign key on [SpecialiteId] in table 'PlansEffectifs'
ALTER TABLE [dbo].[PlansEffectifs]
ADD CONSTRAINT [FK_PlansEffectifs_Specialite]
    FOREIGN KEY ([SpecialiteId])
    REFERENCES [dbo].[RefSpecialite]
        ([SpecialiteId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PlansEffectifs_Specialite'
CREATE INDEX [IX_FK_PlansEffectifs_Specialite]
ON [dbo].[PlansEffectifs]
    ([SpecialiteId]);
GO

-- Creating foreign key on [PlanEffectifId] in table 'PostesEffectifs'
ALTER TABLE [dbo].[PostesEffectifs]
ADD CONSTRAINT [FK_PostesEffectifs_Plan]
    FOREIGN KEY ([PlanEffectifId])
    REFERENCES [dbo].[PlansEffectifs]
        ([PlanEffectifId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PostesEffectifs_Plan'
CREATE INDEX [IX_FK_PostesEffectifs_Plan]
ON [dbo].[PostesEffectifs]
    ([PlanEffectifId]);
GO

-- Creating foreign key on [PosteEffectifId] in table 'RemplacementsTemporaires'
ALTER TABLE [dbo].[RemplacementsTemporaires]
ADD CONSTRAINT [FK_Remplacements_Poste]
    FOREIGN KEY ([PosteEffectifId])
    REFERENCES [dbo].[PostesEffectifs]
        ([PosteEffectifId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Remplacements_Poste'
CREATE INDEX [IX_FK_Remplacements_Poste]
ON [dbo].[RemplacementsTemporaires]
    ([PosteEffectifId]);
GO

-- Creating foreign key on [TypePratiqueReduiteId] in table 'PratiquesReduites'
ALTER TABLE [dbo].[PratiquesReduites]
ADD CONSTRAINT [FK_PratiquesReduites_Type]
    FOREIGN KEY ([TypePratiqueReduiteId])
    REFERENCES [dbo].[RefTypePratiqueReduite]
        ([TypePratiqueReduiteId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PratiquesReduites_Type'
CREATE INDEX [IX_FK_PratiquesReduites_Type]
ON [dbo].[PratiquesReduites]
    ([TypePratiqueReduiteId]);
GO

-- Creating foreign key on [PrivilegeId] in table 'PrivilegeObligations'
ALTER TABLE [dbo].[PrivilegeObligations]
ADD CONSTRAINT [FK_PrivilegeObligations_Privilege]
    FOREIGN KEY ([PrivilegeId])
    REFERENCES [dbo].[PrivilegesMedecins]
        ([PrivilegeId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PrivilegeObligations_Privilege'
CREATE INDEX [IX_FK_PrivilegeObligations_Privilege]
ON [dbo].[PrivilegeObligations]
    ([PrivilegeId]);
GO

-- Creating foreign key on [TypeObligationId] in table 'PrivilegeObligations'
ALTER TABLE [dbo].[PrivilegeObligations]
ADD CONSTRAINT [FK_PrivilegeObligations_TypeObligation]
    FOREIGN KEY ([TypeObligationId])
    REFERENCES [dbo].[RefTypeObligation]
        ([TypeObligationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PrivilegeObligations_TypeObligation'
CREATE INDEX [IX_FK_PrivilegeObligations_TypeObligation]
ON [dbo].[PrivilegeObligations]
    ([TypeObligationId]);
GO

-- Creating foreign key on [ServiceCliniqueId] in table 'PrivilegesMedecins'
ALTER TABLE [dbo].[PrivilegesMedecins]
ADD CONSTRAINT [FK_Privileges_Service]
    FOREIGN KEY ([ServiceCliniqueId])
    REFERENCES [dbo].[ServicesCliniques]
        ([ServiceCliniqueId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Privileges_Service'
CREATE INDEX [IX_FK_Privileges_Service]
ON [dbo].[PrivilegesMedecins]
    ([ServiceCliniqueId]);
GO

-- Creating foreign key on [StatutPrivilegeId] in table 'PrivilegesMedecins'
ALTER TABLE [dbo].[PrivilegesMedecins]
ADD CONSTRAINT [FK_Privileges_StatutPrivilege]
    FOREIGN KEY ([StatutPrivilegeId])
    REFERENCES [dbo].[RefStatutPrivilege]
        ([StatutPrivilegeId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Privileges_StatutPrivilege'
CREATE INDEX [IX_FK_Privileges_StatutPrivilege]
ON [dbo].[PrivilegesMedecins]
    ([StatutPrivilegeId]);
GO

-- Creating foreign key on [TypePrivilegeId] in table 'PrivilegesMedecins'
ALTER TABLE [dbo].[PrivilegesMedecins]
ADD CONSTRAINT [FK_Privileges_TypePrivilege]
    FOREIGN KEY ([TypePrivilegeId])
    REFERENCES [dbo].[RefTypePrivilege]
        ([TypePrivilegeId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Privileges_TypePrivilege'
CREATE INDEX [IX_FK_Privileges_TypePrivilege]
ON [dbo].[PrivilegesMedecins]
    ([TypePrivilegeId]);
GO

-- Creating foreign key on [CategorieMedecinId] in table 'RefSpecialite'
ALTER TABLE [dbo].[RefSpecialite]
ADD CONSTRAINT [FK_RefSpecialite_Categorie]
    FOREIGN KEY ([CategorieMedecinId])
    REFERENCES [dbo].[RefCategorieMedecin]
        ([CategorieMedecinId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_RefSpecialite_Categorie'
CREATE INDEX [IX_FK_RefSpecialite_Categorie]
ON [dbo].[RefSpecialite]
    ([CategorieMedecinId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------