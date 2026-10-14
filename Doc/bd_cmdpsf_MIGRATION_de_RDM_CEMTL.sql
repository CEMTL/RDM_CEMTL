/* MIGRATION DES DONNEES RDM_CEMTL -> bd_cmdpsf
   Conserve les valeurs des cles primaires et etrangeres, y compris ProfessionnelId.
   SOURCE : RDM_CEMTL (aucune modification)
   CIBLE  : bd_cmdpsf (tables metier vides ; comptes Identity existants acceptes).
   IMPORTANT : SAUVEGARDER LES DEUX BASES, arretez l'application avant execution.
   Si des comptes ont deja ete crees dans bd_cmdpsf, ne lancez pas ce script :
   il s'arrete volontairement pour ne pas ecraser des donnees.
   Il ne transfere pas les objets absents de la structure fournie (ex. MedecinProfilCEMTL).
   A executer dans SSMS avec un compte autorise a lire la source et modifier la cible.
*/
USE [bd_cmdpsf];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_ID(N'RDM_CEMTL') IS NULL OR DB_ID(N'bd_cmdpsf') IS NULL
    THROW 51020, N'La base source RDM_CEMTL ou cible bd_cmdpsf est absente.', 1;
IF DB_NAME() <> N'bd_cmdpsf'
    THROW 51021, N'Le contexte SQL actuel n est pas bd_cmdpsf.', 1;

-- Precontrole : toutes les tables attendues et colonnes doivent etre presentes.
DECLARE @manquants nvarchar(max) = N'';
IF OBJECT_ID(N'[RDM_CEMTL].[ref].[StatutMedecin]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[ref].[StatutProfessionnel]',N'U') IS NULL
    SET @manquants += N'ref.StatutMedecin => ref.StatutProfessionnel; ';
IF OBJECT_ID(N'[RDM_CEMTL].[ref].[CategoriePratique]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[ref].[CategoriePratique]',N'U') IS NULL
    SET @manquants += N'ref.CategoriePratique => ref.CategoriePratique; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[Medecin]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[Professionnel]',N'U') IS NULL
    SET @manquants += N'dbo.Medecin => dbo.Professionnel; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[CodeCotisation]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[CodeCotisation]',N'U') IS NULL
    SET @manquants += N'dbo.CodeCotisation => dbo.CodeCotisation; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[CotisationAnnuelle]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'U') IS NULL
    SET @manquants += N'dbo.CotisationAnnuelle => dbo.CotisationAnnuelle; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[Departement]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[Departement]',N'U') IS NULL
    SET @manquants += N'dbo.Departement => dbo.Departement; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[AffectationOrganisationnelle]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'U') IS NULL
    SET @manquants += N'dbo.AffectationOrganisationnelle => dbo.AffectationOrganisationnelle; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[Service]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[Service]',N'U') IS NULL
    SET @manquants += N'dbo.Service => dbo.Service; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[Installation]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[Installation]',N'U') IS NULL
    SET @manquants += N'dbo.Installation => dbo.Installation; ';
IF OBJECT_ID(N'[RDM_CEMTL].[ref].[Profession]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[ref].[Profession]',N'U') IS NULL
    SET @manquants += N'ref.Profession => ref.Profession; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[__MigrationHistory]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[__MigrationHistory]',N'U') IS NULL
    SET @manquants += N'dbo.__MigrationHistory => dbo.__MigrationHistory; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[AspNetRoles]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[AspNetRoles]',N'U') IS NULL
    SET @manquants += N'dbo.AspNetRoles => dbo.AspNetRoles; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[AspNetUserClaims]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[AspNetUserClaims]',N'U') IS NULL
    SET @manquants += N'dbo.AspNetUserClaims => dbo.AspNetUserClaims; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[AspNetUserLogins]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[AspNetUserLogins]',N'U') IS NULL
    SET @manquants += N'dbo.AspNetUserLogins => dbo.AspNetUserLogins; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[AspNetUserRoles]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[AspNetUserRoles]',N'U') IS NULL
    SET @manquants += N'dbo.AspNetUserRoles => dbo.AspNetUserRoles; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[AspNetUsers]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'U') IS NULL
    SET @manquants += N'dbo.AspNetUsers => dbo.AspNetUsers; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[AuditLog]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[AuditLog]',N'U') IS NULL
    SET @manquants += N'dbo.AuditLog => dbo.AuditLog; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[Comite]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[Comite]',N'U') IS NULL
    SET @manquants += N'dbo.Comite => dbo.Comite; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[DocumentMedecin]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'U') IS NULL
    SET @manquants += N'dbo.DocumentMedecin => dbo.DocumentProfessionnel; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[EntenteFDC]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[EntenteFDC]',N'U') IS NULL
    SET @manquants += N'dbo.EntenteFDC => dbo.EntenteFDC; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[FitTest]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[FitTest]',N'U') IS NULL
    SET @manquants += N'dbo.FitTest => dbo.FitTest; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[InstallationService]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[InstallationService]',N'U') IS NULL
    SET @manquants += N'dbo.InstallationService => dbo.InstallationService; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[InstanceDecision]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[InstanceDecision]',N'U') IS NULL
    SET @manquants += N'dbo.InstanceDecision => dbo.InstanceDecision; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[JournalImport]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[JournalImport]',N'U') IS NULL
    SET @manquants += N'dbo.JournalImport => dbo.JournalImport; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[MedecinAdresse]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[ProfessionnelAdresse]',N'U') IS NULL
    SET @manquants += N'dbo.MedecinAdresse => dbo.ProfessionnelAdresse; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[MedecinAffectationClinique]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'U') IS NULL
    SET @manquants += N'dbo.MedecinAffectationClinique => dbo.ProfessionnelAffectationClinique; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[MedecinComite]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[ProfessionnelComite]',N'U') IS NULL
    SET @manquants += N'dbo.MedecinComite => dbo.ProfessionnelComite; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[MedecinCourriel]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[ProfessionnelCourriel]',N'U') IS NULL
    SET @manquants += N'dbo.MedecinCourriel => dbo.ProfessionnelCourriel; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[MedecinInstallation]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[ProfessionnelInstallation]',N'U') IS NULL
    SET @manquants += N'dbo.MedecinInstallation => dbo.ProfessionnelInstallation; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[MedecinPrivilege]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'U') IS NULL
    SET @manquants += N'dbo.MedecinPrivilege => dbo.ProfessionnelPrivilege; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[MedecinTelephone]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[ProfessionnelTelephone]',N'U') IS NULL
    SET @manquants += N'dbo.MedecinTelephone => dbo.ProfessionnelTelephone; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[Nomination]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[Nomination]',N'U') IS NULL
    SET @manquants += N'dbo.Nomination => dbo.Nomination; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[NominationAdministrative]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[NominationAdministrative]',N'U') IS NULL
    SET @manquants += N'dbo.NominationAdministrative => dbo.NominationAdministrative; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[PrivilegeReference]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[PrivilegeReference]',N'U') IS NULL
    SET @manquants += N'dbo.PrivilegeReference => dbo.PrivilegeReference; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[TypeInstanceDecision]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[TypeInstanceDecision]',N'U') IS NULL
    SET @manquants += N'dbo.TypeInstanceDecision => dbo.TypeInstanceDecision; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[TypeNomination]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[TypeNomination]',N'U') IS NULL
    SET @manquants += N'dbo.TypeNomination => dbo.TypeNomination; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[TypePaiement]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[TypePaiement]',N'U') IS NULL
    SET @manquants += N'dbo.TypePaiement => dbo.TypePaiement; ';
IF OBJECT_ID(N'[RDM_CEMTL].[dbo].[UtilisateurMetier]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[dbo].[UtilisateurMetier]',N'U') IS NULL
    SET @manquants += N'dbo.UtilisateurMetier => dbo.UtilisateurMetier; ';
IF OBJECT_ID(N'[RDM_CEMTL].[ref].[Genre]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[ref].[Genre]',N'U') IS NULL
    SET @manquants += N'ref.Genre => ref.Genre; ';
IF OBJECT_ID(N'[RDM_CEMTL].[ref].[TitreMedecin]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[ref].[TitreProfessionnel]',N'U') IS NULL
    SET @manquants += N'ref.TitreMedecin => ref.TitreProfessionnel; ';
IF OBJECT_ID(N'[RDM_CEMTL].[ref].[TypeCourriel]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[ref].[TypeCourriel]',N'U') IS NULL
    SET @manquants += N'ref.TypeCourriel => ref.TypeCourriel; ';
IF OBJECT_ID(N'[RDM_CEMTL].[ref].[TypePermis]',N'U') IS NULL OR OBJECT_ID(N'[bd_cmdpsf].[ref].[TypePermis]',N'U') IS NULL
    SET @manquants += N'ref.TypePermis => ref.TypePermis; ';
IF LEN(@manquants)>0
BEGIN
    PRINT @manquants;
    THROW 51022, N'Tables attendues absentes : migration annulee.', 1;
