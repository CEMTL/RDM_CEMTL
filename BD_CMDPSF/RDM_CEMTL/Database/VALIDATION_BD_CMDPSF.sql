-- Diagnostic de compatibilité pour la solution ASP.NET Core BD_CMDPSF.
-- Lecture seule : aucune donnée et aucun schéma ne seront modifiés.
USE [bd_cmdpsf];
GO
DECLARE @Attendus TABLE (SchemaObjet sysname, Objet sysname, TypeObjet char(2));
INSERT INTO @Attendus (SchemaObjet,Objet,TypeObjet) VALUES
 (N'dbo',N'Professionnel',N'U'),
 (N'dbo',N'ProfessionnelAdresse',N'U'),
 (N'dbo',N'ProfessionnelAffectationClinique',N'U'),
 (N'dbo',N'ProfessionnelComite',N'U'),
 (N'dbo',N'ProfessionnelCourriel',N'U'),
 (N'dbo',N'ProfessionnelInstallation',N'U'),
 (N'dbo',N'ProfessionnelPrivilege',N'U'),
 (N'dbo',N'ProfessionnelTelephone',N'U'),
 (N'dbo',N'DocumentProfessionnel',N'U'),
 (N'dbo',N'CotisationAnnuelle',N'U'),
 (N'dbo',N'EntenteFDC',N'U'),
 (N'dbo',N'FitTest',N'U'),
 (N'dbo',N'NominationAdministrative',N'U'),
 (N'dbo',N'AffectationOrganisationnelle',N'U'),
 (N'dbo',N'AspNetUsers',N'U'),
 (N'dbo',N'AspNetRoles',N'U'),
 (N'dbo',N'AspNetUserRoles',N'U'),
 (N'dbo',N'vw_CotisationsParAnnee',N'V'),
 (N'dbo',N'vw_CMDPSF_Registre',N'V'),
 (N'ref',N'Profession',N'U'),
 (N'ref',N'StatutProfessionnel',N'U'),
 (N'ref',N'TitreProfessionnel',N'U'),
 (N'ref',N'CategoriePratique',N'U'),
 (N'ref',N'Genre',N'U'),
 (N'ref',N'TypePermis',N'U'),
 (N'ref',N'TypeCourriel',N'U');

SELECT a.SchemaObjet, a.Objet, a.TypeObjet,
       CASE WHEN o.object_id IS NULL THEN N'ABSENT' ELSE N'OK' END AS Etat
FROM @Attendus a
LEFT JOIN sys.objects o ON o.object_id = OBJECT_ID(QUOTENAME(a.SchemaObjet)+N'.'+QUOTENAME(a.Objet),a.TypeObjet)
ORDER BY Etat, SchemaObjet, Objet;
GO

-- Vérifier les colonnes ID renommées ainsi que les liaisons de statut.
SELECT v.Objet, v.Colonne, CASE WHEN c.column_id IS NULL THEN N'ABSENTE' ELSE N'OK' END AS Etat
FROM (VALUES
  (N'dbo.Professionnel',N'ProfessionnelId'),
  (N'dbo.ProfessionnelAffectationClinique',N'ProfessionnelAffectationCliniqueId'),
  (N'dbo.ProfessionnelAdresse',N'ProfessionnelAdresseId'),
  (N'dbo.ProfessionnelComite',N'ProfessionnelComiteId'),
  (N'dbo.ProfessionnelCourriel',N'ProfessionnelCourrielId'),
  (N'dbo.ProfessionnelInstallation',N'ProfessionnelInstallationId'),
  (N'dbo.ProfessionnelPrivilege',N'ProfessionnelPrivilegeId'),
  (N'dbo.ProfessionnelTelephone',N'ProfessionnelTelephoneId'),
  (N'dbo.Professionnel',N'StatutProfessionnelId'),
  (N'dbo.Professionnel',N'ProfessionId'),
  (N'dbo.DocumentProfessionnel',N'DocumentProfessionnelId'),
  (N'dbo.DocumentProfessionnel',N'ProfessionnelId'),
  (N'dbo.CotisationAnnuelle',N'ProfessionnelId'),
  (N'dbo.NominationAdministrative',N'ProfessionnelId'),
  (N'dbo.AffectationOrganisationnelle',N'ProfessionnelId'),
  (N'ref.StatutProfessionnel',N'StatutProfessionnelId'),
  (N'dbo.vw_CotisationsParAnnee',N'ProfessionnelId')
) v(Objet,Colonne)
LEFT JOIN sys.columns c
  ON c.object_id = OBJECT_ID(v.Objet) AND c.name = v.Colonne
ORDER BY Etat,v.Objet,v.Colonne;
GO

SELECT p.CodeProfession, p.Libelle, COUNT(pr.ProfessionnelId) AS EffectifTousStatuts
FROM ref.Profession p
LEFT JOIN dbo.Professionnel pr ON pr.ProfessionId = p.ProfessionId
GROUP BY p.CodeProfession,p.Libelle
ORDER BY p.CodeProfession;
GO
