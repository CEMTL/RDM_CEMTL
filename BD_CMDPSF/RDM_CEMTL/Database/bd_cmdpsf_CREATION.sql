-- VERSION CORRIGEE : aucune transaction explicite et aucun COMMIT TRANSACTION.
-- A EXECUTER UNIQUEMENT SUR UNE INSTANCE OU bd_cmdpsf N'EXISTE PAS.
-- Si bd_cmdpsf existe deja, executer bd_cmdpsf_CORRECTIF_VUES_SANS_COMMIT.sql.
-- IMPORTANT : un THROW suivi de GO ne stoppe pas necessairement les lots suivants dans SSMS.
-- Verifier au prealable : SELECT DB_ID(N'bd_cmdpsf') AS BaseExistante;

-- Script de CREATION de bd_cmdpsf (structure vide), généré depuis script_RDM(2).sql.
-- IMPORTANT: aucune donnée existante n'est copiée. EXÉCUTION UNIQUE dans SSMS.
-- La vue vw_ProfessionnelsRegistreCourant est exclue : dépendance absente dbo.ProfessionnelProfilCEMTL.

USE [master];
GO
IF DB_ID(N'bd_cmdpsf') IS NOT NULL
    THROW 51000, N'La base bd_cmdpsf existe déjà. Le script de création doit viser une base absente.', 1;
