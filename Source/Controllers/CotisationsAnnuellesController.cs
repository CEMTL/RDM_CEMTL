using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Models;
using Solution.ViewModels.Shared;
using Solution.ViewModels.CotisationsAnnuelles;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class CotisationsAnnuellesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;
        private AuditService Audit => new AuditService(db, HttpContext);

        public CotisationsAnnuellesController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }

        public ActionResult Index(string search = null, short? annee = null, string etat = null, bool? impayeSeulement = null, int page = 1, int pageSize = 25)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 25 : pageSize;

            var query = db.CotisationAnnuelle
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.CodeCotisation)
                .Include(x => x.TypePaiement)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    (x.Medecin.Permis != null && x.Medecin.Permis.Contains(search)) ||
                    (x.Medecin.Nom != null && x.Medecin.Nom.Contains(search)) ||
                    (x.Medecin.Prenom != null && x.Medecin.Prenom.Contains(search)) ||
                    (x.DescriptionSource != null && x.DescriptionSource.Contains(search)) ||
                    (x.EtatCotisation != null && x.EtatCotisation.Contains(search)) ||
                    (x.NoDepot != null && x.NoDepot.Contains(search)));
            }

            if (annee.HasValue) query = query.Where(x => x.AnneeExercice == annee.Value);
            if (!string.IsNullOrWhiteSpace(etat)) query = query.Where(x => x.EtatCotisation != null && x.EtatCotisation.Contains(etat));
            if (impayeSeulement.HasValue && impayeSeulement.Value)
                query = query.Where(x => x.MontantFinal.HasValue && (!x.MontantPaye.HasValue || x.MontantPaye < x.MontantFinal));

            var totalItems = query.Count();
            var items = query
                .OrderByDescending(x => x.AnneeExercice)
                .ThenBy(x => x.Medecin.Nom)
                .ThenBy(x => x.Medecin.Prenom)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CotisationAnnuelleListViewModel
                {
                    CotisationAnnuelleId = x.CotisationAnnuelleId,
                    MedecinId = x.MedecinId,
                    AnneeExercice = x.AnneeExercice,
                    CodeCotisationId = x.CodeCotisationId,
                    DescriptionSource = x.DescriptionSource,
                    MontantInitial = x.MontantInitial,
                    ReductionAppliquee = x.ReductionAppliquee,
                    MotifReduction = x.MotifReduction,
                    MontantReduction = x.MontantReduction,
                    PenaliteAppliquee = x.PenaliteAppliquee,
                    PenaliteTexte = x.PenaliteTexte,
                    MontantPenalite = x.MontantPenalite,
                    MontantFinal = x.MontantFinal,
                    EtatCotisation = x.EtatCotisation,
                    MontantPaye = x.MontantPaye,
                    DatePaiement = x.DatePaiement,
                    TypePaiementTexte = x.TypePaiementTexte,
                    TypePaiementId = x.TypePaiementId,
                    DateDepot = x.DateDepot,
                    NoDepot = x.NoDepot,
                    RecuImposition = x.RecuImposition,
                    DateRecuImposition = x.DateRecuImposition,
                    MedecinNomComplet = x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom,
                    CodeCotisationLibelle = x.CodeCotisation != null ? x.CodeCotisation.CodeCotisation1 : string.Empty,
                    TypePaiementLibelle = x.TypePaiement != null ? x.TypePaiement.Libelle : x.TypePaiementTexte
                })
                .ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)System.Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("SuperUtilisateur");
            return View(new PagedListViewModel<CotisationAnnuelleListViewModel>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            });
        }

        public ActionResult Details(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (id == null) return BadRequest();

            var entity = db.CotisationAnnuelle
                                .Include(x => x.Medecin)
                                .Include(x => x.CodeCotisation)
                                .Include(x => x.TypePaiement)
                .AsNoTracking()
                .FirstOrDefault(x => x.CotisationAnnuelleId == id.Value);

            if (entity == null) return NotFound();

            var vm = new CotisationAnnuelleDetailsViewModel
            {
            CotisationAnnuelleId = entity.CotisationAnnuelleId,
            MedecinId = entity.MedecinId,
            AnneeExercice = entity.AnneeExercice,
            CodeCotisationId = entity.CodeCotisationId,
            DescriptionSource = entity.DescriptionSource,
            MontantInitial = entity.MontantInitial,
            ReductionAppliquee = entity.ReductionAppliquee,
            MotifReduction = entity.MotifReduction,
            MontantReduction = entity.MontantReduction,
            PenaliteAppliquee = entity.PenaliteAppliquee,
            PenaliteTexte = entity.PenaliteTexte,
            MontantPenalite = entity.MontantPenalite,
            MontantFinal = entity.MontantFinal,
            EtatCotisation = entity.EtatCotisation,
            MontantPaye = entity.MontantPaye,
            DatePaiement = entity.DatePaiement,
            TypePaiementTexte = entity.TypePaiementTexte,
            TypePaiementId = entity.TypePaiementId,
            DateDepot = entity.DateDepot,
            NoDepot = entity.NoDepot,
            RecuImposition = entity.RecuImposition,
            DateRecuImposition = entity.DateRecuImposition,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            CodeCotisationLibelle = (entity.CodeCotisation != null ? entity.CodeCotisation.CodeCotisation1 : ""),
            TypePaiementLibelle = (entity.TypePaiement != null ? entity.TypePaiement.Libelle : ""),
            };

            return View(vm);
        }

        [HttpGet]
        public ActionResult Recu(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (!id.HasValue)
                return BadRequest();

            var entity = db.CotisationAnnuelle
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.CodeCotisation)
                .Include(x => x.TypePaiement)
                .FirstOrDefault(x => x.CotisationAnnuelleId == id.Value);

            if (entity == null)
                return NotFound();

            if (entity.MontantPaye.GetValueOrDefault() <= 0m || !entity.DatePaiement.HasValue)
            {
                TempData["Error"] = "Un reçu peut être imprimé seulement lorsqu'un paiement positif et sa date sont enregistrés.";
                return RedirectToAction(nameof(Details), new
                {
                    id = entity.CotisationAnnuelleId,
                    returnMedecinId,
                    returnTab
                });
            }

            var montantFinal = entity.MontantFinal.GetValueOrDefault();
            var montantPaye = entity.MontantPaye.GetValueOrDefault();
            var medecin = entity.Medecin;

            var model = new CotisationRecuViewModel
            {
                CotisationAnnuelleId = entity.CotisationAnnuelleId,
                NumeroRecu = $"COT-{entity.AnneeExercice}-{entity.CotisationAnnuelleId:D6}",
                AnneeExercice = entity.AnneeExercice,
                MedecinNomComplet = medecin == null
                    ? string.Empty
                    : string.Join(" ", new[] { medecin.Prenom, medecin.Nom }
                        .Where(x => !string.IsNullOrWhiteSpace(x))),
                NumeroPermis = medecin?.Permis,
                CodeCotisation = entity.CodeCotisation?.CodeCotisation1,
                Description = entity.DescriptionSource,
                MontantFinal = montantFinal,
                MontantPaye = montantPaye,
                Solde = Math.Max(0m, montantFinal - montantPaye),
                DatePaiement = entity.DatePaiement.Value,
                TypePaiement = entity.TypePaiement?.Libelle ?? entity.TypePaiementTexte,
                DateDepot = entity.DateDepot,
                NumeroDepot = entity.NoDepot,
                ReferenceRecuImposition = entity.RecuImposition,
                DateRecuImposition = entity.DateRecuImposition,
                DateImpression = DateTime.Now,
                ImprimePar = User?.Identity?.Name
            };

            return View(model);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(int? medecinId = null, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            var vm = BuildFormViewModel(new CotisationAnnuelleFormViewModel
            {
                MedecinId = medecinId ?? 0
            });
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(CotisationAnnuelleFormViewModel model, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            ValiderEtCalculer(model);
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new CotisationAnnuelle { DateCreation = DateTime.Now };
            MapToEntity(model, entity);

            try
            {
                db.CotisationAnnuelle.Add(entity);
                db.SaveChanges();
                Audit.Enregistrer("CotisationAnnuelle", entity.CotisationAnnuelleId, "INSERT", null, Snapshot(entity));
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", UserFriendlyExceptionService.GetMessage(ex, "créer la cotisation annuelle"));
                return View(BuildFormViewModel(model));
            }

            TempData["Success"] = "Cotisation annuelle créée avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (id == null) return BadRequest();

            var entity = db.CotisationAnnuelle.FirstOrDefault(x => x.CotisationAnnuelleId == id.Value);
            if (entity == null) return NotFound();

            var vm = new CotisationAnnuelleFormViewModel
            {
                CotisationAnnuelleId = entity.CotisationAnnuelleId,
                MedecinId = entity.MedecinId,
                AnneeExercice = entity.AnneeExercice,
                CodeCotisationId = entity.CodeCotisationId,
                DescriptionSource = entity.DescriptionSource,
                MontantInitial = entity.MontantInitial,
                ReductionAppliquee = entity.ReductionAppliquee,
                MotifReduction = entity.MotifReduction,
                MontantReduction = entity.MontantReduction,
                PenaliteAppliquee = entity.PenaliteAppliquee,
                PenaliteTexte = entity.PenaliteTexte,
                MontantPenalite = entity.MontantPenalite,
                MontantFinal = entity.MontantFinal,
                EtatCotisation = entity.EtatCotisation,
                MontantPaye = entity.MontantPaye,
                DatePaiement = entity.DatePaiement,
                TypePaiementTexte = entity.TypePaiementTexte,
                TypePaiementId = entity.TypePaiementId,
                DateDepot = entity.DateDepot,
                NoDepot = entity.NoDepot,
                RecuImposition = entity.RecuImposition,
                DateRecuImposition = entity.DateRecuImposition,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int id, CotisationAnnuelleFormViewModel model, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (id != model.CotisationAnnuelleId)
                return BadRequest();

            ValiderEtCalculer(model);
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.CotisationAnnuelle.FirstOrDefault(x => x.CotisationAnnuelleId == id);
            if (entity == null)
                return NotFound();

            var ancienneValeur = Snapshot(entity);
            MapToEntity(model, entity);
            try
            {
                Audit.Enregistrer("CotisationAnnuelle", entity.CotisationAnnuelleId, "UPDATE", ancienneValeur, Snapshot(entity));
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", UserFriendlyExceptionService.GetMessage(ex, "modifier la cotisation annuelle"));
                return View(BuildFormViewModel(model));
            }

            TempData["Success"] = "Cotisation annuelle modifiée avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (id == null) return BadRequest();

            var entity = db.CotisationAnnuelle
                                .Include(x => x.Medecin)
                                .Include(x => x.CodeCotisation)
                                .Include(x => x.TypePaiement)
                .AsNoTracking()
                .FirstOrDefault(x => x.CotisationAnnuelleId == id.Value);

            if (entity == null) return NotFound();

            var vm = new CotisationAnnuelleDetailsViewModel
            {
            CotisationAnnuelleId = entity.CotisationAnnuelleId,
            MedecinId = entity.MedecinId,
            AnneeExercice = entity.AnneeExercice,
            CodeCotisationId = entity.CodeCotisationId,
            DescriptionSource = entity.DescriptionSource,
            MontantInitial = entity.MontantInitial,
            ReductionAppliquee = entity.ReductionAppliquee,
            MotifReduction = entity.MotifReduction,
            MontantReduction = entity.MontantReduction,
            PenaliteAppliquee = entity.PenaliteAppliquee,
            PenaliteTexte = entity.PenaliteTexte,
            MontantPenalite = entity.MontantPenalite,
            MontantFinal = entity.MontantFinal,
            EtatCotisation = entity.EtatCotisation,
            MontantPaye = entity.MontantPaye,
            DatePaiement = entity.DatePaiement,
            TypePaiementTexte = entity.TypePaiementTexte,
            TypePaiementId = entity.TypePaiementId,
            DateDepot = entity.DateDepot,
            NoDepot = entity.NoDepot,
            RecuImposition = entity.RecuImposition,
            DateRecuImposition = entity.DateRecuImposition,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            CodeCotisationLibelle = (entity.CodeCotisation != null ? entity.CodeCotisation.CodeCotisation1 : ""),
            TypePaiementLibelle = (entity.TypePaiement != null ? entity.TypePaiement.Libelle : ""),
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id, int? returnMedecinId = null, string returnTab = null)
        {
            var entity = db.CotisationAnnuelle.FirstOrDefault(x => x.CotisationAnnuelleId == id);
            if (entity == null) return NotFound();

            var ancienneValeur = Snapshot(entity);
            db.CotisationAnnuelle.Remove(entity);
            Audit.Enregistrer("CotisationAnnuelle", entity.CotisationAnnuelleId, "DELETE", ancienneValeur, null);
            db.SaveChanges();

            TempData["Success"] = "Cotisation annuelle supprimée avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        private void PreparerRetour(int? returnMedecinId, string returnTab)
        {
            ViewBag.ReturnMedecinId = returnMedecinId;
            ViewBag.ReturnTab = returnTab;
        }

        private ActionResult RedirigerApresAction(int? returnMedecinId, string returnTab)
        {
            if (returnMedecinId.HasValue)
            {
                return RedirectToAction("Edit", "Medecins", new
                {
                    id = returnMedecinId.Value,
                    tab = string.IsNullOrWhiteSpace(returnTab) ? "cotisations" : returnTab
                });
            }

            return RedirectToAction("Index");
        }

        private CotisationAnnuelleFormViewModel BuildFormViewModel(CotisationAnnuelleFormViewModel model)
        {
            model.MedecinIdOptions = db.Medecin.AsNoTracking()
                .OrderBy(x => (x.Permis + " - " + x.Nom + " " + x.Prenom))
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = (x.Permis + " - " + x.Nom + " " + x.Prenom)
                })
                .ToList();
            model.CodeCotisationIdOptions = db.CodeCotisation.AsNoTracking()
                .OrderBy(x => x.CodeCotisation1)
                .Select(x => new SelectListItem
                {
                    Value = x.CodeCotisationId.ToString(),
                    Text = x.CodeCotisation1
                })
                .ToList();
            model.TypePaiementIdOptions = db.TypePaiement.AsNoTracking()
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.TypePaiementId.ToString(),
                    Text = x.Libelle
                })
                .ToList();
            return model;
        }

        private void ValiderEtCalculer(CotisationAnnuelleFormViewModel model)
        {
            if (model.MedecinId <= 0)
                ModelState.AddModelError(nameof(model.MedecinId), "Le médecin est obligatoire.");

            if (model.AnneeExercice < 2000 || model.AnneeExercice > 2100)
                ModelState.AddModelError(nameof(model.AnneeExercice), "L'année doit être comprise entre 2000 et 2100.");

            decimal initial = model.MontantInitial ?? 0m;
            decimal reduction = model.ReductionAppliquee == true ? (model.MontantReduction ?? 0m) : 0m;
            decimal penalite = model.PenaliteAppliquee == true ? (model.MontantPenalite ?? 0m) : 0m;
            decimal paye = model.MontantPaye ?? 0m;

            if (initial < 0m) ModelState.AddModelError(nameof(model.MontantInitial), "Le montant initial ne peut pas être négatif.");
            if (reduction < 0m) ModelState.AddModelError(nameof(model.MontantReduction), "La réduction ne peut pas être négative.");
            if (penalite < 0m) ModelState.AddModelError(nameof(model.MontantPenalite), "La pénalité ne peut pas être négative.");
            if (paye < 0m) ModelState.AddModelError(nameof(model.MontantPaye), "Le montant payé ne peut pas être négatif.");
            if (reduction > initial) ModelState.AddModelError(nameof(model.MontantReduction), "La réduction ne peut pas dépasser le montant initial.");

            decimal final = Math.Max(0m, initial - reduction + penalite);
            model.MontantFinal = final;

            if (paye > final)
                ModelState.AddModelError(nameof(model.MontantPaye), "Le montant payé ne peut pas dépasser le montant final.");

            if (paye > 0m && !model.DatePaiement.HasValue)
                ModelState.AddModelError(nameof(model.DatePaiement), "La date de paiement est obligatoire lorsqu'un montant est payé.");

            model.EtatCotisation = final == 0m || paye >= final
                ? "Payée"
                : paye > 0m ? "Partiellement payée" : "À payer";

            if (model.ReductionAppliquee == true && reduction > 0m && string.IsNullOrWhiteSpace(model.MotifReduction))
                ModelState.AddModelError(nameof(model.MotifReduction), "Le motif de réduction est obligatoire lorsqu'une réduction est appliquée.");
        }

        private static object Snapshot(CotisationAnnuelle x)
        {
            return new
            {
                x.CotisationAnnuelleId, x.MedecinId, x.AnneeExercice, x.CodeCotisationId,
                x.MontantInitial, x.ReductionAppliquee, x.MontantReduction,
                x.PenaliteAppliquee, x.MontantPenalite, x.MontantFinal,
                x.EtatCotisation, x.MontantPaye, x.DatePaiement, x.TypePaiementId,
                x.DateDepot, x.NoDepot, x.RecuImposition, x.DateRecuImposition
            };
        }

        private static void MapToEntity(CotisationAnnuelleFormViewModel model, CotisationAnnuelle entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.AnneeExercice = model.AnneeExercice;
            entity.CodeCotisationId = model.CodeCotisationId;
            entity.DescriptionSource = model.DescriptionSource;
            entity.MontantInitial = model.MontantInitial ?? 0m;
            entity.ReductionAppliquee = model.ReductionAppliquee == true;
            entity.MotifReduction = model.ReductionAppliquee == true ? model.MotifReduction?.Trim() : null;
            entity.MontantReduction = model.ReductionAppliquee == true ? (model.MontantReduction ?? 0m) : 0m;
            entity.PenaliteAppliquee = model.PenaliteAppliquee == true;
            entity.PenaliteTexte = model.PenaliteAppliquee == true ? model.PenaliteTexte?.Trim() : null;
            entity.MontantPenalite = model.PenaliteAppliquee == true ? (model.MontantPenalite ?? 0m) : 0m;
            entity.MontantFinal = model.MontantFinal ?? 0m;
            entity.EtatCotisation = model.EtatCotisation;
            entity.MontantPaye = model.MontantPaye ?? 0m;
            entity.DatePaiement = model.MontantPaye.GetValueOrDefault() > 0m ? model.DatePaiement : null;
            entity.TypePaiementTexte = model.TypePaiementTexte;
            entity.TypePaiementId = model.TypePaiementId;
            entity.DateDepot = model.DateDepot;
            entity.NoDepot = model.NoDepot;
            entity.RecuImposition = model.RecuImposition;
            entity.DateRecuImposition = model.DateRecuImposition;
        }
}
}
