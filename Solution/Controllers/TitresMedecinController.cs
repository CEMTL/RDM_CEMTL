using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.TitresMedecin;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class TitresMedecinController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(TitreMedecin entity)
        {
            return new
            {
                entity.TitreId,
                entity.Libelle,
                entity.EstActif
            };
        }

        public ActionResult Index(string search = null)
        {
            var query = db.TitreMedecin
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x => x.Libelle != null && x.Libelle.Contains(search));
            }

            var items = query
                .OrderBy(x => x.Libelle)
                .Select(x => new TitreMedecinListViewModel
                {
                    TitreId = x.TitreId,
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

            var entity = db.TitreMedecin
                .AsNoTracking()
                .FirstOrDefault(x => x.TitreId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new TitreMedecinDetailsViewModel
            {
                TitreId = entity.TitreId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new TitreMedecinFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(TitreMedecinFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new TitreMedecin();
            MapToEntity(model, entity);

            db.TitreMedecin.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "TitreMedecin",
                entity.TitreId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "TitreMedecin créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.TitreMedecin.FirstOrDefault(x => x.TitreId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new TitreMedecinFormViewModel
            {
                TitreId = entity.TitreId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(TitreMedecinFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.TitreMedecin.FirstOrDefault(x => x.TitreId == model.TitreId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            MapToEntity(model, entity);

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "TitreMedecin",
                entity.TitreId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "TitreMedecin modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.TitreMedecin
                .AsNoTracking()
                .FirstOrDefault(x => x.TitreId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new TitreMedecinDetailsViewModel
            {
                TitreId = entity.TitreId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int TitreId)
        {
            var entity = db.TitreMedecin.FirstOrDefault(x => x.TitreId == TitreId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.TitreMedecin.Remove(entity);

            _audit.Enregistrer(
                "TitreMedecin",
                TitreId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "TitreMedecin supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private TitreMedecinFormViewModel BuildFormViewModel(TitreMedecinFormViewModel model)
        {
            return model;
        }

        private static void MapToEntity(TitreMedecinFormViewModel model, TitreMedecin entity)
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