GO
CREATE DATABASE [bd_cmdpsf];
GO
ALTER DATABASE [bd_cmdpsf] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [bd_cmdpsf].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [bd_cmdpsf] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET ARITHABORT OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [bd_cmdpsf] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [bd_cmdpsf] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [bd_cmdpsf] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET  DISABLE_BROKER 
GO
ALTER DATABASE [bd_cmdpsf] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [bd_cmdpsf] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [bd_cmdpsf] SET  MULTI_USER 
GO
ALTER DATABASE [bd_cmdpsf] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [bd_cmdpsf] SET DB_CHAINING OFF 
GO
ALTER DATABASE [bd_cmdpsf] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [bd_cmdpsf] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [bd_cmdpsf] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [bd_cmdpsf] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [bd_cmdpsf] SET QUERY_STORE = OFF
GO
USE [bd_cmdpsf]
GO
/****** Object:  Schema [ref]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE SCHEMA [ref]
GO
/****** Object:  Table [ref].[StatutProfessionnel]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ref].[StatutProfessionnel](
	[StatutProfessionnelId] [int] IDENTITY(1,1) NOT NULL,
	[CodeStatut] [nvarchar](30) NOT NULL,
	[Libelle] [nvarchar](100) NULL,
	[EstActif] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[StatutProfessionnelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[CodeStatut] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [ref].[CategoriePratique]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ref].[CategoriePratique](
	[CategoriePratiqueId] [int] IDENTITY(1,1) NOT NULL,
	[Libelle] [nvarchar](50) NOT NULL,
	[EstActif] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CategoriePratiqueId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Libelle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Professionnel]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Professionnel](
	[ProfessionnelId] [int] IDENTITY(1,1) NOT NULL,
	[Permis] [nvarchar](20) NOT NULL,
	[TitreId] [int] NULL,
	[Nom] [nvarchar](150) NOT NULL,
	[Prenom] [nvarchar](150) NOT NULL,
	[TypePermisId] [int] NULL,
	[StatutProfessionnelId] [int] NULL,
	[CategoriePratiqueId] [int] NULL,
	[GenreId] [int] NULL,
	[DateNaissance] [date] NULL,
	[NAM] [nvarchar](20) NULL,
	[DateEntreeFonction] [date] NULL,
	[DateDepart] [date] NULL,
	[Commentaires] [nvarchar](max) NULL,
	[EstActif] [bit] NOT NULL,
	[DateCreation] [datetime2](0) NOT NULL,
	[DateModification] [datetime2](0) NOT NULL,
	[ProfessionId] [int] NOT NULL,
	[DateObtentionPermis] [date] NULL,
	[DateExpirationPermis] [date] NULL,
	[NumeroMembreOrdre] [nvarchar](50) NULL,
	[DateDerniereVerificationOrdre] [date] NULL,
	[ReferenceVerificationOrdre] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ProfessionnelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [dbo].[CodeCotisation]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CodeCotisation](
	[CodeCotisationId] [int] IDENTITY(1,1) NOT NULL,
	[CodeCotisation] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[EstActif] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CodeCotisationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[CodeCotisation] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CotisationAnnuelle]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CotisationAnnuelle](
	[CotisationAnnuelleId] [int] IDENTITY(1,1) NOT NULL,
	[ProfessionnelId] [int] NOT NULL,
	[AnneeExercice] [smallint] NOT NULL,
	[CodeCotisationId] [int] NULL,
	[DescriptionSource] [nvarchar](255) NULL,
	[MontantInitial] [decimal](12, 2) NULL,
	[ReductionAppliquee] [bit] NULL,
	[MotifReduction] [nvarchar](255) NULL,
	[MontantReduction] [decimal](12, 2) NULL,
	[PenaliteAppliquee] [bit] NULL,
	[PenaliteTexte] [nvarchar](100) NULL,
	[MontantPenalite] [decimal](12, 2) NULL,
	[MontantFinal] [decimal](12, 2) NULL,
	[EtatCotisation] [nvarchar](100) NULL,
	[MontantPaye] [decimal](12, 2) NULL,
	[DatePaiement] [date] NULL,
	[TypePaiementTexte] [nvarchar](100) NULL,
	[TypePaiementId] [int] NULL,
	[DateDepot] [date] NULL,
	[NoDepot] [nvarchar](50) NULL,
	[RecuImposition] [nvarchar](50) NULL,
	[DateRecuImposition] [date] NULL,
	[DateCreation] [datetime2](0) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CotisationAnnuelleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_CotisationAnnuelle] UNIQUE NONCLUSTERED 
(
	[ProfessionnelId] ASC,
	[AnneeExercice] ASC,
	[CodeCotisationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Departement]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Departement](
	[DepartementId] [int] IDENTITY(1,1) NOT NULL,
	[NomDepartement] [nvarchar](200) NOT NULL,
	[CodeDepartement] [nvarchar](50) NULL,
	[EstActif] [bit] NOT NULL,
 CONSTRAINT [PK_Departement] PRIMARY KEY CLUSTERED 
(
	[DepartementId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AffectationOrganisationnelle]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AffectationOrganisationnelle](
	[AffectationOrganisationnelleId] [int] IDENTITY(1,1) NOT NULL,
	[DepartementId] [int] NULL,
	[ServiceId] [int] NULL,
	[ProfessionnelId] [int] NULL,
	[RoleCode] [varchar](40) NOT NULL,
	[NomAffiche] [nvarchar](300) NOT NULL,
	[Titre] [nvarchar](300) NULL,
	[Courriel] [nvarchar](255) NULL,
	[CourrielSource] [nvarchar](255) NULL,
	[Telephone] [nvarchar](50) NULL,
	[PosteTelephone] [nvarchar](20) NULL,
	[InstallationId] [int] NULL,
	[SiteNomSource] [nvarchar](200) NULL,
	[RegroupementSource] [nvarchar](200) NULL,
	[DateDebut] [date] NULL,
	[DateFin] [date] NULL,
	[EstInterim] [bit] NOT NULL,
	[EstActive] [bit] NOT NULL,
	[EstAValider] [bit] NOT NULL,
	[SourceDonnees] [nvarchar](200) NULL,
	[SourceLigne] [int] NULL,
	[DateCreation] [datetime2](0) NOT NULL,
	[DateModification] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_AffectationOrganisationnelle] PRIMARY KEY CLUSTERED 
(
	[AffectationOrganisationnelleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Service]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Service](
	[ServiceId] [int] IDENTITY(1,1) NOT NULL,
	[DepartementId] [int] NULL,
	[NomService] [nvarchar](250) NOT NULL,
	[CodeService] [nvarchar](50) NULL,
	[EstActif] [bit] NOT NULL,
	[RegroupementSource] [nvarchar](200) NULL,
 CONSTRAINT [PK_Service] PRIMARY KEY CLUSTERED 
(
	[ServiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Installation]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Installation](
	[InstallationId] [int] IDENTITY(1,1) NOT NULL,
	[NomInstallation] [nvarchar](200) NOT NULL,
	[CodeInstallation] [nvarchar](50) NULL,
	[EstActif] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[InstallationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[NomInstallation] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [ref].[Profession]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ref].[Profession](
	[ProfessionId] [int] IDENTITY(1,1) NOT NULL,
	[CodeProfession] [nvarchar](30) NOT NULL,
	[Libelle] [nvarchar](100) NOT NULL,
	[EstActif] [bit] NOT NULL,
 CONSTRAINT [PK_Profession] PRIMARY KEY CLUSTERED 
(
	[ProfessionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Profession_Code] UNIQUE NONCLUSTERED 
(
	[CodeProfession] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[__MigrationHistory]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__MigrationHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ContextKey] [nvarchar](300) NOT NULL,
	[Model] [varbinary](max) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC,
	[ContextKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](128) NOT NULL,
	[Nom] [nvarchar](100) NOT NULL,
	[Prenom] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[PhoneNumber] [nvarchar](50) NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AuditLog]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditLog](
	[AuditLogId] [int] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](100) NOT NULL,
	[RecordId] [int] NOT NULL,
	[Action] [nvarchar](10) NOT NULL,
	[OldValues] [nvarchar](max) NULL,
	[NewValues] [nvarchar](max) NULL,
	[ChangedFields] [nvarchar](max) NULL,
	[UtilisateurId] [int] NULL,
	[NomUtilisateur] [nvarchar](255) NULL,
	[DateAction] [datetime2](0) NOT NULL,
	[AdresseIP] [nvarchar](50) NULL,
	[UserAgent] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[AuditLogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Comite]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Comite](
	[ComiteId] [int] IDENTITY(1,1) NOT NULL,
	[NumeroSource] [int] NULL,
	[NomComite] [nvarchar](200) NOT NULL,
	[EstActif] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ComiteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[NomComite] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DocumentProfessionnel]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocumentProfessionnel](
	[DocumentProfessionnelId] [int] IDENTITY(1,1) NOT NULL,
	[ProfessionnelId] [int] NOT NULL,
	[TypeDocument] [nvarchar](100) NOT NULL,
	[NomFichier] [nvarchar](255) NOT NULL,
	[ContentType] [nvarchar](100) NOT NULL,
	[TailleOctets] [bigint] NOT NULL,
	[HashSHA256] [varchar](64) NOT NULL,
	[DateDocument] [date] NULL,
	[DateExpiration] [date] NULL,
	[Description] [nvarchar](500) NULL,
	[EntiteLiee] [nvarchar](50) NULL,
	[EntiteId] [int] NULL,
	[Confidentiel] [bit] NOT NULL,
	[Contenu] [varbinary](max) NOT NULL,
	[DateDepot] [datetime2](0) NOT NULL,
	[DeposePar] [nvarchar](256) NULL,
	[EstActif] [bit] NOT NULL,
 CONSTRAINT [PK_DocumentProfessionnel] PRIMARY KEY CLUSTERED 
(
	[DocumentProfessionnelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EntenteFDC]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EntenteFDC](
	[EntenteFDCId] [int] IDENTITY(1,1) NOT NULL,
	[ProfessionnelId] [int] NOT NULL,
	[TypeEntenteFDC] [nvarchar](150) NULL,
	[DateAdhesion] [date] NULL,
	[DateFin] [date] NULL,
	[PEMVise] [nvarchar](150) NULL,
	[EstActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[EntenteFDCId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FitTest]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FitTest](
	[FitTestId] [int] IDENTITY(1,1) NOT NULL,
	[ProfessionnelId] [int] NOT NULL,
	[Resultat] [nvarchar](255) NOT NULL,
	[DateFitTest] [date] NULL,
	[Commentaire] [nvarchar](500) NULL,
	[EstActif] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[FitTestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InstallationService]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InstallationService](
	[InstallationServiceId] [int] IDENTITY(1,1) NOT NULL,
	[InstallationId] [int] NOT NULL,
	[ServiceId] [int] NOT NULL,
	[DateDebut] [date] NULL,
	[DateFin] [date] NULL,
	[EstActif] [bit] NOT NULL,
 CONSTRAINT [PK_InstallationService] PRIMARY KEY CLUSTERED 
(
	[InstallationServiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InstanceDecision]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InstanceDecision](
	[InstanceDecisionId] [int] IDENTITY(1,1) NOT NULL,
	[TypeInstanceDecisionId] [int] NOT NULL,
	[CodeInstance] [nvarchar](100) NOT NULL,
	[DateInstance] [date] NULL,
	[LibelleInstance] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[InstanceDecisionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_InstanceDecision] UNIQUE NONCLUSTERED 
(
	[TypeInstanceDecisionId] ASC,
	[CodeInstance] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[JournalImport]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JournalImport](
	[JournalImportId] [int] IDENTITY(1,1) NOT NULL,
	[NomFichier] [nvarchar](255) NOT NULL,
	[DateImport] [datetime2](0) NOT NULL,
	[LignesImportees] [int] NULL,
	[LignesRejetees] [int] NULL,
	[Commentaire] [nvarchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[JournalImportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProfessionnelAdresse]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfessionnelAdresse](
	[ProfessionnelAdresseId] [int] IDENTITY(1,1) NOT NULL,
	[ProfessionnelId] [int] NOT NULL,
	[AdresseLigne1] [nvarchar](255) NULL,
	[Ville] [nvarchar](100) NULL,
	[Province] [nvarchar](100) NULL,
	[CodePostal] [nvarchar](20) NULL,
	[EstPrincipale] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ProfessionnelAdresseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProfessionnelAffectationClinique]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfessionnelAffectationClinique](
	[ProfessionnelAffectationCliniqueId] [int] IDENTITY(1,1) NOT NULL,
	[ProfessionnelId] [int] NOT NULL,
	[DepartementId] [int] NOT NULL,
	[ServiceId] [int] NOT NULL,
	[InstallationId] [int] NULL,
	[TypeAffectation] [nvarchar](100) NULL,
	[EstPrincipale] [bit] NOT NULL,
	[DateDebut] [date] NULL,
	[DateFin] [date] NULL,
	[EstActive] [bit] NOT NULL,
	[Commentaire] [nvarchar](500) NULL,
	[SourceDonnees] [nvarchar](200) NULL,
	[DateCreation] [datetime2](0) NOT NULL,
	[DateModification] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_ProfessionnelAffectationClinique] PRIMARY KEY CLUSTERED 
(
	[ProfessionnelAffectationCliniqueId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProfessionnelComite]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfessionnelComite](
	[ProfessionnelComiteId] [int] IDENTITY(1,1) NOT NULL,
	[ProfessionnelId] [int] NOT NULL,
	[ComiteId] [int] NOT NULL,
	[RoleMembre] [nvarchar](100) NULL,
	[DateDebut] [date] NULL,
	[DateFin] [date] NULL,
	[Commentaire] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[ProfessionnelComiteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProfessionnelCourriel]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfessionnelCourriel](
	[ProfessionnelCourrielId] [int] IDENTITY(1,1) NOT NULL,
	[ProfessionnelId] [int] NOT NULL,
	[TypeCourriel] [nvarchar](50) NOT NULL,
	[AdresseCourriel] [nvarchar](255) NOT NULL,
	[RangSource] [tinyint] NULL,
	[EstPrincipal] [bit] NOT NULL,
	[TypeCourrielId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ProfessionnelCourrielId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProfessionnelInstallation]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfessionnelInstallation](
	[ProfessionnelInstallationId] [int] IDENTITY(1,1) NOT NULL,
	[ProfessionnelId] [int] NOT NULL,
	[InstallationId] [int] NOT NULL,
	[RangInstallation] [tinyint] NOT NULL,
	[DateDebut] [date] NULL,
	[DateFin] [date] NULL,
	[EstPrincipale] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ProfessionnelInstallationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_ProfessionnelInstallation] UNIQUE NONCLUSTERED 
(
	[ProfessionnelId] ASC,
	[InstallationId] ASC,
	[RangInstallation] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProfessionnelPrivilege]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfessionnelPrivilege](
	[ProfessionnelPrivilegeId] [int] IDENTITY(1,1) NOT NULL,
	[ProfessionnelId] [int] NOT NULL,
	[PrivilegeReferenceId] [int] NOT NULL,
	[InstallationId] [int] NULL,
	[ServiceId] [int] NULL,
	[DateDebut] [datetime] NULL,
	[DateFin] [datetime] NULL,
	[Etat] [nvarchar](50) NOT NULL,
	[NatureActivites] [nvarchar](500) NULL,
	[ChampActivites] [nvarchar](500) NULL,
	[Obligations] [nvarchar](max) NULL,
	[Commentaire] [nvarchar](1000) NULL,
	[NumeroResolutionCA] [nvarchar](50) NULL,
	[DateResolutionCA] [datetime] NULL,
	[ReconnaissanceEcriteRecue] [bit] NOT NULL,
	[EstActif] [bit] NOT NULL,
	[DateCreation] [datetime] NOT NULL,
	[DateModification] [datetime] NULL,
 CONSTRAINT [PK_ProfessionnelPrivilege] PRIMARY KEY CLUSTERED 
(
	[ProfessionnelPrivilegeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProfessionnelTelephone]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfessionnelTelephone](
	[ProfessionnelTelephoneId] [int] IDENTITY(1,1) NOT NULL,
	[ProfessionnelId] [int] NOT NULL,
	[TypeTelephone] [nvarchar](50) NOT NULL,
	[NumeroTelephone] [nvarchar](50) NOT NULL,
	[RangSource] [tinyint] NULL,
	[EstPrincipal] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ProfessionnelTelephoneId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Nomination]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Nomination](
	[NominationId] [int] IDENTITY(1,1) NOT NULL,
	[CodeNomination] [nvarchar](30) NOT NULL,
	[LibelleNomination] [nvarchar](150) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[EstActif] [bit] NOT NULL,
	[DateCreation] [datetime] NOT NULL,
	[DateModification] [datetime] NULL,
 CONSTRAINT [PK_Nomination] PRIMARY KEY CLUSTERED 
(
	[NominationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Nomination_Code] UNIQUE NONCLUSTERED 
(
	[CodeNomination] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Nomination_Libelle] UNIQUE NONCLUSTERED 
(
	[LibelleNomination] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NominationAdministrative]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NominationAdministrative](
	[NominationAdministrativeId] [int] IDENTITY(1,1) NOT NULL,
	[ProfessionnelId] [int] NOT NULL,
	[LibelleNomination] [nvarchar](255) NULL,
	[DateNomination] [date] NULL,
	[DateFin] [date] NULL,
	[EstActive] [bit] NOT NULL,
	[NominationId] [int] NULL,
	[TypeNominationId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[NominationAdministrativeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PrivilegeReference]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PrivilegeReference](
	[PrivilegeReferenceId] [int] IDENTITY(1,1) NOT NULL,
	[CodePrivilege] [nvarchar](50) NOT NULL,
	[LibellePrivilege] [nvarchar](150) NOT NULL,
	[Categorie] [nvarchar](100) NULL,
	[Description] [varchar](max) NULL,
	[NecessiteBlocOperatoire] [bit] NOT NULL,
	[NecessiteInstallation] [bit] NOT NULL,
	[NecessiteUnite] [bit] NOT NULL,
	[OrdreAffichage] [int] NULL,
	[EstActif] [bit] NOT NULL,
	[DateCreation] [datetime] NOT NULL,
	[DateModification] [datetime] NULL,
 CONSTRAINT [PK_PrivilegeReference] PRIMARY KEY CLUSTERED 
(
	[PrivilegeReferenceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_PrivilegeReference_CodePrivilege] UNIQUE NONCLUSTERED 
(
	[CodePrivilege] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TypeInstanceDecision]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TypeInstanceDecision](
	[TypeInstanceDecisionId] [int] IDENTITY(1,1) NOT NULL,
	[CodeType] [nvarchar](50) NOT NULL,
	[Libelle] [nvarchar](150) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TypeInstanceDecisionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[CodeType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TypeNomination]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TypeNomination](
	[TypeNominationId] [int] IDENTITY(1,1) NOT NULL,
	[CodeTypeNomination] [nvarchar](30) NOT NULL,
	[LibelleTypeNomination] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[EstActif] [bit] NOT NULL,
	[DateCreation] [datetime] NOT NULL,
	[DateModification] [datetime] NULL,
 CONSTRAINT [PK_TypeNomination] PRIMARY KEY CLUSTERED 
(
	[TypeNominationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_TypeNomination_Code] UNIQUE NONCLUSTERED 
(
	[CodeTypeNomination] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_TypeNomination_Libelle] UNIQUE NONCLUSTERED 
(
	[LibelleTypeNomination] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TypePaiement]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TypePaiement](
	[TypePaiementId] [int] IDENTITY(1,1) NOT NULL,
	[Libelle] [nvarchar](100) NOT NULL,
	[EstActif] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TypePaiementId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Libelle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UtilisateurMetier]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UtilisateurMetier](
	[UtilisateurMetierId] [int] IDENTITY(1,1) NOT NULL,
	[IdentifiantSource] [nvarchar](100) NULL,
	[Courriel] [nvarchar](255) NULL,
	[NomComplet] [nvarchar](255) NULL,
	[Connexion] [nvarchar](150) NULL,
	[AspNetUserId] [nvarchar](128) NULL,
	[EstActif] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UtilisateurMetierId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [ref].[Genre]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ref].[Genre](
	[GenreId] [int] IDENTITY(1,1) NOT NULL,
	[CodeGenre] [nvarchar](10) NOT NULL,
	[Libelle] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[GenreId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[CodeGenre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [ref].[TitreProfessionnel]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ref].[TitreProfessionnel](
	[TitreId] [int] IDENTITY(1,1) NOT NULL,
	[Libelle] [nvarchar](50) NOT NULL,
	[EstActif] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TitreId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Libelle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [ref].[TypeCourriel]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ref].[TypeCourriel](
	[TypeCourrielId] [int] IDENTITY(1,1) NOT NULL,
	[Libelle] [nvarchar](50) NOT NULL,
	[EstActif] [bit] NOT NULL,
 CONSTRAINT [PK_TypeCourriel] PRIMARY KEY CLUSTERED 
(
	[TypeCourrielId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [ref].[TypePermis]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ref].[TypePermis](
	[TypePermisId] [int] IDENTITY(1,1) NOT NULL,
	[Libelle] [nvarchar](100) NOT NULL,
	[EstActif] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TypePermisId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Libelle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AffectationOrganisationnelle_Departement]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_AffectationOrganisationnelle_Departement] ON [dbo].[AffectationOrganisationnelle]
(
	[DepartementId] ASC,
	[RoleCode] ASC,
	[EstActive] ASC,
	[DateDebut] ASC
)
INCLUDE([NomAffiche],[ProfessionnelId],[DateFin],[RegroupementSource],[EstAValider]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AffectationOrganisationnelle_Service]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_AffectationOrganisationnelle_Service] ON [dbo].[AffectationOrganisationnelle]
(
	[ServiceId] ASC,
	[RoleCode] ASC,
	[EstActive] ASC,
	[DateDebut] ASC
)
INCLUDE([NomAffiche],[ProfessionnelId],[Titre],[Courriel],[CourrielSource],[DateFin],[EstAValider]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_AffectationOrganisationnelle_Source]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_AffectationOrganisationnelle_Source] ON [dbo].[AffectationOrganisationnelle]
(
	[SourceDonnees] ASC,
	[SourceLigne] ASC,
	[RoleCode] ASC
)
WHERE ([SourceDonnees] IS NOT NULL AND [SourceLigne] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_UserId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_UserId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_RoleId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_UserId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[AspNetUserRoles]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UserNameIndex]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CotisationAnnuelle_AnneeExercice]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_CotisationAnnuelle_AnneeExercice] ON [dbo].[CotisationAnnuelle]
(
	[AnneeExercice] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Departement_NomDepartement]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Departement_NomDepartement] ON [dbo].[Departement]
(
	[NomDepartement] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_DocumentProfessionnel_HashSHA256]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_DocumentProfessionnel_HashSHA256] ON [dbo].[DocumentProfessionnel]
(
	[HashSHA256] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DocumentProfessionnel_Professionnel_Actif_Expiration]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_DocumentProfessionnel_Professionnel_Actif_Expiration] ON [dbo].[DocumentProfessionnel]
(
	[ProfessionnelId] ASC,
	[EstActif] ASC,
	[DateExpiration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_EntenteFDC_ProfessionnelId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_EntenteFDC_ProfessionnelId] ON [dbo].[EntenteFDC]
(
	[ProfessionnelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_FitTest_ProfessionnelId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_FitTest_ProfessionnelId] ON [dbo].[FitTest]
(
	[ProfessionnelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_InstallationService_InstallationId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_InstallationService_InstallationId] ON [dbo].[InstallationService]
(
	[InstallationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_InstallationService_ServiceId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_InstallationService_ServiceId] ON [dbo].[InstallationService]
(
	[ServiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_InstallationService_Actif]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_InstallationService_Actif] ON [dbo].[InstallationService]
(
	[InstallationId] ASC,
	[ServiceId] ASC
)
WHERE ([EstActif]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Professionnel_NomPrenom]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_Professionnel_NomPrenom] ON [dbo].[Professionnel]
(
	[Nom] ASC,
	[Prenom] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Professionnel_Profession_Permis]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Professionnel_Profession_Permis] ON [dbo].[Professionnel]
(
	[ProfessionId] ASC,
	[Permis] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProfessionnelAffectationClinique_ProfessionnelOrganisation]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProfessionnelAffectationClinique_ProfessionnelOrganisation] ON [dbo].[ProfessionnelAffectationClinique]
(
	[ProfessionnelId] ASC,
	[DepartementId] ASC,
	[ServiceId] ASC,
	[InstallationId] ASC,
	[DateDebut] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_ProfessionnelAffectationClinique_PrincipaleActive]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ProfessionnelAffectationClinique_PrincipaleActive] ON [dbo].[ProfessionnelAffectationClinique]
(
	[ProfessionnelId] ASC
)
WHERE ([EstPrincipale]=(1) AND [EstActive]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProfessionnelCourriel_TypeCourrielId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProfessionnelCourriel_TypeCourrielId] ON [dbo].[ProfessionnelCourriel]
(
	[TypeCourrielId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProfessionnelInstallation_InstallationId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProfessionnelInstallation_InstallationId] ON [dbo].[ProfessionnelInstallation]
(
	[InstallationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ProfessionnelPrivilege_Etat_DateDebut_DateFin]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProfessionnelPrivilege_Etat_DateDebut_DateFin] ON [dbo].[ProfessionnelPrivilege]
(
	[Etat] ASC,
	[DateDebut] ASC,
	[DateFin] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProfessionnelPrivilege_InstallationId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProfessionnelPrivilege_InstallationId] ON [dbo].[ProfessionnelPrivilege]
(
	[InstallationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProfessionnelPrivilege_ProfessionnelId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProfessionnelPrivilege_ProfessionnelId] ON [dbo].[ProfessionnelPrivilege]
(
	[ProfessionnelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProfessionnelPrivilege_PrivilegeReferenceId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProfessionnelPrivilege_PrivilegeReferenceId] ON [dbo].[ProfessionnelPrivilege]
(
	[PrivilegeReferenceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProfessionnelPrivilege_UniteOrganisationnelleId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProfessionnelPrivilege_UniteOrganisationnelleId] ON [dbo].[ProfessionnelPrivilege]
(
	[ServiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ_ProfessionnelPrivilege]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_ProfessionnelPrivilege] ON [dbo].[ProfessionnelPrivilege]
(
	[ProfessionnelId] ASC,
	[PrivilegeReferenceId] ASC,
	[InstallationId] ASC,
	[ServiceId] ASC,
	[DateDebut] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_NominationAdministrative_ProfessionnelId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_NominationAdministrative_ProfessionnelId] ON [dbo].[NominationAdministrative]
(
	[ProfessionnelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PrivilegeReference_Categorie]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_PrivilegeReference_Categorie] ON [dbo].[PrivilegeReference]
(
	[Categorie] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PrivilegeReference_LibellePrivilege]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_PrivilegeReference_LibellePrivilege] ON [dbo].[PrivilegeReference]
(
	[LibellePrivilege] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Service_DepartementId]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE NONCLUSTERED INDEX [IX_Service_DepartementId] ON [dbo].[Service]
(
	[DepartementId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Service_Departement_NomService]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Service_Departement_NomService] ON [dbo].[Service]
(
	[DepartementId] ASC,
	[NomService] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_CategoriePratique_Libelle]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_CategoriePratique_Libelle] ON [ref].[CategoriePratique]
(
	[Libelle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_TypeCourriel_Libelle]    Script Date: 2026-10-13 10:02:40 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_TypeCourriel_Libelle] ON [ref].[TypeCourriel]
(
	[Libelle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle] ADD  CONSTRAINT [DF_AffectationOrganisationnelle_EstInterim]  DEFAULT ((0)) FOR [EstInterim]
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle] ADD  CONSTRAINT [DF_AffectationOrganisationnelle_EstActive]  DEFAULT ((1)) FOR [EstActive]
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle] ADD  CONSTRAINT [DF_AffectationOrganisationnelle_EstAValider]  DEFAULT ((0)) FOR [EstAValider]
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle] ADD  CONSTRAINT [DF_AffectationOrganisationnelle_DateCreation]  DEFAULT (sysutcdatetime()) FOR [DateCreation]
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle] ADD  CONSTRAINT [DF_AffectationOrganisationnelle_DateModification]  DEFAULT (sysutcdatetime()) FOR [DateModification]
GO
ALTER TABLE [dbo].[AuditLog] ADD  DEFAULT (getutcdate()) FOR [DateAction]
GO
ALTER TABLE [dbo].[CodeCotisation] ADD  CONSTRAINT [DF_CodeCotisation_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[Comite] ADD  CONSTRAINT [DF_Comite_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[CotisationAnnuelle] ADD  CONSTRAINT [DF_CotisationAnnuelle_DateCreation]  DEFAULT (sysdatetime()) FOR [DateCreation]
GO
ALTER TABLE [dbo].[Departement] ADD  CONSTRAINT [DF_Departement_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[DocumentProfessionnel] ADD  CONSTRAINT [DF_DocumentProfessionnel_Confidentiel]  DEFAULT ((0)) FOR [Confidentiel]
GO
ALTER TABLE [dbo].[DocumentProfessionnel] ADD  CONSTRAINT [DF_DocumentProfessionnel_DateDepot]  DEFAULT (sysutcdatetime()) FOR [DateDepot]
GO
ALTER TABLE [dbo].[DocumentProfessionnel] ADD  CONSTRAINT [DF_DocumentProfessionnel_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[EntenteFDC] ADD  CONSTRAINT [DF_EntenteFDC_EstActive]  DEFAULT ((1)) FOR [EstActive]
GO
ALTER TABLE [dbo].[FitTest] ADD  CONSTRAINT [DF_FitTest_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[Installation] ADD  CONSTRAINT [DF_Installation_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[InstallationService] ADD  CONSTRAINT [DF_InstallationService_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[JournalImport] ADD  CONSTRAINT [DF_JournalImport_DateImport]  DEFAULT (sysdatetime()) FOR [DateImport]
GO
ALTER TABLE [dbo].[Professionnel] ADD  CONSTRAINT [DF_Professionnel_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[Professionnel] ADD  CONSTRAINT [DF_Professionnel_DateCreation]  DEFAULT (sysdatetime()) FOR [DateCreation]
GO
ALTER TABLE [dbo].[Professionnel] ADD  CONSTRAINT [DF_Professionnel_DateModification]  DEFAULT (sysdatetime()) FOR [DateModification]
GO
ALTER TABLE [dbo].[Professionnel] ADD  CONSTRAINT [DF_Professionnel_ProfessionId]  DEFAULT ((1)) FOR [ProfessionId]
GO
ALTER TABLE [dbo].[ProfessionnelAdresse] ADD  CONSTRAINT [DF_ProfessionnelAdresse_EstPrincipale]  DEFAULT ((1)) FOR [EstPrincipale]
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique] ADD  CONSTRAINT [DF_ProfessionnelAffectationClinique_EstPrincipale]  DEFAULT ((0)) FOR [EstPrincipale]
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique] ADD  CONSTRAINT [DF_ProfessionnelAffectationClinique_EstActive]  DEFAULT ((1)) FOR [EstActive]
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique] ADD  CONSTRAINT [DF_ProfessionnelAffectationClinique_DateCreation]  DEFAULT (sysdatetime()) FOR [DateCreation]
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique] ADD  CONSTRAINT [DF_ProfessionnelAffectationClinique_DateModification]  DEFAULT (sysdatetime()) FOR [DateModification]
GO
ALTER TABLE [dbo].[ProfessionnelCourriel] ADD  CONSTRAINT [DF_ProfessionnelCourriel_EstPrincipal]  DEFAULT ((0)) FOR [EstPrincipal]
GO
ALTER TABLE [dbo].[ProfessionnelInstallation] ADD  CONSTRAINT [DF_ProfessionnelInstallation_EstPrincipale]  DEFAULT ((0)) FOR [EstPrincipale]
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege] ADD  CONSTRAINT [DF_ProfessionnelPrivilege_Etat]  DEFAULT (N'Actif') FOR [Etat]
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege] ADD  CONSTRAINT [DF_ProfessionnelPrivilege_ReconnaissanceEcriteRecue]  DEFAULT ((0)) FOR [ReconnaissanceEcriteRecue]
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege] ADD  CONSTRAINT [DF_ProfessionnelPrivilege_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege] ADD  CONSTRAINT [DF_ProfessionnelPrivilege_DateCreation]  DEFAULT (getdate()) FOR [DateCreation]
GO
ALTER TABLE [dbo].[ProfessionnelTelephone] ADD  CONSTRAINT [DF_ProfessionnelTelephone_EstPrincipal]  DEFAULT ((0)) FOR [EstPrincipal]
GO
ALTER TABLE [dbo].[Nomination] ADD  CONSTRAINT [DF_Nomination_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[Nomination] ADD  CONSTRAINT [DF_Nomination_DateCreation]  DEFAULT (getdate()) FOR [DateCreation]
GO
ALTER TABLE [dbo].[NominationAdministrative] ADD  CONSTRAINT [DF_NominationAdministrative_EstActive]  DEFAULT ((1)) FOR [EstActive]
GO
ALTER TABLE [dbo].[PrivilegeReference] ADD  CONSTRAINT [DF_PrivilegeReference_NecessiteBlocOperatoire]  DEFAULT ((0)) FOR [NecessiteBlocOperatoire]
GO
ALTER TABLE [dbo].[PrivilegeReference] ADD  CONSTRAINT [DF_PrivilegeReference_NecessiteInstallation]  DEFAULT ((0)) FOR [NecessiteInstallation]
GO
ALTER TABLE [dbo].[PrivilegeReference] ADD  CONSTRAINT [DF_PrivilegeReference_NecessiteUnite]  DEFAULT ((0)) FOR [NecessiteUnite]
GO
ALTER TABLE [dbo].[PrivilegeReference] ADD  CONSTRAINT [DF_PrivilegeReference_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[PrivilegeReference] ADD  CONSTRAINT [DF_PrivilegeReference_DateCreation]  DEFAULT (getdate()) FOR [DateCreation]
GO
ALTER TABLE [dbo].[Service] ADD  CONSTRAINT [DF_Service_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[TypeNomination] ADD  CONSTRAINT [DF_TypeNomination_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[TypeNomination] ADD  CONSTRAINT [DF_TypeNomination_DateCreation]  DEFAULT (getdate()) FOR [DateCreation]
GO
ALTER TABLE [dbo].[TypePaiement] ADD  CONSTRAINT [DF_TypePaiement_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [ref].[CategoriePratique] ADD  CONSTRAINT [DF_ref_CategoriePratique_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [ref].[Profession] ADD  CONSTRAINT [DF_Profession_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [ref].[StatutProfessionnel] ADD  CONSTRAINT [DF_ref_StatutProfessionnel_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [ref].[TitreProfessionnel] ADD  CONSTRAINT [DF_ref_TitreProfessionnel_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [ref].[TypeCourriel] ADD  CONSTRAINT [DF_TypeCourriel_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [ref].[TypePermis] ADD  CONSTRAINT [DF_ref_TypePermis_EstActif]  DEFAULT ((1)) FOR [EstActif]
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle]  WITH CHECK ADD  CONSTRAINT [FK_AffectationOrganisationnelle_Departement] FOREIGN KEY([DepartementId])
REFERENCES [dbo].[Departement] ([DepartementId])
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle] CHECK CONSTRAINT [FK_AffectationOrganisationnelle_Departement]
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle]  WITH CHECK ADD  CONSTRAINT [FK_AffectationOrganisationnelle_Installation] FOREIGN KEY([InstallationId])
REFERENCES [dbo].[Installation] ([InstallationId])
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle] CHECK CONSTRAINT [FK_AffectationOrganisationnelle_Installation]
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle]  WITH CHECK ADD  CONSTRAINT [FK_AffectationOrganisationnelle_Professionnel] FOREIGN KEY([ProfessionnelId])
REFERENCES [dbo].[Professionnel] ([ProfessionnelId])
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle] CHECK CONSTRAINT [FK_AffectationOrganisationnelle_Professionnel]
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle]  WITH CHECK ADD  CONSTRAINT [FK_AffectationOrganisationnelle_Service] FOREIGN KEY([ServiceId])
REFERENCES [dbo].[Service] ([ServiceId])
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle] CHECK CONSTRAINT [FK_AffectationOrganisationnelle_Service]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[CotisationAnnuelle]  WITH CHECK ADD  CONSTRAINT [FK_CotisationAnnuelle_CodeCotisation] FOREIGN KEY([CodeCotisationId])
REFERENCES [dbo].[CodeCotisation] ([CodeCotisationId])
GO
ALTER TABLE [dbo].[CotisationAnnuelle] CHECK CONSTRAINT [FK_CotisationAnnuelle_CodeCotisation]
GO
ALTER TABLE [dbo].[CotisationAnnuelle]  WITH CHECK ADD  CONSTRAINT [FK_CotisationAnnuelle_Professionnel] FOREIGN KEY([ProfessionnelId])
REFERENCES [dbo].[Professionnel] ([ProfessionnelId])
GO
ALTER TABLE [dbo].[CotisationAnnuelle] CHECK CONSTRAINT [FK_CotisationAnnuelle_Professionnel]
GO
ALTER TABLE [dbo].[CotisationAnnuelle]  WITH CHECK ADD  CONSTRAINT [FK_CotisationAnnuelle_TypePaiement] FOREIGN KEY([TypePaiementId])
REFERENCES [dbo].[TypePaiement] ([TypePaiementId])
GO
ALTER TABLE [dbo].[CotisationAnnuelle] CHECK CONSTRAINT [FK_CotisationAnnuelle_TypePaiement]
GO
ALTER TABLE [dbo].[DocumentProfessionnel]  WITH CHECK ADD  CONSTRAINT [FK_DocumentProfessionnel_Professionnel] FOREIGN KEY([ProfessionnelId])
REFERENCES [dbo].[Professionnel] ([ProfessionnelId])
GO
ALTER TABLE [dbo].[DocumentProfessionnel] CHECK CONSTRAINT [FK_DocumentProfessionnel_Professionnel]
GO
ALTER TABLE [dbo].[EntenteFDC]  WITH CHECK ADD  CONSTRAINT [FK_EntenteFDC_Professionnel] FOREIGN KEY([ProfessionnelId])
REFERENCES [dbo].[Professionnel] ([ProfessionnelId])
GO
ALTER TABLE [dbo].[EntenteFDC] CHECK CONSTRAINT [FK_EntenteFDC_Professionnel]
GO
ALTER TABLE [dbo].[FitTest]  WITH CHECK ADD  CONSTRAINT [FK_FitTest_Professionnel] FOREIGN KEY([ProfessionnelId])
REFERENCES [dbo].[Professionnel] ([ProfessionnelId])
GO
ALTER TABLE [dbo].[FitTest] CHECK CONSTRAINT [FK_FitTest_Professionnel]
GO
ALTER TABLE [dbo].[InstallationService]  WITH CHECK ADD  CONSTRAINT [FK_InstallationService_Installation] FOREIGN KEY([InstallationId])
REFERENCES [dbo].[Installation] ([InstallationId])
GO
ALTER TABLE [dbo].[InstallationService] CHECK CONSTRAINT [FK_InstallationService_Installation]
GO
ALTER TABLE [dbo].[InstallationService]  WITH CHECK ADD  CONSTRAINT [FK_InstallationService_Service] FOREIGN KEY([ServiceId])
REFERENCES [dbo].[Service] ([ServiceId])
GO
ALTER TABLE [dbo].[InstallationService] CHECK CONSTRAINT [FK_InstallationService_Service]
GO
ALTER TABLE [dbo].[InstanceDecision]  WITH CHECK ADD  CONSTRAINT [FK_InstanceDecision_Type] FOREIGN KEY([TypeInstanceDecisionId])
REFERENCES [dbo].[TypeInstanceDecision] ([TypeInstanceDecisionId])
GO
ALTER TABLE [dbo].[InstanceDecision] CHECK CONSTRAINT [FK_InstanceDecision_Type]
GO
ALTER TABLE [dbo].[Professionnel]  WITH CHECK ADD  CONSTRAINT [FK_Professionnel_CategoriePratique] FOREIGN KEY([CategoriePratiqueId])
REFERENCES [ref].[CategoriePratique] ([CategoriePratiqueId])
GO
ALTER TABLE [dbo].[Professionnel] CHECK CONSTRAINT [FK_Professionnel_CategoriePratique]
GO
ALTER TABLE [dbo].[Professionnel]  WITH CHECK ADD  CONSTRAINT [FK_Professionnel_Genre] FOREIGN KEY([GenreId])
REFERENCES [ref].[Genre] ([GenreId])
GO
ALTER TABLE [dbo].[Professionnel] CHECK CONSTRAINT [FK_Professionnel_Genre]
GO
ALTER TABLE [dbo].[Professionnel]  WITH CHECK ADD  CONSTRAINT [FK_Professionnel_Profession] FOREIGN KEY([ProfessionId])
REFERENCES [ref].[Profession] ([ProfessionId])
GO
ALTER TABLE [dbo].[Professionnel] CHECK CONSTRAINT [FK_Professionnel_Profession]
GO
ALTER TABLE [dbo].[Professionnel]  WITH CHECK ADD  CONSTRAINT [FK_Professionnel_Statut] FOREIGN KEY([StatutProfessionnelId])
REFERENCES [ref].[StatutProfessionnel] ([StatutProfessionnelId])
GO
ALTER TABLE [dbo].[Professionnel] CHECK CONSTRAINT [FK_Professionnel_Statut]
GO
ALTER TABLE [dbo].[Professionnel]  WITH CHECK ADD  CONSTRAINT [FK_Professionnel_Titre] FOREIGN KEY([TitreId])
REFERENCES [ref].[TitreProfessionnel] ([TitreId])
GO
ALTER TABLE [dbo].[Professionnel] CHECK CONSTRAINT [FK_Professionnel_Titre]
GO
ALTER TABLE [dbo].[Professionnel]  WITH CHECK ADD  CONSTRAINT [FK_Professionnel_TypePermis] FOREIGN KEY([TypePermisId])
REFERENCES [ref].[TypePermis] ([TypePermisId])
GO
ALTER TABLE [dbo].[Professionnel] CHECK CONSTRAINT [FK_Professionnel_TypePermis]
GO
ALTER TABLE [dbo].[ProfessionnelAdresse]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelAdresse_Professionnel] FOREIGN KEY([ProfessionnelId])
REFERENCES [dbo].[Professionnel] ([ProfessionnelId])
GO
ALTER TABLE [dbo].[ProfessionnelAdresse] CHECK CONSTRAINT [FK_ProfessionnelAdresse_Professionnel]
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelAffectationClinique_Departement] FOREIGN KEY([DepartementId])
REFERENCES [dbo].[Departement] ([DepartementId])
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique] CHECK CONSTRAINT [FK_ProfessionnelAffectationClinique_Departement]
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelAffectationClinique_Installation] FOREIGN KEY([InstallationId])
REFERENCES [dbo].[Installation] ([InstallationId])
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique] CHECK CONSTRAINT [FK_ProfessionnelAffectationClinique_Installation]
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelAffectationClinique_Professionnel] FOREIGN KEY([ProfessionnelId])
REFERENCES [dbo].[Professionnel] ([ProfessionnelId])
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique] CHECK CONSTRAINT [FK_ProfessionnelAffectationClinique_Professionnel]
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelAffectationClinique_Service] FOREIGN KEY([ServiceId])
REFERENCES [dbo].[Service] ([ServiceId])
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique] CHECK CONSTRAINT [FK_ProfessionnelAffectationClinique_Service]
GO
ALTER TABLE [dbo].[ProfessionnelComite]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelComite_Comite] FOREIGN KEY([ComiteId])
REFERENCES [dbo].[Comite] ([ComiteId])
GO
ALTER TABLE [dbo].[ProfessionnelComite] CHECK CONSTRAINT [FK_ProfessionnelComite_Comite]
GO
ALTER TABLE [dbo].[ProfessionnelComite]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelComite_Professionnel] FOREIGN KEY([ProfessionnelId])
REFERENCES [dbo].[Professionnel] ([ProfessionnelId])
GO
ALTER TABLE [dbo].[ProfessionnelComite] CHECK CONSTRAINT [FK_ProfessionnelComite_Professionnel]
GO
ALTER TABLE [dbo].[ProfessionnelCourriel]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelCourriel_Professionnel] FOREIGN KEY([ProfessionnelId])
REFERENCES [dbo].[Professionnel] ([ProfessionnelId])
GO
ALTER TABLE [dbo].[ProfessionnelCourriel] CHECK CONSTRAINT [FK_ProfessionnelCourriel_Professionnel]
GO
ALTER TABLE [dbo].[ProfessionnelCourriel]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelCourriel_TypeCourriel] FOREIGN KEY([TypeCourrielId])
REFERENCES [ref].[TypeCourriel] ([TypeCourrielId])
GO
ALTER TABLE [dbo].[ProfessionnelCourriel] CHECK CONSTRAINT [FK_ProfessionnelCourriel_TypeCourriel]
GO
ALTER TABLE [dbo].[ProfessionnelInstallation]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelInstallation_Installation] FOREIGN KEY([InstallationId])
REFERENCES [dbo].[Installation] ([InstallationId])
GO
ALTER TABLE [dbo].[ProfessionnelInstallation] CHECK CONSTRAINT [FK_ProfessionnelInstallation_Installation]
GO
ALTER TABLE [dbo].[ProfessionnelInstallation]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelInstallation_Professionnel] FOREIGN KEY([ProfessionnelId])
REFERENCES [dbo].[Professionnel] ([ProfessionnelId])
GO
ALTER TABLE [dbo].[ProfessionnelInstallation] CHECK CONSTRAINT [FK_ProfessionnelInstallation_Professionnel]
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelPrivilege_Installation] FOREIGN KEY([InstallationId])
REFERENCES [dbo].[Installation] ([InstallationId])
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege] CHECK CONSTRAINT [FK_ProfessionnelPrivilege_Installation]
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelPrivilege_Professionnel] FOREIGN KEY([ProfessionnelId])
REFERENCES [dbo].[Professionnel] ([ProfessionnelId])
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege] CHECK CONSTRAINT [FK_ProfessionnelPrivilege_Professionnel]
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelPrivilege_PrivilegeReference] FOREIGN KEY([PrivilegeReferenceId])
REFERENCES [dbo].[PrivilegeReference] ([PrivilegeReferenceId])
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege] CHECK CONSTRAINT [FK_ProfessionnelPrivilege_PrivilegeReference]
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelPrivilege_Service] FOREIGN KEY([ServiceId])
REFERENCES [dbo].[Service] ([ServiceId])
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege] CHECK CONSTRAINT [FK_ProfessionnelPrivilege_Service]
GO
ALTER TABLE [dbo].[ProfessionnelTelephone]  WITH CHECK ADD  CONSTRAINT [FK_ProfessionnelTelephone_Professionnel] FOREIGN KEY([ProfessionnelId])
REFERENCES [dbo].[Professionnel] ([ProfessionnelId])
GO
ALTER TABLE [dbo].[ProfessionnelTelephone] CHECK CONSTRAINT [FK_ProfessionnelTelephone_Professionnel]
GO
ALTER TABLE [dbo].[NominationAdministrative]  WITH CHECK ADD  CONSTRAINT [FK_NominationAdministrative_Professionnel] FOREIGN KEY([ProfessionnelId])
REFERENCES [dbo].[Professionnel] ([ProfessionnelId])
GO
ALTER TABLE [dbo].[NominationAdministrative] CHECK CONSTRAINT [FK_NominationAdministrative_Professionnel]
GO
ALTER TABLE [dbo].[NominationAdministrative]  WITH CHECK ADD  CONSTRAINT [FK_NominationAdministrative_Nomination] FOREIGN KEY([NominationId])
REFERENCES [dbo].[Nomination] ([NominationId])
GO
ALTER TABLE [dbo].[NominationAdministrative] CHECK CONSTRAINT [FK_NominationAdministrative_Nomination]
GO
ALTER TABLE [dbo].[NominationAdministrative]  WITH CHECK ADD  CONSTRAINT [FK_NominationAdministrative_TypeNomination] FOREIGN KEY([TypeNominationId])
REFERENCES [dbo].[TypeNomination] ([TypeNominationId])
GO
ALTER TABLE [dbo].[NominationAdministrative] CHECK CONSTRAINT [FK_NominationAdministrative_TypeNomination]
GO
ALTER TABLE [dbo].[Service]  WITH CHECK ADD  CONSTRAINT [FK_Service_Departement] FOREIGN KEY([DepartementId])
REFERENCES [dbo].[Departement] ([DepartementId])
GO
ALTER TABLE [dbo].[Service] CHECK CONSTRAINT [FK_Service_Departement]
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle]  WITH CHECK ADD  CONSTRAINT [CK_AffectationOrganisationnelle_Cible] CHECK  (([DepartementId] IS NOT NULL AND [ServiceId] IS NULL OR [DepartementId] IS NULL AND [ServiceId] IS NOT NULL))
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle] CHECK CONSTRAINT [CK_AffectationOrganisationnelle_Cible]
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle]  WITH CHECK ADD  CONSTRAINT [CK_AffectationOrganisationnelle_Dates] CHECK  (([DateFin] IS NULL OR [DateDebut] IS NULL OR [DateFin]>=[DateDebut]))
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle] CHECK CONSTRAINT [CK_AffectationOrganisationnelle_Dates]
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle]  WITH CHECK ADD  CONSTRAINT [CK_AffectationOrganisationnelle_Role] CHECK  (([RoleCode]='CHEF_SERVICE' OR [RoleCode]='ADJOINT_SITE' OR [RoleCode]='SECRETAIRE_DEPARTEMENT' OR [RoleCode]='CHEF_DEPARTEMENT'))
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle] CHECK CONSTRAINT [CK_AffectationOrganisationnelle_Role]
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle]  WITH CHECK ADD  CONSTRAINT [CK_AffectationOrganisationnelle_RoleCible] CHECK  (([RoleCode]='CHEF_SERVICE' AND [ServiceId] IS NOT NULL OR ([RoleCode]='ADJOINT_SITE' OR [RoleCode]='SECRETAIRE_DEPARTEMENT' OR [RoleCode]='CHEF_DEPARTEMENT') AND [DepartementId] IS NOT NULL))
GO
ALTER TABLE [dbo].[AffectationOrganisationnelle] CHECK CONSTRAINT [CK_AffectationOrganisationnelle_RoleCible]
GO
ALTER TABLE [dbo].[CotisationAnnuelle]  WITH CHECK ADD  CONSTRAINT [CK_CotisationAnnuelle_Annee] CHECK  (([AnneeExercice]>=(2000) AND [AnneeExercice]<=(2100)))
GO
ALTER TABLE [dbo].[CotisationAnnuelle] CHECK CONSTRAINT [CK_CotisationAnnuelle_Annee]
GO
ALTER TABLE [dbo].[DocumentProfessionnel]  WITH CHECK ADD  CONSTRAINT [CK_DocumentProfessionnel_Dates] CHECK  (([DateExpiration] IS NULL OR [DateDocument] IS NULL OR [DateExpiration]>=[DateDocument]))
GO
ALTER TABLE [dbo].[DocumentProfessionnel] CHECK CONSTRAINT [CK_DocumentProfessionnel_Dates]
GO
ALTER TABLE [dbo].[DocumentProfessionnel]  WITH CHECK ADD  CONSTRAINT [CK_DocumentProfessionnel_Taille] CHECK  (([TailleOctets]>=(0)))
GO
ALTER TABLE [dbo].[DocumentProfessionnel] CHECK CONSTRAINT [CK_DocumentProfessionnel_Taille]
GO
ALTER TABLE [dbo].[InstallationService]  WITH CHECK ADD  CONSTRAINT [CK_InstallationService_Dates] CHECK  (([DateFin] IS NULL OR [DateDebut] IS NULL OR [DateFin]>=[DateDebut]))
GO
ALTER TABLE [dbo].[InstallationService] CHECK CONSTRAINT [CK_InstallationService_Dates]
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique]  WITH CHECK ADD  CONSTRAINT [CK_ProfessionnelAffectationClinique_Dates] CHECK  (([DateFin] IS NULL OR [DateDebut] IS NULL OR [DateFin]>=[DateDebut]))
GO
ALTER TABLE [dbo].[ProfessionnelAffectationClinique] CHECK CONSTRAINT [CK_ProfessionnelAffectationClinique_Dates]
GO
ALTER TABLE [dbo].[ProfessionnelInstallation]  WITH CHECK ADD  CONSTRAINT [CK_ProfessionnelInstallation_Rang] CHECK  (([RangInstallation]>=(1) AND [RangInstallation]<=(3)))
GO
ALTER TABLE [dbo].[ProfessionnelInstallation] CHECK CONSTRAINT [CK_ProfessionnelInstallation_Rang]
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege]  WITH CHECK ADD  CONSTRAINT [CK_ProfessionnelPrivilege_Dates] CHECK  (([DateFin] IS NULL OR [DateFin]>=[DateDebut]))
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege] CHECK CONSTRAINT [CK_ProfessionnelPrivilege_Dates]
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege]  WITH CHECK ADD  CONSTRAINT [CK_ProfessionnelPrivilege_Etat] CHECK  (([Etat]=N'Non renouvelé' OR [Etat]=N'Refusé' OR [Etat]=N'En attente' OR [Etat]=N'Expiré' OR [Etat]=N'Suspendu' OR [Etat]=N'Actif'))
GO
ALTER TABLE [dbo].[ProfessionnelPrivilege] CHECK CONSTRAINT [CK_ProfessionnelPrivilege_Etat]
GO
USE [master]
GO
ALTER DATABASE [bd_cmdpsf] SET  READ_WRITE 
GO

USE [bd_cmdpsf];
GO

-- VUES DIFFEREES APRES CREATION DES TABLES
/****** Object:  View [dbo].[vw_CotisationsParAnnee]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_CotisationsParAnnee]
AS
SELECT
    c.CotisationAnnuelleId,
    m.ProfessionnelId,
    m.Permis,
    m.Nom,
    m.Prenom,
    c.AnneeExercice,
    cc.CodeCotisation,
    c.MontantInitial,
    c.MontantFinal,
    c.MontantPaye,
    c.EtatCotisation,
    c.DatePaiement
FROM dbo.CotisationAnnuelle c
INNER JOIN dbo.Professionnel m ON m.ProfessionnelId = c.ProfessionnelId
LEFT JOIN dbo.CodeCotisation cc ON cc.CodeCotisationId = c.CodeCotisationId;
GO

/****** Object:  View [dbo].[vw_DepartementInformationCourante]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* Vue des responsables courants d'un departement. */
CREATE   VIEW [dbo].[vw_DepartementInformationCourante]
AS
SELECT
    d.DepartementId,
    d.NomDepartement,
    d.CodeDepartement,
    d.EstActif,

    Chef.ProfessionnelId AS ChefDepartementProfessionnelId,
    Chef.NomAffiche AS ChefDepartement,
    Chef.DateDebut AS DateNominationChefDepartement,
    Chef.EstInterim AS ChefDepartementEstInterim,
    Chef.EstAValider AS ChefDepartementEstAValider,

    Secretaire.NomAffiche AS Secretaire,
    Secretaire.Telephone AS SecretaireTelephone,
    Secretaire.PosteTelephone AS SecretairePosteTelephone,
    Secretaire.EstAValider AS SecretaireEstAValider,

    Adjoint.ProfessionnelId AS AdjointSiteProfessionnelId,
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

