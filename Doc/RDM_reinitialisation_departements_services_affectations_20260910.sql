/*
    RDM - Reinitialisation et rechargement de l'organisation medicale
    Source : CIUSSS-EMTL_Organisation_depts_services_Nominatif_2026-09-10 (1).xlsx
    Feuille : Organisation.Dept.Serv.

    Donnees chargees
    ----------------
    - 11 departements
    - 78 services
    - 125 affectations organisationnelles

    AVERTISSEMENT
    -------------
    Ce script supprime puis recree le contenu de dbo.AffectationOrganisationnelle,
    dbo.Service et dbo.Departement. Prendre une sauvegarde et tester d'abord sur
    une copie de la base.

    Mesures de protection
    ---------------------
    - Transaction unique avec XACT_ABORT.
    - Verification du schema et des volumes sources avant toute suppression.
    - Conservation des ServiceId deja utilises par InstallationService ou
      MedecinPrivilege lorsque le libelle correspond sans ambiguite.
    - Un ancien service encore reference mais absent de la source est conserve
      avec le meme ServiceId, sans departement et avec EstActif = 0.
    - Arret avant suppression lorsqu'une cle etrangere inconnue cible les tables
      rechargees.
    - Revalidation de toutes les cles etrangeres avant la validation finale.
    - Confirmation explicite obligatoire ci-dessous avant l'execution.

    Normalisation explicite
    -----------------------
    - Les dates texte avec barres obliques sont interpretees en mm/jj/aaaa,
      car la source contient notamment 6/13/2016 et 6/14/2020.
    - (Ophtalmologie-CUO) est conserve dans RegroupementSource sous Chirurgie.
    - Grappe Optilab CHUM est traitee comme un departement; son responsable est
      conserve avec le titre Directeur medical.
    - Le service Soins intensifs apparaissant dans deux departements, les deux
      libelles sont qualifies par leur departement pour rester uniques.
    - Les lignes sans valeur metier apres la ligne 112 ne sont pas chargees.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ConfirmerReinitialisation bit = 0; -- Mettre a 1 seulement apres la sauvegarde.
DECLARE @SourceDonnees nvarchar(200) = N'CIUSSS-EMTL_Organisation_depts_services_Nominatif_2026-09-10 (1).xlsx';
DECLARE @NombreDepartementsAttendu int = 11;
DECLARE @NombreServicesAttendu int = 78;
DECLARE @NombreAffectationsAttendu int = 125;

IF @ConfirmerReinitialisation <> 1
BEGIN
    ;THROW 51999, N'Reinitialisation non confirmee. Mettre @ConfirmerReinitialisation a 1 apres avoir sauvegarde la base.', 1;
END;

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
BEGIN
    ;THROW 51998, N'Ce script ne peut pas etre execute dans une base systeme.', 1;
END;

IF OBJECT_ID(N'dbo.Departement', N'U') IS NULL
BEGIN
    ;THROW 52000, N'La table dbo.Departement est absente.', 1;
END;

IF OBJECT_ID(N'dbo.Service', N'U') IS NULL
BEGIN
    ;THROW 52001, N'La table dbo.Service est absente.', 1;
END;

IF OBJECT_ID(N'dbo.AffectationOrganisationnelle', N'U') IS NULL
BEGIN
    ;THROW 52002, N'La table dbo.AffectationOrganisationnelle est absente. Executer d''abord la migration structurelle.', 1;
END;

IF OBJECT_ID(N'dbo.Medecin', N'U') IS NULL
BEGIN
    ;THROW 52003, N'La table dbo.Medecin est absente.', 1;
END;

IF COL_LENGTH(N'dbo.Service', N'RegroupementSource') IS NULL
BEGIN
    ;THROW 52004, N'La colonne dbo.Service.RegroupementSource est absente. Executer d''abord la migration structurelle.', 1;
END;

CREATE TABLE #DepartementSource
(
    SourceOrdre int NOT NULL,
    SourceLigne int NOT NULL,
    NomDepartement nvarchar(200) NOT NULL,
    CodeDepartement nvarchar(50) NULL,
    DepartementId int NOT NULL,
    PRIMARY KEY (NomDepartement),
    UNIQUE (DepartementId)
);

INSERT INTO #DepartementSource
(
    SourceOrdre,
    SourceLigne,
    NomDepartement,
    CodeDepartement,
    DepartementId
)
VALUES
    (1, 3, N'Anesthésiologie', N'1', 1),
    (2, 5, N'Chirurgie', N'2', 2),
    (3, 22, N'Médecine d’urgence', N'3', 3),
    (4, 23, N'Médecine générale', N'4', 4),
    (5, 61, N'Médecine spécialisée', N'5', 5),
    (6, 87, N'Obstétrique-Gynécologique', N'6', 6),
    (7, 90, N'Pédiatrie', N'7', 7),
    (8, 93, N'Pharmacie', N'8', 8),
    (9, 94, N'Psychiatrie', N'9', 9),
    (10, 108, N'Imagerie médicale', N'10', 10),
    (11, 111, N'Grappe Optilab CHUM', NULL, 11);

CREATE TABLE #ServiceSource
(
    SourceOrdre int NOT NULL,
    SourceLigne int NOT NULL,
    NomDepartement nvarchar(200) NOT NULL,
    NomServiceSource nvarchar(250) NOT NULL,
    NomService nvarchar(250) NOT NULL,
    RegroupementSource nvarchar(200) NULL,
    ServiceId int NULL,
    PRIMARY KEY (NomService),
    UNIQUE (NomDepartement, NomService)
);

INSERT INTO #ServiceSource
(
    SourceOrdre,
    SourceLigne,
    NomDepartement,
    NomServiceSource,
    NomService,
    RegroupementSource,
    ServiceId
)
VALUES
    (1, 3, N'Anesthésiologie', N'Anesthésie', N'Anesthésie', NULL, NULL),
    (2, 4, N'Anesthésiologie', N'Clinique de gestion de la douleur', N'Clinique de gestion de la douleur', NULL, NULL),
    (3, 5, N'Chirurgie', N'Chirurgie dentaire', N'Chirurgie dentaire', NULL, NULL),
    (4, 6, N'Chirurgie', N'Chirurgie générale', N'Chirurgie générale', NULL, NULL),
    (5, 7, N'Chirurgie', N'Chirurgie plastique', N'Chirurgie plastique', NULL, NULL),
    (6, 8, N'Chirurgie', N'Chirurgie thoracique', N'Chirurgie thoracique', NULL, NULL),
    (7, 9, N'Chirurgie', N'Chirurgie vasculaire', N'Chirurgie vasculaire', NULL, NULL),
    (8, 10, N'Chirurgie', N'Neurochirurgie', N'Neurochirurgie', NULL, NULL),
    (9, 11, N'Chirurgie', N'Pathologies chirurgicales (anatomopathologie)', N'Pathologies chirurgicales (anatomopathologie)', NULL, NULL),
    (10, 12, N'Chirurgie', N'Orthopédie', N'Orthopédie', NULL, NULL),
    (11, 13, N'Chirurgie', N'Oto-rhino-laryngologie', N'Oto-rhino-laryngologie', NULL, NULL),
    (12, 14, N'Chirurgie', N'Urologie', N'Urologie', NULL, NULL),
    (13, 15, N'Chirurgie', N'Cornée et segment antérieur', N'Cornée et segment antérieur', N'(Ophtalmologie-CUO)', NULL),
    (14, 16, N'Chirurgie', N'Glaucome', N'Glaucome', N'(Ophtalmologie-CUO)', NULL),
    (15, 17, N'Chirurgie', N'Uvéite', N'Uvéite', N'(Ophtalmologie-CUO)', NULL),
    (16, 18, N'Chirurgie', N'Rétine et segment postérieur', N'Rétine et segment postérieur', N'(Ophtalmologie-CUO)', NULL),
    (17, 19, N'Chirurgie', N'Neuro-ophtalmologie et strabisme', N'Neuro-ophtalmologie et strabisme', N'(Ophtalmologie-CUO)', NULL),
    (18, 20, N'Chirurgie', N'Ophtalmologie pédiatrique', N'Ophtalmologie pédiatrique', N'(Ophtalmologie-CUO)', NULL),
    (19, 21, N'Chirurgie', N'Oculoplastie', N'Oculoplastie', N'(Ophtalmologie-CUO)', NULL),
    (20, 23, N'Médecine générale', N'médecine générale (IUSMM)', N'médecine générale (IUSMM)', NULL, NULL),
    (21, 24, N'Médecine générale', N'CLSC Saint-Léonard', N'CLSC Saint-Léonard', NULL, NULL),
    (22, 25, N'Médecine générale', N'CLSC Saint-Michel', N'CLSC Saint-Michel', NULL, NULL),
    (23, 27, N'Médecine générale', N'CLSC Olivier-Guimond', N'CLSC Olivier-Guimond', NULL, NULL),
    (24, 28, N'Médecine générale', N'CLSC et GMF-U Hochelaga-Maisonneuve', N'CLSC et GMF-U Hochelaga-Maisonneuve', NULL, NULL),
    (25, 30, N'Médecine générale', N'CLSC Rosemont', N'CLSC Rosemont', NULL, NULL),
    (26, 31, N'Médecine générale', N'CLSC Mercier-Est', N'CLSC Mercier-Est', NULL, NULL),
    (27, 32, N'Médecine générale', N'CLSC Rivière-des-Prairies', N'CLSC Rivière-des-Prairies', NULL, NULL),
    (28, 33, N'Médecine générale', N'CLSC Est-de-Montréal', N'CLSC Est-de-Montréal', NULL, NULL),
    (29, 34, N'Médecine générale', N'CRID', N'CRID', NULL, NULL),
    (30, 35, N'Médecine générale', N'Gériatrie', N'Gériatrie', NULL, NULL),
    (31, 37, N'Médecine générale', N'Hébergement (CHSLD)', N'Hébergement (CHSLD)', NULL, NULL),
    (32, 52, N'Médecine générale', N'médecine générale (HSCO)', N'médecine générale (HSCO)', NULL, NULL),
    (33, 53, N'Médecine générale', N'Obstétrique et périnatalité', N'Obstétrique et périnatalité', NULL, NULL),
    (34, 54, N'Médecine générale', N'Santé mentale', N'Santé mentale', NULL, NULL),
    (35, 55, N'Médecine générale', N'Soins à domicile', N'Soins à domicile', NULL, NULL),
    (36, 56, N'Médecine générale', N'Soins intensifs', N'Soins intensifs (Médecine générale)', NULL, NULL),
    (37, 57, N'Médecine générale', N'Soins palliatifs', N'Soins palliatifs', NULL, NULL),
    (38, 59, N'Médecine générale', N'GMF-U - Unité de médecine familiale', N'GMF-U - Unité de médecine familiale', NULL, NULL),
    (39, 61, N'Médecine spécialisée', N'Allergologie et immunologie', N'Allergologie et immunologie', NULL, NULL),
    (40, 62, N'Médecine spécialisée', N'Cardiologie', N'Cardiologie', NULL, NULL),
    (41, 64, N'Médecine spécialisée', N'Dermatologie', N'Dermatologie', NULL, NULL),
    (42, 65, N'Médecine spécialisée', N'Diagnostic moléculaire', N'Diagnostic moléculaire', NULL, NULL),
    (43, 66, N'Médecine spécialisée', N'Endocrinologie', N'Endocrinologie', NULL, NULL),
    (44, 68, N'Médecine spécialisée', N'Gastroentérologie', N'Gastroentérologie', NULL, NULL),
    (45, 70, N'Médecine spécialisée', N'Gériatrie spécialisée', N'Gériatrie spécialisée', NULL, NULL),
    (46, 71, N'Médecine spécialisée', N'Hémato-oncologie', N'Hémato-oncologie', NULL, NULL),
    (47, 74, N'Médecine spécialisée', N'Maladies infectieuses et microbiologie médicale', N'Maladies infectieuses et microbiologie médicale', NULL, NULL),
    (48, 76, N'Médecine spécialisée', N'Médecine interne', N'Médecine interne', NULL, NULL),
    (49, 79, N'Médecine spécialisée', N'Néphrologie', N'Néphrologie', NULL, NULL),
    (50, 80, N'Médecine spécialisée', N'Neurologie', N'Neurologie', NULL, NULL),
    (51, 81, N'Médecine spécialisée', N'Pneumologie', N'Pneumologie', NULL, NULL),
    (52, 83, N'Médecine spécialisée', N'Rhumatologie', N'Rhumatologie', NULL, NULL),
    (53, 84, N'Médecine spécialisée', N'Soins intensifs', N'Soins intensifs (Médecine spécialisée)', NULL, NULL),
    (54, 86, N'Médecine spécialisée', N'Radio-Oncologie', N'Radio-Oncologie', NULL, NULL),
    (55, 87, N'Obstétrique-Gynécologique', N'Obstétrique', N'Obstétrique', NULL, NULL),
    (56, 88, N'Obstétrique-Gynécologique', N'gynécologie-oncologique', N'gynécologie-oncologique', NULL, NULL),
    (57, 89, N'Obstétrique-Gynécologique', N'Gynécologie', N'Gynécologie', NULL, NULL),
    (58, 90, N'Pédiatrie', N'Pédiatrie interne', N'Pédiatrie interne', NULL, NULL),
    (59, 91, N'Pédiatrie', N'Néonatalogie', N'Néonatalogie', NULL, NULL),
    (60, 92, N'Pédiatrie', N'Pédiatrie ambulatoire', N'Pédiatrie ambulatoire', NULL, NULL),
    (61, 94, N'Psychiatrie', N'programme de psychiatrie neurodéveloppementale adulte (PPNA)', N'programme de psychiatrie neurodéveloppementale adulte (PPNA)', NULL, NULL),
    (62, 95, N'Psychiatrie', N'Électroconvulsivothérapie', N'Électroconvulsivothérapie', NULL, NULL),
    (63, 96, N'Psychiatrie', N'Psychiatrie légale', N'Psychiatrie légale', NULL, NULL),
    (64, 97, N'Psychiatrie', N'Gérontopsychiatrie', N'Gérontopsychiatrie', NULL, NULL),
    (65, 98, N'Psychiatrie', N'Services hospitaliers', N'Services hospitaliers', NULL, NULL),
    (66, 99, N'Psychiatrie', N'CONNEC-T', N'CONNEC-T', NULL, NULL),
    (67, 100, N'Psychiatrie', N'Urgences psychiatriques', N'Urgences psychiatriques', NULL, NULL),
    (68, 101, N'Psychiatrie', N'troubles relationnels et de la personnalité (STRP)', N'troubles relationnels et de la personnalité (STRP)', NULL, NULL),
    (69, 102, N'Psychiatrie', N'Troubles anxieux et de l''humeur', N'Troubles anxieux et de l''humeur', NULL, NULL),
    (70, 103, N'Psychiatrie', N'Consultation liaison', N'Consultation liaison', NULL, NULL),
    (71, 104, N'Psychiatrie', N'Toxicomanie et maladies psychiatriques', N'Toxicomanie et maladies psychiatriques', NULL, NULL),
    (72, 105, N'Psychiatrie', N'Psychiatrie communautaire', N'Psychiatrie communautaire', NULL, NULL),
    (73, 106, N'Psychiatrie', N'Troubles psychotiques', N'Troubles psychotiques', NULL, NULL),
    (74, 107, N'Psychiatrie', N'Pédopsychiatrie', N'Pédopsychiatrie', NULL, NULL),
    (75, 108, N'Imagerie médicale', N'Radiologie', N'Radiologie', NULL, NULL),
    (76, 109, N'Imagerie médicale', N'Médecine nucléaire', N'Médecine nucléaire', NULL, NULL),
    (77, 111, N'Grappe Optilab CHUM', N'Biochimie', N'Biochimie', NULL, NULL),
    (78, 112, N'Grappe Optilab CHUM', N'Laboratoire d’hématologie', N'Laboratoire d’hématologie', NULL, NULL);

CREATE TABLE #AffectationSource
(
    SourceLigne int NOT NULL,
    RoleCode varchar(40) NOT NULL,
    NomDepartement nvarchar(200) NULL,
    NomService nvarchar(250) NULL,
    NomAffiche nvarchar(300) NOT NULL,
    Titre nvarchar(300) NULL,
    CourrielSource nvarchar(255) NULL,
    Telephone nvarchar(50) NULL,
    PosteTelephone nvarchar(20) NULL,
    SiteNomSource nvarchar(200) NULL,
    RegroupementSource nvarchar(200) NULL,
    DateDebut date NULL,
    EstInterim bit NOT NULL,
    EstMedecinRequis bit NOT NULL,
    NomRecherche nvarchar(150) NULL,
    PrenomRecherche nvarchar(150) NULL,
    PermisRecherche nvarchar(20) NULL,
    EstSourceMalfFormee bit NOT NULL,
    PRIMARY KEY (SourceLigne, RoleCode)
);

INSERT INTO #AffectationSource
(
    SourceLigne,
    RoleCode,
    NomDepartement,
    NomService,
    NomAffiche,
    Titre,
    CourrielSource,
    Telephone,
    PosteTelephone,
    SiteNomSource,
    RegroupementSource,
    DateDebut,
    EstInterim,
    EstMedecinRequis,
    NomRecherche,
    PrenomRecherche,
    PermisRecherche,
    EstSourceMalfFormee
)
VALUES
    (3, 'CHEF_DEPARTEMENT', N'Anesthésiologie', NULL, N'Verdonck, Olivier', N'Chef de département', NULL, NULL, NULL, NULL, NULL, CONVERT(date, '20200611', 112), 0, 1, N'Verdonck', N'Olivier', NULL, 0),
    (3, 'SECRETAIRE_DEPARTEMENT', N'Anesthésiologie', NULL, N'Sedano-Mariluz, Pamela-Liz', N'Secrétaire de département', NULL, NULL, N'3426', NULL, NULL, NULL, 0, 0, N'Sedano-Mariluz', N'Pamela-Liz', NULL, 0),
    (3, 'ADJOINT_SITE', N'Anesthésiologie', NULL, N'Nguyen, Dinh Tam', N'Adjoint de site', NULL, NULL, NULL, N'Santa Cabrini', NULL, NULL, 0, 1, N'Nguyen', N'Dinh Tam', NULL, 0),
    (4, 'CHEF_SERVICE', NULL, N'Clinique de gestion de la douleur', N'Urbanowicz, Robert (17366)', N'Chef de service médical de la clinique de la douleur', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Urbanowicz', N'Robert', N'17366', 0),
    (5, 'CHEF_DEPARTEMENT', N'Chirurgie', NULL, N'Tremblay, Dominique', N'Chef de département', NULL, NULL, NULL, NULL, NULL, CONVERT(date, '20240617', 112), 0, 1, N'Tremblay', N'Dominique', NULL, 0),
    (5, 'SECRETAIRE_DEPARTEMENT', N'Chirurgie', NULL, N'Perreault, Sylvie', N'Secrétaire de département', NULL, NULL, N'3824', NULL, NULL, NULL, 0, 0, N'Perreault', N'Sylvie', NULL, 0),
    (5, 'ADJOINT_SITE', N'Chirurgie', NULL, N'Akkouche, Amine (intérim)', N'Adjoint de site', NULL, NULL, NULL, N'Santa Cabrini', NULL, NULL, 1, 1, N'Akkouche', N'Amine', NULL, 0),
    (5, 'CHEF_SERVICE', NULL, N'Chirurgie dentaire', N'Amer, Zahia (212219)', N'Chef de service médical de chirurgie dentaire', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Amer', N'Zahia', N'212219', 0),
    (6, 'CHEF_SERVICE', NULL, N'Chirurgie générale', N'Tremblay, Jean-François (15205)', N'Chef de service médical de chirurgie générale', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Tremblay', N'Jean-François', N'15205', 0),
    (7, 'CHEF_SERVICE', NULL, N'Chirurgie plastique', N'Tremblay, Dominique (12490)', N'Chef de service médical de chirurgie plastique', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Tremblay', N'Dominique', N'12490', 0),
    (8, 'CHEF_SERVICE', NULL, N'Chirurgie thoracique', N'Rakovich, George (03720)', N'Chef de service médical de chirurgie thoracique', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Rakovich', N'George', N'03720', 0),
    (9, 'CHEF_SERVICE', NULL, N'Chirurgie vasculaire', N'Chaput, Miguel (09085)', N'Chef de service médical de chirurgie vasculaire', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Chaput', N'Miguel', N'09085', 0),
    (10, 'CHEF_SERVICE', NULL, N'Neurochirurgie', N'Florez Jimenez, Salomon (', N'Chef de service médical de neurochirurgie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Florez Jimenez', N'Salomon', NULL, 1),
    (11, 'CHEF_SERVICE', NULL, N'Pathologies chirurgicales (anatomopathologie)', N'Chevarie-Davis, Myriam (14459)', N'Chef de service médical d’anatomopathologie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Chevarie-Davis', N'Myriam', N'14459', 0),
    (12, 'CHEF_SERVICE', NULL, N'Orthopédie', N'Léger-St-Jean, Benjamin (16141)', N'Chef de service médical d''orthopédie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Léger-St-Jean', N'Benjamin', N'16141', 0),
    (13, 'CHEF_SERVICE', NULL, N'Oto-rhino-laryngologie', N'Ahmarani, Christian (87155)', N'Chef de service médical d’oto-rhino-laryngologie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Ahmarani', N'Christian', N'87155', 0),
    (14, 'CHEF_SERVICE', NULL, N'Urologie', N'Lebeau, Thierry (08226)', N'Chef de service médical d''urologie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Lebeau', N'Thierry', N'08226', 0),
    (15, 'CHEF_DEPARTEMENT', N'Chirurgie', NULL, N'Lesk, Mark Richard', N'Chef de département', NULL, NULL, NULL, NULL, N'(Ophtalmologie-CUO)', CONVERT(date, '20230907', 112), 0, 1, N'Lesk', N'Mark Richard', NULL, 0),
    (15, 'CHEF_SERVICE', NULL, N'Cornée et segment antérieur', N'Brunette, Isabelle (84367)', N'Chef de service médical de cornée et segement antérieur', NULL, NULL, NULL, NULL, N'(Ophtalmologie-CUO)', NULL, 0, 1, N'Brunette', N'Isabelle', N'84367', 0),
    (17, 'CHEF_SERVICE', NULL, N'Uvéite', N'Fortin, Éric', N'Chef de service médical d''uvéite', NULL, NULL, NULL, NULL, N'(Ophtalmologie-CUO)', NULL, 0, 1, N'Fortin', N'Éric', NULL, 0),
    (18, 'CHEF_SERVICE', NULL, N'Rétine et segment postérieur', N'Duval, Renaud (13089)', N'Chef de service médical de rétine et segment postérieur', NULL, NULL, NULL, NULL, N'(Ophtalmologie-CUO)', NULL, 0, 1, N'Duval', N'Renaud', N'13089', 0),
    (19, 'CHEF_SERVICE', NULL, N'Neuro-ophtalmologie et strabisme', N'Mathieu-Millaire, Francine (74470)', N'Chef de service médical de neuro-ophtalmologie et strabisme', NULL, NULL, NULL, NULL, N'(Ophtalmologie-CUO)', NULL, 0, 1, N'Mathieu-Millaire', N'Francine', N'74470', 0),
    (21, 'CHEF_SERVICE', NULL, N'Oculoplastie', N'Boulos, Patrick (04011)', N'Chef de service médical d''oculoplastie', NULL, NULL, NULL, NULL, N'(Ophtalmologie-CUO)', NULL, 0, 1, N'Boulos', N'Patrick', N'04011', 0),
    (22, 'CHEF_DEPARTEMENT', N'Médecine d’urgence', NULL, N'Pelletier, Garry', N'Chef de département', NULL, NULL, NULL, NULL, NULL, CONVERT(date, '20240212', 112), 0, 1, N'Pelletier', N'Garry', NULL, 0),
    (22, 'ADJOINT_SITE', N'Médecine d’urgence', NULL, N'Lacroix, Charles-Alexandre', N'Adjoint de site', NULL, NULL, NULL, N'Santa Cabrini', NULL, NULL, 0, 1, N'Lacroix', N'Charles-Alexandre', NULL, 0),
    (23, 'CHEF_DEPARTEMENT', N'Médecine générale', NULL, N'Ayoub, Patricia', N'Chef de département', NULL, NULL, NULL, NULL, NULL, CONVERT(date, '20160613', 112), 0, 1, N'Ayoub', N'Patricia', NULL, 0),
    (23, 'SECRETAIRE_DEPARTEMENT', N'Médecine générale', NULL, N'Toukabri, Sarra', N'Secrétaire de département', NULL, NULL, N'4191', NULL, NULL, NULL, 0, 0, N'Toukabri', N'Sarra', NULL, 0),
    (23, 'ADJOINT_SITE', N'Médecine générale', NULL, N'Noel, Anne-Sophie', N'Adjoint de site', NULL, NULL, NULL, N'Santa Cabrini', NULL, NULL, 0, 1, N'Noel', N'Anne-Sophie', NULL, 0),
    (23, 'CHEF_SERVICE', NULL, N'médecine générale (IUSMM)', N'Brunier, Jean-Louis (10460)', N'Chef de service médical de médecine générale (IUSMM)', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Brunier', N'Jean-Louis', N'10460', 0),
    (24, 'CHEF_SERVICE', NULL, N'CLSC Saint-Léonard', N'Nguyen Do, Uyen Ly (02267)', N'Chef de service médical du CLSC de Saint-Léonard', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Nguyen Do', N'Uyen Ly', N'02267', 0),
    (25, 'CHEF_SERVICE', NULL, N'CLSC Saint-Michel', N'Cadet, Jérémie (18491)', N'Chef adjoint de service médical du CLSC de Saint-Michel', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Cadet', N'Jérémie', N'18491', 0),
    (26, 'CHEF_SERVICE', NULL, N'CLSC Saint-Michel', N'De Longchamp-Dion, Rose Anais (19087)', N'Chef de service médical du CLSC de Saint-Michel', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'De Longchamp-Dion', N'Rose Anais', N'19087', 0),
    (28, 'CHEF_SERVICE', NULL, N'CLSC et GMF-U Hochelaga-Maisonneuve', N'Savaria, Myriam (16385)', N'Chef de service médical intérimaire du CLSC et GMF-U d''Hochelaga-Maisonneuve', NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, N'Savaria', N'Myriam', N'16385', 0),
    (29, 'CHEF_SERVICE', NULL, N'CLSC et GMF-U Hochelaga-Maisonneuve', N'Petrin-Desrosiers, Claudel (00913)', N'assistante-chef intérimaire du CLSC et GMF-U de Hochelaga-Maisonneuve', NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, N'Petrin-Desrosiers', N'Claudel', N'00913', 0),
    (30, 'CHEF_SERVICE', NULL, N'CLSC Rosemont', N'Lam Shang Leen, Laetitia (14385)', N'Chef de service médical du CLSC de Rosemont', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Lam Shang Leen', N'Laetitia', N'14385', 0),
    (31, 'CHEF_SERVICE', NULL, N'CLSC Mercier-Est', N'Boumaaza, Lydia (18071)', N'Chef de service médical du CLSC de Mercier-Est', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Boumaaza', N'Lydia', N'18071', 0),
    (32, 'CHEF_SERVICE', NULL, N'CLSC Rivière-des-Prairies', N'Lecavalier, Danielle (82062)', N'Chef de service médical du CLSC de Rivière-des-Prairies', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Lecavalier', N'Danielle', N'82062', 0),
    (33, 'CHEF_SERVICE', NULL, N'CLSC Est-de-Montréal', N'Ouellet, Joanie (11581)', N'Chef de service médical du CLSC de l''Est de Montréal', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Ouellet', N'Joanie', N'11581', 0),
    (37, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'vacant', N'Chef local du CHSLD Robert-Cliche', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'vacant', NULL, NULL, 0),
    (38, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Dang, Aurelie (20446)', N'Chef local du CHSLD Benjamin-Victor-Rousselot', N'aurelie.dang.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Dang', N'Aurelie', N'20446', 0),
    (39, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Cherigui, Saida (11014)', N'Chef local du CHSLD de Saint-Michel', N'saida.cherigui.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Cherigui', N'Saida', N'11014', 0),
    (40, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Nguyen, Michel Cao Kha (11019)', N'Chef local du CHSLD Éloria-Lepage', N'michel.cao.kha.nguyen.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Nguyen', N'Michel Cao Kha', N'11019', 0),
    (41, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Tourville-Poirier, Anne-Marie (16353)', N'Chef local du CHSLD François-Séguenot', N'anne.marie.tourville.poirier.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Tourville-Poirier', N'Anne-Marie', N'16353', 0),
    (42, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Luu, Huu Ni (92029)', N'Chef local du CHSLD J.-Henri-Charbonneau', N'huu.ni.luu.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Luu', N'Huu Ni', N'92029', 0),
    (43, 'ADJOINT_SITE', N'Médecine générale', NULL, N'Douesnard-Malo, Frederic', N'Chef adjoint', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Douesnard-Malo', N'Frederic', NULL, 0),
    (43, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Tihadi, Khadija (13573)', N'Chef local du CHSLD Jean-Hubert-Biermans', N'khadija.tihadi.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Tihadi', N'Khadija', N'13573', 0),
    (44, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Lam, Vo-Hoa (06017)', N'Chef local du CHSLD Jeanne-Le Ber', N'vo-hoa.lam.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Lam', N'Vo-Hoa', N'06017', 0),
    (45, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Luu, Huu Ni (92029)', N'Chef local du CHSLD Joseph-François-Perreault', N'huu.ni.luu.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Luu', N'Huu Ni', N'92029', 0),
    (46, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Dif, Yasmine (17103)', N'Chef local du CHSLD Judith-Jasmin', N'yasmine.dif.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Dif', N'Yasmine', N'17103', 0),
    (47, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Jouan, Anne-Marie (13213)', N'Chef local du CHSLD Marie-Rollet', N'anne-marie.jouan.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Jouan', N'Anne-Marie', N'13213', 0),
    (48, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Nault-Cousineau, Blanche (14756)', N'Chef local du CHSLD Nicolet', N'blanche.nault-cousineau.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Nault-Cousineau', N'Blanche', N'14756', 0),
    (49, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Benbiga, Loubna (09463)', N'Chef local du CHSLD Pierre-Joseph-Triest', N'loubna.benbiga.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Benbiga', N'Loubna', N'09463', 0),
    (50, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Arafa, Soraya (13652)', N'Chef local du CHSLD Dante', N'soraya.arafa.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Arafa', N'Soraya', N'13652', 0),
    (51, 'CHEF_SERVICE', NULL, N'Hébergement (CHSLD)', N'Okoniewski, Marie-Louise (13482)', N'Chef local du CHSLD Polonais Marie-Curie-Sklodowska', N'marie-louise.okoniewski.med@ssss.gouv.qc.ca', NULL, NULL, NULL, NULL, NULL, 0, 1, N'Okoniewski', N'Marie-Louise', N'13482', 0),
    (52, 'CHEF_SERVICE', NULL, N'médecine générale (HSCO)', N'Noël, Anne-Sophie (19674)', N'assistante-chef - volet médecine générale de l''HSCO', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Noël', N'Anne-Sophie', N'19674', 0),
    (53, 'CHEF_SERVICE', NULL, N'Obstétrique et périnatalité', N'Bourassa, Catherine (18913)', N'Chef de service médical d''obstétrique-périnatalité', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Bourassa', N'Catherine', N'18913', 0),
    (57, 'CHEF_SERVICE', NULL, N'Soins palliatifs', N'Picard, Gabrielle (20716)', N'Chef de service médical de soins palliatifs HMR', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Picard', N'Gabrielle', N'20716', 0),
    (58, 'CHEF_SERVICE', NULL, N'Soins palliatifs', N'Pelletier, Bruno (98003)', N'Chef de service médical des soins palliatifs HSCO', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Pelletier', N'Bruno', N'98003', 0),
    (59, 'CHEF_SERVICE', NULL, N'GMF-U - Unité de médecine familiale', N'Mauguière, Édith (12649)', N'chef adjoint GMF-U Maisonneuve-Rosemont', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Mauguière', N'Édith', N'12649', 0),
    (60, 'CHEF_SERVICE', NULL, N'GMF-U - Unité de médecine familiale', N'Giraldeau-Bélanger, Sarah-Émilie (08155)', N'Chef de service médical du GMF-U Maisonneuve-Rosemont', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Giraldeau-Bélanger', N'Sarah-Émilie', N'08155', 0),
    (61, 'CHEF_DEPARTEMENT', N'Médecine spécialisée', NULL, N'Tremblay, Jean-François', N'Chef de département', NULL, NULL, NULL, NULL, NULL, CONVERT(date, '20240212', 112), 0, 1, N'Tremblay', N'Jean-François', NULL, 0),
    (61, 'SECRETAIRE_DEPARTEMENT', N'Médecine spécialisée', NULL, N'Delaney, Claudia', N'Secrétaire de département', NULL, NULL, N'3456', NULL, NULL, NULL, 0, 0, N'Delaney', N'Claudia', NULL, 0),
    (61, 'ADJOINT_SITE', N'Médecine spécialisée', NULL, N'Trifiro, Silvana', N'Adjoint de site', NULL, NULL, NULL, N'Santa Cabrini', NULL, NULL, 0, 1, N'Trifiro', N'Silvana', NULL, 0),
    (61, 'CHEF_SERVICE', NULL, N'Allergologie et immunologie', N'Picard, Matthieu (12262)', N'Chef de service médical d’allergologie et immunologie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Picard', N'Matthieu', N'12262', 0),
    (62, 'CHEF_SERVICE', NULL, N'Cardiologie', N'Jolicoeur, Philippe (13684)', N'Chef de service médical de cardiologie HMR', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Jolicoeur', N'Philippe', N'13684', 0),
    (63, 'CHEF_SERVICE', NULL, N'Cardiologie', N'Fadlallah, Hussein (02427)', N'Chef de service médical de cardiologie HSCO', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Fadlallah', N'Hussein', N'02427', 0),
    (64, 'CHEF_SERVICE', NULL, N'Dermatologie', N'Lafaille, Philippe (08358)', N'Chef de service médical de dermatologie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Lafaille', N'Philippe', N'08358', 0),
    (65, 'CHEF_SERVICE', NULL, N'Diagnostic moléculaire', N'Busque, Lambert (87181)', N'Chef du service médical de diagnostic moléculaire clinique', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Busque', N'Lambert', N'87181', 0),
    (66, 'ADJOINT_SITE', N'Médecine spécialisée', NULL, N'Lafrance, Jean-Philippe', N'chef adjoint - volet académique', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Lafrance', N'Jean-Philippe', NULL, 0),
    (66, 'CHEF_SERVICE', NULL, N'Endocrinologie', N'Adam, Catherine (14284)', N'Chef du service médical d''endocrinologie HMR', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Adam', N'Catherine', N'14284', 0),
    (67, 'CHEF_SERVICE', NULL, N'Endocrinologie', N'Poirier, Lysane (93294)', N'Chef du service médical d''endocrinologie HSCO', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Poirier', N'Lysane', N'93294', 0),
    (68, 'CHEF_SERVICE', NULL, N'Gastroentérologie', N'Rioux, Louis-Charles (87286)', N'Chef de service médical de gastro-entérologie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Rioux', N'Louis-Charles', N'87286', 0),
    (69, 'CHEF_SERVICE', NULL, N'Gastroentérologie', N'Rubino, Maria (15493)', N'Chef de service médical de gastro-entérologie (HSCO)', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Rubino', N'Maria', N'15493', 0),
    (70, 'CHEF_SERVICE', NULL, N'Gériatrie spécialisée', N'Faubert, Charles (16462)', N'Chef de service médical de gériatrie spécialisée', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Faubert', N'Charles', N'16462', 0),
    (71, 'CHEF_SERVICE', NULL, N'Hémato-oncologie', N'Ahmad, Imran (12628)', N'Chef de service médical d''hémato-oncologie HMR', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Ahmad', N'Imran', N'12628', 0),
    (72, 'CHEF_SERVICE', NULL, N'Hémato-oncologie', N'Lachance, Sylvie (88222)', N'Chef adjoint du service médical d''hémato-oncologie HMR', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Lachance', N'Sylvie', N'88222', 0),
    (73, 'CHEF_SERVICE', NULL, N'Hémato-oncologie', N'Palumbo, Michael O. (12524)', N'Chef de service médical d''hémato-oncologie HSCO', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Palumbo', N'Michael O.', N'12524', 0),
    (74, 'CHEF_SERVICE', NULL, N'Maladies infectieuses et microbiologie médicale', N'Trifiro, Silvana (84057)', N'Chef du service médical de maladies infectieuses et microbiologie clinique (HSCO)', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Trifiro', N'Silvana', N'84057', 0),
    (75, 'CHEF_SERVICE', NULL, N'Maladies infectieuses et microbiologie médicale', N'Labbé, Annie-Claude (99253)', N'Chef du service médical de maladies infectieuses et microbiologie clinique', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Labbé', N'Annie-Claude', N'99253', 0),
    (76, 'CHEF_SERVICE', NULL, N'Médecine interne', N'Viau-Lapointe, Julien', N'Chef de service médical de médecine interne HMR', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Viau-Lapointe', N'Julien', NULL, 0),
    (77, 'CHEF_SERVICE', NULL, N'Médecine interne', N'Le Van Long, Maurice (96034)', N'Chef de service médical de médecine interne IUSMM', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Le Van Long', N'Maurice', N'96034', 0),
    (78, 'CHEF_SERVICE', NULL, N'Médecine interne', N'Krassakopoulos, Nickolaos (15279)', N'Chef de service médical de médecine interne HSCO', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Krassakopoulos', N'Nickolaos', N'15279', 0),
    (79, 'CHEF_SERVICE', NULL, N'Néphrologie', N'Laurin, Louis-Philippe (11286)', N'Chefdu service médical de néphrologie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Laurin', N'Louis-Philippe', N'11286', 0),
    (80, 'CHEF_SERVICE', NULL, N'Neurologie', N'Chayer, Céline (97044)', N'Chef de service médical de neurologie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Chayer', N'Céline', N'97044', 0),
    (81, 'CHEF_SERVICE', NULL, N'Pneumologie', N'Lévesque, Jonathan (16083)', N'Chef de service médical de pneumologie HMR', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Lévesque', N'Jonathan', N'16083', 0),
    (82, 'CHEF_SERVICE', NULL, N'Pneumologie', N'Béjar-Ardiles, Katia Rocio (16106)', N'Chef de service médical de pneumologie HSCO', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Béjar-Ardiles', N'Katia Rocio', N'16106', 0),
    (83, 'CHEF_SERVICE', NULL, N'Rhumatologie', N'Yuen, Sai Yan (08025)', N'Chef de service médical de rhumatologie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Yuen', N'Sai Yan', N'08025', 0),
    (84, 'CHEF_SERVICE', NULL, N'Soins intensifs (Médecine spécialisée)', N'Rola, Philippe (00484)', N'Chef de service médical des soins intensifs HSCO', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Rola', N'Philippe', N'00484', 0),
    (85, 'CHEF_SERVICE', NULL, N'Soins intensifs (Médecine spécialisée)', N'Marquis, François (03255)', N'Chef de service médical des soins intensifs HMR', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Marquis', N'François', N'03255', 0),
    (86, 'CHEF_SERVICE', NULL, N'Radio-Oncologie', N'Fortin, Bernard (01264)', N'Chef de service de radio-oncologie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Fortin', N'Bernard', N'01264', 0),
    (87, 'CHEF_DEPARTEMENT', N'Obstétrique-Gynécologique', NULL, N'Rivard, Chantal', N'Chef de département', NULL, NULL, NULL, NULL, NULL, CONVERT(date, '20220614', 112), 0, 1, N'Rivard', N'Chantal', NULL, 0),
    (87, 'SECRETAIRE_DEPARTEMENT', N'Obstétrique-Gynécologique', NULL, N'Naud, Isabelle', N'Secrétaire de département', NULL, NULL, N'3819', NULL, NULL, NULL, 0, 0, N'Naud', N'Isabelle', NULL, 0),
    (87, 'CHEF_SERVICE', NULL, N'Obstétrique', N'Tordjman, Laurent-Henri (14120)', N'Chef de service médical d’obstétrique', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Tordjman', N'Laurent-Henri', N'14120', 0),
    (88, 'CHEF_SERVICE', NULL, N'gynécologie-oncologique', N'Fortin, Suzanne (08229)', N'Chef de service médical de gynécologie-oncologique', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Fortin', N'Suzanne', N'08229', 0),
    (89, 'CHEF_SERVICE', NULL, N'Gynécologie', N'Bernard, Martine (00274)', N'chef de service médical de gynécologie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Bernard', N'Martine', N'00274', 0),
    (90, 'CHEF_DEPARTEMENT', N'Pédiatrie', NULL, N'Faucher, Frédéric', N'Chef de département', NULL, NULL, NULL, NULL, NULL, CONVERT(date, '20200614', 112), 0, 1, N'Faucher', N'Frédéric', NULL, 0),
    (90, 'SECRETAIRE_DEPARTEMENT', N'Pédiatrie', NULL, N'Ardouin, Jocelyne', N'Secrétaire de département', NULL, NULL, N'3807', NULL, NULL, NULL, 0, 0, N'Ardouin', N'Jocelyne', NULL, 0),
    (90, 'CHEF_SERVICE', NULL, N'Pédiatrie interne', N'Maisonneuve, Marie-Hélène (17196)', N'Chef de service médical de pédiatrie interne', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Maisonneuve', N'Marie-Hélène', N'17196', 0),
    (91, 'CHEF_SERVICE', NULL, N'Néonatalogie', N'Mandel, Romain (01045)', N'Chef de service médical de néonatalogie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Mandel', N'Romain', N'01045', 0),
    (92, 'CHEF_SERVICE', NULL, N'Pédiatrie ambulatoire', N'Desjardins, Noemie (14275)', N'Chef de service médical de pédiatrie ambulatoire', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Desjardins', N'Noemie', N'14275', 0),
    (93, 'CHEF_DEPARTEMENT', N'Pharmacie', NULL, N'Goulet, Jude', N'Chef de département', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Goulet', N'Jude', NULL, 0),
    (93, 'SECRETAIRE_DEPARTEMENT', N'Pharmacie', NULL, N'Mellak, Karima', N'Secrétaire de département', NULL, NULL, N'4135', NULL, NULL, NULL, 0, 0, N'Mellak', N'Karima', NULL, 0),
    (93, 'ADJOINT_SITE', N'Pharmacie', NULL, N'Mme Sabrina Sanzari, adjointe au chef', N'Adjointe au chef', NULL, NULL, NULL, N'Santa Cabrini', NULL, NULL, 0, 1, N'Sabrina Sanzari', NULL, NULL, 0),
    (94, 'CHEF_DEPARTEMENT', N'Psychiatrie', NULL, N'Bérubé, Felix-Antoine', N'Chef de département', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Bérubé', N'Felix-Antoine', NULL, 0),
    (94, 'SECRETAIRE_DEPARTEMENT', N'Psychiatrie', NULL, N'Bettner, Geneviève', N'Secrétaire de département', NULL, N'514 251-4000', N'2935', NULL, NULL, NULL, 0, 0, N'Bettner', N'Geneviève', NULL, 0),
    (94, 'CHEF_SERVICE', NULL, N'programme de psychiatrie neurodéveloppementale adulte (PPNA)', N'Beauchamp-Chatel, Alexis (16602)', N'Chef de service médical du programme de psychiatrie neurodéveloppementale adulte', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Beauchamp-Chatel', N'Alexis', N'16602', 0),
    (95, 'CHEF_SERVICE', NULL, N'Électroconvulsivothérapie', N'Tourjman, Valérie (85519)', N'Chef de service médical d’électro-convulsivothérapie / Coordonnatrice médicale de la troisième ligne', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Tourjman', N'Valérie', N'85519', 0),
    (96, 'CHEF_SERVICE', NULL, N'Psychiatrie légale', N'Borduas-Pagé, Stéphanie (10255)', N'Chef de service médical de la psychiatrie légale', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Borduas-Pagé', N'Stéphanie', N'10255', 0),
    (97, 'CHEF_SERVICE', NULL, N'Gérontopsychiatrie', N'Couture, Vincent (03631)', N'Chef de service médical de gérontopsychiatrie', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Couture', N'Vincent', N'03631', 0),
    (98, 'CHEF_SERVICE', NULL, N'Services hospitaliers', N'Lemire, Myriam (99452)', N'Chef de service médical des services hospitaliers', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Lemire', N'Myriam', N'99452', 0),
    (99, 'CHEF_SERVICE', NULL, N'CONNEC-T', N'Bérubé, Félix-Antoine (07249)', N'Chef du service médical CONNEC-T', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Bérubé', N'Félix-Antoine', N'07249', 0),
    (100, 'CHEF_SERVICE', NULL, N'Urgences psychiatriques', N'Lestage, Laurent (02728)', N'Chef de service médical des urgences psychiatriques', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Lestage', N'Laurent', N'02728', 0),
    (101, 'CHEF_SERVICE', NULL, N'troubles relationnels et de la personnalité (STRP)', N'Poirier, Simon (12093)', N'Chef de service médical du service des troubles relationnels et de la personnalité', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Poirier', N'Simon', N'12093', 0),
    (102, 'CHEF_SERVICE', NULL, N'Troubles anxieux et de l''humeur', N'Pinard, Georges-Franck (87279)', N'Chef de service médical du service des troubles anxieux et de l''humeur', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Pinard', N'Georges-Franck', N'87279', 0),
    (103, 'CHEF_SERVICE', NULL, N'Consultation liaison', N'Caron, David (02647)', N'Chef de service médical de consultation-liaison', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Caron', N'David', N'02647', 0),
    (104, 'CHEF_SERVICE', NULL, N'Toxicomanie et maladies psychiatriques', N'Noël, François (10648)', N'Chef de service médical de toxicomanie et maladies psychiatriques', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Noël', N'François', N'10648', 0),
    (105, 'CHEF_SERVICE', NULL, N'Psychiatrie communautaire', N'De Benedictis, Luigi (11210)', N'Chef de service médical de la psychiatrie communautaire', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'De Benedictis', N'Luigi', N'11210', 0),
    (106, 'CHEF_SERVICE', NULL, N'Troubles psychotiques', N'Lacombe, Cédric (05967)', N'Chef de service médical du programme des troubles psychotiques', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Lacombe', N'Cédric', N'05967', 0),
    (108, 'CHEF_DEPARTEMENT', N'Imagerie médicale', NULL, N'Ugolini, Patricia', N'Chef de département', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Ugolini', N'Patricia', NULL, 0),
    (108, 'SECRETAIRE_DEPARTEMENT', N'Imagerie médicale', NULL, N'Chabot-Rainville, Krystel', N'Secrétaire de département', NULL, NULL, N'6115', NULL, NULL, NULL, 0, 0, N'Chabot-Rainville', N'Krystel', NULL, 0),
    (108, 'ADJOINT_SITE', N'Imagerie médicale', NULL, N'Dugas, Alexandre (pour HMR)', N'Adjoint de site', NULL, NULL, NULL, N'HMR', NULL, NULL, 0, 1, N'Dugas', N'Alexandre', NULL, 0),
    (108, 'CHEF_SERVICE', NULL, N'Radiologie', N'Hannouche, Nabil (78607)', N'Chef de service médical de la radiologie (HSCO)', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Hannouche', N'Nabil', N'78607', 0),
    (109, 'CHEF_SERVICE', NULL, N'Médecine nucléaire', N'Laflamme, Louis (84246)', N'Chef de service médical de médecine nucléaire', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Laflamme', N'Louis', N'84246', 0),
    (111, 'CHEF_DEPARTEMENT', N'Grappe Optilab CHUM', NULL, N'Lavallée, Christian', N'Directeur médical', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Lavallée', N'Christian', NULL, 0),
    (112, 'CHEF_SERVICE', NULL, N'Laboratoire d’hématologie', N'Bergeron, Julie (04316)', N'chef de service médical d’hématologie-laboratoire', NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, N'Bergeron', N'Julie', N'04316', 0);

IF (SELECT COUNT(*) FROM #DepartementSource) <> @NombreDepartementsAttendu
BEGIN
    ;THROW 52010, N'Le nombre de departements sources est inattendu.', 1;
END;

IF (SELECT COUNT(*) FROM #ServiceSource) <> @NombreServicesAttendu
BEGIN
    ;THROW 52011, N'Le nombre de services sources est inattendu.', 1;
END;

IF (SELECT COUNT(*) FROM #AffectationSource) <> @NombreAffectationsAttendu
BEGIN
    ;THROW 52012, N'Le nombre d''affectations sources est inattendu.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM #ServiceSource AS s
    LEFT JOIN #DepartementSource AS d
        ON d.NomDepartement = s.NomDepartement
    WHERE d.DepartementId IS NULL
)
BEGIN
    ;THROW 52013, N'Un service source ne peut pas etre rattache a un departement source.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM #AffectationSource AS a
    LEFT JOIN #DepartementSource AS d
        ON d.NomDepartement = a.NomDepartement
    LEFT JOIN #ServiceSource AS s
        ON s.NomService = a.NomService
    WHERE (a.RoleCode = 'CHEF_SERVICE' AND s.NomService IS NULL)
       OR (a.RoleCode <> 'CHEF_SERVICE' AND d.NomDepartement IS NULL)
)
BEGIN
    ;THROW 52014, N'Une affectation source ne peut pas etre rattachee a sa cible.', 1;
END;

/* Refuser les dependances non prises en charge avant toute suppression. */
IF EXISTS
(
    SELECT 1
    FROM sys.foreign_keys AS fk
    WHERE fk.referenced_object_id = OBJECT_ID(N'dbo.Service')
      AND OBJECT_SCHEMA_NAME(fk.parent_object_id) + N'.' + OBJECT_NAME(fk.parent_object_id) NOT IN
      (
          N'dbo.AffectationOrganisationnelle',
          N'dbo.InstallationService',
          N'dbo.MedecinPrivilege'
      )
)
BEGIN
    ;THROW 52015, N'Une cle etrangere non prise en charge reference dbo.Service. La reinitialisation est annulee.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM sys.foreign_keys AS fk
    WHERE fk.referenced_object_id = OBJECT_ID(N'dbo.Departement')
      AND OBJECT_SCHEMA_NAME(fk.parent_object_id) + N'.' + OBJECT_NAME(fk.parent_object_id) NOT IN
      (
          N'dbo.AffectationOrganisationnelle',
          N'dbo.Service'
      )
)
BEGIN
    ;THROW 52016, N'Une cle etrangere non prise en charge reference dbo.Departement. La reinitialisation est annulee.', 1;
