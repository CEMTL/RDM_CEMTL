using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.DossiersPrivilegeDecision;

namespace Solution.Controllers
{
    [Authorize]
    public class DossiersPrivilegeDecisionController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null)
        {
            var query = db.DossierPrivilegeDecision
                .Include(x => x.DossierPrivilege)
                .Include(x => x.InstanceDecision)
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => (x.TexteDecision != null && x.TexteDecision.Contains(search)));
            }
            var items = query
                .OrderBy(x => x.DossierPrivilegeId)
                .Select(x => new DossierPrivilegeDecisionListViewModel
                {
                    DossierPrivilegeDecisionId = x.DossierPrivilegeDecisionId,
                    DossierPrivilegeId = x.DossierPrivilegeId,
                    InstanceDecisionId = x.InstanceDecisionId,
                    TexteDecision = x.TexteDecision,
                    DossierPrivilegeLibelle = x.DossierPrivilege != null ? (x.DossierPrivilege.NumeroResolution ?? ("Dossier #" + x.DossierPrivilege.DossierPrivilegeId)) : string.Empty,
                    InstanceDecisionLibelle = x.InstanceDecision != null ? (x.InstanceDecision.CodeInstance + " - " + (x.InstanceDecision.LibelleInstance ?? string.Empty)) : string.Empty,
                })
                .ToList();
            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.DossierPrivilegeDecision
                                .Include(x => x.DossierPrivilege)
                                .Include(x => x.InstanceDecision)
                .AsNoTracking()
                .FirstOrDefault(x => x.DossierPrivilegeDecisionId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new DossierPrivilegeDecisionDetailsViewModel
            {
            DossierPrivilegeDecisionId = entity.DossierPrivilegeDecisionId,
            DossierPrivilegeId = entity.DossierPrivilegeId,
            InstanceDecisionId = entity.InstanceDecisionId,
            TexteDecision = entity.TexteDecision,
            DossierPrivilegeLibelle = (entity.DossierPrivilege != null ? (entity.DossierPrivilege.NumeroResolution ?? ("Dossier #" + entity.DossierPrivilege.DossierPrivilegeId)) : ""),
            InstanceDecisionLibelle = (entity.InstanceDecision != null ? (entity.InstanceDecision.CodeInstance + " - " + (entity.InstanceDecision.LibelleInstance ?? "")) : ""),
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new DossierPrivilegeDecisionFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(DossierPrivilegeDecisionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new DossierPrivilegeDecision();
            MapToEntity(model, entity);

            db.DossierPrivilegeDecision.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "DossierPrivilegeDecision créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.DossierPrivilegeDecision.FirstOrDefault(x => x.DossierPrivilegeDecisionId == id.Value);
            if (entity == null) return HttpNotFound();

            var vm = new DossierPrivilegeDecisionFormViewModel
            {
                DossierPrivilegeDecisionId = entity.DossierPrivilegeDecisionId,
                DossierPrivilegeId = entity.DossierPrivilegeId,
                InstanceDecisionId = entity.InstanceDecisionId,
                TexteDecision = entity.TexteDecision,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(DossierPrivilegeDecisionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.DossierPrivilegeDecision.FirstOrDefault(x => x.DossierPrivilegeDecisionId == model.DossierPrivilegeDecisionId);
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "DossierPrivilegeDecision modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.DossierPrivilegeDecision
                                .Include(x => x.DossierPrivilege)
                                .Include(x => x.InstanceDecision)
                .AsNoTracking()
                .FirstOrDefault(x => x.DossierPrivilegeDecisionId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new DossierPrivilegeDecisionDetailsViewModel
            {
            DossierPrivilegeDecisionId = entity.DossierPrivilegeDecisionId,
            DossierPrivilegeId = entity.DossierPrivilegeId,
            InstanceDecisionId = entity.InstanceDecisionId,
            TexteDecision = entity.TexteDecision,
            DossierPrivilegeLibelle = (entity.DossierPrivilege != null ? (entity.DossierPrivilege.NumeroResolution ?? ("Dossier #" + entity.DossierPrivilege.DossierPrivilegeId)) : ""),
            InstanceDecisionLibelle = (entity.InstanceDecision != null ? (entity.InstanceDecision.CodeInstance + " - " + (entity.InstanceDecision.LibelleInstance ?? "")) : ""),
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.DossierPrivilegeDecision.FirstOrDefault(x => x.DossierPrivilegeDecisionId == id);
            if (entity == null) return HttpNotFound();

            db.DossierPrivilegeDecision.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "DossierPrivilegeDecision supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private DossierPrivilegeDecisionFormViewModel BuildFormViewModel(DossierPrivilegeDecisionFormViewModel model)
        {
            model.DossierPrivilegeIdOptions = db.DossierPrivilege.AsNoTracking()
                .OrderBy(x => (x.NumeroResolution ?? ("Dossier #" + x.DossierPrivilegeId)))
                .Select(x => new SelectListItem
                {
                    Value = x.DossierPrivilegeId.ToString(),
                    Text = (x.NumeroResolution ?? ("Dossier #" + x.DossierPrivilegeId))
                })
                .ToList();
            model.InstanceDecisionIdOptions = db.InstanceDecision.AsNoTracking()
                .OrderBy(x => (x.CodeInstance + " - " + (x.LibelleInstance ?? "")))
                .Select(x => new SelectListItem
                {
                    Value = x.InstanceDecisionId.ToString(),
                    Text = (x.CodeInstance + " - " + (x.LibelleInstance ?? ""))
                })
                .ToList();
            return model;
        }

        private static void MapToEntity(DossierPrivilegeDecisionFormViewModel model, DossierPrivilegeDecision entity)
        {
            entity.DossierPrivilegeId = model.DossierPrivilegeId;
            entity.InstanceDecisionId = model.InstanceDecisionId;
            entity.TexteDecision = model.TexteDecision;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
