/*
    RDM - Migration structurelle des departements, services et affectations

    Ce script modifie uniquement le schema SQL Server necessaire a l'application.
    Il ne charge aucune donnee provenant du fichier Excel et ne modifie aucun
    enregistrement metier existant.

    Objets geres
    ------------
    1. Colonne dbo.Service.RegroupementSource.
    2. Table dbo.AffectationOrganisationnelle.
    3. Contraintes, cles et index associes.
    4. Vues de lecture des informations organisationnelles courantes.

    Execution
    ---------
    - Tester d'abord sur une copie de la base et prendre une sauvegarde.
    - Executer avec un compte autorise a ALTER et CREATE.
    - Le fichier est encode en UTF-8. Avec sqlcmd, utiliser -f 65001.
    - Le script est rejouable et ne cree aucun enregistrement metier.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Departement', N'U') IS NULL
BEGIN
    ;THROW 51000, N'La table dbo.Departement est absente.', 1;
END;

IF OBJECT_ID(N'dbo.Service', N'U') IS NULL
BEGIN
    ;THROW 51001, N'La table dbo.Service est absente.', 1;
END;

IF OBJECT_ID(N'dbo.Medecin', N'U') IS NULL
BEGIN
    ;THROW 51002, N'La table dbo.Medecin est absente.', 1;
END;

IF OBJECT_ID(N'dbo.Installation', N'U') IS NULL
BEGIN
    ;THROW 51003, N'La table dbo.Installation est absente.', 1;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    /* Extension du referentiel Service. */
    IF COL_LENGTH(N'dbo.Service', N'RegroupementSource') IS NULL
    BEGIN
        ALTER TABLE dbo.Service
            ADD RegroupementSource nvarchar(200) NULL;
    END;

    /*
       Les responsables sont historises dans une table distincte afin que
       plusieurs roles et plusieurs periodes puissent coexister.
    */
    IF OBJECT_ID(N'dbo.AffectationOrganisationnelle', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.AffectationOrganisationnelle
        (
            AffectationOrganisationnelleId int IDENTITY(1,1) NOT NULL,
            DepartementId int NULL,
            ServiceId int NULL,
            MedecinId int NULL,
            RoleCode varchar(40) NOT NULL,
            NomAffiche nvarchar(300) NOT NULL,
            Titre nvarchar(300) NULL,
            Courriel nvarchar(255) NULL,
            CourrielSource nvarchar(255) NULL,
            Telephone nvarchar(50) NULL,
            PosteTelephone nvarchar(20) NULL,
            InstallationId int NULL,
            SiteNomSource nvarchar(200) NULL,
            RegroupementSource nvarchar(200) NULL,
            DateDebut date NULL,
            DateFin date NULL,
            EstInterim bit NOT NULL
                CONSTRAINT DF_AffectationOrganisationnelle_EstInterim DEFAULT (0),
            EstActive bit NOT NULL
                CONSTRAINT DF_AffectationOrganisationnelle_EstActive DEFAULT (1),
            EstAValider bit NOT NULL
                CONSTRAINT DF_AffectationOrganisationnelle_EstAValider DEFAULT (0),
            SourceDonnees nvarchar(200) NULL,
            SourceLigne int NULL,
            DateCreation datetime2(0) NOT NULL
                CONSTRAINT DF_AffectationOrganisationnelle_DateCreation DEFAULT (sysutcdatetime()),
            DateModification datetime2(0) NOT NULL
                CONSTRAINT DF_AffectationOrganisationnelle_DateModification DEFAULT (sysutcdatetime()),

            CONSTRAINT PK_AffectationOrganisationnelle
                PRIMARY KEY CLUSTERED (AffectationOrganisationnelleId),

            CONSTRAINT FK_AffectationOrganisationnelle_Departement
                FOREIGN KEY (DepartementId)
                REFERENCES dbo.Departement (DepartementId),

            CONSTRAINT FK_AffectationOrganisationnelle_Service
                FOREIGN KEY (ServiceId)
                REFERENCES dbo.Service (ServiceId),

            CONSTRAINT FK_AffectationOrganisationnelle_Medecin
                FOREIGN KEY (MedecinId)
                REFERENCES dbo.Medecin (MedecinId),

            CONSTRAINT FK_AffectationOrganisationnelle_Installation
                FOREIGN KEY (InstallationId)
                REFERENCES dbo.Installation (InstallationId),

            CONSTRAINT CK_AffectationOrganisationnelle_Role
                CHECK
                (
                    RoleCode IN
                    (
                        'CHEF_DEPARTEMENT',
                        'SECRETAIRE_DEPARTEMENT',
                        'ADJOINT_SITE',
                        'CHEF_SERVICE'
                    )
                ),

            CONSTRAINT CK_AffectationOrganisationnelle_Cible
                CHECK
                (
                    (DepartementId IS NOT NULL AND ServiceId IS NULL)
                    OR
                    (DepartementId IS NULL AND ServiceId IS NOT NULL)
                ),

            CONSTRAINT CK_AffectationOrganisationnelle_RoleCible
                CHECK
                (
                    (RoleCode = 'CHEF_SERVICE' AND ServiceId IS NOT NULL)
                    OR
                    (
                        RoleCode IN
                        (
                            'CHEF_DEPARTEMENT',
                            'SECRETAIRE_DEPARTEMENT',
                            'ADJOINT_SITE'
                        )
                        AND DepartementId IS NOT NULL
                    )
                ),

            CONSTRAINT CK_AffectationOrganisationnelle_Dates
                CHECK
                (
                    DateFin IS NULL
                    OR DateDebut IS NULL
                    OR DateFin >= DateDebut
                )
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.AffectationOrganisationnelle')
          AND name = N'UX_AffectationOrganisationnelle_Source'
    )
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UX_AffectationOrganisationnelle_Source
            ON dbo.AffectationOrganisationnelle
            (
                SourceDonnees,
                SourceLigne,
                RoleCode
            )
            WHERE SourceDonnees IS NOT NULL
              AND SourceLigne IS NOT NULL;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.AffectationOrganisationnelle')
          AND name = N'IX_AffectationOrganisationnelle_Departement'
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_AffectationOrganisationnelle_Departement
            ON dbo.AffectationOrganisationnelle
            (
                DepartementId,
                RoleCode,
                EstActive,
                DateDebut
            )
            INCLUDE
            (
                NomAffiche,
                MedecinId,
                DateFin,
                RegroupementSource,
                EstAValider
            );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.AffectationOrganisationnelle')
          AND name = N'IX_AffectationOrganisationnelle_Service'
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_AffectationOrganisationnelle_Service
            ON dbo.AffectationOrganisationnelle
            (
                ServiceId,
                RoleCode,
                EstActive,
                DateDebut
            )
            INCLUDE
            (
                NomAffiche,
                MedecinId,
                Titre,
                Courriel,
                CourrielSource,
                DateFin,
                EstAValider
            );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

/* Vue des responsables courants d'un departement. */
CREATE OR ALTER VIEW dbo.vw_DepartementInformationCourante
AS
SELECT
    d.DepartementId,
    d.NomDepartement,
    d.CodeDepartement,
    d.EstActif,

    Chef.MedecinId AS ChefDepartementMedecinId,
    Chef.NomAffiche AS ChefDepartement,
    Chef.DateDebut AS DateNominationChefDepartement,
    Chef.EstInterim AS ChefDepartementEstInterim,
    Chef.EstAValider AS ChefDepartementEstAValider,

    Secretaire.NomAffiche AS Secretaire,
    Secretaire.Telephone AS SecretaireTelephone,
    Secretaire.PosteTelephone AS SecretairePosteTelephone,
    Secretaire.EstAValider AS SecretaireEstAValider,

    Adjoint.MedecinId AS AdjointSiteMedecinId,
    Adjoint.NomAffiche AS AdjointSite,
    Adjoint.SiteNomSource AS AdjointSiteNomInstallation,
    Adjoint.EstInterim AS AdjointSiteEstInterim,
    Adjoint.EstAValider AS AdjointSiteEstAValider,

    Validation.NombreAffectationsAValider
FROM dbo.Departement AS d
OUTER APPLY
(
    SELECT TOP (1) a.*
    FROM dbo.AffectationOrganisationnelle AS a
    WHERE a.DepartementId = d.DepartementId
      AND a.RoleCode = 'CHEF_DEPARTEMENT'
      AND a.RegroupementSource IS NULL
      AND a.EstActive = 1
      AND (a.DateDebut IS NULL OR a.DateDebut <= CONVERT(date, GETDATE()))
      AND (a.DateFin IS NULL OR a.DateFin >= CONVERT(date, GETDATE()))
    ORDER BY
        CASE WHEN a.DateDebut IS NULL THEN 1 ELSE 0 END,
        a.DateDebut DESC,
        a.AffectationOrganisationnelleId DESC
) AS Chef
OUTER APPLY
(
    SELECT TOP (1) a.*
    FROM dbo.AffectationOrganisationnelle AS a
    WHERE a.DepartementId = d.DepartementId
      AND a.RoleCode = 'SECRETAIRE_DEPARTEMENT'
      AND a.RegroupementSource IS NULL
      AND a.EstActive = 1
      AND (a.DateDebut IS NULL OR a.DateDebut <= CONVERT(date, GETDATE()))
      AND (a.DateFin IS NULL OR a.DateFin >= CONVERT(date, GETDATE()))
    ORDER BY
        CASE WHEN a.DateDebut IS NULL THEN 1 ELSE 0 END,
        a.DateDebut DESC,
        a.AffectationOrganisationnelleId DESC
) AS Secretaire
OUTER APPLY
(
    SELECT TOP (1) a.*
    FROM dbo.AffectationOrganisationnelle AS a
    WHERE a.DepartementId = d.DepartementId
      AND a.RoleCode = 'ADJOINT_SITE'
      AND a.RegroupementSource IS NULL
      AND a.EstActive = 1
      AND (a.DateDebut IS NULL OR a.DateDebut <= CONVERT(date, GETDATE()))
      AND (a.DateFin IS NULL OR a.DateFin >= CONVERT(date, GETDATE()))
    ORDER BY
        CASE WHEN a.DateDebut IS NULL THEN 1 ELSE 0 END,
        a.DateDebut DESC,
        a.AffectationOrganisationnelleId DESC
) AS Adjoint
OUTER APPLY
(
    SELECT COUNT(*) AS NombreAffectationsAValider
    FROM dbo.AffectationOrganisationnelle AS a
    WHERE a.DepartementId = d.DepartementId
      AND a.EstActive = 1
      AND a.EstAValider = 1
) AS Validation;
GO

/* Vue des responsables courants d'un service. */
CREATE OR ALTER VIEW dbo.vw_ServiceInformationCourante
AS
SELECT
    s.ServiceId,
    s.DepartementId,
    d.NomDepartement,
    s.NomService,
    s.CodeService,
    s.RegroupementSource,
    s.EstActif,

    Chef.MedecinId AS ChefServiceMedecinId,
    Chef.NomAffiche AS ChefService,
    Chef.Titre AS TitreChefService,
    Chef.Courriel AS CourrielChefServiceValide,
    Chef.CourrielSource AS CourrielChefServiceSource,
    Chef.DateDebut AS DateDebutChefService,
    Chef.EstInterim AS ChefServiceEstInterim,
    Chef.EstAValider AS ChefServiceEstAValider
FROM dbo.Service AS s
LEFT JOIN dbo.Departement AS d
    ON d.DepartementId = s.DepartementId
OUTER APPLY
(
    SELECT TOP (1) a.*
    FROM dbo.AffectationOrganisationnelle AS a
    WHERE a.ServiceId = s.ServiceId
      AND a.RoleCode = 'CHEF_SERVICE'
      AND a.EstActive = 1
      AND (a.DateDebut IS NULL OR a.DateDebut <= CONVERT(date, GETDATE()))
      AND (a.DateFin IS NULL OR a.DateFin >= CONVERT(date, GETDATE()))
    ORDER BY
        CASE WHEN a.DateDebut IS NULL THEN 1 ELSE 0 END,
        a.DateDebut DESC,
        a.AffectationOrganisationnelleId DESC
) AS Chef;
GO

/* Vue de toutes les affectations actuellement actives. */
CREATE OR ALTER VIEW dbo.vw_AffectationOrganisationnelleCourante
AS
SELECT
    a.AffectationOrganisationnelleId,
    a.RoleCode,
    a.DepartementId,
    d.NomDepartement,
    a.ServiceId,
    s.NomService,
    a.RegroupementSource,
    a.MedecinId,
    a.NomAffiche,
    a.Titre,
    a.Courriel,
    a.CourrielSource,
    a.Telephone,
    a.PosteTelephone,
    a.InstallationId,
    COALESCE(i.NomInstallation, a.SiteNomSource) AS Site,
    a.DateDebut,
    a.DateFin,
    a.EstInterim,
    a.EstAValider,
    a.SourceDonnees,
    a.SourceLigne
FROM dbo.AffectationOrganisationnelle AS a
LEFT JOIN dbo.Departement AS d
    ON d.DepartementId = a.DepartementId
LEFT JOIN dbo.Service AS s
    ON s.ServiceId = a.ServiceId
LEFT JOIN dbo.Installation AS i
    ON i.InstallationId = a.InstallationId
WHERE a.EstActive = 1
  AND (a.DateDebut IS NULL OR a.DateDebut <= CONVERT(date, GETDATE()))
  AND (a.DateFin IS NULL OR a.DateFin >= CONVERT(date, GETDATE()));
GO

PRINT N'Migration structurelle terminee. Aucune donnee metier n''a ete chargee.';
GO
