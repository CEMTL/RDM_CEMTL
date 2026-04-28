using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.Nominations;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class NominationsController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(Nomination entity)
        {
            return new
            {
                entity.NominationId,
                entity.CodeNomination,
                entity.LibelleNomination,
                entity.Description,
                entity.EstActif
            };
        }

        public ActionResult Index(string search = null, bool? actifsSeulement = null)
        {
            var query = db.Nomination.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    (x.CodeNomination != null && x.CodeNomination.Contains(search)) ||
                    (x.LibelleNomination != null && x.LibelleNomination.Contains(search)) ||
                    (x.Description != null && x.Description.Contains(search)));
            }

            var actifsOnly = actifsSeulement.HasValue && actifsSeulement.Value;
            if (actifsOnly)
            {
                query = query.Where(x => x.EstActif);
            }

            var items = query
                .OrderBy(x => x.LibelleNomination)
                .Select(x => new NominationListViewModel
                {
                    NominationId = x.NominationId,
                    CodeNomination = x.CodeNomination,
                    LibelleNomination = x.LibelleNomination,
                    Description = x.Description,
                    EstActif = x.EstActif
                })
                .ToList();

            ViewBag.Search = search;
            ViewBag.ActifsSeulement = actifsOnly;
            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("SuperUtilisateur");

            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.Nomination.AsNoTracking().FirstOrDefault(x => x.NominationId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new NominationDetailsViewModel
            {
                NominationId = entity.NominationId,
                CodeNomination = entity.CodeNomination,
                LibelleNomination = entity.LibelleNomination,
                Description = entity.Description,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            return View(new NominationFormViewModel { EstActif = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(NominationFormViewModel model)
        {
            ValidateUniqueCode(model);

            if (!ModelState.IsValid)
                return View(model);

            var entity = new Nomination
            {
                CodeNomination = (model.CodeNomination ?? "").Trim(),
                LibelleNomination = (model.LibelleNomination ?? "").Trim(),
                Description = model.Description,
                EstActif = model.EstActif,
                DateCreation = DateTime.Now,
                DateModification = null
            };

            db.Nomination.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "Nomination",
                entity.NominationId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "Nomination créée avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.Nomination.FirstOrDefault(x => x.NominationId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new NominationFormViewModel
            {
                NominationId = entity.NominationId,
                CodeNomination = entity.CodeNomination,
                LibelleNomination = entity.LibelleNomination,
                Description = entity.Description,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int id, NominationFormViewModel model)
        {
            if (id != model.NominationId)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ValidateUniqueCode(model);

            if (!ModelState.IsValid)
                return View(model);

            var entity = db.Nomination.FirstOrDefault(x => x.NominationId == id);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            entity.CodeNomination = (model.CodeNomination ?? "").Trim();
            entity.LibelleNomination = (model.LibelleNomination ?? "").Trim();
            entity.Description = model.Description;
            entity.EstActif = model.EstActif;
            entity.DateModification = DateTime.Now;

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "Nomination",
                entity.NominationId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "Nomination modifiée avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.Nomination.AsNoTracking().FirstOrDefault(x => x.NominationId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new NominationDetailsViewModel
            {
                NominationId = entity.NominationId,
                CodeNomination = entity.CodeNomination,
                LibelleNomination = entity.LibelleNomination,
                Description = entity.Description,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int NominationId)
        {
            var isUsed = db.NominationAdministrative.Any(x => x.NominationId == NominationId);
            if (isUsed)
            {
                TempData["Erreur"] = "Impossible de supprimer cette nomination, car elle est déjà utilisée.";
                return RedirectToAction("Index");
            }

            var entity = db.Nomination.FirstOrDefault(x => x.NominationId == NominationId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.Nomination.Remove(entity);

            _audit.Enregistrer(
                "Nomination",
                NominationId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "Nomination supprimée avec succès.";
            return RedirectToAction("Index");
        }

        private void ValidateUniqueCode(NominationFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CodeNomination))
                return;

            var code = model.CodeNomination.Trim();

            var exists = db.Nomination.Any(x =>
                x.NominationId != model.NominationId &&
                x.CodeNomination == code);

            if (exists)
            {
                ModelState.AddModelError("CodeNomination", "Ce code nomination existe déjà.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
