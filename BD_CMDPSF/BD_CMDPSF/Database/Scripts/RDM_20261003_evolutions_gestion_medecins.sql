/*
================================================================================
RDM_CEMTL - Évolutions fonctionnelles Gestion des médecins
Date : 2026-10-03

À exécuter APRÈS le script de structure actuel de RDM_CEMTL.
Ce script :
  1) corrige l'unicité des services par département;
  2) ajoute les affectations cliniques médecin -> département/service;
  3) ajoute la gestion documentaire du dossier médecin.

Le script est conçu pour être rejouable : les objets déjà présents sont ignorés.
================================================================================
*/
USE [RDM_CEMTL];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ============================================================================
   1. SERVICE : unicité dans un département et non globalement
   ============================================================================ */
IF EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Service')
      AND name = N'UX_Service_NomService'
)
    DROP INDEX [UX_Service_NomService] ON [dbo].[Service];
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Service')
      AND name = N'UX_Service_Departement_NomService'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Service_Departement_NomService]
    ON [dbo].[Service]([DepartementId], [NomService]);
END;
GO

/* ============================================================================
   2. AFFECTATIONS CLINIQUES
   Distinctes des responsabilités organisationnelles et des privilèges.
   ============================================================================ */
IF OBJECT_ID(N'dbo.MedecinAffectationClinique', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MedecinAffectationClinique]
    (
        [MedecinAffectationCliniqueId] int IDENTITY(1,1) NOT NULL,
        [MedecinId] int NOT NULL,
        [DepartementId] int NOT NULL,
        [ServiceId] int NOT NULL,
        [InstallationId] int NULL,
        [TypeAffectation] nvarchar(100) NULL,
        [EstPrincipale] bit NOT NULL
            CONSTRAINT [DF_MedecinAffectationClinique_EstPrincipale] DEFAULT(0),
        [DateDebut] date NULL,
        [DateFin] date NULL,
        [EstActive] bit NOT NULL
            CONSTRAINT [DF_MedecinAffectationClinique_EstActive] DEFAULT(1),
        [Commentaire] nvarchar(500) NULL,
        [SourceDonnees] nvarchar(200) NULL,
        [DateCreation] datetime2(0) NOT NULL
            CONSTRAINT [DF_MedecinAffectationClinique_DateCreation] DEFAULT(sysdatetime()),
        [DateModification] datetime2(0) NOT NULL
            CONSTRAINT [DF_MedecinAffectationClinique_DateModification] DEFAULT(sysdatetime()),

        CONSTRAINT [PK_MedecinAffectationClinique]
            PRIMARY KEY CLUSTERED ([MedecinAffectationCliniqueId]),
        CONSTRAINT [FK_MedecinAffectationClinique_Medecin]
            FOREIGN KEY([MedecinId]) REFERENCES [dbo].[Medecin]([MedecinId]),
        CONSTRAINT [FK_MedecinAffectationClinique_Departement]
            FOREIGN KEY([DepartementId]) REFERENCES [dbo].[Departement]([DepartementId]),
        CONSTRAINT [FK_MedecinAffectationClinique_Service]
            FOREIGN KEY([ServiceId]) REFERENCES [dbo].[Service]([ServiceId]),
        CONSTRAINT [FK_MedecinAffectationClinique_Installation]
            FOREIGN KEY([InstallationId]) REFERENCES [dbo].[Installation]([InstallationId]),
        CONSTRAINT [CK_MedecinAffectationClinique_Dates]
            CHECK ([DateFin] IS NULL OR [DateDebut] IS NULL OR [DateFin] >= [DateDebut])
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.MedecinAffectationClinique')
      AND name = N'IX_MedecinAffectationClinique_MedecinOrganisation'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MedecinAffectationClinique_MedecinOrganisation]
    ON [dbo].[MedecinAffectationClinique]
       ([MedecinId], [DepartementId], [ServiceId], [InstallationId], [DateDebut]);
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.MedecinAffectationClinique')
      AND name = N'UX_MedecinAffectationClinique_PrincipaleActive'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_MedecinAffectationClinique_PrincipaleActive]
    ON [dbo].[MedecinAffectationClinique]([MedecinId])
    WHERE [EstPrincipale] = 1 AND [EstActive] = 1;
END;
GO

/* ============================================================================
   3. DOCUMENTS DU MÉDECIN
   ============================================================================ */
IF OBJECT_ID(N'dbo.DocumentMedecin', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DocumentMedecin]
    (
        [DocumentMedecinId] int IDENTITY(1,1) NOT NULL,
        [MedecinId] int NOT NULL,
        [TypeDocument] nvarchar(100) NOT NULL,
        [NomFichier] nvarchar(255) NOT NULL,
        [ContentType] nvarchar(100) NOT NULL,
        [TailleOctets] bigint NOT NULL,
        [HashSHA256] varchar(64) NOT NULL,
        [DateDocument] date NULL,
        [DateExpiration] date NULL,
        [Description] nvarchar(500) NULL,
        [EntiteLiee] nvarchar(50) NULL,
        [EntiteId] int NULL,
        [Confidentiel] bit NOT NULL
            CONSTRAINT [DF_DocumentMedecin_Confidentiel] DEFAULT(0),
        [Contenu] varbinary(max) NOT NULL,
        [DateDepot] datetime2(0) NOT NULL
            CONSTRAINT [DF_DocumentMedecin_DateDepot] DEFAULT(sysutcdatetime()),
        [DeposePar] nvarchar(256) NULL,
        [EstActif] bit NOT NULL
            CONSTRAINT [DF_DocumentMedecin_EstActif] DEFAULT(1),

        CONSTRAINT [PK_DocumentMedecin]
            PRIMARY KEY CLUSTERED ([DocumentMedecinId]),
        CONSTRAINT [FK_DocumentMedecin_Medecin]
            FOREIGN KEY([MedecinId]) REFERENCES [dbo].[Medecin]([MedecinId]),
        CONSTRAINT [CK_DocumentMedecin_Dates]
            CHECK ([DateExpiration] IS NULL OR [DateDocument] IS NULL OR [DateExpiration] >= [DateDocument]),
        CONSTRAINT [CK_DocumentMedecin_Taille]
            CHECK ([TailleOctets] >= 0)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.DocumentMedecin')
      AND name = N'IX_DocumentMedecin_Medecin_Actif_Expiration'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DocumentMedecin_Medecin_Actif_Expiration]
    ON [dbo].[DocumentMedecin]([MedecinId], [EstActif], [DateExpiration]);
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.DocumentMedecin')
      AND name = N'IX_DocumentMedecin_HashSHA256'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DocumentMedecin_HashSHA256]
    ON [dbo].[DocumentMedecin]([HashSHA256]);
END;
GO

/* ============================================================================
   4. CONTRÔLES
   ============================================================================ */
SELECT
    CASE WHEN OBJECT_ID(N'dbo.MedecinAffectationClinique', N'U') IS NOT NULL THEN 1 ELSE 0 END AS MedecinAffectationClinique_OK,
    CASE WHEN OBJECT_ID(N'dbo.DocumentMedecin', N'U') IS NOT NULL THEN 1 ELSE 0 END AS DocumentMedecin_OK;
GO