END;

CREATE TABLE #ServiceReferenceExterne
(
    ServiceId int NOT NULL,
    CONSTRAINT PK_ServiceReferenceExterne PRIMARY KEY (ServiceId)
);

IF OBJECT_ID(N'dbo.InstallationService', N'U') IS NOT NULL
BEGIN
    INSERT INTO #ServiceReferenceExterne (ServiceId)
    SELECT DISTINCT x.ServiceId
    FROM dbo.InstallationService AS x
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM #ServiceReferenceExterne AS e
        WHERE e.ServiceId = x.ServiceId
    );
END;

IF OBJECT_ID(N'dbo.MedecinPrivilege', N'U') IS NOT NULL
BEGIN
    INSERT INTO #ServiceReferenceExterne (ServiceId)
    SELECT DISTINCT x.ServiceId
    FROM dbo.MedecinPrivilege AS x
    WHERE x.ServiceId IS NOT NULL
      AND NOT EXISTS
      (
          SELECT 1
          FROM #ServiceReferenceExterne AS e
          WHERE e.ServiceId = x.ServiceId
      );
END;

IF EXISTS
(
    SELECT s.NomService
    FROM #ServiceReferenceExterne AS r
    INNER JOIN dbo.Service AS s
        ON s.ServiceId = r.ServiceId
    GROUP BY s.NomService
    HAVING COUNT(DISTINCT s.ServiceId) > 1
)
BEGIN
    ;THROW 52017, N'Plusieurs services references portent le meme nom. La conservation des identifiants serait ambigue.', 1;
