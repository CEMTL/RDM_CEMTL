using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.Installations;

namespace Solution.Controllers
{
    [Authorize]
    public class InstallationsController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null)
        {
            var query = db.Installation.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => (x.NomInstallation != null && x.NomInstallation.Contains(search)) || (x.CodeInstallation != null && x.CodeInstallation.Contains(search)));
            }
            var items = query
                .OrderBy(x => x.NomInstallation)
                .Select(x => new InstallationListViewModel
                {
                    InstallationId = x.InstallationId,
                    NomInstallation = x.NomInstallation,
                    CodeInstallation = x.CodeInstallation,
                    EstActif = x.EstActif,
                })
                .ToList();
            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.Installation

                .AsNoTracking()
                .FirstOrDefault(x => x.InstallationId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new InstallationDetailsViewModel
            {
            InstallationId = entity.InstallationId,
            NomInstallation = entity.NomInstallation,
            CodeInstallation = entity.CodeInstallation,
            EstActif = entity.EstActif,
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new InstallationFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(InstallationFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new Installation();
            MapToEntity(model, entity);

            db.Installation.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "Installation créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.Installation.FirstOrDefault(x => x.InstallationId == id.Value);
            if (entity == null) return HttpNotFound();

            var vm = new InstallationFormViewModel
            {
                InstallationId = entity.InstallationId,
                NomInstallation = entity.NomInstallation,
                CodeInstallation = entity.CodeInstallation,
                EstActif = entity.EstActif,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(InstallationFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.Installation.FirstOrDefault(x => x.InstallationId == model.InstallationId);
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "Installation modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.Installation

                .AsNoTracking()
                .FirstOrDefault(x => x.InstallationId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new InstallationDetailsViewModel
            {
            InstallationId = entity.InstallationId,
            NomInstallation = entity.NomInstallation,
            CodeInstallation = entity.CodeInstallation,
            EstActif = entity.EstActif,
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.Installation.FirstOrDefault(x => x.InstallationId == id);
            if (entity == null) return HttpNotFound();

            db.Installation.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "Installation supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private InstallationFormViewModel BuildFormViewModel(InstallationFormViewModel model)
        {
            // aucune liste déroulante
            return model;
        }

        private static void MapToEntity(InstallationFormViewModel model, Installation entity)
        {
            entity.NomInstallation = model.NomInstallation;
            entity.CodeInstallation = model.CodeInstallation;
            entity.EstActif = model.EstActif;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