END;
-- Refuser le chargement par-dessus des dossiers existants.
-- Seule ref.Profession (4 valeurs creees par le script de structure) est autorisee.
DECLARE @occupees nvarchar(max)=N'';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[ref].[StatutProfessionnel]) SET @occupees += N'ref.StatutProfessionnel; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[ref].[CategoriePratique]) SET @occupees += N'ref.CategoriePratique; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[Professionnel]) SET @occupees += N'dbo.Professionnel; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[CodeCotisation]) SET @occupees += N'dbo.CodeCotisation; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[CotisationAnnuelle]) SET @occupees += N'dbo.CotisationAnnuelle; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[Departement]) SET @occupees += N'dbo.Departement; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[AffectationOrganisationnelle]) SET @occupees += N'dbo.AffectationOrganisationnelle; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[Service]) SET @occupees += N'dbo.Service; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[Installation]) SET @occupees += N'dbo.Installation; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[__MigrationHistory]) SET @occupees += N'dbo.__MigrationHistory; ';
-- Comptes existants preserves : AspNetRoles
-- Comptes existants preserves : AspNetUserClaims
-- Comptes existants preserves : AspNetUserLogins
-- Comptes existants preserves : AspNetUserRoles
-- Comptes existants preserves : AspNetUsers
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[AuditLog]) SET @occupees += N'dbo.AuditLog; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[Comite]) SET @occupees += N'dbo.Comite; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[DocumentProfessionnel]) SET @occupees += N'dbo.DocumentProfessionnel; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[EntenteFDC]) SET @occupees += N'dbo.EntenteFDC; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[FitTest]) SET @occupees += N'dbo.FitTest; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[InstallationService]) SET @occupees += N'dbo.InstallationService; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[InstanceDecision]) SET @occupees += N'dbo.InstanceDecision; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[JournalImport]) SET @occupees += N'dbo.JournalImport; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[ProfessionnelAdresse]) SET @occupees += N'dbo.ProfessionnelAdresse; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]) SET @occupees += N'dbo.ProfessionnelAffectationClinique; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[ProfessionnelComite]) SET @occupees += N'dbo.ProfessionnelComite; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[ProfessionnelCourriel]) SET @occupees += N'dbo.ProfessionnelCourriel; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[ProfessionnelInstallation]) SET @occupees += N'dbo.ProfessionnelInstallation; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[ProfessionnelPrivilege]) SET @occupees += N'dbo.ProfessionnelPrivilege; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[ProfessionnelTelephone]) SET @occupees += N'dbo.ProfessionnelTelephone; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[Nomination]) SET @occupees += N'dbo.Nomination; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[NominationAdministrative]) SET @occupees += N'dbo.NominationAdministrative; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[PrivilegeReference]) SET @occupees += N'dbo.PrivilegeReference; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[TypeInstanceDecision]) SET @occupees += N'dbo.TypeInstanceDecision; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[TypeNomination]) SET @occupees += N'dbo.TypeNomination; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[TypePaiement]) SET @occupees += N'dbo.TypePaiement; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[dbo].[UtilisateurMetier]) SET @occupees += N'dbo.UtilisateurMetier; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[ref].[Genre]) SET @occupees += N'ref.Genre; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[ref].[TitreProfessionnel]) SET @occupees += N'ref.TitreProfessionnel; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[ref].[TypeCourriel]) SET @occupees += N'ref.TypeCourriel; ';
IF EXISTS(SELECT TOP(1) 1 FROM [bd_cmdpsf].[ref].[TypePermis]) SET @occupees += N'ref.TypePermis; ';
IF LEN(@occupees)>0
BEGIN
    PRINT N'Tables cibles deja remplies : ' + @occupees;
    THROW 51023, N'Une ou plusieurs tables metier sont deja remplies ; migration annulee.', 1;
END;

-- Si des enregistrements source utilisent des tables absentes de la structure cible,
-- la migration exige une prise en charge explicite : aucun oubli silencieux.
-- Le controle ci-dessous couvre les tables de base non correspondantes.
DECLARE @horsModele nvarchar(max)=N'';
DECLARE @sql nvarchar(max);
DECLARE @sc sysname, @tb sysname, @cible sysname, @nb bigint;
DECLARE curSource CURSOR LOCAL FAST_FORWARD FOR
 SELECT ss.name,st.name FROM [RDM_CEMTL].sys.tables st
 INNER JOIN [RDM_CEMTL].sys.schemas ss ON ss.schema_id=st.schema_id
 WHERE st.is_ms_shipped=0 AND st.name NOT IN (N'sysdiagrams');
OPEN curSource; FETCH NEXT FROM curSource INTO @sc,@tb;
WHILE @@FETCH_STATUS=0
BEGIN
 SET @cible=REPLACE(@tb,N'Medecin',N'Professionnel');
 IF OBJECT_ID(N'[bd_cmdpsf].'+QUOTENAME(@sc)+N'.'+QUOTENAME(@cible),N'U') IS NULL
 BEGIN
   SET @sql=N'SELECT @n=COUNT_BIG(*) FROM [RDM_CEMTL].'+QUOTENAME(@sc)+N'.'+QUOTENAME(@tb);
   EXEC sys.sp_executesql @sql,N'@n bigint OUTPUT',@n=@nb OUTPUT;
   IF @nb>0 SET @horsModele+=QUOTENAME(@sc)+N'.'+QUOTENAME(@tb)+N' ('+CONVERT(nvarchar(30),@nb)+N' lignes); ';
 END;
 FETCH NEXT FROM curSource INTO @sc,@tb;
END;
CLOSE curSource; DEALLOCATE curSource;
IF LEN(@horsModele)>0
BEGIN
 PRINT N'Donnees source non prises en charge : '+@horsModele;
 THROW 51024,N'Des tables source peuplees ne figurent pas dans bd_cmdpsf. Aucun transfert effectue.',1;
END;