/****** Object:  View [dbo].[vw_ServiceInformationCourante]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* Vue des responsables courants d'un service. */
CREATE   VIEW [dbo].[vw_ServiceInformationCourante]
AS
SELECT
    s.ServiceId,
    s.DepartementId,
    d.NomDepartement,
    s.NomService,
    s.CodeService,
    s.RegroupementSource,
    s.EstActif,

    Chef.ProfessionnelId AS ChefServiceProfessionnelId,
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

/****** Object:  View [dbo].[vw_AffectationOrganisationnelleCourante]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* Vue de toutes les affectations actuellement actives. */
CREATE   VIEW [dbo].[vw_AffectationOrganisationnelleCourante]
AS
SELECT
    a.AffectationOrganisationnelleId,
    a.RoleCode,
    a.DepartementId,
    d.NomDepartement,
    a.ServiceId,
    s.NomService,
    a.RegroupementSource,
    a.ProfessionnelId,
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

/****** Object:  View [dbo].[vw_CMDPSF_Registre]    Script Date: 2026-10-13 10:02:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   VIEW [dbo].[vw_CMDPSF_Registre]
AS
SELECT
    m.ProfessionnelId,
    m.ProfessionId,
    p.CodeProfession,
    p.Libelle AS Profession,
    m.Permis,
    m.Nom,
    m.Prenom,
    m.TitreId,
    m.TypePermisId,
    m.StatutProfessionnelId,
    m.CategoriePratiqueId,
    m.DateEntreeFonction,
    m.DateDepart,
    m.DateObtentionPermis,
    m.DateExpirationPermis,
    m.NumeroMembreOrdre,
    m.DateDerniereVerificationOrdre,
    m.ReferenceVerificationOrdre,
    m.EstActif,
    m.DateCreation,
    m.DateModification
FROM dbo.Professionnel AS m
INNER JOIN ref.Profession AS p ON p.ProfessionId = m.ProfessionId;

GO

-- Référentiel minimal de professions. Les tables sont créées sans les données métier historiques.
USE [bd_cmdpsf];
GO
IF OBJECT_ID(N'ref.Profession',N'U') IS NULL
    THROW 51001, N'La table ref.Profession est absente.', 1;
GO
INSERT INTO ref.Profession(CodeProfession,Libelle,EstActif)
SELECT p.CodeProfession,p.Libelle,CONVERT(bit,1)
FROM (VALUES
 (N'MEDECIN',N'Médecin'),
 (N'DENTISTE',N'Dentiste'),
 (N'PHARMACIEN',N'Pharmacien'),
 (N'SAGE_FEMME',N'Sage-femme')
) AS p(CodeProfession,Libelle)
WHERE NOT EXISTS(SELECT 1 FROM ref.Profession x WHERE x.CodeProfession=p.CodeProfession);
GO