END;

CREATE TABLE #ServiceHistoriqueConserve
(
    ServiceId int NOT NULL,
    NomService nvarchar(250) NOT NULL,
    CodeService nvarchar(50) NULL,
    RegroupementSource nvarchar(200) NULL,
    CONSTRAINT PK_ServiceHistoriqueConserve PRIMARY KEY (ServiceId),
    CONSTRAINT UQ_ServiceHistoriqueConserve_Nom UNIQUE (NomService)
);

/*
    Les services absents de la nouvelle source ne peuvent pas etre supprimes
    lorsqu'ils sont encore utilises. Ils sont conserves comme historiques
    inactifs afin de ne supprimer ni lien d'installation ni privilege medical.
*/
INSERT INTO #ServiceHistoriqueConserve
(
    ServiceId,
    NomService,
    CodeService,
    RegroupementSource
)
SELECT
    ancien.ServiceId,
    ancien.NomService,
    ancien.CodeService,
    ancien.RegroupementSource
FROM #ServiceReferenceExterne AS r
INNER JOIN dbo.Service AS ancien
    ON ancien.ServiceId = r.ServiceId
LEFT JOIN #ServiceSource AS source
    ON source.NomService = ancien.NomService
WHERE source.SourceOrdre IS NULL;

DECLARE @NombreServicesHistoriquesConserves int =
    (SELECT COUNT(*) FROM #ServiceHistoriqueConserve);

IF @NombreServicesHistoriquesConserves > 0
BEGIN
    SELECT
        ServiceId,
        NomService,
        N'Conserve comme service historique inactif parce qu''il est encore utilise' AS Traitement
    FROM #ServiceHistoriqueConserve
    ORDER BY NomService;
END;

/* Conserver uniquement les identifiants de services necessaires aux dependances. */
UPDATE source
SET ServiceId = ancien.ServiceId
FROM #ServiceSource AS source
INNER JOIN dbo.Service AS ancien
    ON ancien.NomService = source.NomService
INNER JOIN #ServiceReferenceExterne AS r
    ON r.ServiceId = ancien.ServiceId;

DECLARE @PlusGrandServiceIdConserve int =
(
    SELECT ISNULL(MAX(ids.ServiceId), 0)
    FROM
    (
        SELECT ServiceId
        FROM #ServiceSource
        WHERE ServiceId IS NOT NULL

        UNION ALL

        SELECT ServiceId
        FROM #ServiceHistoriqueConserve
    ) AS ids
);

;WITH ServicesNouveaux AS
(
    SELECT
        SourceOrdre,
        ROW_NUMBER() OVER (ORDER BY SourceOrdre) AS NouveauRang
    FROM #ServiceSource
    WHERE ServiceId IS NULL
)
UPDATE source
SET ServiceId = @PlusGrandServiceIdConserve + nouveau.NouveauRang
FROM #ServiceSource AS source
INNER JOIN ServicesNouveaux AS nouveau
    ON nouveau.SourceOrdre = source.SourceOrdre;

IF EXISTS
(
    SELECT ids.ServiceId
    FROM
    (
        SELECT ServiceId FROM #ServiceSource
        UNION ALL
        SELECT ServiceId FROM #ServiceHistoriqueConserve
    ) AS ids
    GROUP BY ids.ServiceId
    HAVING COUNT(*) > 1
)
BEGIN
    ;THROW 52019, N'La preparation a produit des identifiants de service dupliques.', 1;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    DELETE FROM dbo.AffectationOrganisationnelle;

    DECLARE @SqlDesactivation nvarchar(max) = N'';
    SELECT @SqlDesactivation = @SqlDesactivation
        + N'ALTER TABLE '
        + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id))
        + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
        + N' NOCHECK CONSTRAINT ' + QUOTENAME(fk.name) + N';'
    FROM sys.foreign_keys AS fk
    WHERE fk.referenced_object_id = OBJECT_ID(N'dbo.Service')
      AND fk.parent_object_id IN
      (
          OBJECT_ID(N'dbo.InstallationService'),
          OBJECT_ID(N'dbo.MedecinPrivilege')
      );

    IF @SqlDesactivation <> N''
        EXEC sys.sp_executesql @SqlDesactivation;

    DELETE FROM dbo.Service;
    DELETE FROM dbo.Departement;

    DBCC CHECKIDENT (N'dbo.Departement', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Service', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.AffectationOrganisationnelle', RESEED, 0) WITH NO_INFOMSGS;

    SET IDENTITY_INSERT dbo.Departement ON;

    INSERT INTO dbo.Departement
    (
        DepartementId,
        NomDepartement,
        CodeDepartement,
        EstActif
    )
    SELECT
        DepartementId,
        NomDepartement,
        CodeDepartement,
        1
    FROM #DepartementSource
    ORDER BY SourceOrdre;

    SET IDENTITY_INSERT dbo.Departement OFF;

    SET IDENTITY_INSERT dbo.Service ON;

    INSERT INTO dbo.Service
    (
        ServiceId,
        DepartementId,
        NomService,
        CodeService,
        EstActif,
        RegroupementSource
    )
    SELECT
        s.ServiceId,
        d.DepartementId,
        s.NomService,
        NULL,
        1,
        s.RegroupementSource
    FROM #ServiceSource AS s
    INNER JOIN #DepartementSource AS d
        ON d.NomDepartement = s.NomDepartement
    ORDER BY s.SourceOrdre;

    INSERT INTO dbo.Service
    (
        ServiceId,
        DepartementId,
        NomService,
        CodeService,
        EstActif,
        RegroupementSource
    )
    SELECT
        h.ServiceId,
        NULL,
        h.NomService,
        h.CodeService,
        0,
        h.RegroupementSource
    FROM #ServiceHistoriqueConserve AS h
    ORDER BY h.ServiceId;

    SET IDENTITY_INSERT dbo.Service OFF;

    DECLARE @SqlReactivation nvarchar(max) = N'';
    SELECT @SqlReactivation = @SqlReactivation
        + N'ALTER TABLE '
        + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id))
        + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
        + N' WITH CHECK CHECK CONSTRAINT ' + QUOTENAME(fk.name) + N';'
    FROM sys.foreign_keys AS fk
    WHERE fk.referenced_object_id = OBJECT_ID(N'dbo.Service')
      AND fk.parent_object_id IN
      (
          OBJECT_ID(N'dbo.InstallationService'),
          OBJECT_ID(N'dbo.MedecinPrivilege')
      );

    IF @SqlReactivation <> N''
        EXEC sys.sp_executesql @SqlReactivation;

    INSERT INTO dbo.AffectationOrganisationnelle
    (
        DepartementId,
        ServiceId,
        MedecinId,
        RoleCode,
        NomAffiche,
        Titre,
        Courriel,
        CourrielSource,
        Telephone,
        PosteTelephone,
        InstallationId,
        SiteNomSource,
        RegroupementSource,
        DateDebut,
        DateFin,
        EstInterim,
        EstActive,
        EstAValider,
        SourceDonnees,
        SourceLigne
    )
    SELECT
        CASE WHEN a.RoleCode = 'CHEF_SERVICE' THEN NULL ELSE d.DepartementId END,
        CASE WHEN a.RoleCode = 'CHEF_SERVICE' THEN s.ServiceId ELSE NULL END,
        CASE
            WHEN a.EstMedecinRequis = 1 AND correspondance.NombreCorrespondances = 1
                THEN correspondance.MedecinId
            ELSE NULL
        END,
        a.RoleCode,
        a.NomAffiche,
        a.Titre,
        CASE
            WHEN a.CourrielSource LIKE N'%_@_%._%' THEN a.CourrielSource
            ELSE NULL
        END,
        a.CourrielSource,
        a.Telephone,
        a.PosteTelephone,
        NULL,
        a.SiteNomSource,
        a.RegroupementSource,
        a.DateDebut,
        NULL,
        a.EstInterim,
        1,
        CAST
        (
            CASE
                WHEN a.EstSourceMalfFormee = 1 THEN 1
                WHEN a.EstMedecinRequis = 1
                 AND correspondance.NombreCorrespondances <> 1 THEN 1
                WHEN a.CourrielSource IS NOT NULL
                 AND a.CourrielSource NOT LIKE N'%_@_%._%' THEN 1
                ELSE 0
            END
            AS bit
        ),
        @SourceDonnees,
        a.SourceLigne
    FROM #AffectationSource AS a
    LEFT JOIN #DepartementSource AS d
        ON d.NomDepartement = a.NomDepartement
    LEFT JOIN #ServiceSource AS s
        ON s.NomService = a.NomService
    OUTER APPLY
    (
        SELECT
            COUNT(*) AS NombreCorrespondances,
            MIN(m.MedecinId) AS MedecinId
        FROM dbo.Medecin AS m
        WHERE
            (
                a.PermisRecherche IS NOT NULL
                AND
                (
                    LTRIM(RTRIM(m.Permis)) = a.PermisRecherche
                    OR
                    (
                        TRY_CONVERT(bigint, m.Permis) IS NOT NULL
                        AND TRY_CONVERT(bigint, a.PermisRecherche) IS NOT NULL
                        AND TRY_CONVERT(bigint, m.Permis) = TRY_CONVERT(bigint, a.PermisRecherche)
                    )
                )
            )
            OR
            (
                a.PermisRecherche IS NULL
                AND a.NomRecherche IS NOT NULL
                AND a.PrenomRecherche IS NOT NULL
                AND LTRIM(RTRIM(m.Nom)) COLLATE Latin1_General_100_CI_AI
                    = a.NomRecherche COLLATE Latin1_General_100_CI_AI
                AND LTRIM(RTRIM(m.Prenom)) COLLATE Latin1_General_100_CI_AI
                    = a.PrenomRecherche COLLATE Latin1_General_100_CI_AI
            )
    ) AS correspondance;

    IF (SELECT COUNT(*) FROM dbo.Departement) <> @NombreDepartementsAttendu
    BEGIN
        ;THROW 52020, N'Le controle final des departements a echoue.', 1;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.Service
    ) <> @NombreServicesAttendu + @NombreServicesHistoriquesConserves
    BEGIN
        ;THROW 52021, N'Le controle final des services a echoue.', 1;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.Service
        WHERE EstActif = 1
    ) <> @NombreServicesAttendu
    BEGIN
        ;THROW 52024, N'Le controle final des services actifs provenant de la source a echoue.', 1;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.AffectationOrganisationnelle
        WHERE SourceDonnees = @SourceDonnees
    ) <> @NombreAffectationsAttendu
    BEGIN
        ;THROW 52022, N'Le controle final des affectations a echoue.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys AS fk
        WHERE fk.referenced_object_id IN
        (
            OBJECT_ID(N'dbo.Departement'),
            OBJECT_ID(N'dbo.Service')
        )
          AND (fk.is_disabled = 1 OR fk.is_not_trusted = 1)
    )
    BEGIN
        ;THROW 52023, N'Une cle etrangere est demeuree desactivee ou non approuvee.', 1;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    BEGIN TRY
        SET IDENTITY_INSERT dbo.Departement OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT dbo.Service OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

