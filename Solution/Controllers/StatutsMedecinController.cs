using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.StatutsMedecin;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class StatutsMedecinController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(StatutMedecin entity)
        {
            return new
            {
                entity.StatutMedecinId,
                entity.CodeStatut,
                entity.Libelle,
                entity.EstActif
            };
        }

        public ActionResult Index(string search = null)
        {
            var query = db.StatutMedecin
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    (x.CodeStatut != null && x.CodeStatut.Contains(search)) ||
                    (x.Libelle != null && x.Libelle.Contains(search)));
            }

            var items = query
                .OrderBy(x => x.Libelle)
                .Select(x => new StatutMedecinListViewModel
                {
                    StatutMedecinId = x.StatutMedecinId,
                    CodeStatut = x.CodeStatut,
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

            var entity = db.StatutMedecin
                .AsNoTracking()
                .FirstOrDefault(x => x.StatutMedecinId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new StatutMedecinDetailsViewModel
            {
                StatutMedecinId = entity.StatutMedecinId,
                CodeStatut = entity.CodeStatut,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new StatutMedecinFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(StatutMedecinFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new StatutMedecin();
            MapToEntity(model, entity);

            db.StatutMedecin.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "StatutMedecin",
                entity.StatutMedecinId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "StatutMedecin créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.StatutMedecin.FirstOrDefault(x => x.StatutMedecinId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new StatutMedecinFormViewModel
            {
                StatutMedecinId = entity.StatutMedecinId,
                CodeStatut = entity.CodeStatut,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(StatutMedecinFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.StatutMedecin.FirstOrDefault(x => x.StatutMedecinId == model.StatutMedecinId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            MapToEntity(model, entity);

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "StatutMedecin",
                entity.StatutMedecinId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "StatutMedecin modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.StatutMedecin
                .AsNoTracking()
                .FirstOrDefault(x => x.StatutMedecinId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new StatutMedecinDetailsViewModel
            {
                StatutMedecinId = entity.StatutMedecinId,
                CodeStatut = entity.CodeStatut,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int StatutMedecinId)
        {
            var entity = db.StatutMedecin.FirstOrDefault(x => x.StatutMedecinId == StatutMedecinId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.StatutMedecin.Remove(entity);

            _audit.Enregistrer(
                "StatutMedecin",
                StatutMedecinId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "StatutMedecin supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private StatutMedecinFormViewModel BuildFormViewModel(StatutMedecinFormViewModel model)
        {
            return model;
        }

        private static void MapToEntity(StatutMedecinFormViewModel model, StatutMedecin entity)
        {
            entity.CodeStatut = model.CodeStatut;
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