using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Solution.Models;
using Solution.ViewModels.Medecins;
using Solution.Services;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinsController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;
        private readonly DbContextOptions<HopitalMedecinsDbEntities> _dbOptions;
        private readonly IMedecinAlerteService _alertes;
        private readonly ILogger<MedecinsController> _logger;

        public MedecinsController(
            HopitalMedecinsDbEntities db,
            DbContextOptions<HopitalMedecinsDbEntities> dbOptions,
            IMedecinAlerteService alertes,
            ILogger<MedecinsController> logger)
        {
            this.db = db;
            _dbOptions = dbOptions;
            _alertes = alertes;
            _logger = logger;
        }
        private AuditService _audit => new AuditService(db, HttpContext);

        private const string SearchCollation = "Latin1_General_100_CI_AI";

        private sealed class MedecinIndexRow
        {
            public int MedecinId { get; set; }
            public string Permis { get; set; }
            public string NAM { get; set; }
            public string Nom { get; set; }
            public string Prenom { get; set; }
            public string NomComplet { get; set; }
            public string TypePermis { get; set; }
            public string StatutMedecin { get; set; }
            public string CategoriePratique { get; set; }
            public string TelephonePrincipal { get; set; }
            public string CourrielPrincipal { get; set; }
            public string InstallationPrincipale { get; set; }
            public string NominationActive { get; set; }
            public bool EstActifBool { get; set; }
            public string EstActif { get; set; }
        }

        private static string EscapeLikePattern(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_")
                .Replace("[", "\\[");
        }

        [HttpPost]
        public ActionResult GetMedecins(string searchTop, bool? actifsSeulement)
        {
            int draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault());
            int start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            int length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());

            if (length <= 0)
                length = 10;

            // Le champ de recherche principal est celui affiché au-dessus du tableau.
            // On conserve aussi la prise en charge de search[value] au cas où DataTables
            // envoie une recherche globale depuis un autre écran/client.
            string dataTableSearch = Request.Form["search[value]"].FirstOrDefault();
            string termeRecherche = !string.IsNullOrWhiteSpace(searchTop)
                ? searchTop.Trim()
                : (!string.IsNullOrWhiteSpace(dataTableSearch) ? dataTableSearch.Trim() : null);

            bool filtreActifs = actifsSeulement ?? false;

            // Projection SQL des valeurs réellement affichées dans le tableau.
            // Ainsi, la recherche porte bien sur TOUTES les colonnes visibles.
            IQueryable<MedecinIndexRow> query = db.Medecin
                .AsNoTracking()
                .Select(m => new MedecinIndexRow
                {
                    MedecinId = m.MedecinId,
                    Permis = m.Permis ?? "",
                    NAM = m.NAM ?? "",
                    Nom = m.Nom ?? "",
                    Prenom = m.Prenom ?? "",
                    NomComplet = ((m.TitreMedecin != null ? m.TitreMedecin.Libelle : "") + " " +
                                  (m.Prenom ?? "") + " " + (m.Nom ?? "")).Trim(),
                    TypePermis = m.TypePermis != null ? m.TypePermis.Libelle : "",
                    StatutMedecin = m.StatutMedecin != null ? m.StatutMedecin.Libelle : "",
                    CategoriePratique = m.CategoriePratique != null ? m.CategoriePratique.Libelle : "",
                    TelephonePrincipal = m.MedecinTelephone
                        .OrderByDescending(t => t.EstPrincipal)
                        .ThenBy(t => t.RangSource)
                        .Select(t => t.NumeroTelephone)
                        .FirstOrDefault() ?? "",
                    CourrielPrincipal = m.MedecinCourriel
                        .OrderByDescending(c => c.EstPrincipal)
                        .ThenBy(c => c.RangSource)
                        .Select(c => c.AdresseCourriel)
                        .FirstOrDefault() ?? "",
                    InstallationPrincipale = m.MedecinInstallation
                        .OrderByDescending(i => i.EstPrincipale)
                        .ThenBy(i => i.RangInstallation)
                        .Select(i => i.Installation != null ? i.Installation.NomInstallation : "")
                        .FirstOrDefault() ?? "",
                    NominationActive = m.NominationAdministrative
                        .OrderByDescending(n => n.EstActive)
                        .ThenByDescending(n => n.DateNomination)
                        .Select(n => n.LibelleNomination)
                        .FirstOrDefault() ?? "",
                    EstActifBool = m.EstActif,
                    EstActif = m.EstActif ? "Oui" : "Non"
                });

            int recordsTotal = query.Count();

            if (filtreActifs)
            {
                query = query.Where(m => m.EstActifBool);
            }

            if (!string.IsNullOrWhiteSpace(termeRecherche))
            {
                // Chaque mot doit être retrouvé quelque part dans la ligne.
                // Exemple : "Jose Tremblay" trouve "José Tremblay", même si le prénom
                // et le nom sont stockés dans deux champs distincts.
                var termes = termeRecherche
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var terme in termes)
                {
                    string motif = "%" + EscapeLikePattern(terme) + "%";

                    query = query.Where(m =>
                        EF.Functions.Like(EF.Functions.Collate(m.Permis ?? "", SearchCollation), motif, "\\") ||
                        EF.Functions.Like(EF.Functions.Collate(m.NAM ?? "", SearchCollation), motif, "\\") ||
                        EF.Functions.Like(EF.Functions.Collate(m.NomComplet ?? "", SearchCollation), motif, "\\") ||
                        EF.Functions.Like(EF.Functions.Collate(m.TypePermis ?? "", SearchCollation), motif, "\\") ||
                        EF.Functions.Like(EF.Functions.Collate(m.StatutMedecin ?? "", SearchCollation), motif, "\\") ||
                        EF.Functions.Like(EF.Functions.Collate(m.CategoriePratique ?? "", SearchCollation), motif, "\\") ||
                        EF.Functions.Like(EF.Functions.Collate(m.TelephonePrincipal ?? "", SearchCollation), motif, "\\") ||
                        EF.Functions.Like(EF.Functions.Collate(m.CourrielPrincipal ?? "", SearchCollation), motif, "\\") ||
                        EF.Functions.Like(EF.Functions.Collate(m.InstallationPrincipale ?? "", SearchCollation), motif, "\\") ||
                        EF.Functions.Like(EF.Functions.Collate(m.NominationActive ?? "", SearchCollation), motif, "\\") ||
                        EF.Functions.Like(EF.Functions.Collate(m.EstActif ?? "", SearchCollation), motif, "\\"));
                }
            }

            int recordsFiltered = query.Count();

            // DataTables est en mode serverSide : le tri doit également être effectué
            // côté serveur pour correspondre réellement à la colonne sélectionnée.
            int orderColumn = Convert.ToInt32(Request.Form["order[0][column]"].FirstOrDefault() ?? "1");
            bool descending = string.Equals(
                Request.Form["order[0][dir]"].FirstOrDefault(),
                "desc",
                StringComparison.OrdinalIgnoreCase);

            IOrderedQueryable<MedecinIndexRow> orderedQuery = orderColumn switch
            {
                0 => descending ? query.OrderByDescending(m => m.Permis) : query.OrderBy(m => m.Permis),
                1 => descending
                    ? query.OrderByDescending(m => m.Nom).ThenByDescending(m => m.Prenom)
                    : query.OrderBy(m => m.Nom).ThenBy(m => m.Prenom),
                2 => descending ? query.OrderByDescending(m => m.TypePermis) : query.OrderBy(m => m.TypePermis),
                3 => descending ? query.OrderByDescending(m => m.StatutMedecin) : query.OrderBy(m => m.StatutMedecin),
                4 => descending ? query.OrderByDescending(m => m.CategoriePratique) : query.OrderBy(m => m.CategoriePratique),
                5 => descending ? query.OrderByDescending(m => m.TelephonePrincipal) : query.OrderBy(m => m.TelephonePrincipal),
                6 => descending ? query.OrderByDescending(m => m.CourrielPrincipal) : query.OrderBy(m => m.CourrielPrincipal),
                7 => descending ? query.OrderByDescending(m => m.InstallationPrincipale) : query.OrderBy(m => m.InstallationPrincipale),
                8 => descending ? query.OrderByDescending(m => m.NominationActive) : query.OrderBy(m => m.NominationActive),
                9 => descending ? query.OrderByDescending(m => m.EstActifBool) : query.OrderBy(m => m.EstActifBool),
                _ => query.OrderBy(m => m.Nom).ThenBy(m => m.Prenom)
            };

            var data = orderedQuery
                .Skip(start)
                .Take(length)
                .Select(m => new
                {
                    m.MedecinId,
                    m.Permis,
                    m.NomComplet,
                    m.TypePermis,
                    m.StatutMedecin,
                    m.CategoriePratique,
                    m.TelephonePrincipal,
                    m.CourrielPrincipal,
                    m.InstallationPrincipale,
                    m.NominationActive,
                    m.EstActif
                })
                .ToList();

            return Json(new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data
            });
        }
        private object CreerSnapshotAudit(Medecin entity)
        {
            return new
            {
                entity.MedecinId,
                entity.TitreId,
                entity.Permis,
                entity.NAM,
                entity.Nom,
                entity.Prenom,
                entity.TypePermisId,
                entity.StatutMedecinId,
                entity.CategoriePratiqueId,
                entity.GenreId,
                entity.DateNaissance,
                entity.DateEntreeFonction,
                entity.DateDepart,
                entity.Commentaires,
                entity.EstActif,
                entity.DateCreation,
                entity.DateModification
            };
        }

        [HttpGet]
        public ActionResult Index(string search, bool? actifsSeulement, int? openMedecin = null, string tab = null)
        {
            ViewBag.Search = search;
            ViewBag.ActifsSeulement = actifsSeulement ?? false;
            ViewBag.OpenMedecin = openMedecin;
            ViewBag.OpenTab = NormaliserOngletDetails(tab);
            return View();
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        [HttpGet]
        public ActionResult Create()
        {
            return RedirectToAction("Edit", new { id = 0 });
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        [HttpGet]
        public ActionResult Edit(int? id, string tab = null)
        {
            ViewBag.ActiveEditTab = NormaliserOngletEdit(tab);

            if (!id.HasValue || id.Value == 0)
            {
                var nouveau = new MedecinEditViewModel
                {
                    EstActif = true
                };

                nouveau.Adresses.Add(new MedecinAdresseEditItemViewModel());
                nouveau.Telephones.Add(new MedecinTelephoneEditItemViewModel());
                nouveau.Courriels.Add(new MedecinCourrielEditItemViewModel());
                nouveau.Installations.Add(new MedecinInstallationEditItemViewModel { RangInstallation = 1 });
                nouveau.NominationsAdministratives.Add(new NominationAdministrativeEditItemViewModel());

                ChargerListesEdit(nouveau);
                return View(nouveau);
            }

            var entity = db.Medecin
                .Include(m => m.MedecinAdresse)
                .Include(m => m.MedecinTelephone)
                .Include(m => m.MedecinCourriel).ThenInclude(c => c.TypeCourrielReference)
                .Include(m => m.MedecinInstallation)
                .Include(m => m.NominationAdministrative)
                .FirstOrDefault(m => m.MedecinId == id.Value);

            if (entity == null)
                return NotFound();

            var model = MapEdit(entity);
            ChargerListesEdit(model);
            ChargerDossierEdit(model);
            return View(model);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MedecinEditViewModel model, string activeTab)
        {
            var ongletActif = NormaliserOngletEdit(activeTab);
            ViewBag.ActiveEditTab = ongletActif;

            model.Adresses = model.Adresses ?? new List<MedecinAdresseEditItemViewModel>();
            model.Telephones = model.Telephones ?? new List<MedecinTelephoneEditItemViewModel>();
            model.Courriels = model.Courriels ?? new List<MedecinCourrielEditItemViewModel>();
            model.Installations = model.Installations ?? new List<MedecinInstallationEditItemViewModel>();
            model.NominationsAdministratives = model.NominationsAdministratives ?? new List<NominationAdministrativeEditItemViewModel>();

            // Les validateurs de propriétés sont exécutés avant l'action par MVC, y compris
            // pour une ligne que l'utilisateur vient de marquer « Retirer ». Ces erreurs ne
            // doivent pas empêcher la suppression demandée.
            IgnorerErreursLignesSupprimees(model);

            if (!string.IsNullOrWhiteSpace(model.Permis))
            {
                var permis = model.Permis.Trim();
                bool existe = db.Medecin.Any(m => m.Permis == permis && m.MedecinId != model.MedecinId);
                if (existe)
                {
                    AjouterErreurModelStateUnique("Permis", "Un autre médecin possède déjà ce permis.");
                }
            }

            // Validation serveur complémentaire. Elle transforme notamment les références
            // supprimées/inactives et les identifiants de lignes devenus invalides en
            // erreurs rattachées à un champ précis au lieu de laisser lever une exception.
            ValiderDonneesEdit(model);

            if (!ModelState.IsValid)
            {
                return RetournerEditAvecErreurs(model);
            }

            bool creation = model.MedecinId == 0;
            object ancienneValeur = null;

            if (!creation)
            {
                var entityAvantModification = db.Medecin
                    .AsNoTracking()
                    .FirstOrDefault(m => m.MedecinId == model.MedecinId);

                if (entityAvantModification == null)
                    return NotFound();

                ancienneValeur = CreerSnapshotAudit(entityAvantModification);
            }

            string etapeEnregistrement = "Informations générales";
            string cleEtape = "InformationsGenerales";
            object nouvelleValeur = null;
            int medecinIdEnregistre = model.MedecinId;

            try
            {
                // EnableRetryOnFailure interdit une transaction créée directement. Toute
                // l'unité transactionnelle doit être rejouable par la stratégie SQL.
                // Un DbContext neuf est créé à chaque tentative afin qu'une reprise ne
                // conserve pas des états EF déjà acceptés lors d'une tentative annulée.
                var executionStrategy = db.Database.CreateExecutionStrategy();
                executionStrategy.Execute(() =>
                {
                    using var transactionDb = new HopitalMedecinsDbEntities(_dbOptions);
                    using var transaction = transactionDb.Database.BeginTransaction();

                    Medecin entity;
                    if (creation)
                    {
                        entity = new Medecin { DateCreation = DateTime.Now };
                        transactionDb.Medecin.Add(entity);
                    }
                    else
                    {
                        entity = transactionDb.Medecin
                            .Include(m => m.MedecinAdresse)
                            .Include(m => m.MedecinTelephone)
                            .Include(m => m.MedecinCourriel).ThenInclude(c => c.TypeCourrielReference)
                            .Include(m => m.MedecinInstallation)
                            .Include(m => m.NominationAdministrative)
                            .FirstOrDefault(m => m.MedecinId == model.MedecinId);

                        if (entity == null)
                            throw new DbUpdateConcurrencyException(
                                "Le médecin a été supprimé depuis l'ouverture de la page.");
                    }

                    entity.TitreId = model.TitreId;
                    entity.Permis = (model.Permis ?? "").Trim();
                    entity.NAM = NullIfWhiteSpace(model.NAM);
                    entity.Nom = (model.Nom ?? "").Trim();
                    entity.Prenom = (model.Prenom ?? "").Trim();
                    entity.TypePermisId = model.TypePermisId;
                    entity.StatutMedecinId = model.StatutMedecinId;
                    entity.CategoriePratiqueId = model.CategoriePratiqueId;
                    entity.GenreId = model.GenreId;
                    entity.DateNaissance = model.DateNaissance;
                    entity.DateEntreeFonction = model.DateEntreeFonction;
                    entity.DateDepart = model.DateDepart;
                    entity.Commentaires = NullIfWhiteSpace(model.Commentaires);
                    entity.EstActif = model.EstActif;
                    entity.DateModification = DateTime.Now;

                    if (creation)
                    {
                        etapeEnregistrement = "Création du médecin";
                        cleEtape = "InformationsGenerales";
                        transactionDb.SaveChanges();
                    }
                    else
                    {
                        transactionDb.SaveChanges();
                    }

                    etapeEnregistrement = "Adresses";
                    cleEtape = "Adresses";
                    SynchroniserAdresses(transactionDb, entity, model);
                    transactionDb.SaveChanges();

                    etapeEnregistrement = "Téléphones";
                    cleEtape = "Telephones";
                    SynchroniserTelephones(transactionDb, entity, model);
                    transactionDb.SaveChanges();

                    etapeEnregistrement = "Courriels";
                    cleEtape = "Courriels";
                    SynchroniserCourriels(transactionDb, entity, model);
                    transactionDb.SaveChanges();

                    etapeEnregistrement = "Installations";
                    cleEtape = "Installations";
                    SynchroniserInstallations(transactionDb, entity, model);
                    transactionDb.SaveChanges();

                    etapeEnregistrement = "Nominations administratives";
                    cleEtape = "NominationsAdministratives";
                    SynchroniserNominations(transactionDb, entity, model);
                    transactionDb.SaveChanges();

                    etapeEnregistrement = "Préparation du journal d'audit";
                    cleEtape = "JournalAudit";
                    nouvelleValeur = CreerSnapshotAudit(entity);

                    etapeEnregistrement = "Validation de la transaction";
                    cleEtape = "";
                    transaction.Commit();

                    medecinIdEnregistre = entity.MedecinId;
                });

                model.MedecinId = medecinIdEnregistre;
            }
            catch (DbUpdateException ex)
            {
                AjouterErreurEnregistrementMedecin(ex, etapeEnregistrement, cleEtape, model);

                if (creation)
                    model.MedecinId = 0;

                return RetournerEditAvecErreurs(model);
            }
            catch (SqlException ex)
            {
                // Une erreur SQL peut être levée directement par Commit(), sans être
                // enveloppée dans DbUpdateException.
                AjouterErreurEnregistrementMedecin(ex, etapeEnregistrement, cleEtape, model);

                if (creation)
                    model.MedecinId = 0;

                return RetournerEditAvecErreurs(model);
            }
            catch (Exception ex)
            {
                AjouterErreurExceptionInattendue(ex, etapeEnregistrement, cleEtape, model);

                if (creation)
                    model.MedecinId = 0;

                return RetournerEditAvecErreurs(model);
            }

            // L'audit est important, mais une indisponibilité ou une incompatibilité de
            // dbo.AuditLog ne doit jamais annuler une modification métier déjà valide.
            // Il est donc écrit après la validation de la transaction principale.
            try
            {
                _audit.Enregistrer(
                    "Medecin",
                    medecinIdEnregistre,
                    creation ? "INSERT" : "UPDATE",
                    ancienneValeur,
                    nouvelleValeur);

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                var referenceAudit = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpperInvariant();
                _logger.LogError(ex,
                    "Le médecin {MedecinId} a été enregistré, mais son audit a échoué. Référence {Reference}.",
                    medecinIdEnregistre, referenceAudit);

                // Ne pas laisser une ligne d'audit invalide dans le contexte EF.
                foreach (var entree in db.ChangeTracker.Entries<AuditLog>()
                             .Where(x => x.State == EntityState.Added)
                             .ToList())
                    entree.State = EntityState.Detached;

                TempData["Warning"] =
                    $"Le médecin a été enregistré, mais le journal d'audit n'a pas pu être écrit. Référence : {referenceAudit}.";
            }

            TempData["Success"] = creation
                ? "Le médecin a été créé avec succès."
                : "Le médecin a été modifié avec succès.";

            return RedirectToAction("Edit", new { id = medecinIdEnregistre, tab = ongletActif });
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
                return BadRequest();

            var entity = db.Medecin
                .AsNoTracking()
                .Include(m => m.TitreMedecin)
                .Include(m => m.TypePermis)
                .Include(m => m.StatutMedecin)
                .Include(m => m.CategoriePratique)
                .Include(m => m.MedecinAdresse)
                .Include(m => m.MedecinTelephone)
                .Include(m => m.MedecinCourriel).ThenInclude(c => c.TypeCourrielReference)
                .Include(m => m.MedecinInstallation)
                .Include(m => m.NominationAdministrative)
                .FirstOrDefault(m => m.MedecinId == id.Value);

            if (entity == null)
                return NotFound();

            var model = new MedecinDeleteViewModel
            {
                MedecinId = entity.MedecinId,
                Permis = entity.Permis,
                NomComplet = ((entity.TitreMedecin != null ? entity.TitreMedecin.Libelle + " " : "") + entity.Prenom + " " + entity.Nom).Trim(),
                TypePermis = entity.TypePermis != null ? entity.TypePermis.Libelle : "",
                StatutMedecin = entity.StatutMedecin != null ? entity.StatutMedecin.Libelle : "",
                CategoriePratique = entity.CategoriePratique != null ? entity.CategoriePratique.Libelle : "",
                EstActif = entity.EstActif,
                DateEntreeFonction = entity.DateEntreeFonction,
                DateDepart = entity.DateDepart,
                Commentaires = entity.Commentaires,
                NbAdresses = entity.MedecinAdresse.Count,
                NbTelephones = entity.MedecinTelephone.Count,
                NbCourriels = entity.MedecinCourriel.Count,
                NbInstallations = entity.MedecinInstallation.Count,
                NbNominationsAdministratives = entity.NominationAdministrative.Count,
                NbCotisationsAnnuelles = db.CotisationAnnuelle.Count(x => x.MedecinId == entity.MedecinId),
                NbEntentesFDC = db.EntenteFDC.Count(x => x.MedecinId == entity.MedecinId),
                NbFitTests = db.FitTest.Count(x => x.MedecinId == entity.MedecinId),
                NbComites = db.MedecinComite.Count(x => x.MedecinId == entity.MedecinId),
                NbPrivileges = db.MedecinPrivilege.Count(x => x.MedecinId == entity.MedecinId),
                NbAffectationsOrganisationnelles = db.AffectationOrganisationnelle.Count(x => x.MedecinId == entity.MedecinId),
                NbAffectationsCliniques = db.MedecinAffectationClinique.Count(x => x.MedecinId == entity.MedecinId),
                NbDocuments = db.DocumentMedecin.Count(x => x.MedecinId == entity.MedecinId && x.EstActif)
            };

            return View(model);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int MedecinId)
        {
            var entity = db.Medecin
                .Include(m => m.MedecinAdresse)
                .Include(m => m.MedecinTelephone)
                .Include(m => m.MedecinCourriel).ThenInclude(c => c.TypeCourrielReference)
                .Include(m => m.MedecinInstallation)
                .Include(m => m.NominationAdministrative)
                .FirstOrDefault(m => m.MedecinId == MedecinId);

            if (entity == null)
                return NotFound();

            // Les données métier/historiques ne sont jamais supprimées automatiquement.
            // Elles doivent être traitées ou réaffectées avant la suppression du médecin.
            var dependancesBloquantes = new List<string>();

            int nbCotisations = db.CotisationAnnuelle.Count(x => x.MedecinId == MedecinId);
            int nbEntentesFdc = db.EntenteFDC.Count(x => x.MedecinId == MedecinId);
            int nbFitTests = db.FitTest.Count(x => x.MedecinId == MedecinId);
            int nbComites = db.MedecinComite.Count(x => x.MedecinId == MedecinId);
            int nbPrivileges = db.MedecinPrivilege.Count(x => x.MedecinId == MedecinId);
            int nbAffectationsOrganisationnelles = db.AffectationOrganisationnelle.Count(x => x.MedecinId == MedecinId);
            int nbAffectationsCliniques = db.MedecinAffectationClinique.Count(x => x.MedecinId == MedecinId);
            int nbDocuments = db.DocumentMedecin.Count(x => x.MedecinId == MedecinId && x.EstActif);

            if (nbCotisations > 0) dependancesBloquantes.Add($"{nbCotisations} cotisation(s) annuelle(s)");
            if (nbEntentesFdc > 0) dependancesBloquantes.Add($"{nbEntentesFdc} entente(s) FDC");
            if (nbFitTests > 0) dependancesBloquantes.Add($"{nbFitTests} Fit Test(s)");
            if (nbComites > 0) dependancesBloquantes.Add($"{nbComites} association(s) à un comité");
            if (nbPrivileges > 0) dependancesBloquantes.Add($"{nbPrivileges} privilège(s)");
            if (nbAffectationsOrganisationnelles > 0) dependancesBloquantes.Add($"{nbAffectationsOrganisationnelles} fonction(s) organisationnelle(s)");
            if (nbAffectationsCliniques > 0) dependancesBloquantes.Add($"{nbAffectationsCliniques} affectation(s) clinique(s)");
            if (nbDocuments > 0) dependancesBloquantes.Add($"{nbDocuments} document(s) actif(s)");

            if (dependancesBloquantes.Any())
            {
                TempData["Error"] =
                    "Suppression impossible : ce médecin possède encore des données métier ou historiques : "
                    + string.Join(", ", dependancesBloquantes)
                    + ". Retirez ou réaffectez d'abord ces données.";

                return RedirectToAction("Delete", new { id = MedecinId });
            }

            var ancienneValeur = new
            {
                Medecin = CreerSnapshotAudit(entity),
                NbAdresses = entity.MedecinAdresse.Count,
                NbTelephones = entity.MedecinTelephone.Count,
                NbCourriels = entity.MedecinCourriel.Count,
                NbInstallations = entity.MedecinInstallation.Count,
                NbNominationsAdministratives = entity.NominationAdministrative.Count
            };

            if (entity.NominationAdministrative.Any())
                db.NominationAdministrative.RemoveRange(entity.NominationAdministrative.ToList());

            if (entity.MedecinInstallation.Any())
                db.MedecinInstallation.RemoveRange(entity.MedecinInstallation.ToList());

            if (entity.MedecinCourriel.Any())
                db.MedecinCourriel.RemoveRange(entity.MedecinCourriel.ToList());

            if (entity.MedecinTelephone.Any())
                db.MedecinTelephone.RemoveRange(entity.MedecinTelephone.ToList());

            if (entity.MedecinAdresse.Any())
                db.MedecinAdresse.RemoveRange(entity.MedecinAdresse.ToList());

            db.Medecin.Remove(entity);

            _audit.Enregistrer(
                "Medecin",
                MedecinId,
                "DELETE",
                ancienneValeur,
                null);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = UserFriendlyExceptionService.GetMessage(ex, "supprimer le médecin");
                return RedirectToAction("Delete", new { id = MedecinId });
            }

            TempData["Success"] = "Le médecin et ses données liées ont été supprimés avec succès.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DetailsModal(int? id, string tab = null)
        {
            if (!id.HasValue)
                return BadRequest();

            var model = ConstruireDossierMedecin(id.Value);
            if (model == null)
                return NotFound();

            ViewBag.ActiveTab = NormaliserOngletDetails(tab);
            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("SuperUtilisateur");

            return PartialView("_DetailsModal", model);
        }

        private MedecinDetailsModalViewModel ConstruireDossierMedecin(int medecinId)
        {
            var medecin = db.Medecin
                .AsNoTracking()
                .Include(m => m.TitreMedecin)
                .Include(m => m.TypePermis)
                .Include(m => m.StatutMedecin)
                .Include(m => m.CategoriePratique)
                .Include(m => m.Genre)
                .Include(m => m.MedecinAdresse)
                .Include(m => m.MedecinTelephone)
                .Include(m => m.MedecinCourriel).ThenInclude(c => c.TypeCourrielReference)
                .Include(m => m.MedecinInstallation).ThenInclude(i => i.Installation)
                .Include(m => m.NominationAdministrative)
                .FirstOrDefault(m => m.MedecinId == medecinId);

            if (medecin == null)
                return null;

            var model = new MedecinDetailsModalViewModel
            {
                MedecinId = medecin.MedecinId,
                Permis = medecin.Permis,
                Titre = medecin.TitreMedecin != null ? medecin.TitreMedecin.Libelle : "",
                Nom = medecin.Nom,
                Prenom = medecin.Prenom,
                NomComplet = ((medecin.TitreMedecin != null ? medecin.TitreMedecin.Libelle + " " : "") + medecin.Prenom + " " + medecin.Nom).Trim(),
                TypePermis = medecin.TypePermis != null ? medecin.TypePermis.Libelle : "",
                StatutMedecin = medecin.StatutMedecin != null ? medecin.StatutMedecin.Libelle : "",
                CategoriePratique = medecin.CategoriePratique != null ? medecin.CategoriePratique.Libelle : "",
                Genre = medecin.Genre != null ? medecin.Genre.Libelle : "",
                DateNaissance = medecin.DateNaissance,
                NAM = medecin.NAM,
                DateEntreeFonction = medecin.DateEntreeFonction,
                DateDepart = medecin.DateDepart,
                EstActif = medecin.EstActif,
                Commentaires = medecin.Commentaires,
                DateCreation = medecin.DateCreation,
                DateModification = medecin.DateModification
            };

            model.Adresses = medecin.MedecinAdresse
                .OrderByDescending(a => a.EstPrincipale)
                .Select(a => new MedecinAdresseItemVm
                {
                    AdresseLigne1 = a.AdresseLigne1,
                    Ville = a.Ville,
                    Province = a.Province,
                    CodePostal = a.CodePostal,
                    EstPrincipale = a.EstPrincipale
                }).ToList();

            model.Telephones = medecin.MedecinTelephone
                .OrderByDescending(t => t.EstPrincipal)
                .ThenBy(t => t.RangSource)
                .Select(t => new MedecinTelephoneItemVm
                {
                    TypeTelephone = t.TypeTelephone,
                    NumeroTelephone = t.NumeroTelephone,
                    EstPrincipal = t.EstPrincipal,
                    RangSource = t.RangSource
                }).ToList();

            model.Courriels = medecin.MedecinCourriel
                .OrderByDescending(c => c.EstPrincipal)
                .ThenBy(c => c.RangSource)
                .Select(c => new MedecinCourrielItemVm
                {
                    TypeCourriel = c.TypeCourrielReference != null ? c.TypeCourrielReference.Libelle : c.TypeCourriel,
                    AdresseCourriel = c.AdresseCourriel,
                    EstPrincipal = c.EstPrincipal,
                    RangSource = c.RangSource
                }).ToList();

            model.Installations = medecin.MedecinInstallation
                .OrderByDescending(i => i.EstPrincipale)
                .ThenBy(i => i.RangInstallation)
                .Select(i => new MedecinInstallationItemVm
                {
                    NomInstallation = i.Installation != null ? i.Installation.NomInstallation : "",
                    CodeInstallation = i.Installation != null ? i.Installation.CodeInstallation : "",
                    RangInstallation = i.RangInstallation,
                    EstPrincipale = i.EstPrincipale,
                    DateDebut = i.DateDebut,
                    DateFin = i.DateFin
                }).ToList();

            model.NominationsAdministratives = medecin.NominationAdministrative
                .OrderByDescending(n => n.EstActive)
                .ThenByDescending(n => n.DateNomination)
                .Select(n => new NominationAdministrativeItemVm
                {
                    NominationAdministrativeId = n.NominationAdministrativeId,
                    LibelleNomination = n.LibelleNomination,
                    DateNomination = n.DateNomination,
                    DateFin = n.DateFin,
                    EstActive = n.EstActive
                }).ToList();

            var adressePrincipale = model.Adresses.FirstOrDefault(a => a.EstPrincipale) ?? model.Adresses.FirstOrDefault();
            if (adressePrincipale != null)
            {
                model.AdressePrincipale = string.Join(", ",
                    new[] { adressePrincipale.AdresseLigne1, adressePrincipale.Ville, adressePrincipale.Province, adressePrincipale.CodePostal }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            model.TelephonePrincipal = model.Telephones.FirstOrDefault(t => t.EstPrincipal)?.NumeroTelephone ?? model.Telephones.FirstOrDefault()?.NumeroTelephone;
            model.CourrielPrincipal = model.Courriels.FirstOrDefault(c => c.EstPrincipal)?.AdresseCourriel ?? model.Courriels.FirstOrDefault()?.AdresseCourriel;
            model.InstallationPrincipale = model.Installations.FirstOrDefault(i => i.EstPrincipale)?.NomInstallation ?? model.Installations.FirstOrDefault()?.NomInstallation;
            model.NominationActive = model.NominationsAdministratives.FirstOrDefault(n => n.EstActive)?.LibelleNomination ?? model.NominationsAdministratives.FirstOrDefault()?.LibelleNomination;

            // Charger les volets métier dans des requêtes séparées. Cela évite un très
            // gros JOIN cartésien lorsque le médecin possède plusieurs coordonnées,
            // privilèges, cotisations, fit tests et ententes en même temps.
            var today = DateTime.Today;

            model.Privileges = db.MedecinPrivilege
                .AsNoTracking()
                .Include(x => x.PrivilegeReference)
                .Include(x => x.Installation)
                .Include(x => x.Service).ThenInclude(x => x.Departement)
                .Where(x => x.MedecinId == medecin.MedecinId)
                .OrderByDescending(x => x.EstActif && x.Etat == "Actif" && x.DateDebut <= today && (!x.DateFin.HasValue || x.DateFin >= today))
                .ThenByDescending(x => x.DateDebut)
                .ThenBy(x => x.PrivilegeReference.LibellePrivilege)
                .Select(x => new MedecinPrivilegeDetailsItemVm
                {
                    MedecinPrivilegeId = x.MedecinPrivilegeId,
                    CodePrivilege = x.PrivilegeReference != null ? x.PrivilegeReference.CodePrivilege : "",
                    PrivilegeLibelle = x.PrivilegeReference != null ? x.PrivilegeReference.LibellePrivilege : "",
                    InstallationLibelle = x.Installation != null ? x.Installation.NomInstallation : "",
                    ServiceLibelle = x.Service != null ? x.Service.NomService : "",
                    DepartementLibelle = x.Service != null && x.Service.Departement != null ? x.Service.Departement.NomDepartement : "",
                    DateDebut = x.DateDebut,
                    DateFin = x.DateFin,
                    Etat = x.Etat,
                    EstActif = x.EstActif && x.Etat == "Actif" && x.DateDebut <= today && (!x.DateFin.HasValue || x.DateFin >= today)
                })
                .ToList();

            model.AffectationsCliniques = db.MedecinAffectationClinique
                .AsNoTracking()
                .Include(x => x.Departement)
                .Include(x => x.Service)
                .Include(x => x.Installation)
                .Where(x => x.MedecinId == medecin.MedecinId)
                .OrderByDescending(x => x.EstActive)
                .ThenByDescending(x => x.EstPrincipale)
                .ThenBy(x => x.Departement.NomDepartement)
                .ThenBy(x => x.Service.NomService)
                .Select(x => new MedecinAffectationCliniqueDetailsItemVm
                {
                    MedecinAffectationCliniqueId = x.MedecinAffectationCliniqueId,
                    Departement = x.Departement != null ? x.Departement.NomDepartement : "",
                    Service = x.Service != null ? x.Service.NomService : "",
                    Installation = x.Installation != null ? x.Installation.NomInstallation : "",
                    TypeAffectation = x.TypeAffectation,
                    EstPrincipale = x.EstPrincipale,
                    DateDebut = x.DateDebut,
                    DateFin = x.DateFin,
                    EstActive = x.EstActive
                })
                .ToList();

            // Résumé Département / Service fondé sur les affectations cliniques,
            // et non plus sur les privilèges.
            model.Services = model.AffectationsCliniques
                .Where(x => !string.IsNullOrWhiteSpace(x.Service))
                .GroupBy(x => new { x.Departement, x.Service })
                .OrderBy(x => x.Key.Departement)
                .ThenBy(x => x.Key.Service)
                .Select(x => new MedecinServiceItemVm
                {
                    Departement = x.Key.Departement,
                    Service = x.Key.Service,
                    NombrePrivileges = model.Privileges.Count(p =>
                        p.DepartementLibelle == x.Key.Departement &&
                        p.ServiceLibelle == x.Key.Service)
                })
                .ToList();

            model.AffectationsOrganisationnelles = db.AffectationOrganisationnelle
                .AsNoTracking()
                .Include(x => x.Departement)
                .Include(x => x.Service).ThenInclude(x => x.Departement)
                .Where(x => x.MedecinId == medecin.MedecinId)
                .OrderByDescending(x => x.EstActive)
                .ThenByDescending(x => x.DateDebut)
                .ThenBy(x => x.RoleCode)
                .Select(x => new MedecinAffectationOrganisationnelleItemVm
                {
                    AffectationOrganisationnelleId = x.AffectationOrganisationnelleId,
                    Role = x.RoleCode == RoleAffectationOrganisationnelle.ChefDepartement
                        ? "Chef de département"
                        : x.RoleCode == RoleAffectationOrganisationnelle.SecretaireDepartement
                            ? "Secrétaire de département"
                            : x.RoleCode == RoleAffectationOrganisationnelle.AdjointSite
                                ? "Adjoint de site"
                                : x.RoleCode == RoleAffectationOrganisationnelle.ChefService
                                    ? "Chef de service"
                                    : x.RoleCode,
                    Departement = x.Departement != null
                        ? x.Departement.NomDepartement
                        : x.Service != null && x.Service.Departement != null
                            ? x.Service.Departement.NomDepartement
                            : "",
                    Service = x.Service != null ? x.Service.NomService : "",
                    Regroupement = x.RegroupementSource,
                    Titre = x.Titre,
                    DateDebut = x.DateDebut,
                    DateFin = x.DateFin,
                    EstActive = x.EstActive,
                    EstAValider = x.EstAValider
                })
                .ToList();

            model.Cotisations = db.CotisationAnnuelle
                .AsNoTracking()
                .Include(x => x.CodeCotisation)
                .Include(x => x.TypePaiement)
                .Where(x => x.MedecinId == medecin.MedecinId)
                .OrderByDescending(x => x.AnneeExercice)
                .ThenByDescending(x => x.CotisationAnnuelleId)
                .Select(x => new MedecinCotisationDetailsItemVm
                {
                    CotisationAnnuelleId = x.CotisationAnnuelleId,
                    AnneeExercice = x.AnneeExercice,
                    CodeCotisation = x.CodeCotisation != null ? x.CodeCotisation.CodeCotisation1 : "",
                    MontantInitial = x.MontantInitial,
                    MontantFinal = x.MontantFinal,
                    MontantPaye = x.MontantPaye,
                    EtatCotisation = x.EtatCotisation,
                    DatePaiement = x.DatePaiement,
                    TypePaiement = x.TypePaiement != null ? x.TypePaiement.Libelle : x.TypePaiementTexte
                })
                .ToList();

            model.FitTests = db.FitTest
                .AsNoTracking()
                .Where(x => x.MedecinId == medecin.MedecinId)
                .OrderByDescending(x => x.DateFitTest)
                .ThenByDescending(x => x.FitTestId)
                .Select(x => new MedecinFitTestDetailsItemVm
                {
                    FitTestId = x.FitTestId,
                    Resultat = x.Resultat,
                    DateFitTest = x.DateFitTest,
                    Commentaire = x.Commentaire,
                    EstActif = x.EstActif
                })
                .ToList();

            model.EntentesFDC = db.EntenteFDC
                .AsNoTracking()
                .Where(x => x.MedecinId == medecin.MedecinId)
                .OrderByDescending(x => x.EstActive)
                .ThenByDescending(x => x.DateAdhesion)
                .ThenByDescending(x => x.EntenteFDCId)
                .Select(x => new MedecinEntenteFdcDetailsItemVm
                {
                    EntenteFDCId = x.EntenteFDCId,
                    TypeEntenteFDC = x.TypeEntenteFDC,
                    DateAdhesion = x.DateAdhesion,
                    DateFin = x.DateFin,
                    PEMVise = x.PEMVise,
                    EstActive = x.EstActive
                })
                .ToList();


            model.Comites = db.MedecinComite
                .AsNoTracking()
                .Include(x => x.Comite)
                .Where(x => x.MedecinId == medecin.MedecinId)
                .OrderByDescending(x => x.DateFin == null || x.DateFin >= today)
                .ThenByDescending(x => x.DateDebut)
                .Select(x => new MedecinComiteDetailsItemVm
                {
                    MedecinComiteId = x.MedecinComiteId,
                    Comite = x.Comite != null ? x.Comite.NomComite : "",
                    RoleMembre = x.RoleMembre,
                    DateDebut = x.DateDebut,
                    DateFin = x.DateFin,
                    Commentaire = x.Commentaire
                })
                .ToList();

            model.Documents = db.DocumentMedecin
                .AsNoTracking()
                .Where(x => x.MedecinId == medecin.MedecinId)
                .OrderByDescending(x => x.EstActif)
                .ThenByDescending(x => x.DateDocument)
                .ThenByDescending(x => x.DateDepot)
                .Select(x => new MedecinDocumentDetailsItemVm
                {
                    DocumentMedecinId = x.DocumentMedecinId,
                    TypeDocument = x.TypeDocument,
                    NomFichier = x.NomFichier,
                    DateDocument = x.DateDocument,
                    DateExpiration = x.DateExpiration,
                    Confidentiel = x.Confidentiel,
                    EstActif = x.EstActif
                })
                .ToList();

            model.Alertes = _alertes.PourMedecin(medecin.MedecinId);

            return model;
        }

        private void ChargerDossierEdit(MedecinEditViewModel model)
        {
            if (model == null)
                return;

            model.Dossier = model.MedecinId > 0
                ? ConstruireDossierMedecin(model.MedecinId) ?? new MedecinDetailsModalViewModel()
                : new MedecinDetailsModalViewModel();
        }

        private static string NormaliserOngletDetails(string tab)
        {
            if (string.IsNullOrWhiteSpace(tab))
                return "coordonnees";

            switch (tab.Trim().ToLowerInvariant())
            {
                case "coordonnees":
                case "organisation":
                case "nominations":
                case "privileges":
                case "cotisations":
                case "fittests":
                case "ententes":
                case "comites":
                case "documents":
                case "alertes":
                    return tab.Trim().ToLowerInvariant();
                default:
                    return "coordonnees";
            }
        }

        private MedecinEditViewModel MapEdit(Medecin entity)
        {
            return new MedecinEditViewModel
            {
                MedecinId = entity.MedecinId,
                TitreId = entity.TitreId,
                Permis = entity.Permis,
                NAM = entity.NAM,
                Nom = entity.Nom,
                Prenom = entity.Prenom,
                TypePermisId = entity.TypePermisId,
                StatutMedecinId = entity.StatutMedecinId,
                CategoriePratiqueId = entity.CategoriePratiqueId,
                GenreId = entity.GenreId,
                DateNaissance = entity.DateNaissance,
                DateEntreeFonction = entity.DateEntreeFonction,
                DateDepart = entity.DateDepart,
                Commentaires = entity.Commentaires,
                EstActif = entity.EstActif,
                DateCreation = entity.DateCreation,
                DateModification = entity.DateModification,
                Adresses = entity.MedecinAdresse
                    .OrderByDescending(a => a.EstPrincipale)
                    .ThenBy(a => a.MedecinAdresseId)
                    .Select(a => new MedecinAdresseEditItemViewModel
                    {
                        MedecinAdresseId = a.MedecinAdresseId,
                        AdresseLigne1 = a.AdresseLigne1,
                        Ville = a.Ville,
                        Province = a.Province,
                        CodePostal = a.CodePostal,
                        EstPrincipale = a.EstPrincipale
                    }).ToList(),
                Telephones = entity.MedecinTelephone
                    .OrderByDescending(t => t.EstPrincipal)
                    .ThenBy(t => t.RangSource)
                    .ThenBy(t => t.MedecinTelephoneId)
                    .Select(t => new MedecinTelephoneEditItemViewModel
                    {
                        MedecinTelephoneId = t.MedecinTelephoneId,
                        TypeTelephone = t.TypeTelephone,
                        NumeroTelephone = t.NumeroTelephone,
                        RangSource = t.RangSource,
                        EstPrincipal = t.EstPrincipal
                    }).ToList(),
                Courriels = entity.MedecinCourriel
                    .OrderByDescending(c => c.EstPrincipal)
                    .ThenBy(c => c.RangSource)
                    .ThenBy(c => c.MedecinCourrielId)
                    .Select(c => new MedecinCourrielEditItemViewModel
                    {
                        MedecinCourrielId = c.MedecinCourrielId,
                        TypeCourriel = c.TypeCourrielReference != null ? c.TypeCourrielReference.Libelle : c.TypeCourriel,
                        AdresseCourriel = c.AdresseCourriel,
                        RangSource = c.RangSource,
                        EstPrincipal = c.EstPrincipal
                    }).ToList(),
                Installations = entity.MedecinInstallation
                    .OrderByDescending(i => i.EstPrincipale)
                    .ThenBy(i => i.RangInstallation)
                    .ThenBy(i => i.MedecinInstallationId)
                    .Select(i => new MedecinInstallationEditItemViewModel
                    {
                        MedecinInstallationId = i.MedecinInstallationId,
                        InstallationId = i.InstallationId,
                        RangInstallation = i.RangInstallation,
                        EstPrincipale = i.EstPrincipale,
                        DateDebut = i.DateDebut,
                        DateFin = i.DateFin
                    }).ToList(),
                NominationsAdministratives = entity.NominationAdministrative
                    .OrderByDescending(n => n.EstActive)
                    .ThenByDescending(n => n.DateNomination)
                    .ThenBy(n => n.NominationAdministrativeId)
                    .Select(n => new NominationAdministrativeEditItemViewModel
                    {
                        NominationAdministrativeId = n.NominationAdministrativeId,
                        NominationId = n.NominationId,
                        TypeNominationId = n.TypeNominationId,
                        LibelleNomination = n.LibelleNomination,
                        DateNomination = n.DateNomination,
                        DateFin = n.DateFin,
                        EstActive = n.EstActive
                    }).ToList()
            };
        }

        private void SynchroniserAdresses(
            HopitalMedecinsDbEntities contexte,
            Medecin entity,
            MedecinEditViewModel model)
        {
            var idsModel = model.Adresses.Where(x => x.MedecinAdresseId > 0).Select(x => x.MedecinAdresseId).ToList();
            var aSupprimerParAbsence = entity.MedecinAdresse.Where(x => !idsModel.Contains(x.MedecinAdresseId)).ToList();

            foreach (var item in aSupprimerParAbsence)
                contexte.MedecinAdresse.Remove(item);

            foreach (var item in model.Adresses)
            {
                bool vide = string.IsNullOrWhiteSpace(item.AdresseLigne1)
                    && string.IsNullOrWhiteSpace(item.Ville)
                    && string.IsNullOrWhiteSpace(item.Province)
                    && string.IsNullOrWhiteSpace(item.CodePostal);

                if (item.Supprimer || vide)
                {
                    if (item.MedecinAdresseId > 0)
                    {
                        var existant = entity.MedecinAdresse.FirstOrDefault(x => x.MedecinAdresseId == item.MedecinAdresseId);
                        if (existant != null)
                            contexte.MedecinAdresse.Remove(existant);
                    }
                    continue;
                }

                MedecinAdresse adresse;
                if (item.MedecinAdresseId > 0)
                {
                    adresse = entity.MedecinAdresse.First(x => x.MedecinAdresseId == item.MedecinAdresseId);
                }
                else
                {
                    adresse = new MedecinAdresse { MedecinId = entity.MedecinId };
                    contexte.MedecinAdresse.Add(adresse);
                }

                adresse.AdresseLigne1 = NullIfWhiteSpace(item.AdresseLigne1);
                adresse.Ville = NullIfWhiteSpace(item.Ville);
                adresse.Province = NullIfWhiteSpace(item.Province);
                adresse.CodePostal = NullIfWhiteSpace(item.CodePostal);
                adresse.EstPrincipale = item.EstPrincipale;
            }
        }

        private void SynchroniserTelephones(
            HopitalMedecinsDbEntities contexte,
            Medecin entity,
            MedecinEditViewModel model)
        {
            var idsModel = model.Telephones.Where(x => x.MedecinTelephoneId > 0).Select(x => x.MedecinTelephoneId).ToList();
            var aSupprimerParAbsence = entity.MedecinTelephone.Where(x => !idsModel.Contains(x.MedecinTelephoneId)).ToList();

            foreach (var item in aSupprimerParAbsence)
                contexte.MedecinTelephone.Remove(item);

            foreach (var item in model.Telephones)
            {
                bool vide = string.IsNullOrWhiteSpace(item.TypeTelephone) && string.IsNullOrWhiteSpace(item.NumeroTelephone);

                if (item.Supprimer || vide)
                {
                    if (item.MedecinTelephoneId > 0)
                    {
                        var existant = entity.MedecinTelephone.FirstOrDefault(x => x.MedecinTelephoneId == item.MedecinTelephoneId);
                        if (existant != null)
                            contexte.MedecinTelephone.Remove(existant);
                    }
                    continue;
                }

                MedecinTelephone tel;
                if (item.MedecinTelephoneId > 0)
                {
                    tel = entity.MedecinTelephone.First(x => x.MedecinTelephoneId == item.MedecinTelephoneId);
                }
                else
                {
                    tel = new MedecinTelephone { MedecinId = entity.MedecinId };
                    contexte.MedecinTelephone.Add(tel);
                }

                tel.TypeTelephone = NullIfWhiteSpace(item.TypeTelephone);
                tel.NumeroTelephone = NullIfWhiteSpace(item.NumeroTelephone);
                tel.RangSource = item.RangSource;
                tel.EstPrincipal = item.EstPrincipal;
            }
        }

        private void SynchroniserCourriels(
            HopitalMedecinsDbEntities contexte,
            Medecin entity,
            MedecinEditViewModel model)
        {
            var libellesDemandes = model.Courriels
                .Where(x => !x.Supprimer && !string.IsNullOrWhiteSpace(x.TypeCourriel))
                .Select(x => x.TypeCourriel.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var typesParLibelle = contexte.TypeCourriel
                .Where(x => libellesDemandes.Contains(x.Libelle))
                .ToList()
                .ToDictionary(x => x.Libelle, StringComparer.OrdinalIgnoreCase);

            var idsModel = model.Courriels.Where(x => x.MedecinCourrielId > 0).Select(x => x.MedecinCourrielId).ToList();
            var aSupprimerParAbsence = entity.MedecinCourriel.Where(x => !idsModel.Contains(x.MedecinCourrielId)).ToList();

            foreach (var item in aSupprimerParAbsence)
                contexte.MedecinCourriel.Remove(item);

            foreach (var item in model.Courriels)
            {
                bool vide = string.IsNullOrWhiteSpace(item.TypeCourriel) && string.IsNullOrWhiteSpace(item.AdresseCourriel);

                if (item.Supprimer || vide)
                {
                    if (item.MedecinCourrielId > 0)
                    {
                        var existant = entity.MedecinCourriel.FirstOrDefault(x => x.MedecinCourrielId == item.MedecinCourrielId);
                        if (existant != null)
                            contexte.MedecinCourriel.Remove(existant);
                    }
                    continue;
                }

                MedecinCourriel courriel;
                if (item.MedecinCourrielId > 0)
                {
                    courriel = entity.MedecinCourriel.First(x => x.MedecinCourrielId == item.MedecinCourrielId);
                }
                else
                {
                    courriel = new MedecinCourriel { MedecinId = entity.MedecinId };
                    contexte.MedecinCourriel.Add(courriel);
                }

                var libelleDemande = NullIfWhiteSpace(item.TypeCourriel);
                typesParLibelle.TryGetValue(libelleDemande ?? string.Empty, out var typeCourriel);
                courriel.TypeCourrielId = typeCourriel?.TypeCourrielId;
                courriel.TypeCourriel = typeCourriel?.Libelle ?? libelleDemande;
                courriel.AdresseCourriel = NullIfWhiteSpace(item.AdresseCourriel);
                courriel.RangSource = item.RangSource;
                courriel.EstPrincipal = item.EstPrincipal;
            }
        }

        private void SynchroniserInstallations(
            HopitalMedecinsDbEntities contexte,
            Medecin entity,
            MedecinEditViewModel model)
        {
            var idsModel = model.Installations.Where(x => x.MedecinInstallationId > 0).Select(x => x.MedecinInstallationId).ToList();
            var aSupprimerParAbsence = entity.MedecinInstallation.Where(x => !idsModel.Contains(x.MedecinInstallationId)).ToList();

            foreach (var item in aSupprimerParAbsence)
                contexte.MedecinInstallation.Remove(item);

            foreach (var item in model.Installations)
            {
                bool vide = !item.InstallationId.HasValue || item.InstallationId.Value <= 0;

                if (item.Supprimer || vide)
                {
                    if (item.MedecinInstallationId > 0)
                    {
                        var existant = entity.MedecinInstallation.FirstOrDefault(x => x.MedecinInstallationId == item.MedecinInstallationId);
                        if (existant != null)
                            contexte.MedecinInstallation.Remove(existant);
                    }
                    continue;
                }

                MedecinInstallation installation;
                if (item.MedecinInstallationId > 0)
                {
                    installation = entity.MedecinInstallation.First(x => x.MedecinInstallationId == item.MedecinInstallationId);
                }
                else
                {
                    installation = new MedecinInstallation { MedecinId = entity.MedecinId };
                    contexte.MedecinInstallation.Add(installation);
                }

                installation.InstallationId = item.InstallationId.Value;
                installation.RangInstallation = item.RangInstallation.Value;
                installation.EstPrincipale = item.EstPrincipale;
                installation.DateDebut = item.DateDebut;
                installation.DateFin = item.DateFin;
            }
        }

        private void SynchroniserNominations(
            HopitalMedecinsDbEntities contexte,
            Medecin entity,
            MedecinEditViewModel model)
        {
            var idsModel = model.NominationsAdministratives
                .Where(x => x.NominationAdministrativeId > 0)
                .Select(x => x.NominationAdministrativeId)
                .ToList();

            var aSupprimerParAbsence = entity.NominationAdministrative
                .Where(x => !idsModel.Contains(x.NominationAdministrativeId))
                .ToList();

            foreach (var item in aSupprimerParAbsence)
                contexte.NominationAdministrative.Remove(item);

            foreach (var item in model.NominationsAdministratives)
            {
                bool vide =
                    (!item.NominationId.HasValue || item.NominationId.Value <= 0) &&
                    (!item.TypeNominationId.HasValue || item.TypeNominationId.Value <= 0) &&
                    string.IsNullOrWhiteSpace(item.LibelleNomination) &&
                    !item.DateNomination.HasValue &&
                    !item.DateFin.HasValue;

                if (item.Supprimer || vide)
                {
                    if (item.NominationAdministrativeId > 0)
                    {
                        var existant = entity.NominationAdministrative
                            .FirstOrDefault(x => x.NominationAdministrativeId == item.NominationAdministrativeId);

                        if (existant != null)
                            contexte.NominationAdministrative.Remove(existant);
                    }
                    continue;
                }

                NominationAdministrative nomination;
                if (item.NominationAdministrativeId > 0)
                {
                    nomination = entity.NominationAdministrative
                        .First(x => x.NominationAdministrativeId == item.NominationAdministrativeId);
                }
                else
                {
                    nomination = new NominationAdministrative
                    {
                        MedecinId = entity.MedecinId
                    };
                    contexte.NominationAdministrative.Add(nomination);
                }

                nomination.NominationId = item.NominationId;
                nomination.TypeNominationId = item.TypeNominationId;
                nomination.LibelleNomination = NullIfWhiteSpace(item.LibelleNomination);
                nomination.DateNomination = item.DateNomination;
                nomination.DateFin = item.DateFin;
                nomination.EstActive = item.EstActive;
            }
        }

        private void ChargerListesEdit(MedecinEditViewModel model)
        {
            model.Titres = db.TitreMedecin
                .Where(x => x.EstActif || (model.TitreId.HasValue && x.TitreId == model.TitreId.Value))
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.TitreId.ToString(),
                    Text = x.Libelle + (x.EstActif ? "" : " (inactif)"),
                    Selected = model.TitreId.HasValue && x.TitreId == model.TitreId.Value
                }).ToList();

            model.TypesPermis = db.TypePermis
                .Where(x => x.EstActif || (model.TypePermisId.HasValue && x.TypePermisId == model.TypePermisId.Value))
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.TypePermisId.ToString(),
                    Text = x.Libelle + (x.EstActif ? "" : " (inactif)"),
                    Selected = model.TypePermisId.HasValue && x.TypePermisId == model.TypePermisId.Value
                }).ToList();

            model.StatutsMedecin = db.StatutMedecin
                .Where(x => x.EstActif || (model.StatutMedecinId.HasValue && x.StatutMedecinId == model.StatutMedecinId.Value))
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.StatutMedecinId.ToString(),
                    Text = x.Libelle + (x.EstActif ? "" : " (inactif)"),
                    Selected = model.StatutMedecinId.HasValue && x.StatutMedecinId == model.StatutMedecinId.Value
                }).ToList();

            model.CategoriesPratique = db.CategoriePratique
                .Where(x => x.EstActif || (model.CategoriePratiqueId.HasValue && x.CategoriePratiqueId == model.CategoriePratiqueId.Value))
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.CategoriePratiqueId.ToString(),
                    Text = x.Libelle + (x.EstActif ? "" : " (inactif)"),
                    Selected = model.CategoriePratiqueId.HasValue && x.CategoriePratiqueId == model.CategoriePratiqueId.Value
                }).ToList();

            model.Genres = db.Genre
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.GenreId.ToString(),
                    Text = x.Libelle,
                    Selected = model.GenreId.HasValue && x.GenreId == model.GenreId.Value
                }).ToList();

            model.TypesTelephoneDisponibles = new[]
            {
                "Personnel",
                "Professionnel",
                "Cellulaire",
                "Bureau",
                "Clinique",
                "Hospitalier",
                "Résidence",
                "Secrétariat",
                "Urgence",
                "Télécopieur"
            }
            .Concat(model.Telephones
                .Where(x => !x.Supprimer && !string.IsNullOrWhiteSpace(x.TypeTelephone))
                .Select(x => x.TypeTelephone.Trim()))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(x => new SelectListItem
            {
                Value = x,
                Text = x
            })
            .ToList();

            var typesCourrielActifs = db.TypeCourriel
                .AsNoTracking()
                .Where(x => x.EstActif)
                .OrderBy(x => x.Libelle)
                .Select(x => x.Libelle)
                .ToList();
            var typesCourrielConnus = typesCourrielActifs.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var optionsTypesCourriel = typesCourrielActifs
                .Select(x => new SelectListItem { Value = x, Text = x })
                .ToList();

            foreach (var ancienType in model.Courriels
                .Where(x => !x.Supprimer && !string.IsNullOrWhiteSpace(x.TypeCourriel))
                .Select(x => x.TypeCourriel.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(x => !typesCourrielConnus.Contains(x))
                .OrderBy(x => x))
            {
                optionsTypesCourriel.Add(new SelectListItem
                {
                    Value = ancienType,
                    Text = ancienType + " (ancienne valeur)"
                });
            }

            model.TypesCourrielDisponibles = optionsTypesCourriel;
            var installationIdsSelectionnees = model.Installations
                .Where(x => !x.Supprimer && x.InstallationId.HasValue && x.InstallationId.Value > 0)
                .Select(x => x.InstallationId.Value)
                .Distinct()
                .ToList();

            model.InstallationsDisponibles = db.Installation
                .Where(x => x.EstActif || installationIdsSelectionnees.Contains(x.InstallationId))
                .OrderBy(x => x.NomInstallation)
                .Select(x => new SelectListItem
                {
                    Value = x.InstallationId.ToString(),
                    Text = x.NomInstallation + (x.EstActif ? "" : " (inactive)")
                }).ToList();

            var nominationIdsSelectionnees = model.NominationsAdministratives
                .Where(x => !x.Supprimer && x.NominationId.HasValue && x.NominationId.Value > 0)
                .Select(x => x.NominationId.Value)
                .Distinct()
                .ToList();

            model.NominationsDisponibles = db.Nomination
                .Where(x => x.EstActif || nominationIdsSelectionnees.Contains(x.NominationId))
                .OrderBy(x => x.LibelleNomination)
                .Select(x => new SelectListItem
                {
                    Value = x.NominationId.ToString(),
                    Text = x.LibelleNomination + (x.EstActif ? "" : " (inactive)")
                }).ToList();

            var typeNominationIdsSelectionnes = model.NominationsAdministratives
                .Where(x => !x.Supprimer && x.TypeNominationId.HasValue && x.TypeNominationId.Value > 0)
                .Select(x => x.TypeNominationId.Value)
                .Distinct()
                .ToList();

            model.TypesNominationDisponibles = db.TypeNomination
                .Where(x => x.EstActif || typeNominationIdsSelectionnes.Contains(x.TypeNominationId))
                .OrderBy(x => x.LibelleTypeNomination)
                .Select(x => new SelectListItem
                {
                    Value = x.TypeNominationId.ToString(),
                    Text = x.LibelleTypeNomination + (x.EstActif ? "" : " (inactif)")
                }).ToList();
        }

        private ActionResult RetournerEditAvecErreurs(MedecinEditViewModel model)
        {
            ChargerListesEdit(model);

            try
            {
                ChargerDossierEdit(model);
            }
            catch (Exception ex)
            {
                // La vue d'édition doit rester affichable même si un volet secondaire du
                // dossier ne peut pas être rechargé. L'erreur d'origine reste journalisée.
                _logger.LogError(ex,
                    "Impossible de recharger le dossier complet du médecin {MedecinId} après une erreur de validation.",
                    model.MedecinId);
                model.Dossier = model.Dossier ?? new MedecinDetailsModalViewModel();
            }

            model.ErreursValidation = ConstruireListeErreursValidation();
            return View(model);
        }

        private void IgnorerErreursLignesSupprimees(MedecinEditViewModel model)
        {
            IgnorerErreursLignesSupprimees(model.Adresses, x => x.Supprimer, "Adresses");
            IgnorerErreursLignesSupprimees(model.Telephones, x => x.Supprimer, "Telephones");
            IgnorerErreursLignesSupprimees(model.Courriels, x => x.Supprimer, "Courriels");
            IgnorerErreursLignesSupprimees(model.Installations, x => x.Supprimer, "Installations");
            IgnorerErreursLignesSupprimees(model.NominationsAdministratives, x => x.Supprimer, "NominationsAdministratives");
        }

        private void IgnorerErreursLignesSupprimees<T>(
            IReadOnlyList<T> lignes,
            Func<T, bool> estSupprimee,
            string collection)
        {
            for (int i = 0; i < lignes.Count; i++)
            {
                if (!estSupprimee(lignes[i]))
                    continue;

                var prefixe = $"{collection}[{i}].";
                var cles = ModelState.Keys
                    .Where(x => x.StartsWith(prefixe, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var cle in cles)
                    ModelState.Remove(cle);
            }
        }

        private void AjouterErreurModelStateUnique(string cle, string message)
        {
            cle ??= string.Empty;
            message = string.IsNullOrWhiteSpace(message)
                ? "La valeur saisie n'est pas valide."
                : message;

            if (ModelState.TryGetValue(cle, out var etat) &&
                etat.Errors.Any(x => string.Equals(x.ErrorMessage, message, StringComparison.Ordinal)))
                return;

            ModelState.AddModelError(cle, message);
        }

        private void ValiderDonneesEdit(MedecinEditViewModel model)
        {
            // MVC peut omettre IValidatableObject lorsqu'une propriété possède déjà une
            // erreur de conversion. On rejoue explicitement les validations métier pour
            // retourner tous les problèmes du formulaire en une seule réponse.
            foreach (var resultat in model.Validate(new ValidationContext(model)))
            {
                var membres = resultat.MemberNames != null && resultat.MemberNames.Any()
                    ? resultat.MemberNames
                    : new[] { string.Empty };

                foreach (var membre in membres)
                    AjouterErreurModelStateUnique(membre, resultat.ErrorMessage);
            }

            // Référentiels de la fiche principale. Une valeur devenue invalide entre
            // l'ouverture et l'enregistrement doit apparaître comme une erreur du champ.
            if (model.TitreId.HasValue && !db.TitreMedecin.Any(x => x.TitreId == model.TitreId.Value))
                ModelState.AddModelError("TitreId", "Le titre sélectionné n'existe plus. Sélectionnez une autre valeur.");

            if (model.TypePermisId.HasValue && !db.TypePermis.Any(x => x.TypePermisId == model.TypePermisId.Value))
                ModelState.AddModelError("TypePermisId", "Le type de permis sélectionné n'existe plus. Sélectionnez une autre valeur.");

            if (model.StatutMedecinId.HasValue && !db.StatutMedecin.Any(x => x.StatutMedecinId == model.StatutMedecinId.Value))
                ModelState.AddModelError("StatutMedecinId", "Le statut sélectionné n'existe plus. Sélectionnez une autre valeur.");

            if (model.CategoriePratiqueId.HasValue && !db.CategoriePratique.Any(x => x.CategoriePratiqueId == model.CategoriePratiqueId.Value))
                ModelState.AddModelError("CategoriePratiqueId", "La catégorie de pratique sélectionnée n'existe plus. Sélectionnez une autre valeur.");

            if (model.GenreId.HasValue && !db.Genre.Any(x => x.GenreId == model.GenreId.Value))
                ModelState.AddModelError("GenreId", "Le genre sélectionné n'existe plus. Sélectionnez une autre valeur.");

            var typesCourrielActifs = db.TypeCourriel
                .Where(x => x.EstActif)
                .Select(x => x.Libelle)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var anciensTypesCourriel = model.MedecinId > 0
                ? db.MedecinCourriel
                    .Where(x => x.MedecinId == model.MedecinId)
                    .Select(x => new { x.MedecinCourrielId, x.TypeCourriel })
                    .ToDictionary(x => x.MedecinCourrielId, x => x.TypeCourriel)
                : new Dictionary<int, string>();

            for (int i = 0; i < model.Courriels.Count; i++)
            {
                var courriel = model.Courriels[i];
                if (courriel.Supprimer || string.IsNullOrWhiteSpace(courriel.TypeCourriel))
                    continue;

                var libelle = courriel.TypeCourriel.Trim();
                bool estActif = typesCourrielActifs.Contains(libelle);
                bool ancienneValeurInchangee = courriel.MedecinCourrielId > 0 &&
                    anciensTypesCourriel.TryGetValue(courriel.MedecinCourrielId, out var ancienLibelle) &&
                    string.Equals(ancienLibelle?.Trim(), libelle, StringComparison.OrdinalIgnoreCase);

                if (!estActif && !ancienneValeurInchangee)
                    ModelState.AddModelError($"Courriels[{i}].TypeCourriel", "Sélectionnez un type de courriel actif dans la liste.");
            }

            // Références des lignes de collections.
            var installationIds = model.Installations
                .Where(x => !x.Supprimer && x.InstallationId.HasValue && x.InstallationId.Value > 0)
                .Select(x => x.InstallationId.Value)
                .Distinct()
                .ToList();
            var installationsValides = db.Installation
                .Where(x => installationIds.Contains(x.InstallationId))
                .Select(x => x.InstallationId)
                .ToHashSet();

            for (int i = 0; i < model.Installations.Count; i++)
            {
                var item = model.Installations[i];
                if (!item.Supprimer && item.InstallationId.HasValue && item.InstallationId.Value > 0 &&
                    !installationsValides.Contains(item.InstallationId.Value))
                    ModelState.AddModelError($"Installations[{i}].InstallationId", "L'installation sélectionnée n'existe plus. Sélectionnez une autre installation.");

                if (!item.Supprimer && item.InstallationId.HasValue && item.InstallationId.Value > 0 &&
                    item.RangInstallation.HasValue &&
                    (item.RangInstallation.Value < 1 || item.RangInstallation.Value > 3))
                    ModelState.AddModelError($"Installations[{i}].RangInstallation", "Le rang de l'installation doit être compris entre 1 et 3.");
            }

            var nominationIds = model.NominationsAdministratives
                .Where(x => !x.Supprimer && x.NominationId.HasValue && x.NominationId.Value > 0)
                .Select(x => x.NominationId.Value)
                .Distinct()
                .ToList();
            var nominationsValides = db.Nomination
                .Where(x => nominationIds.Contains(x.NominationId))
                .Select(x => x.NominationId)
                .ToHashSet();

            var typeNominationIds = model.NominationsAdministratives
                .Where(x => !x.Supprimer && x.TypeNominationId.HasValue && x.TypeNominationId.Value > 0)
                .Select(x => x.TypeNominationId.Value)
                .Distinct()
                .ToList();
            var typesNominationsValides = db.TypeNomination
                .Where(x => typeNominationIds.Contains(x.TypeNominationId))
                .Select(x => x.TypeNominationId)
                .ToHashSet();

            for (int i = 0; i < model.NominationsAdministratives.Count; i++)
            {
                var item = model.NominationsAdministratives[i];
                if (item.Supprimer)
                    continue;

                if (item.NominationId.HasValue && item.NominationId.Value > 0 && !nominationsValides.Contains(item.NominationId.Value))
                    ModelState.AddModelError($"NominationsAdministratives[{i}].NominationId", "La nomination sélectionnée n'existe plus.");

                if (item.TypeNominationId.HasValue && item.TypeNominationId.Value > 0 && !typesNominationsValides.Contains(item.TypeNominationId.Value))
                    ModelState.AddModelError($"NominationsAdministratives[{i}].TypeNominationId", "Le type de nomination sélectionné n'existe plus.");
            }

            if (model.MedecinId <= 0)
                return;

            // Vérifier que chaque identifiant de sous-ligne appartient toujours au médecin.
            // Cela évite les First(...) qui produisaient une erreur inattendue sans champ.
            var adresseExistantes = db.MedecinAdresse
                .Where(x => x.MedecinId == model.MedecinId)
                .Select(x => x.MedecinAdresseId)
                .ToHashSet();
            for (int i = 0; i < model.Adresses.Count; i++)
            {
                if (model.Adresses[i].Supprimer)
                    continue;

                var id = model.Adresses[i].MedecinAdresseId;
                if (id > 0 && !adresseExistantes.Contains(id))
                    ModelState.AddModelError($"Adresses[{i}].AdresseLigne1", "Cette adresse a été modifiée ou supprimée depuis l'ouverture de la page. Rechargez le dossier.");
            }

            var telephoneExistants = db.MedecinTelephone
                .Where(x => x.MedecinId == model.MedecinId)
                .Select(x => x.MedecinTelephoneId)
                .ToHashSet();
            for (int i = 0; i < model.Telephones.Count; i++)
            {
                if (model.Telephones[i].Supprimer)
                    continue;

                var id = model.Telephones[i].MedecinTelephoneId;
                if (id > 0 && !telephoneExistants.Contains(id))
                    ModelState.AddModelError($"Telephones[{i}].NumeroTelephone", "Ce téléphone a été modifié ou supprimé depuis l'ouverture de la page. Rechargez le dossier.");
            }

            var courrielExistants = db.MedecinCourriel
                .Where(x => x.MedecinId == model.MedecinId)
                .Select(x => x.MedecinCourrielId)
                .ToHashSet();
            for (int i = 0; i < model.Courriels.Count; i++)
            {
                if (model.Courriels[i].Supprimer)
                    continue;

                var id = model.Courriels[i].MedecinCourrielId;
                if (id > 0 && !courrielExistants.Contains(id))
                    ModelState.AddModelError($"Courriels[{i}].AdresseCourriel", "Ce courriel a été modifié ou supprimé depuis l'ouverture de la page. Rechargez le dossier.");
            }

            var installationExistantes = db.MedecinInstallation
                .Where(x => x.MedecinId == model.MedecinId)
                .Select(x => x.MedecinInstallationId)
                .ToHashSet();
            for (int i = 0; i < model.Installations.Count; i++)
            {
                if (model.Installations[i].Supprimer)
                    continue;

                var id = model.Installations[i].MedecinInstallationId;
                if (id > 0 && !installationExistantes.Contains(id))
                    ModelState.AddModelError($"Installations[{i}].InstallationId", "Cette installation du médecin a été modifiée ou supprimée depuis l'ouverture de la page. Rechargez le dossier.");
            }

            var nominationExistantes = db.NominationAdministrative
                .Where(x => x.MedecinId == model.MedecinId)
                .Select(x => x.NominationAdministrativeId)
                .ToHashSet();
            for (int i = 0; i < model.NominationsAdministratives.Count; i++)
            {
                if (model.NominationsAdministratives[i].Supprimer)
                    continue;

                var id = model.NominationsAdministratives[i].NominationAdministrativeId;
                if (id > 0 && !nominationExistantes.Contains(id))
                    ModelState.AddModelError($"NominationsAdministratives[{i}].LibelleNomination", "Cette nomination a été modifiée ou supprimée depuis l'ouverture de la page. Rechargez le dossier.");
            }
        }

        private List<MedecinEditValidationIssueViewModel> ConstruireListeErreursValidation()
        {
            var resultat = new List<MedecinEditValidationIssueViewModel>();

            foreach (var entree in ModelState)
            {
                if (entree.Value == null || entree.Value.Errors.Count == 0)
                    continue;

                var cle = entree.Key ?? string.Empty;
                var champ = ObtenirLibelleChamp(cle);
                var onglet = ObtenirOngletChamp(cle);

                foreach (var erreur in entree.Value.Errors)
                {
                    var message = !string.IsNullOrWhiteSpace(erreur.ErrorMessage)
                        ? erreur.ErrorMessage
                        : ObtenirMessageErreurModelState(erreur.Exception);

                    if (resultat.Any(x =>
                        string.Equals(x.Cle, cle, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.Message, message, StringComparison.Ordinal)))
                        continue;

                    resultat.Add(new MedecinEditValidationIssueViewModel
                    {
                        Cle = cle,
                        Champ = champ,
                        Message = message,
                        Onglet = onglet
                    });
                }
            }

            return resultat;
        }

        private static string ObtenirMessageErreurModelState(Exception exception)
        {
            if (exception is FormatException || exception is OverflowException)
                return "La valeur saisie n'est pas dans un format valide.";

            return "La valeur saisie n'est pas valide.";
        }

        private static string ObtenirLibelleChamp(string cle)
        {
            if (string.IsNullOrWhiteSpace(cle))
                return "Enregistrement du médecin";

            var libellesPrincipaux = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["TitreId"] = "Titre",
                ["Permis"] = "Permis",
                ["NAM"] = "NAM",
                ["Nom"] = "Nom",
                ["Prenom"] = "Prénom",
                ["TypePermisId"] = "Type de permis",
                ["StatutMedecinId"] = "Statut",
                ["CategoriePratiqueId"] = "Catégorie de pratique",
                ["GenreId"] = "Genre",
                ["DateNaissance"] = "Date de naissance",
                ["DateEntreeFonction"] = "Date d'entrée en fonction",
                ["DateDepart"] = "Date de départ",
                ["Commentaires"] = "Commentaires",
                ["InformationsGenerales"] = "Informations générales",
                ["Adresses"] = "Adresses",
                ["Telephones"] = "Téléphones",
                ["Courriels"] = "Courriels",
                ["Installations"] = "Installations",
                ["NominationsAdministratives"] = "Nominations administratives",
                ["JournalAudit"] = "Journal d'audit"
            };

            if (libellesPrincipaux.TryGetValue(cle, out var libelle))
                return libelle;

            var match = Regex.Match(cle, @"^(Adresses|Telephones|Courriels|Installations|NominationsAdministratives)\[(\d+)\]\.(.+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return cle;

            var collection = match.Groups[1].Value;
            var ligne = int.Parse(match.Groups[2].Value) + 1;
            var propriete = match.Groups[3].Value;

            var nomCollection = collection.Equals("Adresses", StringComparison.OrdinalIgnoreCase) ? "Adresse"
                : collection.Equals("Telephones", StringComparison.OrdinalIgnoreCase) ? "Téléphone"
                : collection.Equals("Courriels", StringComparison.OrdinalIgnoreCase) ? "Courriel"
                : collection.Equals("Installations", StringComparison.OrdinalIgnoreCase) ? "Installation"
                : "Nomination";

            var champs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["AdresseLigne1"] = "Adresse",
                ["Ville"] = "Ville",
                ["Province"] = "Province",
                ["CodePostal"] = "Code postal",
                ["EstPrincipale"] = "Principale",
                ["TypeTelephone"] = "Type",
                ["NumeroTelephone"] = "Numéro",
                ["RangSource"] = "Rang",
                ["EstPrincipal"] = "Principal",
                ["TypeCourriel"] = "Type",
                ["AdresseCourriel"] = "Adresse courriel",
                ["InstallationId"] = "Installation",
                ["RangInstallation"] = "Rang",
                ["DateDebut"] = "Date de début",
                ["DateFin"] = "Date de fin",
                ["NominationId"] = "Nomination",
                ["TypeNominationId"] = "Type de nomination",
                ["LibelleNomination"] = "Libellé",
                ["DateNomination"] = "Date de nomination",
                ["EstActive"] = "Active"
            };

            var nomChamp = champs.TryGetValue(propriete, out var nom) ? nom : propriete;
            return $"{nomCollection} #{ligne} — {nomChamp}";
        }

        private static string ObtenirOngletChamp(string cle)
        {
            if (string.IsNullOrWhiteSpace(cle))
                return "tab-general";

            if (cle.StartsWith("Adresses", StringComparison.OrdinalIgnoreCase) ||
                cle.StartsWith("Telephones", StringComparison.OrdinalIgnoreCase) ||
                cle.StartsWith("Courriels", StringComparison.OrdinalIgnoreCase))
                return "tab-coordonnees";

            if (cle.StartsWith("Installations", StringComparison.OrdinalIgnoreCase))
                return "tab-organisation";

            if (cle.StartsWith("NominationsAdministratives", StringComparison.OrdinalIgnoreCase))
                return "tab-nominations";

            return "tab-general";
        }

        private static string NormaliserOngletEdit(string onglet)
        {
            var valeur = (onglet ?? string.Empty).Trim().TrimStart('#').ToLowerInvariant();
            if (!valeur.StartsWith("tab-", StringComparison.Ordinal))
                valeur = "tab-" + valeur;

            var ongletsPermis = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "tab-general",
                "tab-coordonnees",
                "tab-organisation",
                "tab-nominations",
                "tab-privileges",
                "tab-cotisations",
                "tab-fittests",
                "tab-ententes",
                "tab-comites",
                "tab-documents",
                "tab-alertes",
                "tab-commentaires"
            };

            return ongletsPermis.Contains(valeur) ? valeur : "tab-general";
        }

        private void AjouterErreurEnregistrementMedecin(
            Exception exception,
            string etapeEnregistrement,
            string cleEtape,
            MedecinEditViewModel model)
        {
            var reference = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpperInvariant();
            _logger.LogError(exception,
                "Erreur SQL/EF pendant {Etape} pour MedecinId {MedecinId}. Référence {Reference}.",
                etapeEnregistrement, model.MedecinId, reference);

            var messageUtilisateur = UserFriendlyExceptionService.GetMessage(exception, "enregistrer le médecin");
            var detailTechnique = exception.ToString();

            string cle = null;

            if (detailTechnique.Contains("UQ_Medecin_Permis", StringComparison.OrdinalIgnoreCase) ||
                detailTechnique.Contains("Medecin.Permis", StringComparison.OrdinalIgnoreCase) ||
                detailTechnique.Contains("'Permis'", StringComparison.OrdinalIgnoreCase))
                cle = "Permis";
            else if (detailTechnique.Contains("MedecinAdresse", StringComparison.OrdinalIgnoreCase) ||
                     detailTechnique.Contains("AdresseLigne1", StringComparison.OrdinalIgnoreCase) ||
                     detailTechnique.Contains("CodePostal", StringComparison.OrdinalIgnoreCase))
                cle = "Adresses";
            else if (detailTechnique.Contains("TypeTelephone", StringComparison.OrdinalIgnoreCase) ||
                     detailTechnique.Contains("NumeroTelephone", StringComparison.OrdinalIgnoreCase) ||
                     detailTechnique.Contains("MedecinTelephone", StringComparison.OrdinalIgnoreCase))
                cle = "Telephones";
            else if (detailTechnique.Contains("TypeCourriel", StringComparison.OrdinalIgnoreCase) ||
                     detailTechnique.Contains("AdresseCourriel", StringComparison.OrdinalIgnoreCase) ||
                     detailTechnique.Contains("MedecinCourriel", StringComparison.OrdinalIgnoreCase))
                cle = "Courriels";
            else if (detailTechnique.Contains("RangInstallation", StringComparison.OrdinalIgnoreCase) ||
                     detailTechnique.Contains("UQ_MedecinInstallation", StringComparison.OrdinalIgnoreCase) ||
                     detailTechnique.Contains("CK_MedecinInstallation", StringComparison.OrdinalIgnoreCase) ||
                     detailTechnique.Contains("MedecinInstallation", StringComparison.OrdinalIgnoreCase))
                cle = "Installations";
            else if (detailTechnique.Contains("NominationAdministrative", StringComparison.OrdinalIgnoreCase))
                cle = "NominationsAdministratives";
            else if (detailTechnique.Contains("AuditLog", StringComparison.OrdinalIgnoreCase))
                cle = "JournalAudit";
            else if (Regex.IsMatch(detailTechnique, @"\bNAM\b", RegexOptions.IgnoreCase))
                cle = "NAM";
            else if (Regex.IsMatch(detailTechnique, @"\bPrenom\b", RegexOptions.IgnoreCase))
                cle = "Prenom";
            else if (Regex.IsMatch(detailTechnique, @"\bNom\b", RegexOptions.IgnoreCase))
                cle = "Nom";
            else if (Regex.IsMatch(detailTechnique, @"\bTitreId\b", RegexOptions.IgnoreCase))
                cle = "TitreId";
            else if (Regex.IsMatch(detailTechnique, @"\bTypePermisId\b", RegexOptions.IgnoreCase))
                cle = "TypePermisId";
            else if (Regex.IsMatch(detailTechnique, @"\bStatutMedecinId\b", RegexOptions.IgnoreCase))
                cle = "StatutMedecinId";
            else if (Regex.IsMatch(detailTechnique, @"\bCategoriePratiqueId\b", RegexOptions.IgnoreCase))
                cle = "CategoriePratiqueId";
            else if (Regex.IsMatch(detailTechnique, @"\bGenreId\b", RegexOptions.IgnoreCase))
                cle = "GenreId";

            cle ??= string.IsNullOrWhiteSpace(cleEtape) ? string.Empty : cleEtape;

            var prefixe = string.IsNullOrWhiteSpace(etapeEnregistrement)
                ? string.Empty
                : $"Étape : {etapeEnregistrement}. ";

            ModelState.AddModelError(cle, $"{prefixe}{messageUtilisateur} Référence : {reference}.");
        }

        private void AjouterErreurExceptionInattendue(
            Exception exception,
            string etapeEnregistrement,
            string cleEtape,
            MedecinEditViewModel model)
        {
            var reference = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpperInvariant();

            _logger.LogError(exception,
                "Erreur inattendue pendant {Etape} pour MedecinId {MedecinId}. Référence {Reference}.",
                etapeEnregistrement, model.MedecinId, reference);

            var cle = string.IsNullOrWhiteSpace(cleEtape) ? string.Empty : cleEtape;
            var champOuSection = string.IsNullOrWhiteSpace(cle)
                ? "Aucun champ précis n'a pu être identifié automatiquement."
                : "Vérifiez les champs de cette section.";

            ModelState.AddModelError(
                cle,
                $"Une erreur est survenue pendant l'étape « {etapeEnregistrement} ». {champOuSection} Référence : {reference}.");
        }

        private string NullIfWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
}
}