/* Resultats de controle apres validation de la transaction. */
SELECT
    (SELECT COUNT(*) FROM dbo.Departement) AS NombreDepartements,
    (SELECT COUNT(*) FROM dbo.Service) AS NombreServices,
    (SELECT COUNT(*) FROM dbo.Service WHERE EstActif = 1) AS NombreServicesActifs,
    (SELECT COUNT(*) FROM dbo.Service WHERE EstActif = 0) AS NombreServicesHistoriquesConserves,
    (
        SELECT COUNT(*)
        FROM dbo.AffectationOrganisationnelle
        WHERE SourceDonnees = N'CIUSSS-EMTL_Organisation_depts_services_Nominatif_2026-09-10 (1).xlsx'
    ) AS NombreAffectations,
    (
        SELECT COUNT(*)
        FROM dbo.AffectationOrganisationnelle
        WHERE SourceDonnees = N'CIUSSS-EMTL_Organisation_depts_services_Nominatif_2026-09-10 (1).xlsx'
          AND EstAValider = 1
    ) AS NombreAffectationsAValider;

SELECT
    RoleCode,
    COUNT(*) AS Nombre
FROM dbo.AffectationOrganisationnelle
WHERE SourceDonnees = N'CIUSSS-EMTL_Organisation_depts_services_Nominatif_2026-09-10 (1).xlsx'
GROUP BY RoleCode
ORDER BY RoleCode;
GO
