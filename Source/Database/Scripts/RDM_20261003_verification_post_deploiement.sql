USE [RDM_CEMTL];
GO

SELECT 'Departement' AS Objet, COUNT(*) AS Nombre FROM dbo.Departement
UNION ALL SELECT 'Service', COUNT(*) FROM dbo.Service
UNION ALL SELECT 'AffectationOrganisationnelle', COUNT(*) FROM dbo.AffectationOrganisationnelle
UNION ALL SELECT 'MedecinAffectationClinique', COUNT(*) FROM dbo.MedecinAffectationClinique
UNION ALL SELECT 'DocumentMedecin', COUNT(*) FROM dbo.DocumentMedecin;
GO

SELECT TOP (50)
    m.Permis,
    m.Nom,
    m.Prenom,
    d.NomDepartement,
    s.NomService,
    a.EstPrincipale,
    a.EstActive,
    a.DateDebut,
    a.DateFin
FROM dbo.MedecinAffectationClinique a
INNER JOIN dbo.Medecin m ON m.MedecinId = a.MedecinId
INNER JOIN dbo.Departement d ON d.DepartementId = a.DepartementId
INNER JOIN dbo.Service s ON s.ServiceId = a.ServiceId
ORDER BY m.Nom, m.Prenom, a.EstPrincipale DESC;
GO
