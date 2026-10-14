using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Solution.Models;
using Solution.ViewModels.Shared;

namespace Solution.Services
{
    public interface IMedecinAlerteService
    {
        List<AlerteMedecinItemViewModel> PourMedecin(int medecinId);
        TableauBordAlertesViewModel ConstruireTableauBord(int maxAlertes = 30);
    }

    public class MedecinAlerteService : IMedecinAlerteService
    {
        private readonly HopitalMedecinsDbEntities db;
        private readonly IConfiguration configuration;
        private readonly IMemoryCache cache;

        public MedecinAlerteService(
            HopitalMedecinsDbEntities db,
            IConfiguration configuration,
            IMemoryCache cache)
        {
            this.db = db;
            this.configuration = configuration;
            this.cache = cache;
        }

        public List<AlerteMedecinItemViewModel> PourMedecin(int medecinId)
        {
            return ConstruireAlertes(medecinId).OrderBy(Priorite)
                .ThenBy(x => x.DateEcheance ?? DateTime.MaxValue)
                .ToList();
        }

        public TableauBordAlertesViewModel ConstruireTableauBord(int maxAlertes = 30)
        {
            var today = DateTime.Today;
            var fitMois = Math.Max(1, configuration.GetValue<int?>("Alertes:FitTestValiditeMois") ?? 12);
            var limite = Math.Max(1, maxAlertes);
            var cacheKey = $"rdm-tableau-bord-effectifs-v3-{today:yyyyMMdd}-{fitMois}-{limite}";

            return cache.GetOrCreate(cacheKey, entry =>
            {
                // Le tableau de bord est identique pour tous les utilisateurs autorisés.
                // Ce cache très court absorbe les rafraîchissements successifs sans
                // masquer longtemps une modification métier récente.
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return ConstruireTableauBordNonCache(limite, today, fitMois);
            });
        }

        private TableauBordAlertesViewModel ConstruireTableauBordNonCache(
            int maxAlertes,
            DateTime today,
            int fitMois)
        {
            var d30 = today.AddDays(30);
            var d90 = today.AddDays(90);
            var fitLimite = today.AddMonths(-fitMois);

            // Agrégation SQL sans aucun filtre sur l'activité ni sur le statut :
            // chaque professionnel contribue exactement une fois à sa profession,
            // et les médecins sont répartis via leur catégorie de pratique.
            var effectifs = db.Medecin.AsNoTracking()
                .GroupBy(m => new
                {
                    CodeProfession = m.Profession != null ? m.Profession.CodeProfession : "MEDECIN",
                    CategoriePratique = m.CategoriePratique != null ? m.CategoriePratique.Libelle : null
                })
                .Select(g => new
                {
                    g.Key.CodeProfession,
                    g.Key.CategoriePratique,
                    Nombre = g.Count()
                })
                .ToList();

            var vm = new TableauBordAlertesViewModel
            {
                DateGeneration = DateTime.Now,
                Privileges30Jours = db.MedecinPrivilege.AsNoTracking().Count(x =>
                    x.EstActif && x.DateFin.HasValue && x.DateFin.Value >= today && x.DateFin.Value <= d30),

                Privileges90Jours = db.MedecinPrivilege.AsNoTracking().Count(x =>
                    x.EstActif && x.DateFin.HasValue && x.DateFin.Value > d30 && x.DateFin.Value <= d90),

                CotisationsNonReglees = db.CotisationAnnuelle.AsNoTracking().Count(x =>
                    x.EtatCotisation == null ||
                    (x.EtatCotisation != "Payée" && x.EtatCotisation != "Payee" &&
                     x.EtatCotisation != "Annulée" && x.EtatCotisation != "Exemptée")),

                FitTestsAEcheance = db.FitTest.AsNoTracking().Count(x =>
                    x.EstActif && x.DateFitTest.HasValue && x.DateFitTest.Value <= fitLimite),

                EntentesFdc90Jours = db.EntenteFDC.AsNoTracking().Count(x =>
                    x.EstActive && x.DateFin.HasValue && x.DateFin.Value >= today && x.DateFin.Value <= d90),

                AffectationsAValider = db.AffectationOrganisationnelle.AsNoTracking().Count(x =>
                    x.EstActive && x.EstAValider),

                Documents90Jours = db.DocumentMedecin.AsNoTracking().Count(x =>
                    x.EstActif && x.DateExpiration.HasValue &&
                    x.DateExpiration.Value >= today && x.DateExpiration.Value <= d90)
            };

            foreach (var ligne in effectifs)
            {
                var profession = (ligne.CodeProfession ?? "MEDECIN").Trim().ToUpperInvariant();
                vm.TotalProfessionnels += ligne.Nombre;

                switch (profession)
                {
                    case "DENTISTE":
                        vm.Dentistes += ligne.Nombre;
                        break;
                    case "PHARMACIEN":
                        vm.Pharmaciens += ligne.Nombre;
                        break;
                    case "SAGE_FEMME":
                        vm.SagesFemmes += ligne.Nombre;
                        break;
                    case "MEDECIN":
                        vm.TotalMedecins += ligne.Nombre;
                        var categorie = MedecinCategorieRegistre.Determiner(ligne.CategoriePratique);
                        if (categorie == MedecinCategorieRegistre.Omnipraticien)
                        {
                            vm.MedecinsOmnipraticiens += ligne.Nombre;
                        }
                        else if (categorie == MedecinCategorieRegistre.Specialiste)
                        {
                            vm.MedecinsSpecialistes += ligne.Nombre;
                        }
                        else
                        {
                            // Ne pas attribuer arbitrairement une catégorie inconnue.
                            // Ces médecins figurent dans le total et restent visibles
                            // dans le récapitulatif du tableau de bord.
                            vm.MedecinsNonClasses += ligne.Nombre;
                        }
                        break;
                }
            }

            vm.Alertes = ChargerAlertesTableauBord(maxAlertes, today, d30, d90, fitLimite, fitMois);

            return vm;
        }