-- Precontrole des colonnes mappees avant toute ecriture.
DECLARE @colonnesAbsentes nvarchar(max)=N'';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[StatutProfessionnel]',N'StatutProfessionnelId') IS NULL SET @colonnesAbsentes += N'ref.StatutProfessionnel.StatutProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[StatutProfessionnel]',N'CodeStatut') IS NULL SET @colonnesAbsentes += N'ref.StatutProfessionnel.CodeStatut; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[StatutProfessionnel]',N'Libelle') IS NULL SET @colonnesAbsentes += N'ref.StatutProfessionnel.Libelle; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[StatutProfessionnel]',N'EstActif') IS NULL SET @colonnesAbsentes += N'ref.StatutProfessionnel.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[CategoriePratique]',N'CategoriePratiqueId') IS NULL SET @colonnesAbsentes += N'ref.CategoriePratique.CategoriePratiqueId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[CategoriePratique]',N'Libelle') IS NULL SET @colonnesAbsentes += N'ref.CategoriePratique.Libelle; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[CategoriePratique]',N'EstActif') IS NULL SET @colonnesAbsentes += N'ref.CategoriePratique.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'Permis') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.Permis; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'TitreId') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.TitreId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'Nom') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.Nom; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'Prenom') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.Prenom; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'TypePermisId') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.TypePermisId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'StatutProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.StatutProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'CategoriePratiqueId') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.CategoriePratiqueId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'GenreId') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.GenreId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'DateNaissance') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.DateNaissance; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'NAM') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.NAM; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'DateEntreeFonction') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.DateEntreeFonction; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'DateDepart') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.DateDepart; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'Commentaires') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.Commentaires; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'DateCreation') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.DateCreation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'DateModification') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.DateModification; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'ProfessionId') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.ProfessionId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'DateObtentionPermis') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.DateObtentionPermis; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'DateExpirationPermis') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.DateExpirationPermis; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'NumeroMembreOrdre') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.NumeroMembreOrdre; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'DateDerniereVerificationOrdre') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.DateDerniereVerificationOrdre; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Professionnel]',N'ReferenceVerificationOrdre') IS NULL SET @colonnesAbsentes += N'dbo.Professionnel.ReferenceVerificationOrdre; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CodeCotisation]',N'CodeCotisationId') IS NULL SET @colonnesAbsentes += N'dbo.CodeCotisation.CodeCotisationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CodeCotisation]',N'CodeCotisation') IS NULL SET @colonnesAbsentes += N'dbo.CodeCotisation.CodeCotisation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CodeCotisation]',N'Description') IS NULL SET @colonnesAbsentes += N'dbo.CodeCotisation.Description; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CodeCotisation]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.CodeCotisation.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'CotisationAnnuelleId') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.CotisationAnnuelleId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'AnneeExercice') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.AnneeExercice; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'CodeCotisationId') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.CodeCotisationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'DescriptionSource') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.DescriptionSource; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'MontantInitial') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.MontantInitial; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'ReductionAppliquee') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.ReductionAppliquee; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'MotifReduction') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.MotifReduction; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'MontantReduction') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.MontantReduction; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'PenaliteAppliquee') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.PenaliteAppliquee; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'PenaliteTexte') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.PenaliteTexte; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'MontantPenalite') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.MontantPenalite; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'MontantFinal') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.MontantFinal; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'EtatCotisation') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.EtatCotisation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'MontantPaye') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.MontantPaye; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'DatePaiement') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.DatePaiement; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'TypePaiementTexte') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.TypePaiementTexte; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'TypePaiementId') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.TypePaiementId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'DateDepot') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.DateDepot; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'NoDepot') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.NoDepot; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'RecuImposition') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.RecuImposition; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'DateRecuImposition') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.DateRecuImposition; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[CotisationAnnuelle]',N'DateCreation') IS NULL SET @colonnesAbsentes += N'dbo.CotisationAnnuelle.DateCreation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Departement]',N'DepartementId') IS NULL SET @colonnesAbsentes += N'dbo.Departement.DepartementId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Departement]',N'NomDepartement') IS NULL SET @colonnesAbsentes += N'dbo.Departement.NomDepartement; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Departement]',N'CodeDepartement') IS NULL SET @colonnesAbsentes += N'dbo.Departement.CodeDepartement; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Departement]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.Departement.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'AffectationOrganisationnelleId') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.AffectationOrganisationnelleId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'DepartementId') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.DepartementId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'ServiceId') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.ServiceId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'RoleCode') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.RoleCode; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'NomAffiche') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.NomAffiche; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'Titre') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.Titre; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'Courriel') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.Courriel; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'CourrielSource') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.CourrielSource; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'Telephone') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.Telephone; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'PosteTelephone') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.PosteTelephone; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'InstallationId') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.InstallationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'SiteNomSource') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.SiteNomSource; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'RegroupementSource') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.RegroupementSource; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'DateDebut') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.DateDebut; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'DateFin') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.DateFin; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'EstInterim') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.EstInterim; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'EstActive') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.EstActive; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'EstAValider') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.EstAValider; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'SourceDonnees') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.SourceDonnees; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'SourceLigne') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.SourceLigne; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'DateCreation') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.DateCreation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AffectationOrganisationnelle]',N'DateModification') IS NULL SET @colonnesAbsentes += N'dbo.AffectationOrganisationnelle.DateModification; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Service]',N'ServiceId') IS NULL SET @colonnesAbsentes += N'dbo.Service.ServiceId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Service]',N'DepartementId') IS NULL SET @colonnesAbsentes += N'dbo.Service.DepartementId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Service]',N'NomService') IS NULL SET @colonnesAbsentes += N'dbo.Service.NomService; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Service]',N'CodeService') IS NULL SET @colonnesAbsentes += N'dbo.Service.CodeService; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Service]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.Service.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Service]',N'RegroupementSource') IS NULL SET @colonnesAbsentes += N'dbo.Service.RegroupementSource; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Installation]',N'InstallationId') IS NULL SET @colonnesAbsentes += N'dbo.Installation.InstallationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Installation]',N'NomInstallation') IS NULL SET @colonnesAbsentes += N'dbo.Installation.NomInstallation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Installation]',N'CodeInstallation') IS NULL SET @colonnesAbsentes += N'dbo.Installation.CodeInstallation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Installation]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.Installation.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[Profession]',N'ProfessionId') IS NULL SET @colonnesAbsentes += N'ref.Profession.ProfessionId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[Profession]',N'CodeProfession') IS NULL SET @colonnesAbsentes += N'ref.Profession.CodeProfession; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[Profession]',N'Libelle') IS NULL SET @colonnesAbsentes += N'ref.Profession.Libelle; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[Profession]',N'EstActif') IS NULL SET @colonnesAbsentes += N'ref.Profession.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[__MigrationHistory]',N'MigrationId') IS NULL SET @colonnesAbsentes += N'dbo.__MigrationHistory.MigrationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[__MigrationHistory]',N'ContextKey') IS NULL SET @colonnesAbsentes += N'dbo.__MigrationHistory.ContextKey; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[__MigrationHistory]',N'Model') IS NULL SET @colonnesAbsentes += N'dbo.__MigrationHistory.Model; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[__MigrationHistory]',N'ProductVersion') IS NULL SET @colonnesAbsentes += N'dbo.__MigrationHistory.ProductVersion; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetRoles]',N'Id') IS NULL SET @colonnesAbsentes += N'dbo.AspNetRoles.Id; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetRoles]',N'Name') IS NULL SET @colonnesAbsentes += N'dbo.AspNetRoles.Name; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUserClaims]',N'Id') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUserClaims.Id; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUserClaims]',N'UserId') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUserClaims.UserId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUserClaims]',N'ClaimType') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUserClaims.ClaimType; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUserClaims]',N'ClaimValue') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUserClaims.ClaimValue; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUserLogins]',N'LoginProvider') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUserLogins.LoginProvider; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUserLogins]',N'ProviderKey') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUserLogins.ProviderKey; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUserLogins]',N'UserId') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUserLogins.UserId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUserRoles]',N'UserId') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUserRoles.UserId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUserRoles]',N'RoleId') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUserRoles.RoleId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'Id') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.Id; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'Nom') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.Nom; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'Prenom') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.Prenom; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'Email') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.Email; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'PhoneNumber') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.PhoneNumber; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'UserName') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.UserName; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'EmailConfirmed') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.EmailConfirmed; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'PasswordHash') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.PasswordHash; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'SecurityStamp') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.SecurityStamp; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'PhoneNumberConfirmed') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.PhoneNumberConfirmed; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'TwoFactorEnabled') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.TwoFactorEnabled; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'LockoutEndDateUtc') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.LockoutEndDateUtc; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'LockoutEnabled') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.LockoutEnabled; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AspNetUsers]',N'AccessFailedCount') IS NULL SET @colonnesAbsentes += N'dbo.AspNetUsers.AccessFailedCount; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AuditLog]',N'AuditLogId') IS NULL SET @colonnesAbsentes += N'dbo.AuditLog.AuditLogId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AuditLog]',N'TableName') IS NULL SET @colonnesAbsentes += N'dbo.AuditLog.TableName; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AuditLog]',N'RecordId') IS NULL SET @colonnesAbsentes += N'dbo.AuditLog.RecordId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AuditLog]',N'Action') IS NULL SET @colonnesAbsentes += N'dbo.AuditLog.Action; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AuditLog]',N'OldValues') IS NULL SET @colonnesAbsentes += N'dbo.AuditLog.OldValues; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AuditLog]',N'NewValues') IS NULL SET @colonnesAbsentes += N'dbo.AuditLog.NewValues; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AuditLog]',N'ChangedFields') IS NULL SET @colonnesAbsentes += N'dbo.AuditLog.ChangedFields; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AuditLog]',N'UtilisateurId') IS NULL SET @colonnesAbsentes += N'dbo.AuditLog.UtilisateurId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AuditLog]',N'NomUtilisateur') IS NULL SET @colonnesAbsentes += N'dbo.AuditLog.NomUtilisateur; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AuditLog]',N'DateAction') IS NULL SET @colonnesAbsentes += N'dbo.AuditLog.DateAction; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AuditLog]',N'AdresseIP') IS NULL SET @colonnesAbsentes += N'dbo.AuditLog.AdresseIP; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[AuditLog]',N'UserAgent') IS NULL SET @colonnesAbsentes += N'dbo.AuditLog.UserAgent; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Comite]',N'ComiteId') IS NULL SET @colonnesAbsentes += N'dbo.Comite.ComiteId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Comite]',N'NumeroSource') IS NULL SET @colonnesAbsentes += N'dbo.Comite.NumeroSource; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Comite]',N'NomComite') IS NULL SET @colonnesAbsentes += N'dbo.Comite.NomComite; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Comite]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.Comite.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'DocumentProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.DocumentProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'TypeDocument') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.TypeDocument; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'NomFichier') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.NomFichier; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'ContentType') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.ContentType; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'TailleOctets') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.TailleOctets; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'HashSHA256') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.HashSHA256; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'DateDocument') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.DateDocument; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'DateExpiration') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.DateExpiration; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'Description') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.Description; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'EntiteLiee') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.EntiteLiee; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'EntiteId') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.EntiteId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'Confidentiel') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.Confidentiel; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'Contenu') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.Contenu; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'DateDepot') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.DateDepot; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'DeposePar') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.DeposePar; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[DocumentProfessionnel]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.DocumentProfessionnel.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[EntenteFDC]',N'EntenteFDCId') IS NULL SET @colonnesAbsentes += N'dbo.EntenteFDC.EntenteFDCId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[EntenteFDC]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.EntenteFDC.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[EntenteFDC]',N'TypeEntenteFDC') IS NULL SET @colonnesAbsentes += N'dbo.EntenteFDC.TypeEntenteFDC; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[EntenteFDC]',N'DateAdhesion') IS NULL SET @colonnesAbsentes += N'dbo.EntenteFDC.DateAdhesion; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[EntenteFDC]',N'DateFin') IS NULL SET @colonnesAbsentes += N'dbo.EntenteFDC.DateFin; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[EntenteFDC]',N'PEMVise') IS NULL SET @colonnesAbsentes += N'dbo.EntenteFDC.PEMVise; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[EntenteFDC]',N'EstActive') IS NULL SET @colonnesAbsentes += N'dbo.EntenteFDC.EstActive; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[FitTest]',N'FitTestId') IS NULL SET @colonnesAbsentes += N'dbo.FitTest.FitTestId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[FitTest]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.FitTest.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[FitTest]',N'Resultat') IS NULL SET @colonnesAbsentes += N'dbo.FitTest.Resultat; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[FitTest]',N'DateFitTest') IS NULL SET @colonnesAbsentes += N'dbo.FitTest.DateFitTest; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[FitTest]',N'Commentaire') IS NULL SET @colonnesAbsentes += N'dbo.FitTest.Commentaire; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[FitTest]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.FitTest.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[InstallationService]',N'InstallationServiceId') IS NULL SET @colonnesAbsentes += N'dbo.InstallationService.InstallationServiceId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[InstallationService]',N'InstallationId') IS NULL SET @colonnesAbsentes += N'dbo.InstallationService.InstallationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[InstallationService]',N'ServiceId') IS NULL SET @colonnesAbsentes += N'dbo.InstallationService.ServiceId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[InstallationService]',N'DateDebut') IS NULL SET @colonnesAbsentes += N'dbo.InstallationService.DateDebut; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[InstallationService]',N'DateFin') IS NULL SET @colonnesAbsentes += N'dbo.InstallationService.DateFin; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[InstallationService]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.InstallationService.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[InstanceDecision]',N'InstanceDecisionId') IS NULL SET @colonnesAbsentes += N'dbo.InstanceDecision.InstanceDecisionId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[InstanceDecision]',N'TypeInstanceDecisionId') IS NULL SET @colonnesAbsentes += N'dbo.InstanceDecision.TypeInstanceDecisionId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[InstanceDecision]',N'CodeInstance') IS NULL SET @colonnesAbsentes += N'dbo.InstanceDecision.CodeInstance; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[InstanceDecision]',N'DateInstance') IS NULL SET @colonnesAbsentes += N'dbo.InstanceDecision.DateInstance; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[InstanceDecision]',N'LibelleInstance') IS NULL SET @colonnesAbsentes += N'dbo.InstanceDecision.LibelleInstance; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[JournalImport]',N'JournalImportId') IS NULL SET @colonnesAbsentes += N'dbo.JournalImport.JournalImportId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[JournalImport]',N'NomFichier') IS NULL SET @colonnesAbsentes += N'dbo.JournalImport.NomFichier; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[JournalImport]',N'DateImport') IS NULL SET @colonnesAbsentes += N'dbo.JournalImport.DateImport; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[JournalImport]',N'LignesImportees') IS NULL SET @colonnesAbsentes += N'dbo.JournalImport.LignesImportees; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[JournalImport]',N'LignesRejetees') IS NULL SET @colonnesAbsentes += N'dbo.JournalImport.LignesRejetees; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[JournalImport]',N'Commentaire') IS NULL SET @colonnesAbsentes += N'dbo.JournalImport.Commentaire; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAdresse]',N'ProfessionnelAdresseId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAdresse.ProfessionnelAdresseId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAdresse]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAdresse.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAdresse]',N'AdresseLigne1') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAdresse.AdresseLigne1; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAdresse]',N'Ville') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAdresse.Ville; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAdresse]',N'Province') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAdresse.Province; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAdresse]',N'CodePostal') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAdresse.CodePostal; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAdresse]',N'EstPrincipale') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAdresse.EstPrincipale; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'ProfessionnelAffectationCliniqueId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.ProfessionnelAffectationCliniqueId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'DepartementId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.DepartementId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'ServiceId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.ServiceId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'InstallationId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.InstallationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'TypeAffectation') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.TypeAffectation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'EstPrincipale') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.EstPrincipale; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'DateDebut') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.DateDebut; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'DateFin') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.DateFin; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'EstActive') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.EstActive; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'Commentaire') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.Commentaire; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'SourceDonnees') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.SourceDonnees; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'DateCreation') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.DateCreation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelAffectationClinique]',N'DateModification') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelAffectationClinique.DateModification; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelComite]',N'ProfessionnelComiteId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelComite.ProfessionnelComiteId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelComite]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelComite.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelComite]',N'ComiteId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelComite.ComiteId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelComite]',N'RoleMembre') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelComite.RoleMembre; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelComite]',N'DateDebut') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelComite.DateDebut; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelComite]',N'DateFin') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelComite.DateFin; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelComite]',N'Commentaire') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelComite.Commentaire; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelCourriel]',N'ProfessionnelCourrielId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelCourriel.ProfessionnelCourrielId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelCourriel]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelCourriel.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelCourriel]',N'TypeCourriel') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelCourriel.TypeCourriel; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelCourriel]',N'AdresseCourriel') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelCourriel.AdresseCourriel; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelCourriel]',N'RangSource') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelCourriel.RangSource; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelCourriel]',N'EstPrincipal') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelCourriel.EstPrincipal; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelCourriel]',N'TypeCourrielId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelCourriel.TypeCourrielId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelInstallation]',N'ProfessionnelInstallationId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelInstallation.ProfessionnelInstallationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelInstallation]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelInstallation.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelInstallation]',N'InstallationId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelInstallation.InstallationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelInstallation]',N'RangInstallation') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelInstallation.RangInstallation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelInstallation]',N'DateDebut') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelInstallation.DateDebut; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelInstallation]',N'DateFin') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelInstallation.DateFin; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelInstallation]',N'EstPrincipale') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelInstallation.EstPrincipale; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'ProfessionnelPrivilegeId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.ProfessionnelPrivilegeId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'PrivilegeReferenceId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.PrivilegeReferenceId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'InstallationId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.InstallationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'ServiceId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.ServiceId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'DateDebut') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.DateDebut; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'DateFin') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.DateFin; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'Etat') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.Etat; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'NatureActivites') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.NatureActivites; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'ChampActivites') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.ChampActivites; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'Obligations') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.Obligations; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'Commentaire') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.Commentaire; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'NumeroResolutionCA') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.NumeroResolutionCA; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'DateResolutionCA') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.DateResolutionCA; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'ReconnaissanceEcriteRecue') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.ReconnaissanceEcriteRecue; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'DateCreation') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.DateCreation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelPrivilege]',N'DateModification') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelPrivilege.DateModification; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelTelephone]',N'ProfessionnelTelephoneId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelTelephone.ProfessionnelTelephoneId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelTelephone]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelTelephone.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelTelephone]',N'TypeTelephone') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelTelephone.TypeTelephone; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelTelephone]',N'NumeroTelephone') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelTelephone.NumeroTelephone; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelTelephone]',N'RangSource') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelTelephone.RangSource; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[ProfessionnelTelephone]',N'EstPrincipal') IS NULL SET @colonnesAbsentes += N'dbo.ProfessionnelTelephone.EstPrincipal; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Nomination]',N'NominationId') IS NULL SET @colonnesAbsentes += N'dbo.Nomination.NominationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Nomination]',N'CodeNomination') IS NULL SET @colonnesAbsentes += N'dbo.Nomination.CodeNomination; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Nomination]',N'LibelleNomination') IS NULL SET @colonnesAbsentes += N'dbo.Nomination.LibelleNomination; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Nomination]',N'Description') IS NULL SET @colonnesAbsentes += N'dbo.Nomination.Description; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Nomination]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.Nomination.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Nomination]',N'DateCreation') IS NULL SET @colonnesAbsentes += N'dbo.Nomination.DateCreation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[Nomination]',N'DateModification') IS NULL SET @colonnesAbsentes += N'dbo.Nomination.DateModification; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[NominationAdministrative]',N'NominationAdministrativeId') IS NULL SET @colonnesAbsentes += N'dbo.NominationAdministrative.NominationAdministrativeId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[NominationAdministrative]',N'ProfessionnelId') IS NULL SET @colonnesAbsentes += N'dbo.NominationAdministrative.ProfessionnelId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[NominationAdministrative]',N'LibelleNomination') IS NULL SET @colonnesAbsentes += N'dbo.NominationAdministrative.LibelleNomination; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[NominationAdministrative]',N'DateNomination') IS NULL SET @colonnesAbsentes += N'dbo.NominationAdministrative.DateNomination; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[NominationAdministrative]',N'DateFin') IS NULL SET @colonnesAbsentes += N'dbo.NominationAdministrative.DateFin; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[NominationAdministrative]',N'EstActive') IS NULL SET @colonnesAbsentes += N'dbo.NominationAdministrative.EstActive; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[NominationAdministrative]',N'NominationId') IS NULL SET @colonnesAbsentes += N'dbo.NominationAdministrative.NominationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[NominationAdministrative]',N'TypeNominationId') IS NULL SET @colonnesAbsentes += N'dbo.NominationAdministrative.TypeNominationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[PrivilegeReference]',N'PrivilegeReferenceId') IS NULL SET @colonnesAbsentes += N'dbo.PrivilegeReference.PrivilegeReferenceId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[PrivilegeReference]',N'CodePrivilege') IS NULL SET @colonnesAbsentes += N'dbo.PrivilegeReference.CodePrivilege; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[PrivilegeReference]',N'LibellePrivilege') IS NULL SET @colonnesAbsentes += N'dbo.PrivilegeReference.LibellePrivilege; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[PrivilegeReference]',N'Categorie') IS NULL SET @colonnesAbsentes += N'dbo.PrivilegeReference.Categorie; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[PrivilegeReference]',N'Description') IS NULL SET @colonnesAbsentes += N'dbo.PrivilegeReference.Description; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[PrivilegeReference]',N'NecessiteBlocOperatoire') IS NULL SET @colonnesAbsentes += N'dbo.PrivilegeReference.NecessiteBlocOperatoire; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[PrivilegeReference]',N'NecessiteInstallation') IS NULL SET @colonnesAbsentes += N'dbo.PrivilegeReference.NecessiteInstallation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[PrivilegeReference]',N'NecessiteUnite') IS NULL SET @colonnesAbsentes += N'dbo.PrivilegeReference.NecessiteUnite; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[PrivilegeReference]',N'OrdreAffichage') IS NULL SET @colonnesAbsentes += N'dbo.PrivilegeReference.OrdreAffichage; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[PrivilegeReference]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.PrivilegeReference.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[PrivilegeReference]',N'DateCreation') IS NULL SET @colonnesAbsentes += N'dbo.PrivilegeReference.DateCreation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[PrivilegeReference]',N'DateModification') IS NULL SET @colonnesAbsentes += N'dbo.PrivilegeReference.DateModification; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[TypeInstanceDecision]',N'TypeInstanceDecisionId') IS NULL SET @colonnesAbsentes += N'dbo.TypeInstanceDecision.TypeInstanceDecisionId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[TypeInstanceDecision]',N'CodeType') IS NULL SET @colonnesAbsentes += N'dbo.TypeInstanceDecision.CodeType; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[TypeInstanceDecision]',N'Libelle') IS NULL SET @colonnesAbsentes += N'dbo.TypeInstanceDecision.Libelle; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[TypeNomination]',N'TypeNominationId') IS NULL SET @colonnesAbsentes += N'dbo.TypeNomination.TypeNominationId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[TypeNomination]',N'CodeTypeNomination') IS NULL SET @colonnesAbsentes += N'dbo.TypeNomination.CodeTypeNomination; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[TypeNomination]',N'LibelleTypeNomination') IS NULL SET @colonnesAbsentes += N'dbo.TypeNomination.LibelleTypeNomination; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[TypeNomination]',N'Description') IS NULL SET @colonnesAbsentes += N'dbo.TypeNomination.Description; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[TypeNomination]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.TypeNomination.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[TypeNomination]',N'DateCreation') IS NULL SET @colonnesAbsentes += N'dbo.TypeNomination.DateCreation; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[TypeNomination]',N'DateModification') IS NULL SET @colonnesAbsentes += N'dbo.TypeNomination.DateModification; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[TypePaiement]',N'TypePaiementId') IS NULL SET @colonnesAbsentes += N'dbo.TypePaiement.TypePaiementId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[TypePaiement]',N'Libelle') IS NULL SET @colonnesAbsentes += N'dbo.TypePaiement.Libelle; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[TypePaiement]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.TypePaiement.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[UtilisateurMetier]',N'UtilisateurMetierId') IS NULL SET @colonnesAbsentes += N'dbo.UtilisateurMetier.UtilisateurMetierId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[UtilisateurMetier]',N'IdentifiantSource') IS NULL SET @colonnesAbsentes += N'dbo.UtilisateurMetier.IdentifiantSource; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[UtilisateurMetier]',N'Courriel') IS NULL SET @colonnesAbsentes += N'dbo.UtilisateurMetier.Courriel; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[UtilisateurMetier]',N'NomComplet') IS NULL SET @colonnesAbsentes += N'dbo.UtilisateurMetier.NomComplet; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[UtilisateurMetier]',N'Connexion') IS NULL SET @colonnesAbsentes += N'dbo.UtilisateurMetier.Connexion; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[UtilisateurMetier]',N'AspNetUserId') IS NULL SET @colonnesAbsentes += N'dbo.UtilisateurMetier.AspNetUserId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[dbo].[UtilisateurMetier]',N'EstActif') IS NULL SET @colonnesAbsentes += N'dbo.UtilisateurMetier.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[Genre]',N'GenreId') IS NULL SET @colonnesAbsentes += N'ref.Genre.GenreId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[Genre]',N'CodeGenre') IS NULL SET @colonnesAbsentes += N'ref.Genre.CodeGenre; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[Genre]',N'Libelle') IS NULL SET @colonnesAbsentes += N'ref.Genre.Libelle; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[TitreProfessionnel]',N'TitreId') IS NULL SET @colonnesAbsentes += N'ref.TitreProfessionnel.TitreId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[TitreProfessionnel]',N'Libelle') IS NULL SET @colonnesAbsentes += N'ref.TitreProfessionnel.Libelle; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[TitreProfessionnel]',N'EstActif') IS NULL SET @colonnesAbsentes += N'ref.TitreProfessionnel.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[TypeCourriel]',N'TypeCourrielId') IS NULL SET @colonnesAbsentes += N'ref.TypeCourriel.TypeCourrielId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[TypeCourriel]',N'Libelle') IS NULL SET @colonnesAbsentes += N'ref.TypeCourriel.Libelle; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[TypeCourriel]',N'EstActif') IS NULL SET @colonnesAbsentes += N'ref.TypeCourriel.EstActif; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[TypePermis]',N'TypePermisId') IS NULL SET @colonnesAbsentes += N'ref.TypePermis.TypePermisId; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[TypePermis]',N'Libelle') IS NULL SET @colonnesAbsentes += N'ref.TypePermis.Libelle; ';
IF COL_LENGTH(N'[bd_cmdpsf].[ref].[TypePermis]',N'EstActif') IS NULL SET @colonnesAbsentes += N'ref.TypePermis.EstActif; ';
IF LEN(@colonnesAbsentes)>0
BEGIN
 PRINT LEFT(@colonnesAbsentes,4000);
 THROW 51025,N'Colonnes de destination absentes : verifier le schema.',1;
