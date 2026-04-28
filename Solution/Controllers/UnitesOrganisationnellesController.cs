using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.UnitesOrganisationnelles;

namespace Solution.Controllers
{
    [Authorize]
    public class UnitesOrganisationnellesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null)
        {
            var query = db.UniteOrganisationnelle
                .Include(x => x.UniteOrganisationnelle1)
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => (x.Libelle != null && x.Libelle.Contains(search)) || (x.TypeUnite != null && x.TypeUnite.Contains(search)));
            }
            var items = query
                .OrderBy(x => x.Libelle)
                .Select(x => new UniteOrganisationnelleListViewModel
                {
                    UniteOrganisationnelleId = x.UniteOrganisationnelleId,
                    Libelle = x.Libelle,
                    TypeUnite = x.TypeUnite,
                    UniteParentId = x.UniteParentId,
                    EstActif = x.EstActif,
                    UniteParentLibelle = x.UniteOrganisationnelle2 != null ? x.UniteOrganisationnelle2.Libelle : string.Empty,
                })
                .ToList();
            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.UniteOrganisationnelle
                                .Include(x => x.UniteOrganisationnelle1)
                .AsNoTracking()
                .FirstOrDefault(x => x.UniteOrganisationnelleId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new UniteOrganisationnelleDetailsViewModel
            {
            UniteOrganisationnelleId = entity.UniteOrganisationnelleId,
            Libelle = entity.Libelle,
            TypeUnite = entity.TypeUnite,
            UniteParentId = entity.UniteParentId,
            EstActif = entity.EstActif,
            UniteParentLibelle = (entity.UniteOrganisationnelle2 != null ? entity.UniteOrganisationnelle2.Libelle : ""),
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new UniteOrganisationnelleFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(UniteOrganisationnelleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new UniteOrganisationnelle();
            MapToEntity(model, entity);

            db.UniteOrganisationnelle.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "UniteOrganisationnelle créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.UniteOrganisationnelle.FirstOrDefault(x => x.UniteOrganisationnelleId == id.Value);
            if (entity == null) return HttpNotFound();

            var vm = new UniteOrganisationnelleFormViewModel
            {
                UniteOrganisationnelleId = entity.UniteOrganisationnelleId,
                Libelle = entity.Libelle,
                TypeUnite = entity.TypeUnite,
                UniteParentId = entity.UniteParentId,
                EstActif = entity.EstActif,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(UniteOrganisationnelleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.UniteOrganisationnelle.FirstOrDefault(x => x.UniteOrganisationnelleId == model.UniteOrganisationnelleId);
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "UniteOrganisationnelle modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.UniteOrganisationnelle
                                .Include(x => x.UniteOrganisationnelle1)
                .AsNoTracking()
                .FirstOrDefault(x => x.UniteOrganisationnelleId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new UniteOrganisationnelleDetailsViewModel
            {
            UniteOrganisationnelleId = entity.UniteOrganisationnelleId,
            Libelle = entity.Libelle,
            TypeUnite = entity.TypeUnite,
            UniteParentId = entity.UniteParentId,
            EstActif = entity.EstActif,
            UniteParentLibelle = (entity.UniteOrganisationnelle2 != null ? entity.UniteOrganisationnelle2.Libelle : ""),
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.UniteOrganisationnelle.FirstOrDefault(x => x.UniteOrganisationnelleId == id);
            if (entity == null) return HttpNotFound();

            db.UniteOrganisationnelle.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "UniteOrganisationnelle supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private UniteOrganisationnelleFormViewModel BuildFormViewModel(UniteOrganisationnelleFormViewModel model)
        {
            model.UniteParentIdOptions = db.UniteOrganisationnelle.AsNoTracking()
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.UniteOrganisationnelleId.ToString(),
                    Text = x.Libelle
                })
                .ToList();
            return model;
        }

        private static void MapToEntity(UniteOrganisationnelleFormViewModel model, UniteOrganisationnelle entity)
        {
            entity.Libelle = model.Libelle;
            entity.TypeUnite = model.TypeUnite;
            entity.UniteParentId = model.UniteParentId;
            entity.EstActif = model.EstActif;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
