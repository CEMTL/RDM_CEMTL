using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.TypesPermis;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class TypesPermisController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(TypePermis entity)
        {
            return new
            {
                entity.TypePermisId,
                entity.Libelle,
                entity.EstActif
            };
        }

        public ActionResult Index(string search = null)
        {
            var query = db.TypePermis
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x => x.Libelle != null && x.Libelle.Contains(search));
            }

            var items = query
                .OrderBy(x => x.Libelle)
                .Select(x => new TypePermisListViewModel
                {
                    TypePermisId = x.TypePermisId,
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

            var entity = db.TypePermis
                .AsNoTracking()
                .FirstOrDefault(x => x.TypePermisId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new TypePermisDetailsViewModel
            {
                TypePermisId = entity.TypePermisId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new TypePermisFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(TypePermisFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new TypePermis();
            MapToEntity(model, entity);

            db.TypePermis.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "TypePermis",
                entity.TypePermisId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "TypePermis créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.TypePermis.FirstOrDefault(x => x.TypePermisId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new TypePermisFormViewModel
            {
                TypePermisId = entity.TypePermisId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(TypePermisFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.TypePermis.FirstOrDefault(x => x.TypePermisId == model.TypePermisId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            MapToEntity(model, entity);

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "TypePermis",
                entity.TypePermisId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "TypePermis modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.TypePermis
                .AsNoTracking()
                .FirstOrDefault(x => x.TypePermisId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new TypePermisDetailsViewModel
            {
                TypePermisId = entity.TypePermisId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int TypePermisId)
        {
            var entity = db.TypePermis.FirstOrDefault(x => x.TypePermisId == TypePermisId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.TypePermis.Remove(entity);

            _audit.Enregistrer(
                "TypePermis",
                TypePermisId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "TypePermis supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private TypePermisFormViewModel BuildFormViewModel(TypePermisFormViewModel model)
        {
            return model;
        }

        private static void MapToEntity(TypePermisFormViewModel model, TypePermis entity)
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