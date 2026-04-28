using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.InstancesDecision;

namespace Solution.Controllers
{
    [Authorize]
    public class InstancesDecisionController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null)
        {
            var query = db.InstanceDecision
                .Include(x => x.TypeInstanceDecision)
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => (x.CodeInstance != null && x.CodeInstance.Contains(search)) || (x.LibelleInstance != null && x.LibelleInstance.Contains(search)));
            }
            var items = query
                .OrderBy(x => x.TypeInstanceDecisionId)
                .Select(x => new InstanceDecisionListViewModel
                {
                    InstanceDecisionId = x.InstanceDecisionId,
                    TypeInstanceDecisionId = x.TypeInstanceDecisionId,
                    CodeInstance = x.CodeInstance,
                    DateInstance = x.DateInstance,
                    LibelleInstance = x.LibelleInstance,
                    TypeInstanceDecisionLibelle = x.TypeInstanceDecision != null ? x.TypeInstanceDecision.Libelle : string.Empty,
                })
                .ToList();
            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.InstanceDecision
                                .Include(x => x.TypeInstanceDecision)
                .AsNoTracking()
                .FirstOrDefault(x => x.InstanceDecisionId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new InstanceDecisionDetailsViewModel
            {
            InstanceDecisionId = entity.InstanceDecisionId,
            TypeInstanceDecisionId = entity.TypeInstanceDecisionId,
            CodeInstance = entity.CodeInstance,
            DateInstance = entity.DateInstance,
            LibelleInstance = entity.LibelleInstance,
            TypeInstanceDecisionLibelle = (entity.TypeInstanceDecision != null ? entity.TypeInstanceDecision.Libelle : ""),
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new InstanceDecisionFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(InstanceDecisionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new InstanceDecision();
            MapToEntity(model, entity);

            db.InstanceDecision.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "InstanceDecision créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.InstanceDecision.FirstOrDefault(x => x.InstanceDecisionId == id.Value);
            if (entity == null) return HttpNotFound();

            var vm = new InstanceDecisionFormViewModel
            {
                InstanceDecisionId = entity.InstanceDecisionId,
                TypeInstanceDecisionId = entity.TypeInstanceDecisionId,
                CodeInstance = entity.CodeInstance,
                DateInstance = entity.DateInstance,
                LibelleInstance = entity.LibelleInstance,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(InstanceDecisionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.InstanceDecision.FirstOrDefault(x => x.InstanceDecisionId == model.InstanceDecisionId);
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "InstanceDecision modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.InstanceDecision
                                .Include(x => x.TypeInstanceDecision)
                .AsNoTracking()
                .FirstOrDefault(x => x.InstanceDecisionId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new InstanceDecisionDetailsViewModel
            {
            InstanceDecisionId = entity.InstanceDecisionId,
            TypeInstanceDecisionId = entity.TypeInstanceDecisionId,
            CodeInstance = entity.CodeInstance,
            DateInstance = entity.DateInstance,
            LibelleInstance = entity.LibelleInstance,
            TypeInstanceDecisionLibelle = (entity.TypeInstanceDecision != null ? entity.TypeInstanceDecision.Libelle : ""),
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.InstanceDecision.FirstOrDefault(x => x.InstanceDecisionId == id);
            if (entity == null) return HttpNotFound();

            db.InstanceDecision.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "InstanceDecision supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private InstanceDecisionFormViewModel BuildFormViewModel(InstanceDecisionFormViewModel model)
        {
            model.TypeInstanceDecisionIdOptions = db.TypeInstanceDecision.AsNoTracking()
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.TypeInstanceDecisionId.ToString(),
                    Text = x.Libelle
                })
                .ToList();
            return model;
        }

        private static void MapToEntity(InstanceDecisionFormViewModel model, InstanceDecision entity)
        {
            entity.TypeInstanceDecisionId = model.TypeInstanceDecisionId;
            entity.CodeInstance = model.CodeInstance;
            entity.DateInstance = model.DateInstance;
            entity.LibelleInstance = model.LibelleInstance;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
