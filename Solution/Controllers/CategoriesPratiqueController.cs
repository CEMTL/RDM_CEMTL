using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.CategoriesPratique;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class CategoriesPratiqueController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(CategoriePratique entity)
        {
            return new
            {
                entity.CategoriePratiqueId,
                entity.Libelle,
                entity.EstActif
            };
        }

        public ActionResult Index(string search = null)
        {
            var query = db.CategoriePratique
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x => x.Libelle != null && x.Libelle.Contains(search));
            }

            var items = query
                .OrderBy(x => x.Libelle)
                .Select(x => new CategoriePratiqueListViewModel
                {
                    CategoriePratiqueId = x.CategoriePratiqueId,
                    Libelle = x.Libelle,
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

            var entity = db.CategoriePratique
                .AsNoTracking()
                .FirstOrDefault(x => x.CategoriePratiqueId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new CategoriePratiqueDetailsViewModel
            {
                CategoriePratiqueId = entity.CategoriePratiqueId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new CategoriePratiqueFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(CategoriePratiqueFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new CategoriePratique();
            MapToEntity(model, entity);

            db.CategoriePratique.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "CategoriePratique",
                entity.CategoriePratiqueId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "CategoriePratique créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.CategoriePratique.FirstOrDefault(x => x.CategoriePratiqueId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new CategoriePratiqueFormViewModel
            {
                CategoriePratiqueId = entity.CategoriePratiqueId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(CategoriePratiqueFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.CategoriePratique.FirstOrDefault(x => x.CategoriePratiqueId == model.CategoriePratiqueId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            MapToEntity(model, entity);

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "CategoriePratique",
                entity.CategoriePratiqueId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "CategoriePratique modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.CategoriePratique
                .AsNoTracking()
                .FirstOrDefault(x => x.CategoriePratiqueId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new CategoriePratiqueDetailsViewModel
            {
                CategoriePratiqueId = entity.CategoriePratiqueId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int CategoriePratiqueId)
        {
            var entity = db.CategoriePratique.FirstOrDefault(x => x.CategoriePratiqueId == CategoriePratiqueId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.CategoriePratique.Remove(entity);

            _audit.Enregistrer(
                "CategoriePratique",
                CategoriePratiqueId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "CategoriePratique supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private CategoriePratiqueFormViewModel BuildFormViewModel(CategoriePratiqueFormViewModel model)
        {
            return model;
        }

        private static void MapToEntity(CategoriePratiqueFormViewModel model, CategoriePratique entity)
        {
            entity.Libelle = model.Libelle;
            entity.EstActif = model.EstActif;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}