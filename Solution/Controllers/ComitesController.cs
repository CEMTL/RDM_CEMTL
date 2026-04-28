using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.Comites;

namespace Solution.Controllers
{
    [Authorize]
    public class ComitesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null)
        {
            var query = db.Comite.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => (x.NomComite != null && x.NomComite.Contains(search)));
            }
            var items = query
                .OrderBy(x => x.NumeroSource)
                .Select(x => new ComiteListViewModel
                {
                    ComiteId = x.ComiteId,
                    NumeroSource = x.NumeroSource,
                    NomComite = x.NomComite,
                    EstActif = x.EstActif,
                })
                .ToList();
            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.Comite

                .AsNoTracking()
                .FirstOrDefault(x => x.ComiteId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new ComiteDetailsViewModel
            {
            ComiteId = entity.ComiteId,
            NumeroSource = entity.NumeroSource,
            NomComite = entity.NomComite,
            EstActif = entity.EstActif,
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new ComiteFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(ComiteFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new Comite();
            MapToEntity(model, entity);

            db.Comite.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "Comite créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.Comite.FirstOrDefault(x => x.ComiteId == id.Value);
            if (entity == null) return HttpNotFound();

            var vm = new ComiteFormViewModel
            {
                ComiteId = entity.ComiteId,
                NumeroSource = entity.NumeroSource,
                NomComite = entity.NomComite,
                EstActif = entity.EstActif,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(ComiteFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.Comite.FirstOrDefault(x => x.ComiteId == model.ComiteId);
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "Comite modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.Comite

                .AsNoTracking()
                .FirstOrDefault(x => x.ComiteId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new ComiteDetailsViewModel
            {
            ComiteId = entity.ComiteId,
            NumeroSource = entity.NumeroSource,
            NomComite = entity.NomComite,
            EstActif = entity.EstActif,
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.Comite.FirstOrDefault(x => x.ComiteId == id);
            if (entity == null) return HttpNotFound();

            db.Comite.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "Comite supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private ComiteFormViewModel BuildFormViewModel(ComiteFormViewModel model)
        {
            // aucune liste déroulante
            return model;
        }

        private static void MapToEntity(ComiteFormViewModel model, Comite entity)
        {
            entity.NumeroSource = model.NumeroSource;
            entity.NomComite = model.NomComite;
            entity.EstActif = model.EstActif;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
