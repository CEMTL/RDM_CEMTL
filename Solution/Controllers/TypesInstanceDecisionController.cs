using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.TypesInstanceDecision;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class TypesInstanceDecisionController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(TypeInstanceDecision entity)
        {
            return new
            {
                entity.TypeInstanceDecisionId,
                entity.CodeType,
                entity.Libelle
            };
        }

        public ActionResult Index(string search = null)
        {
            var query = db.TypeInstanceDecision
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    (x.CodeType != null && x.CodeType.Contains(search)) ||
                    (x.Libelle != null && x.Libelle.Contains(search)));
            }

            var items = query
                .OrderBy(x => x.CodeType)
                .Select(x => new TypeInstanceDecisionListViewModel
                {
                    TypeInstanceDecisionId = x.TypeInstanceDecisionId,
                    CodeType = x.CodeType,
                    Libelle = x.Libelle
                })
                .ToList();

            ViewBag.Search = search;

            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.TypeInstanceDecision
                .AsNoTracking()
                .FirstOrDefault(x => x.TypeInstanceDecisionId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new TypeInstanceDecisionDetailsViewModel
            {
                TypeInstanceDecisionId = entity.TypeInstanceDecisionId,
                CodeType = entity.CodeType,
                Libelle = entity.Libelle
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new TypeInstanceDecisionFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(TypeInstanceDecisionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new TypeInstanceDecision();
            MapToEntity(model, entity);

            db.TypeInstanceDecision.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "TypeInstanceDecision",
                entity.TypeInstanceDecisionId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "TypeInstanceDecision créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.TypeInstanceDecision.FirstOrDefault(x => x.TypeInstanceDecisionId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new TypeInstanceDecisionFormViewModel
            {
                TypeInstanceDecisionId = entity.TypeInstanceDecisionId,
                CodeType = entity.CodeType,
                Libelle = entity.Libelle
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(TypeInstanceDecisionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.TypeInstanceDecision.FirstOrDefault(x => x.TypeInstanceDecisionId == model.TypeInstanceDecisionId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            MapToEntity(model, entity);

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "TypeInstanceDecision",
                entity.TypeInstanceDecisionId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "TypeInstanceDecision modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.TypeInstanceDecision
                .AsNoTracking()
                .FirstOrDefault(x => x.TypeInstanceDecisionId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new TypeInstanceDecisionDetailsViewModel
            {
                TypeInstanceDecisionId = entity.TypeInstanceDecisionId,
                CodeType = entity.CodeType,
                Libelle = entity.Libelle
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int TypeInstanceDecisionId)
        {
            var entity = db.TypeInstanceDecision.FirstOrDefault(x => x.TypeInstanceDecisionId == TypeInstanceDecisionId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.TypeInstanceDecision.Remove(entity);

            _audit.Enregistrer(
                "TypeInstanceDecision",
                TypeInstanceDecisionId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "TypeInstanceDecision supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private TypeInstanceDecisionFormViewModel BuildFormViewModel(TypeInstanceDecisionFormViewModel model)
        {
            return model;
        }

        private static void MapToEntity(TypeInstanceDecisionFormViewModel model, TypeInstanceDecision entity)
        {
            entity.CodeType = model.CodeType;
            entity.Libelle = model.Libelle;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}