        /// <summary>
        /// Charge directement les alertes prioritaires par catégorie. Le nombre de
        /// requêtes est fixe et ne dépend plus du nombre de médecins concernés.
        /// Chaque requête ne projette que les colonnes affichées; le contenu binaire
        /// des documents n'est notamment jamais chargé par le tableau de bord.
        /// </summary>
        private List<AlerteMedecinItemViewModel> ChargerAlertesTableauBord(
            int maxAlertes,
            DateTime today,
            DateTime d30,
            DateTime d90,
            DateTime fitLimite,
            int fitMois)
        {
            var alertes = new List<AlerteMedecinItemViewModel>(maxAlertes * 6);

            var privileges = db.MedecinPrivilege.AsNoTracking()
                .Where(x => x.EstActif && x.DateFin.HasValue &&
                            x.DateFin.Value >= today && x.DateFin.Value <= d90)
                .OrderBy(x => x.DateFin)
                .Select(x => new
                {
                    x.MedecinPrivilegeId,
                    x.MedecinId,
                    x.DateFin,
                    Libelle = x.PrivilegeReference != null
                        ? x.PrivilegeReference.LibellePrivilege
                        : null,
                    x.Medecin.Permis,
                    x.Medecin.Nom,
                    x.Medecin.Prenom
                })
                .Take(maxAlertes)
                .ToList();

            foreach (var privilege in privileges)
            {
                var jours = (privilege.DateFin.Value.Date - today).Days;
                alertes.Add(new AlerteMedecinItemViewModel
                {
                    MedecinId = privilege.MedecinId,
                    Medecin = NomMedecin(privilege.Permis, privilege.Nom, privilege.Prenom),
                    Categorie = "Privilège",
                    Niveau = privilege.DateFin.Value <= d30 ? "danger" : "warning",
                    Message = $"{privilege.Libelle ?? "Privilège"} expire dans {jours} jour(s).",
                    DateEcheance = privilege.DateFin,
                    Controller = "MedecinPrivileges",
                    Action = "Edit",
                    Id = privilege.MedecinPrivilegeId
                });
            }

            var cotisations = db.CotisationAnnuelle.AsNoTracking()
                .Where(x => x.EtatCotisation == null ||
                            (x.EtatCotisation != "Payée" && x.EtatCotisation != "Payee" &&
                             x.EtatCotisation != "Annulée" && x.EtatCotisation != "Exemptée"))
                .OrderByDescending(x => x.AnneeExercice)
                .ThenBy(x => x.MedecinId)
                .Select(x => new
                {
                    x.CotisationAnnuelleId,
                    x.MedecinId,
                    x.AnneeExercice,
                    x.EtatCotisation,
                    x.Medecin.Permis,
                    x.Medecin.Nom,
                    x.Medecin.Prenom
                })
                .Take(maxAlertes)
                .ToList();

            foreach (var cotisation in cotisations)
            {
                alertes.Add(new AlerteMedecinItemViewModel
                {
                    MedecinId = cotisation.MedecinId,
                    Medecin = NomMedecin(cotisation.Permis, cotisation.Nom, cotisation.Prenom),
                    Categorie = "Cotisation",
                    Niveau = "warning",
                    Message = $"Cotisation {cotisation.AnneeExercice} non réglée ({cotisation.EtatCotisation ?? "état non défini"}).",
                    Controller = "CotisationsAnnuelles",
                    Action = "Edit",
                    Id = cotisation.CotisationAnnuelleId
                });
            }

            var fitTests = db.FitTest.AsNoTracking()
                .Where(x => x.EstActif && x.DateFitTest.HasValue && x.DateFitTest.Value <= fitLimite)
                .OrderBy(x => x.DateFitTest)
                .Select(x => new
                {
                    x.FitTestId,
                    x.MedecinId,
                    x.DateFitTest,
                    x.Medecin.Permis,
                    x.Medecin.Nom,
                    x.Medecin.Prenom
                })
                .Take(maxAlertes)
                .ToList();

            foreach (var fitTest in fitTests)
            {
                alertes.Add(new AlerteMedecinItemViewModel
                {
                    MedecinId = fitTest.MedecinId,
                    Medecin = NomMedecin(fitTest.Permis, fitTest.Nom, fitTest.Prenom),
                    Categorie = "Fit Test",
                    Niveau = "warning",
                    Message = $"Fit Test daté du {fitTest.DateFitTest:yyyy-MM-dd}; vérifier le renouvellement.",
                    DateEcheance = fitTest.DateFitTest.Value.AddMonths(fitMois),
                    Controller = "FitTests",
                    Action = "Edit",
                    Id = fitTest.FitTestId
                });
            }

            var ententes = db.EntenteFDC.AsNoTracking()
                .Where(x => x.EstActive && x.DateFin.HasValue &&
                            x.DateFin.Value >= today && x.DateFin.Value <= d90)
                .OrderBy(x => x.DateFin)
                .Select(x => new
                {
                    x.EntenteFDCId,
                    x.MedecinId,
                    x.DateFin,
                    x.Medecin.Permis,
                    x.Medecin.Nom,
                    x.Medecin.Prenom
                })
                .Take(maxAlertes)
                .ToList();

            foreach (var entente in ententes)
            {
                var jours = (entente.DateFin.Value.Date - today).Days;
                alertes.Add(new AlerteMedecinItemViewModel
                {
                    MedecinId = entente.MedecinId,
                    Medecin = NomMedecin(entente.Permis, entente.Nom, entente.Prenom),
                    Categorie = "Entente FDC",
                    Niveau = entente.DateFin.Value <= d30 ? "danger" : "warning",
                    Message = $"Entente FDC expire dans {jours} jour(s).",
                    DateEcheance = entente.DateFin,
                    Controller = "EntentesFDC",
                    Action = "Edit",
                    Id = entente.EntenteFDCId
                });
            }

            var affectations = db.AffectationOrganisationnelle.AsNoTracking()
                .Where(x => x.EstActive && x.EstAValider && x.MedecinId.HasValue)
                .OrderBy(x => x.DateModification)
                .Select(x => new
                {
                    x.AffectationOrganisationnelleId,
                    MedecinId = x.MedecinId.Value,
                    x.NomAffiche,
                    x.Medecin.Permis,
                    x.Medecin.Nom,
                    x.Medecin.Prenom
                })
                .Take(maxAlertes)
                .ToList();

            foreach (var affectation in affectations)
            {
                alertes.Add(new AlerteMedecinItemViewModel
                {
                    MedecinId = affectation.MedecinId,
                    Medecin = NomMedecin(affectation.Permis, affectation.Nom, affectation.Prenom),
                    Categorie = "Organisation",
                    Niveau = "warning",
                    Message = $"Affectation organisationnelle « {affectation.NomAffiche} » à valider.",
                    Controller = "AffectationsOrganisationnelles",
                    Action = "Edit",
                    Id = affectation.AffectationOrganisationnelleId
                });
            }

            var documents = db.DocumentMedecin.AsNoTracking()
                .Where(x => x.EstActif && x.DateExpiration.HasValue &&
                            x.DateExpiration.Value >= today && x.DateExpiration.Value <= d90)
                .OrderBy(x => x.DateExpiration)
                .Select(x => new
                {
                    x.DocumentMedecinId,
                    x.MedecinId,
                    x.TypeDocument,
                    x.DateExpiration,
                    x.Medecin.Permis,
                    x.Medecin.Nom,
                    x.Medecin.Prenom
                })
                .Take(maxAlertes)
                .ToList();

            foreach (var document in documents)
            {
                var jours = (document.DateExpiration.Value.Date - today).Days;
                alertes.Add(new AlerteMedecinItemViewModel
                {
                    MedecinId = document.MedecinId,
                    Medecin = NomMedecin(document.Permis, document.Nom, document.Prenom),
                    Categorie = "Document",
                    Niveau = document.DateExpiration.Value <= d30 ? "danger" : "warning",
                    Message = $"Document « {document.TypeDocument} » expire dans {jours} jour(s).",
                    DateEcheance = document.DateExpiration,
                    Controller = "DocumentsMedecin",
                    Action = "Details",
                    Id = document.DocumentMedecinId
                });
            }

            return alertes
                .OrderBy(Priorite)
                .ThenBy(x => x.DateEcheance ?? DateTime.MaxValue)
                .ThenBy(x => x.Categorie)
                .ThenBy(x => x.Medecin)
                .Take(maxAlertes)
                .ToList();
        }