END;

BEGIN TRY
    BEGIN TRANSACTION;
    PRINT N'Debut migration vers bd_cmdpsf';
    -- Les clefs sont restaurees et verifiees avant la validation finale.
    DECLARE @schema sysname, @table sysname;
    DECLARE curFk CURSOR LOCAL FAST_FORWARD FOR
      SELECT s.name,t.name FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
      WHERE t.is_ms_shipped=0;
    OPEN curFk; FETCH NEXT FROM curFk INTO @schema,@table;
    WHILE @@FETCH_STATUS=0
    BEGIN
      SET @sql=N'ALTER TABLE '+QUOTENAME(@schema)+N'.'+QUOTENAME(@table)+N' NOCHECK CONSTRAINT ALL;';
      EXEC sys.sp_executesql @sql;
      FETCH NEXT FROM curFk INTO @schema,@table;
    END;
    CLOSE curFk; DEALLOCATE curFk;

    -- La table ref.Profession est pre-remplie par le script de creation.
    -- Remplacer ces seuls codes initiaux pour conserver les ProfessionId de la source.
    IF EXISTS (SELECT 1 FROM ref.Profession WHERE CodeProfession NOT IN
          (N'MEDECIN',N'DENTISTE',N'PHARMACIEN',N'SAGE_FEMME'))
         THROW 51026,N'Le referentiel ref.Profession cible contient des valeurs non initiales.',1;
    DELETE FROM ref.Profession;
    PRINT N'Migration : ref.StatutMedecin => ref.StatutProfessionnel';
    SET IDENTITY_INSERT [ref].[StatutProfessionnel] ON;
    INSERT INTO [ref].[StatutProfessionnel] ([StatutProfessionnelId], [CodeStatut], [Libelle], [EstActif])
    SELECT [StatutMedecinId], [CodeStatut], [Libelle], [EstActif] FROM [RDM_CEMTL].[ref].[StatutMedecin];
    SET IDENTITY_INSERT [ref].[StatutProfessionnel] OFF;

    PRINT N'Migration : ref.CategoriePratique => ref.CategoriePratique';
    SET IDENTITY_INSERT [ref].[CategoriePratique] ON;
    INSERT INTO [ref].[CategoriePratique] ([CategoriePratiqueId], [Libelle], [EstActif])
    SELECT [CategoriePratiqueId], [Libelle], [EstActif] FROM [RDM_CEMTL].[ref].[CategoriePratique];
    SET IDENTITY_INSERT [ref].[CategoriePratique] OFF;

    PRINT N'Migration : dbo.Medecin => dbo.Professionnel';
    SET IDENTITY_INSERT [dbo].[Professionnel] ON;
    INSERT INTO [dbo].[Professionnel] ([ProfessionnelId], [Permis], [TitreId], [Nom], [Prenom], [TypePermisId], [StatutProfessionnelId], [CategoriePratiqueId], [GenreId], [DateNaissance], [NAM], [DateEntreeFonction], [DateDepart], [Commentaires], [EstActif], [DateCreation], [DateModification], [ProfessionId], [DateObtentionPermis], [DateExpirationPermis], [NumeroMembreOrdre], [DateDerniereVerificationOrdre], [ReferenceVerificationOrdre])
    SELECT [MedecinId], [Permis], [TitreId], [Nom], [Prenom], [TypePermisId], [StatutMedecinId], [CategoriePratiqueId], [GenreId], [DateNaissance], [NAM], [DateEntreeFonction], [DateDepart], [Commentaires], [EstActif], [DateCreation], [DateModification], [ProfessionId], [DateObtentionPermis], [DateExpirationPermis], [NumeroMembreOrdre], [DateDerniereVerificationOrdre], [ReferenceVerificationOrdre] FROM [RDM_CEMTL].[dbo].[Medecin];
    SET IDENTITY_INSERT [dbo].[Professionnel] OFF;

    PRINT N'Migration : dbo.CodeCotisation => dbo.CodeCotisation';
    SET IDENTITY_INSERT [dbo].[CodeCotisation] ON;
    INSERT INTO [dbo].[CodeCotisation] ([CodeCotisationId], [CodeCotisation], [Description], [EstActif])
    SELECT [CodeCotisationId], [CodeCotisation], [Description], [EstActif] FROM [RDM_CEMTL].[dbo].[CodeCotisation];
    SET IDENTITY_INSERT [dbo].[CodeCotisation] OFF;

    PRINT N'Migration : dbo.CotisationAnnuelle => dbo.CotisationAnnuelle';
    SET IDENTITY_INSERT [dbo].[CotisationAnnuelle] ON;
    INSERT INTO [dbo].[CotisationAnnuelle] ([CotisationAnnuelleId], [ProfessionnelId], [AnneeExercice], [CodeCotisationId], [DescriptionSource], [MontantInitial], [ReductionAppliquee], [MotifReduction], [MontantReduction], [PenaliteAppliquee], [PenaliteTexte], [MontantPenalite], [MontantFinal], [EtatCotisation], [MontantPaye], [DatePaiement], [TypePaiementTexte], [TypePaiementId], [DateDepot], [NoDepot], [RecuImposition], [DateRecuImposition], [DateCreation])
    SELECT [CotisationAnnuelleId], [MedecinId], [AnneeExercice], [CodeCotisationId], [DescriptionSource], [MontantInitial], [ReductionAppliquee], [MotifReduction], [MontantReduction], [PenaliteAppliquee], [PenaliteTexte], [MontantPenalite], [MontantFinal], [EtatCotisation], [MontantPaye], [DatePaiement], [TypePaiementTexte], [TypePaiementId], [DateDepot], [NoDepot], [RecuImposition], [DateRecuImposition], [DateCreation] FROM [RDM_CEMTL].[dbo].[CotisationAnnuelle];
    SET IDENTITY_INSERT [dbo].[CotisationAnnuelle] OFF;

    PRINT N'Migration : dbo.Departement => dbo.Departement';
    SET IDENTITY_INSERT [dbo].[Departement] ON;
    INSERT INTO [dbo].[Departement] ([DepartementId], [NomDepartement], [CodeDepartement], [EstActif])
    SELECT [DepartementId], [NomDepartement], [CodeDepartement], [EstActif] FROM [RDM_CEMTL].[dbo].[Departement];
    SET IDENTITY_INSERT [dbo].[Departement] OFF;

    PRINT N'Migration : dbo.AffectationOrganisationnelle => dbo.AffectationOrganisationnelle';
    SET IDENTITY_INSERT [dbo].[AffectationOrganisationnelle] ON;
    INSERT INTO [dbo].[AffectationOrganisationnelle] ([AffectationOrganisationnelleId], [DepartementId], [ServiceId], [ProfessionnelId], [RoleCode], [NomAffiche], [Titre], [Courriel], [CourrielSource], [Telephone], [PosteTelephone], [InstallationId], [SiteNomSource], [RegroupementSource], [DateDebut], [DateFin], [EstInterim], [EstActive], [EstAValider], [SourceDonnees], [SourceLigne], [DateCreation], [DateModification])
    SELECT [AffectationOrganisationnelleId], [DepartementId], [ServiceId], [MedecinId], [RoleCode], [NomAffiche], [Titre], [Courriel], [CourrielSource], [Telephone], [PosteTelephone], [InstallationId], [SiteNomSource], [RegroupementSource], [DateDebut], [DateFin], [EstInterim], [EstActive], [EstAValider], [SourceDonnees], [SourceLigne], [DateCreation], [DateModification] FROM [RDM_CEMTL].[dbo].[AffectationOrganisationnelle];
    SET IDENTITY_INSERT [dbo].[AffectationOrganisationnelle] OFF;

    PRINT N'Migration : dbo.Service => dbo.Service';
    SET IDENTITY_INSERT [dbo].[Service] ON;
    INSERT INTO [dbo].[Service] ([ServiceId], [DepartementId], [NomService], [CodeService], [EstActif], [RegroupementSource])
    SELECT [ServiceId], [DepartementId], [NomService], [CodeService], [EstActif], [RegroupementSource] FROM [RDM_CEMTL].[dbo].[Service];
    SET IDENTITY_INSERT [dbo].[Service] OFF;

    PRINT N'Migration : dbo.Installation => dbo.Installation';
    SET IDENTITY_INSERT [dbo].[Installation] ON;
    INSERT INTO [dbo].[Installation] ([InstallationId], [NomInstallation], [CodeInstallation], [EstActif])
    SELECT [InstallationId], [NomInstallation], [CodeInstallation], [EstActif] FROM [RDM_CEMTL].[dbo].[Installation];
    SET IDENTITY_INSERT [dbo].[Installation] OFF;

    PRINT N'Migration : ref.Profession => ref.Profession';
    SET IDENTITY_INSERT [ref].[Profession] ON;
    INSERT INTO [ref].[Profession] ([ProfessionId], [CodeProfession], [Libelle], [EstActif])
    SELECT [ProfessionId], [CodeProfession], [Libelle], [EstActif] FROM [RDM_CEMTL].[ref].[Profession];
    SET IDENTITY_INSERT [ref].[Profession] OFF;

    PRINT N'Migration : dbo.__MigrationHistory => dbo.__MigrationHistory';
    INSERT INTO [dbo].[__MigrationHistory] ([MigrationId], [ContextKey], [Model], [ProductVersion])
    SELECT [MigrationId], [ContextKey], [Model], [ProductVersion] FROM [RDM_CEMTL].[dbo].[__MigrationHistory];

    -- Fusion non destructive des comptes ASP.NET Identity.
    -- Un compte cible de meme UserName est conserve, y compris son ID et son hash.
    -- Les roles sont identifies par Name, pas par Id, car les GUID peuvent differer.
    PRINT N'Fusion des comptes ASP.NET Identity';
    IF EXISTS (
      SELECT 1 FROM [RDM_CEMTL].dbo.AspNetUsers src
      JOIN dbo.AspNetUsers dst ON dst.Id = src.Id
      WHERE dst.UserName <> src.UserName
    ) THROW 51131, N'Conflit AspNetUsers : meme Id attribue a des comptes differents.', 1;
    IF EXISTS (
      SELECT 1 FROM [RDM_CEMTL].dbo.AspNetRoles src
      JOIN dbo.AspNetRoles dst ON dst.Id = src.Id
      WHERE dst.Name <> src.Name
    ) THROW 51132, N'Conflit AspNetRoles : meme Id attribue a des roles differents.', 1;

    -- Identifiants existants ne sont jamais modifies.
    INSERT dbo.AspNetRoles (Id,Name)
    SELECT src.Id,src.Name FROM [RDM_CEMTL].dbo.AspNetRoles src
    WHERE NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles dst WHERE dst.Name=src.Name OR dst.Id=src.Id);

    INSERT dbo.AspNetUsers
      (Id,Nom,Prenom,Email,PhoneNumber,UserName,EmailConfirmed,PasswordHash,SecurityStamp,
       PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount)
    SELECT src.Id,src.Nom,src.Prenom,src.Email,src.PhoneNumber,src.UserName,src.EmailConfirmed,
           src.PasswordHash,src.SecurityStamp,src.PhoneNumberConfirmed,src.TwoFactorEnabled,
           src.LockoutEndDateUtc,src.LockoutEnabled,src.AccessFailedCount
    FROM [RDM_CEMTL].dbo.AspNetUsers src
    WHERE NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers dst WHERE dst.UserName=src.UserName OR dst.Id=src.Id);

    -- Mapping stable des GUID lorsque des comptes/roles portent le meme nom.
    CREATE TABLE #MapUsers(SourceId nvarchar(128) NOT NULL PRIMARY KEY, CibleId nvarchar(128) NOT NULL);
    INSERT #MapUsers (SourceId,CibleId)
    SELECT src.Id,dst.Id FROM [RDM_CEMTL].dbo.AspNetUsers src
    JOIN dbo.AspNetUsers dst ON dst.UserName=src.UserName;
    IF (SELECT COUNT_BIG(*) FROM #MapUsers) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].dbo.AspNetUsers)
       THROW 51133,N'Correspondance de comptes Identity incomplete.',1;

    CREATE TABLE #MapRoles(SourceId nvarchar(128) NOT NULL PRIMARY KEY, CibleId nvarchar(128) NOT NULL);
    INSERT #MapRoles(SourceId,CibleId)
    SELECT src.Id,dst.Id FROM [RDM_CEMTL].dbo.AspNetRoles src
    JOIN dbo.AspNetRoles dst ON dst.Name=src.Name;
    IF (SELECT COUNT_BIG(*) FROM #MapRoles) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].dbo.AspNetRoles)
       THROW 51134,N'Correspondance de roles Identity incomplete.',1;

    -- Ne supprimer aucun role existant, notamment BootstrapAdmin.
    INSERT dbo.AspNetUserRoles (UserId,RoleId)
    SELECT mu.CibleId,mr.CibleId
    FROM [RDM_CEMTL].dbo.AspNetUserRoles src
    JOIN #MapUsers mu ON mu.SourceId=src.UserId
    JOIN #MapRoles mr ON mr.SourceId=src.RoleId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles dst
                      WHERE dst.UserId=mu.CibleId AND dst.RoleId=mr.CibleId);

    -- Claims : Id INT IDENTITY local a chaque base; generer de nouveaux Id.
    -- Conserver la multiplicite exacte des claims source, meme en cas de doublons.
    INSERT dbo.AspNetUserClaims (UserId,ClaimType,ClaimValue)
    SELECT mu.CibleId,src.ClaimType,src.ClaimValue
    FROM [RDM_CEMTL].dbo.AspNetUserClaims src
    JOIN #MapUsers mu ON mu.SourceId=src.UserId;

    -- Logins : proteger les cles externes qui seraient deja attribuees a un autre compte.
    IF EXISTS (
       SELECT 1 FROM [RDM_CEMTL].dbo.AspNetUserLogins src
       JOIN #MapUsers mu ON mu.SourceId=src.UserId
       JOIN dbo.AspNetUserLogins dst ON dst.LoginProvider=src.LoginProvider AND dst.ProviderKey=src.ProviderKey
       WHERE dst.UserId <> mu.CibleId
    ) THROW 51135,N'Conflit AspNetUserLogins : un login externe appartient a un autre compte.',1;
    INSERT dbo.AspNetUserLogins (LoginProvider,ProviderKey,UserId)
    SELECT src.LoginProvider,src.ProviderKey,mu.CibleId
    FROM [RDM_CEMTL].dbo.AspNetUserLogins src
    JOIN #MapUsers mu ON mu.SourceId=src.UserId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.AspNetUserLogins dst
                      WHERE dst.LoginProvider=src.LoginProvider AND dst.ProviderKey=src.ProviderKey);

    PRINT N'Migration : dbo.AuditLog => dbo.AuditLog';
    SET IDENTITY_INSERT [dbo].[AuditLog] ON;
    INSERT INTO [dbo].[AuditLog] ([AuditLogId], [TableName], [RecordId], [Action], [OldValues], [NewValues], [ChangedFields], [UtilisateurId], [NomUtilisateur], [DateAction], [AdresseIP], [UserAgent])
    SELECT [AuditLogId], [TableName], [RecordId], [Action], [OldValues], [NewValues], [ChangedFields], [UtilisateurId], [NomUtilisateur], [DateAction], [AdresseIP], [UserAgent] FROM [RDM_CEMTL].[dbo].[AuditLog];
    SET IDENTITY_INSERT [dbo].[AuditLog] OFF;

    PRINT N'Migration : dbo.Comite => dbo.Comite';
    SET IDENTITY_INSERT [dbo].[Comite] ON;
    INSERT INTO [dbo].[Comite] ([ComiteId], [NumeroSource], [NomComite], [EstActif])
    SELECT [ComiteId], [NumeroSource], [NomComite], [EstActif] FROM [RDM_CEMTL].[dbo].[Comite];
    SET IDENTITY_INSERT [dbo].[Comite] OFF;

    PRINT N'Migration : dbo.DocumentMedecin => dbo.DocumentProfessionnel';
    SET IDENTITY_INSERT [dbo].[DocumentProfessionnel] ON;
    INSERT INTO [dbo].[DocumentProfessionnel] ([DocumentProfessionnelId], [ProfessionnelId], [TypeDocument], [NomFichier], [ContentType], [TailleOctets], [HashSHA256], [DateDocument], [DateExpiration], [Description], [EntiteLiee], [EntiteId], [Confidentiel], [Contenu], [DateDepot], [DeposePar], [EstActif])
    SELECT [DocumentMedecinId], [MedecinId], [TypeDocument], [NomFichier], [ContentType], [TailleOctets], [HashSHA256], [DateDocument], [DateExpiration], [Description], [EntiteLiee], [EntiteId], [Confidentiel], [Contenu], [DateDepot], [DeposePar], [EstActif] FROM [RDM_CEMTL].[dbo].[DocumentMedecin];
    SET IDENTITY_INSERT [dbo].[DocumentProfessionnel] OFF;

    PRINT N'Migration : dbo.EntenteFDC => dbo.EntenteFDC';
    SET IDENTITY_INSERT [dbo].[EntenteFDC] ON;
    INSERT INTO [dbo].[EntenteFDC] ([EntenteFDCId], [ProfessionnelId], [TypeEntenteFDC], [DateAdhesion], [DateFin], [PEMVise], [EstActive])
    SELECT [EntenteFDCId], [MedecinId], [TypeEntenteFDC], [DateAdhesion], [DateFin], [PEMVise], [EstActive] FROM [RDM_CEMTL].[dbo].[EntenteFDC];
    SET IDENTITY_INSERT [dbo].[EntenteFDC] OFF;

    PRINT N'Migration : dbo.FitTest => dbo.FitTest';
    SET IDENTITY_INSERT [dbo].[FitTest] ON;
    INSERT INTO [dbo].[FitTest] ([FitTestId], [ProfessionnelId], [Resultat], [DateFitTest], [Commentaire], [EstActif])
    SELECT [FitTestId], [MedecinId], [Resultat], [DateFitTest], [Commentaire], [EstActif] FROM [RDM_CEMTL].[dbo].[FitTest];
    SET IDENTITY_INSERT [dbo].[FitTest] OFF;

    PRINT N'Migration : dbo.InstallationService => dbo.InstallationService';
    SET IDENTITY_INSERT [dbo].[InstallationService] ON;
    INSERT INTO [dbo].[InstallationService] ([InstallationServiceId], [InstallationId], [ServiceId], [DateDebut], [DateFin], [EstActif])
    SELECT [InstallationServiceId], [InstallationId], [ServiceId], [DateDebut], [DateFin], [EstActif] FROM [RDM_CEMTL].[dbo].[InstallationService];
    SET IDENTITY_INSERT [dbo].[InstallationService] OFF;

    PRINT N'Migration : dbo.InstanceDecision => dbo.InstanceDecision';
    SET IDENTITY_INSERT [dbo].[InstanceDecision] ON;
    INSERT INTO [dbo].[InstanceDecision] ([InstanceDecisionId], [TypeInstanceDecisionId], [CodeInstance], [DateInstance], [LibelleInstance])
    SELECT [InstanceDecisionId], [TypeInstanceDecisionId], [CodeInstance], [DateInstance], [LibelleInstance] FROM [RDM_CEMTL].[dbo].[InstanceDecision];
    SET IDENTITY_INSERT [dbo].[InstanceDecision] OFF;

    PRINT N'Migration : dbo.JournalImport => dbo.JournalImport';
    SET IDENTITY_INSERT [dbo].[JournalImport] ON;
    INSERT INTO [dbo].[JournalImport] ([JournalImportId], [NomFichier], [DateImport], [LignesImportees], [LignesRejetees], [Commentaire])
    SELECT [JournalImportId], [NomFichier], [DateImport], [LignesImportees], [LignesRejetees], [Commentaire] FROM [RDM_CEMTL].[dbo].[JournalImport];
    SET IDENTITY_INSERT [dbo].[JournalImport] OFF;

    PRINT N'Migration : dbo.MedecinAdresse => dbo.ProfessionnelAdresse';
    SET IDENTITY_INSERT [dbo].[ProfessionnelAdresse] ON;
    INSERT INTO [dbo].[ProfessionnelAdresse] ([ProfessionnelAdresseId], [ProfessionnelId], [AdresseLigne1], [Ville], [Province], [CodePostal], [EstPrincipale])
    SELECT [MedecinAdresseId], [MedecinId], [AdresseLigne1], [Ville], [Province], [CodePostal], [EstPrincipale] FROM [RDM_CEMTL].[dbo].[MedecinAdresse];
    SET IDENTITY_INSERT [dbo].[ProfessionnelAdresse] OFF;

    PRINT N'Migration : dbo.MedecinAffectationClinique => dbo.ProfessionnelAffectationClinique';
    SET IDENTITY_INSERT [dbo].[ProfessionnelAffectationClinique] ON;
    INSERT INTO [dbo].[ProfessionnelAffectationClinique] ([ProfessionnelAffectationCliniqueId], [ProfessionnelId], [DepartementId], [ServiceId], [InstallationId], [TypeAffectation], [EstPrincipale], [DateDebut], [DateFin], [EstActive], [Commentaire], [SourceDonnees], [DateCreation], [DateModification])
    SELECT [MedecinAffectationCliniqueId], [MedecinId], [DepartementId], [ServiceId], [InstallationId], [TypeAffectation], [EstPrincipale], [DateDebut], [DateFin], [EstActive], [Commentaire], [SourceDonnees], [DateCreation], [DateModification] FROM [RDM_CEMTL].[dbo].[MedecinAffectationClinique];
    SET IDENTITY_INSERT [dbo].[ProfessionnelAffectationClinique] OFF;

    PRINT N'Migration : dbo.MedecinComite => dbo.ProfessionnelComite';
    SET IDENTITY_INSERT [dbo].[ProfessionnelComite] ON;
    INSERT INTO [dbo].[ProfessionnelComite] ([ProfessionnelComiteId], [ProfessionnelId], [ComiteId], [RoleMembre], [DateDebut], [DateFin], [Commentaire])
    SELECT [MedecinComiteId], [MedecinId], [ComiteId], [RoleMembre], [DateDebut], [DateFin], [Commentaire] FROM [RDM_CEMTL].[dbo].[MedecinComite];
    SET IDENTITY_INSERT [dbo].[ProfessionnelComite] OFF;

    PRINT N'Migration : dbo.MedecinCourriel => dbo.ProfessionnelCourriel';
    SET IDENTITY_INSERT [dbo].[ProfessionnelCourriel] ON;
    INSERT INTO [dbo].[ProfessionnelCourriel] ([ProfessionnelCourrielId], [ProfessionnelId], [TypeCourriel], [AdresseCourriel], [RangSource], [EstPrincipal], [TypeCourrielId])
    SELECT [MedecinCourrielId], [MedecinId], [TypeCourriel], [AdresseCourriel], [RangSource], [EstPrincipal], [TypeCourrielId] FROM [RDM_CEMTL].[dbo].[MedecinCourriel];
    SET IDENTITY_INSERT [dbo].[ProfessionnelCourriel] OFF;

    PRINT N'Migration : dbo.MedecinInstallation => dbo.ProfessionnelInstallation';
    SET IDENTITY_INSERT [dbo].[ProfessionnelInstallation] ON;
    INSERT INTO [dbo].[ProfessionnelInstallation] ([ProfessionnelInstallationId], [ProfessionnelId], [InstallationId], [RangInstallation], [DateDebut], [DateFin], [EstPrincipale])
    SELECT [MedecinInstallationId], [MedecinId], [InstallationId], [RangInstallation], [DateDebut], [DateFin], [EstPrincipale] FROM [RDM_CEMTL].[dbo].[MedecinInstallation];
    SET IDENTITY_INSERT [dbo].[ProfessionnelInstallation] OFF;

    PRINT N'Migration : dbo.MedecinPrivilege => dbo.ProfessionnelPrivilege';
    SET IDENTITY_INSERT [dbo].[ProfessionnelPrivilege] ON;
    INSERT INTO [dbo].[ProfessionnelPrivilege] ([ProfessionnelPrivilegeId], [ProfessionnelId], [PrivilegeReferenceId], [InstallationId], [ServiceId], [DateDebut], [DateFin], [Etat], [NatureActivites], [ChampActivites], [Obligations], [Commentaire], [NumeroResolutionCA], [DateResolutionCA], [ReconnaissanceEcriteRecue], [EstActif], [DateCreation], [DateModification])
    SELECT [MedecinPrivilegeId], [MedecinId], [PrivilegeReferenceId], [InstallationId], [ServiceId], [DateDebut], [DateFin], [Etat], [NatureActivites], [ChampActivites], [Obligations], [Commentaire], [NumeroResolutionCA], [DateResolutionCA], [ReconnaissanceEcriteRecue], [EstActif], [DateCreation], [DateModification] FROM [RDM_CEMTL].[dbo].[MedecinPrivilege];
    SET IDENTITY_INSERT [dbo].[ProfessionnelPrivilege] OFF;

    PRINT N'Migration : dbo.MedecinTelephone => dbo.ProfessionnelTelephone';
    SET IDENTITY_INSERT [dbo].[ProfessionnelTelephone] ON;
    INSERT INTO [dbo].[ProfessionnelTelephone] ([ProfessionnelTelephoneId], [ProfessionnelId], [TypeTelephone], [NumeroTelephone], [RangSource], [EstPrincipal])
    SELECT [MedecinTelephoneId], [MedecinId], [TypeTelephone], [NumeroTelephone], [RangSource], [EstPrincipal] FROM [RDM_CEMTL].[dbo].[MedecinTelephone];
    SET IDENTITY_INSERT [dbo].[ProfessionnelTelephone] OFF;

    PRINT N'Migration : dbo.Nomination => dbo.Nomination';
    SET IDENTITY_INSERT [dbo].[Nomination] ON;
    INSERT INTO [dbo].[Nomination] ([NominationId], [CodeNomination], [LibelleNomination], [Description], [EstActif], [DateCreation], [DateModification])
    SELECT [NominationId], [CodeNomination], [LibelleNomination], [Description], [EstActif], [DateCreation], [DateModification] FROM [RDM_CEMTL].[dbo].[Nomination];
    SET IDENTITY_INSERT [dbo].[Nomination] OFF;

    PRINT N'Migration : dbo.NominationAdministrative => dbo.NominationAdministrative';
    SET IDENTITY_INSERT [dbo].[NominationAdministrative] ON;
    INSERT INTO [dbo].[NominationAdministrative] ([NominationAdministrativeId], [ProfessionnelId], [LibelleNomination], [DateNomination], [DateFin], [EstActive], [NominationId], [TypeNominationId])
    SELECT [NominationAdministrativeId], [MedecinId], [LibelleNomination], [DateNomination], [DateFin], [EstActive], [NominationId], [TypeNominationId] FROM [RDM_CEMTL].[dbo].[NominationAdministrative];
    SET IDENTITY_INSERT [dbo].[NominationAdministrative] OFF;

    PRINT N'Migration : dbo.PrivilegeReference => dbo.PrivilegeReference';
    SET IDENTITY_INSERT [dbo].[PrivilegeReference] ON;
    INSERT INTO [dbo].[PrivilegeReference] ([PrivilegeReferenceId], [CodePrivilege], [LibellePrivilege], [Categorie], [Description], [NecessiteBlocOperatoire], [NecessiteInstallation], [NecessiteUnite], [OrdreAffichage], [EstActif], [DateCreation], [DateModification])
    SELECT [PrivilegeReferenceId], [CodePrivilege], [LibellePrivilege], [Categorie], [Description], [NecessiteBlocOperatoire], [NecessiteInstallation], [NecessiteUnite], [OrdreAffichage], [EstActif], [DateCreation], [DateModification] FROM [RDM_CEMTL].[dbo].[PrivilegeReference];
    SET IDENTITY_INSERT [dbo].[PrivilegeReference] OFF;

    PRINT N'Migration : dbo.TypeInstanceDecision => dbo.TypeInstanceDecision';
    SET IDENTITY_INSERT [dbo].[TypeInstanceDecision] ON;
    INSERT INTO [dbo].[TypeInstanceDecision] ([TypeInstanceDecisionId], [CodeType], [Libelle])
    SELECT [TypeInstanceDecisionId], [CodeType], [Libelle] FROM [RDM_CEMTL].[dbo].[TypeInstanceDecision];
    SET IDENTITY_INSERT [dbo].[TypeInstanceDecision] OFF;

    PRINT N'Migration : dbo.TypeNomination => dbo.TypeNomination';
    SET IDENTITY_INSERT [dbo].[TypeNomination] ON;
    INSERT INTO [dbo].[TypeNomination] ([TypeNominationId], [CodeTypeNomination], [LibelleTypeNomination], [Description], [EstActif], [DateCreation], [DateModification])
    SELECT [TypeNominationId], [CodeTypeNomination], [LibelleTypeNomination], [Description], [EstActif], [DateCreation], [DateModification] FROM [RDM_CEMTL].[dbo].[TypeNomination];
    SET IDENTITY_INSERT [dbo].[TypeNomination] OFF;

    PRINT N'Migration : dbo.TypePaiement => dbo.TypePaiement';
    SET IDENTITY_INSERT [dbo].[TypePaiement] ON;
    INSERT INTO [dbo].[TypePaiement] ([TypePaiementId], [Libelle], [EstActif])
    SELECT [TypePaiementId], [Libelle], [EstActif] FROM [RDM_CEMTL].[dbo].[TypePaiement];
    SET IDENTITY_INSERT [dbo].[TypePaiement] OFF;

    PRINT N'Migration : dbo.UtilisateurMetier => dbo.UtilisateurMetier';
    SET IDENTITY_INSERT [dbo].[UtilisateurMetier] ON;
    INSERT INTO [dbo].[UtilisateurMetier] ([UtilisateurMetierId], [IdentifiantSource], [Courriel], [NomComplet], [Connexion], [AspNetUserId], [EstActif])
    SELECT src.[UtilisateurMetierId], src.[IdentifiantSource], src.[Courriel], src.[NomComplet], src.[Connexion], COALESCE(mu.CibleId,src.[AspNetUserId]), src.[EstActif] FROM [RDM_CEMTL].[dbo].[UtilisateurMetier] src LEFT JOIN #MapUsers mu ON mu.SourceId=src.[AspNetUserId];
    SET IDENTITY_INSERT [dbo].[UtilisateurMetier] OFF;

    PRINT N'Migration : ref.Genre => ref.Genre';
    SET IDENTITY_INSERT [ref].[Genre] ON;
    INSERT INTO [ref].[Genre] ([GenreId], [CodeGenre], [Libelle])
    SELECT [GenreId], [CodeGenre], [Libelle] FROM [RDM_CEMTL].[ref].[Genre];
    SET IDENTITY_INSERT [ref].[Genre] OFF;

    PRINT N'Migration : ref.TitreMedecin => ref.TitreProfessionnel';
    SET IDENTITY_INSERT [ref].[TitreProfessionnel] ON;
    INSERT INTO [ref].[TitreProfessionnel] ([TitreId], [Libelle], [EstActif])
    SELECT [TitreId], [Libelle], [EstActif] FROM [RDM_CEMTL].[ref].[TitreMedecin];
    SET IDENTITY_INSERT [ref].[TitreProfessionnel] OFF;

    PRINT N'Migration : ref.TypeCourriel => ref.TypeCourriel';
    SET IDENTITY_INSERT [ref].[TypeCourriel] ON;
    INSERT INTO [ref].[TypeCourriel] ([TypeCourrielId], [Libelle], [EstActif])
    SELECT [TypeCourrielId], [Libelle], [EstActif] FROM [RDM_CEMTL].[ref].[TypeCourriel];
    SET IDENTITY_INSERT [ref].[TypeCourriel] OFF;

    PRINT N'Migration : ref.TypePermis => ref.TypePermis';
    SET IDENTITY_INSERT [ref].[TypePermis] ON;
    INSERT INTO [ref].[TypePermis] ([TypePermisId], [Libelle], [EstActif])
    SELECT [TypePermisId], [Libelle], [EstActif] FROM [RDM_CEMTL].[ref].[TypePermis];
    SET IDENTITY_INSERT [ref].[TypePermis] OFF;

    -- Verifier toutes les cardinalites avant de valider.
    IF (SELECT COUNT_BIG(*) FROM [ref].[StatutProfessionnel]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[ref].[StatutMedecin])
        THROW 51100, N'Nombre de lignes non conforme : ref.StatutProfessionnel',1;
    IF (SELECT COUNT_BIG(*) FROM [ref].[CategoriePratique]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[ref].[CategoriePratique])
        THROW 51100, N'Nombre de lignes non conforme : ref.CategoriePratique',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[Professionnel]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[Medecin])
        THROW 51100, N'Nombre de lignes non conforme : dbo.Professionnel',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[CodeCotisation]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[CodeCotisation])
        THROW 51100, N'Nombre de lignes non conforme : dbo.CodeCotisation',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[CotisationAnnuelle]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[CotisationAnnuelle])
        THROW 51100, N'Nombre de lignes non conforme : dbo.CotisationAnnuelle',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[Departement]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[Departement])
        THROW 51100, N'Nombre de lignes non conforme : dbo.Departement',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[AffectationOrganisationnelle]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[AffectationOrganisationnelle])
        THROW 51100, N'Nombre de lignes non conforme : dbo.AffectationOrganisationnelle',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[Service]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[Service])
        THROW 51100, N'Nombre de lignes non conforme : dbo.Service',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[Installation]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[Installation])
        THROW 51100, N'Nombre de lignes non conforme : dbo.Installation',1;
    IF (SELECT COUNT_BIG(*) FROM [ref].[Profession]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[ref].[Profession])
        THROW 51100, N'Nombre de lignes non conforme : ref.Profession',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[__MigrationHistory]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[__MigrationHistory])
        THROW 51100, N'Nombre de lignes non conforme : dbo.__MigrationHistory',1;
    -- Verifier la couverture des lignes source sans interdire les comptes deja presents.
    IF EXISTS (SELECT 1 FROM [RDM_CEMTL].dbo.AspNetRoles src WHERE NOT EXISTS
              (SELECT 1 FROM dbo.AspNetRoles dst WHERE dst.Name=src.Name))
        THROW 51100,N'Role source manquant dans la destination.',1;
    IF EXISTS (SELECT 1 FROM [RDM_CEMTL].dbo.AspNetUsers src WHERE NOT EXISTS
              (SELECT 1 FROM dbo.AspNetUsers dst WHERE dst.UserName=src.UserName))
        THROW 51100,N'Utilisateur source manquant dans la destination.',1;
    IF EXISTS (SELECT 1 FROM [RDM_CEMTL].dbo.AspNetUserRoles src
               JOIN #MapUsers mu ON mu.SourceId=src.UserId
               JOIN #MapRoles mr ON mr.SourceId=src.RoleId
               WHERE NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles dst
                                 WHERE dst.UserId=mu.CibleId AND dst.RoleId=mr.CibleId))
        THROW 51100,N'Attribution de role manquante.',1;
    IF EXISTS (SELECT 1 FROM [RDM_CEMTL].dbo.AspNetUserLogins src
               JOIN #MapUsers mu ON mu.SourceId=src.UserId
               WHERE NOT EXISTS (SELECT 1 FROM dbo.AspNetUserLogins dst
                  WHERE dst.LoginProvider=src.LoginProvider AND dst.ProviderKey=src.ProviderKey
                  AND dst.UserId=mu.CibleId))
        THROW 51100,N'Login externe manquant.',1;
    IF (SELECT COUNT_BIG(*) FROM dbo.AspNetUserClaims) <
       (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].dbo.AspNetUserClaims)
        THROW 51100,N'Nombre de claims insuffisant.',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[AuditLog]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[AuditLog])
        THROW 51100, N'Nombre de lignes non conforme : dbo.AuditLog',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[Comite]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[Comite])
        THROW 51100, N'Nombre de lignes non conforme : dbo.Comite',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[DocumentProfessionnel]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[DocumentMedecin])
        THROW 51100, N'Nombre de lignes non conforme : dbo.DocumentProfessionnel',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[EntenteFDC]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[EntenteFDC])
        THROW 51100, N'Nombre de lignes non conforme : dbo.EntenteFDC',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[FitTest]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[FitTest])
        THROW 51100, N'Nombre de lignes non conforme : dbo.FitTest',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[InstallationService]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[InstallationService])
        THROW 51100, N'Nombre de lignes non conforme : dbo.InstallationService',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[InstanceDecision]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[InstanceDecision])
        THROW 51100, N'Nombre de lignes non conforme : dbo.InstanceDecision',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[JournalImport]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[JournalImport])
        THROW 51100, N'Nombre de lignes non conforme : dbo.JournalImport',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[ProfessionnelAdresse]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[MedecinAdresse])
        THROW 51100, N'Nombre de lignes non conforme : dbo.ProfessionnelAdresse',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[ProfessionnelAffectationClinique]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[MedecinAffectationClinique])
        THROW 51100, N'Nombre de lignes non conforme : dbo.ProfessionnelAffectationClinique',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[ProfessionnelComite]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[MedecinComite])
        THROW 51100, N'Nombre de lignes non conforme : dbo.ProfessionnelComite',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[ProfessionnelCourriel]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[MedecinCourriel])
        THROW 51100, N'Nombre de lignes non conforme : dbo.ProfessionnelCourriel',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[ProfessionnelInstallation]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[MedecinInstallation])
        THROW 51100, N'Nombre de lignes non conforme : dbo.ProfessionnelInstallation',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[ProfessionnelPrivilege]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[MedecinPrivilege])
        THROW 51100, N'Nombre de lignes non conforme : dbo.ProfessionnelPrivilege',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[ProfessionnelTelephone]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[MedecinTelephone])
        THROW 51100, N'Nombre de lignes non conforme : dbo.ProfessionnelTelephone',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[Nomination]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[Nomination])
        THROW 51100, N'Nombre de lignes non conforme : dbo.Nomination',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[NominationAdministrative]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[NominationAdministrative])
        THROW 51100, N'Nombre de lignes non conforme : dbo.NominationAdministrative',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[PrivilegeReference]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[PrivilegeReference])
        THROW 51100, N'Nombre de lignes non conforme : dbo.PrivilegeReference',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[TypeInstanceDecision]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[TypeInstanceDecision])
        THROW 51100, N'Nombre de lignes non conforme : dbo.TypeInstanceDecision',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[TypeNomination]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[TypeNomination])
        THROW 51100, N'Nombre de lignes non conforme : dbo.TypeNomination',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[TypePaiement]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[TypePaiement])
        THROW 51100, N'Nombre de lignes non conforme : dbo.TypePaiement',1;
    IF (SELECT COUNT_BIG(*) FROM [dbo].[UtilisateurMetier]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[dbo].[UtilisateurMetier])
        THROW 51100, N'Nombre de lignes non conforme : dbo.UtilisateurMetier',1;
    IF (SELECT COUNT_BIG(*) FROM [ref].[Genre]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[ref].[Genre])
        THROW 51100, N'Nombre de lignes non conforme : ref.Genre',1;
    IF (SELECT COUNT_BIG(*) FROM [ref].[TitreProfessionnel]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[ref].[TitreMedecin])
        THROW 51100, N'Nombre de lignes non conforme : ref.TitreProfessionnel',1;
    IF (SELECT COUNT_BIG(*) FROM [ref].[TypeCourriel]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[ref].[TypeCourriel])
        THROW 51100, N'Nombre de lignes non conforme : ref.TypeCourriel',1;
    IF (SELECT COUNT_BIG(*) FROM [ref].[TypePermis]) <> (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].[ref].[TypePermis])
        THROW 51100, N'Nombre de lignes non conforme : ref.TypePermis',1;

    -- Retour a des contraintes verifiees (validation de toutes les FKs).
    DECLARE curCheck CURSOR LOCAL FAST_FORWARD FOR
      SELECT s.name,t.name FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
      WHERE t.is_ms_shipped=0;
    OPEN curCheck; FETCH NEXT FROM curCheck INTO @schema,@table;
    WHILE @@FETCH_STATUS=0
    BEGIN
      SET @sql=N'ALTER TABLE '+QUOTENAME(@schema)+N'.'+QUOTENAME(@table)+N' WITH CHECK CHECK CONSTRAINT ALL;';
      EXEC sys.sp_executesql @sql;
      FETCH NEXT FROM curCheck INTO @schema,@table;
    END;
    CLOSE curCheck; DEALLOCATE curCheck;

    COMMIT TRANSACTION;
    PRINT N'Migration terminee et contraintes verifiees.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    PRINT N'Migration annulee. Erreur SQL : ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO

-- Etat final, a comparer aux effectifs de la source.
USE [bd_cmdpsf];
GO
SELECT N'Professionnel' AS TableMetier, (SELECT COUNT_BIG(*) FROM dbo.Professionnel) AS LignesCible,
       (SELECT COUNT_BIG(*) FROM [RDM_CEMTL].dbo.Medecin) AS LignesSource;
SELECT p.CodeProfession,COUNT(pr.ProfessionnelId) AS Nombre
FROM ref.Profession p LEFT JOIN dbo.Professionnel pr ON pr.ProfessionId=p.ProfessionId
GROUP BY p.CodeProfession ORDER BY p.CodeProfession;
SELECT fk.name AS ContrainteNonFiable FROM sys.foreign_keys fk WHERE fk.is_not_trusted=1;
GO
