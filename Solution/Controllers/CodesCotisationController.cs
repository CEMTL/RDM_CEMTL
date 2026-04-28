using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.CodesCotisation;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class CodesCotisationController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(CodeCotisation entity)
        {
            return new
            {
                entity.CodeCotisationId,
                CodeCotisation = entity.CodeCotisation1,
                entity.Description,
                entity.EstActif
            };
        }

        public ActionResult Index(string search = null)
        {
            var query = db.CodeCotisation
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    (x.CodeCotisation1 != null && x.CodeCotisation1.Contains(search)) ||
                    (x.Description != null && x.Description.Contains(search)));
            }

            var items = query
                .OrderBy(x => x.CodeCotisation1)
                .Select(x => new CodeCotisationListViewModel
                {
                    CodeCotisationId = x.CodeCotisationId,
                    CodeCotisation = x.CodeCotisation1,
                    Description = x.Description,
                    EstActif = x.EstActif
                })
                .ToList();

            ViewBag.Search = search;

            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.CodeCotisation
                .AsNoTracking()
                .FirstOrDefault(x => x.CodeCotisationId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new CodeCotisationDetailsViewModel
            {
                CodeCotisationId = entity.CodeCotisationId,
                CodeCotisation = entity.CodeCotisation1,
                Description = entity.Description,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new CodeCotisationFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(CodeCotisationFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new CodeCotisation();
            MapToEntity(model, entity);

            db.CodeCotisation.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "CodeCotisation",
                entity.CodeCotisationId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "CodeCotisation créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.CodeCotisation.FirstOrDefault(x => x.CodeCotisationId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new CodeCotisationFormViewModel
            {
                CodeCotisationId = entity.CodeCotisationId,
                CodeCotisation = entity.CodeCotisation1,
                Description = entity.Description,
                EstActif = entity.EstActif
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(CodeCotisationFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.CodeCotisation.FirstOrDefault(x => x.CodeCotisationId == model.CodeCotisationId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            MapToEntity(model, entity);

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "CodeCotisation",
                entity.CodeCotisationId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "CodeCotisation modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.CodeCotisation
                .AsNoTracking()
                .FirstOrDefault(x => x.CodeCotisationId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new CodeCotisationDetailsViewModel
            {
                CodeCotisationId = entity.CodeCotisationId,
                CodeCotisation = entity.CodeCotisation1,
                Description = entity.Description,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int CodeCotisationId)
        {
            var entity = db.CodeCotisation.FirstOrDefault(x => x.CodeCotisationId == CodeCotisationId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.CodeCotisation.Remove(entity);

            _audit.Enregistrer(
                "CodeCotisation",
                CodeCotisationId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "CodeCotisation supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private CodeCotisationFormViewModel BuildFormViewModel(CodeCotisationFormViewModel model)
        {
            return model;
        }

        private static void MapToEntity(CodeCotisationFormViewModel model, CodeCotisation entity)
        {
            entity.CodeCotisation1 = model.CodeCotisation;
            entity.Description = model.Description;
            entity.EstActif = model.EstActif;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}