        private static string NomMedecin(string permis, string nom, string prenom)
        {
            return $"{permis} - {nom} {prenom}".Trim();
        }

        private IEnumerable<AlerteMedecinItemViewModel> ConstruireAlertes(int medecinId)
        {
            var today = DateTime.Today;
            var d30 = today.AddDays(30);
            var d90 = today.AddDays(90);
            var fitMois = Math.Max(1, configuration.GetValue<int?>("Alertes:FitTestValiditeMois") ?? 12);
            var fitLimite = today.AddMonths(-fitMois);

            var medecin = db.Medecin.AsNoTracking()
                .Where(x => x.MedecinId == medecinId)
                .Select(x => new { x.MedecinId, x.Permis, x.Nom, x.Prenom })
                .FirstOrDefault();

            if (medecin == null)
                yield break;

            var nom = medecin.Permis + " - " + medecin.Nom + " " + medecin.Prenom;

            foreach (var p in db.MedecinPrivilege.AsNoTracking()
                         .Where(x => x.MedecinId == medecinId && x.EstActif &&
                                     x.DateFin.HasValue && x.DateFin.Value >= today && x.DateFin.Value <= d90)
                         .Include(x => x.PrivilegeReference)
                         .OrderBy(x => x.DateFin))
            {
                var jours = (p.DateFin.Value.Date - today).Days;
                yield return new AlerteMedecinItemViewModel
                {
                    MedecinId = medecinId,
                    Medecin = nom,
                    Categorie = "Privilège",
                    Niveau = jours <= 30 ? "danger" : "warning",
                    Message = $"{p.PrivilegeReference?.LibellePrivilege ?? "Privilège"} expire dans {jours} jour(s).",
                    DateEcheance = p.DateFin,
                    Controller = "MedecinPrivileges",
                    Action = "Edit",
                    Id = p.MedecinPrivilegeId
                };
            }

            foreach (var c in db.CotisationAnnuelle.AsNoTracking()
                         .Where(x => x.MedecinId == medecinId &&
                                     (x.EtatCotisation == null ||
                                      (x.EtatCotisation != "Payée" && x.EtatCotisation != "Payee" &&
                                       x.EtatCotisation != "Annulée" && x.EtatCotisation != "Exemptée")))
                         .OrderByDescending(x => x.AnneeExercice))
            {
                yield return new AlerteMedecinItemViewModel
                {
                    MedecinId = medecinId,
                    Medecin = nom,
                    Categorie = "Cotisation",
                    Niveau = "warning",
                    Message = $"Cotisation {c.AnneeExercice} non réglée ({c.EtatCotisation ?? "état non défini"}).",
                    Controller = "CotisationsAnnuelles",
                    Action = "Edit",
                    Id = c.CotisationAnnuelleId
                };
            }

            foreach (var f in db.FitTest.AsNoTracking()
                         .Where(x => x.MedecinId == medecinId && x.EstActif &&
                                     x.DateFitTest.HasValue && x.DateFitTest.Value <= fitLimite)
                         .OrderBy(x => x.DateFitTest))
            {
                yield return new AlerteMedecinItemViewModel
                {
                    MedecinId = medecinId,
                    Medecin = nom,
                    Categorie = "Fit Test",
                    Niveau = "warning",
                    Message = $"Fit Test daté du {f.DateFitTest:yyyy-MM-dd}; vérifier le renouvellement.",
                    DateEcheance = f.DateFitTest.Value.AddMonths(fitMois),
                    Controller = "FitTests",
                    Action = "Edit",
                    Id = f.FitTestId
                };
            }

            foreach (var e in db.EntenteFDC.AsNoTracking()
                         .Where(x => x.MedecinId == medecinId && x.EstActive &&
                                     x.DateFin.HasValue && x.DateFin.Value >= today && x.DateFin.Value <= d90)
                         .OrderBy(x => x.DateFin))
            {
                var jours = (e.DateFin.Value.Date - today).Days;
                yield return new AlerteMedecinItemViewModel
                {
                    MedecinId = medecinId,
                    Medecin = nom,
                    Categorie = "Entente FDC",
                    Niveau = jours <= 30 ? "danger" : "warning",
                    Message = $"Entente FDC expire dans {jours} jour(s).",
                    DateEcheance = e.DateFin,
                    Controller = "EntentesFDC",
                    Action = "Edit",
                    Id = e.EntenteFDCId
                };
            }

            foreach (var a in db.AffectationOrganisationnelle.AsNoTracking()
                         .Where(x => x.MedecinId == medecinId && x.EstActive && x.EstAValider))
            {
                yield return new AlerteMedecinItemViewModel
                {
                    MedecinId = medecinId,
                    Medecin = nom,
                    Categorie = "Organisation",
                    Niveau = "warning",
                    Message = $"Affectation organisationnelle « {a.NomAffiche} » à valider.",
                    Controller = "AffectationsOrganisationnelles",
                    Action = "Edit",
                    Id = a.AffectationOrganisationnelleId
                };
            }

            foreach (var doc in db.DocumentMedecin.AsNoTracking()
                         .Where(x => x.MedecinId == medecinId && x.EstActif &&
                                     x.DateExpiration.HasValue &&
                                     x.DateExpiration.Value >= today && x.DateExpiration.Value <= d90)
                         .OrderBy(x => x.DateExpiration))
            {
                var jours = (doc.DateExpiration.Value.Date - today).Days;
                yield return new AlerteMedecinItemViewModel
                {
                    MedecinId = medecinId,
                    Medecin = nom,
                    Categorie = "Document",
                    Niveau = jours <= 30 ? "danger" : "warning",
                    Message = $"Document « {doc.TypeDocument} » expire dans {jours} jour(s).",
                    DateEcheance = doc.DateExpiration,
                    Controller = "DocumentsMedecin",
                    Action = "Details",
                    Id = doc.DocumentMedecinId
                };
            }
        }

        private static int Priorite(AlerteMedecinItemViewModel x)
        {
            return x.Niveau switch
            {
                "danger" => 0,
                "warning" => 1,
                _ => 2
            };
        }
    }
}
