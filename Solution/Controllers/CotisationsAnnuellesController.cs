using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.Shared;
using Solution.ViewModels.CotisationsAnnuelles;

namespace Solution.Controllers
{
    [Authorize]
    public class CotisationsAnnuellesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

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

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.CotisationAnnuelle
                                .Include(x => x.Medecin)
                                .Include(x => x.CodeCotisation)
                                .Include(x => x.TypePaiement)
                .AsNoTracking()
                .FirstOrDefault(x => x.CotisationAnnuelleId == id.Value);

            if (entity == null) return HttpNotFound();

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

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new CotisationAnnuelleFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(CotisationAnnuelleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new CotisationAnnuelle();
            MapToEntity(model, entity);

            db.CotisationAnnuelle.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "CotisationAnnuelle créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.CotisationAnnuelle.FirstOrDefault(x => x.CotisationAnnuelleId == id.Value);
            if (entity == null) return HttpNotFound();

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
        public ActionResult Edit(int id, CotisationAnnuelleFormViewModel model)
        {
            if (id != model.CotisationAnnuelleId)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.CotisationAnnuelle.FirstOrDefault(x => x.CotisationAnnuelleId == id);
            if (entity == null)
                return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "Cotisation annuelle modifiée avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.CotisationAnnuelle
                                .Include(x => x.Medecin)
                                .Include(x => x.CodeCotisation)
                                .Include(x => x.TypePaiement)
                .AsNoTracking()
                .FirstOrDefault(x => x.CotisationAnnuelleId == id.Value);

            if (entity == null) return HttpNotFound();

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
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.CotisationAnnuelle.FirstOrDefault(x => x.CotisationAnnuelleId == id);
            if (entity == null) return HttpNotFound();

            db.CotisationAnnuelle.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "CotisationAnnuelle supprimé avec succès.";
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

        private static void MapToEntity(CotisationAnnuelleFormViewModel model, CotisationAnnuelle entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.AnneeExercice = model.AnneeExercice;
            entity.CodeCotisationId = model.CodeCotisationId;
            entity.DescriptionSource = model.DescriptionSource;
            entity.MontantInitial = model.MontantInitial;
            entity.ReductionAppliquee = model.ReductionAppliquee;
            entity.MotifReduction = model.MotifReduction;
            entity.MontantReduction = model.MontantReduction;
            entity.PenaliteAppliquee = model.PenaliteAppliquee;
            entity.PenaliteTexte = model.PenaliteTexte;
            entity.MontantPenalite = model.MontantPenalite;
            entity.MontantFinal = model.MontantFinal;
            entity.EtatCotisation = model.EtatCotisation;
            entity.MontantPaye = model.MontantPaye;
            entity.DatePaiement = model.DatePaiement;
            entity.TypePaiementTexte = model.TypePaiementTexte;
            entity.TypePaiementId = model.TypePaiementId;
            entity.DateDepot = model.DateDepot;
            entity.NoDepot = model.NoDepot;
            entity.RecuImposition = model.RecuImposition;
            entity.DateRecuImposition = model.DateRecuImposition;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
