/*
    Référentiel des types de courriel.
    - Conserve dbo.MedecinCourriel.TypeCourriel pour la compatibilité et les anciennes valeurs.
    - Ajoute une référence normalisée nullable afin de ne perdre aucune donnée existante.
    - Migre uniquement les trois libellés reconnus, sans interpréter les anciennes valeurs.
*/

USE [RDM_CEMTL];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'RDM_CEMTL'
    THROW 50001, N'La migration doit être exécutée dans la base RDM_CEMTL.', 1;

IF OBJECT_ID(N'dbo.MedecinCourriel', N'U') IS NULL
    THROW 50002, N'La table dbo.MedecinCourriel est introuvable dans RDM_CEMTL.', 1;

IF COL_LENGTH(N'dbo.MedecinCourriel', N'TypeCourriel') IS NULL
    THROW 50003, N'La colonne historique dbo.MedecinCourriel.TypeCourriel est introuvable.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF SCHEMA_ID(N'ref') IS NULL
        EXEC(N'CREATE SCHEMA ref AUTHORIZATION dbo;');

    IF OBJECT_ID(N'ref.TypeCourriel', N'U') IS NULL
    BEGIN
        CREATE TABLE ref.TypeCourriel
        (
            TypeCourrielId int IDENTITY(1,1) NOT NULL,
            Libelle nvarchar(50) NOT NULL,
            EstActif bit NOT NULL CONSTRAINT DF_TypeCourriel_EstActif DEFAULT (1),
            CONSTRAINT PK_TypeCourriel PRIMARY KEY CLUSTERED (TypeCourrielId)
        );

        CREATE UNIQUE INDEX UX_TypeCourriel_Libelle
            ON ref.TypeCourriel(Libelle);
    END;

    IF NOT EXISTS
    (
        SELECT 1
          FROM sys.indexes
         WHERE object_id = OBJECT_ID(N'ref.TypeCourriel')
           AND name = N'UX_TypeCourriel_Libelle'
    )
        EXEC sys.sp_executesql N'
            CREATE UNIQUE INDEX UX_TypeCourriel_Libelle
                ON ref.TypeCourriel(Libelle);';

    EXEC sys.sp_executesql N'
        MERGE ref.TypeCourriel AS cible
        USING (VALUES
            (N''Personnel''),
            (N''Universitaire''),
            (N''Institutionnel'')
        ) AS source(Libelle)
            ON LTRIM(RTRIM(cible.Libelle)) COLLATE Latin1_General_100_CI_AI =
               source.Libelle COLLATE Latin1_General_100_CI_AI
        WHEN MATCHED THEN
            UPDATE SET Libelle = source.Libelle, EstActif = 1
        WHEN NOT MATCHED THEN
            INSERT (Libelle, EstActif) VALUES (source.Libelle, 1);';

    IF COL_LENGTH(N'dbo.MedecinCourriel', N'TypeCourrielId') IS NULL
        ALTER TABLE dbo.MedecinCourriel ADD TypeCourrielId int NULL;

    -- SQL dynamique obligatoire : la colonne peut avoir été ajoutée juste au-dessus.
    -- Elle doit être compilée après l'ALTER TABLE, et non au démarrage du lot.
    EXEC sys.sp_executesql N'
        UPDATE mc
           SET mc.TypeCourrielId = tc.TypeCourrielId,
               mc.TypeCourriel = tc.Libelle
          FROM dbo.MedecinCourriel AS mc
          INNER JOIN ref.TypeCourriel AS tc
            ON LTRIM(RTRIM(mc.TypeCourriel)) COLLATE Latin1_General_100_CI_AI =
               tc.Libelle COLLATE Latin1_General_100_CI_AI
         WHERE mc.TypeCourrielId IS NULL;';

    IF NOT EXISTS
    (
        SELECT 1
          FROM sys.indexes
         WHERE object_id = OBJECT_ID(N'dbo.MedecinCourriel')
           AND name = N'IX_MedecinCourriel_TypeCourrielId'
    )
        EXEC sys.sp_executesql N'
            CREATE INDEX IX_MedecinCourriel_TypeCourrielId
                ON dbo.MedecinCourriel(TypeCourrielId);';

    IF NOT EXISTS
    (
        SELECT 1
          FROM sys.foreign_keys
         WHERE parent_object_id = OBJECT_ID(N'dbo.MedecinCourriel')
           AND name = N'FK_MedecinCourriel_TypeCourriel'
    )
        EXEC sys.sp_executesql N'
            ALTER TABLE dbo.MedecinCourriel WITH CHECK
                ADD CONSTRAINT FK_MedecinCourriel_TypeCourriel
                FOREIGN KEY (TypeCourrielId)
                REFERENCES ref.TypeCourriel(TypeCourrielId);';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;

EXEC sys.sp_executesql N'
    SELECT TypeCourrielId, Libelle, EstActif
      FROM ref.TypeCourriel
     ORDER BY Libelle;

    SELECT COUNT(*) AS NombreCourrielsAnciensNonAssocies
      FROM dbo.MedecinCourriel
     WHERE TypeCourrielId IS NULL;';
