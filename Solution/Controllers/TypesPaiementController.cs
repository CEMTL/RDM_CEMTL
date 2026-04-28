using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.TypesPaiement;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class TypesPaiementController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(TypePaiement entity)
        {
            return new
            {
                entity.TypePaiementId,
                entity.Libelle,
                entity.EstActif
            };
        }

        public ActionResult Index(string search = null)
        {
            var query = db.TypePaiement
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x => x.Libelle != null && x.Libelle.Contains(search));
            }

            var items = query
                .OrderBy(x => x.Libelle)
                .Select(x => new TypePaiementListViewModel
                {
                    TypePaiementId = x.TypePaiementId,
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

            var entity = db.TypePaiement
                .AsNoTracking()
                .FirstOrDefault(x => x.TypePaiementId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new TypePaiementDetailsViewModel
            {
                TypePaiementId = entity.TypePaiementId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new TypePaiementFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(TypePaiementFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new TypePaiement();
            MapToEntity(model, entity);

            db.TypePaiement.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "TypePaiement",
                entity.TypePaiementId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "TypePaiement créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.TypePaiement.FirstOrDefault(x => x.TypePaiementId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new TypePaiementFormViewModel
            {
                TypePaiementId = entity.TypePaiementId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(TypePaiementFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.TypePaiement.FirstOrDefault(x => x.TypePaiementId == model.TypePaiementId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            MapToEntity(model, entity);

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "TypePaiement",
                entity.TypePaiementId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "TypePaiement modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.TypePaiement
                .AsNoTracking()
                .FirstOrDefault(x => x.TypePaiementId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new TypePaiementDetailsViewModel
            {
                TypePaiementId = entity.TypePaiementId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int TypePaiementId)
        {
            var entity = db.TypePaiement.FirstOrDefault(x => x.TypePaiementId == TypePaiementId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.TypePaiement.Remove(entity);

            _audit.Enregistrer(
                "TypePaiement",
                TypePaiementId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "TypePaiement supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private TypePaiementFormViewModel BuildFormViewModel(TypePaiementFormViewModel model)
        {
            return model;
        }

        private static void MapToEntity(TypePaiementFormViewModel model, TypePaiement entity)
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