using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.MedecinsInstallations;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinsInstallationsController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null)
        {
            var query = db.MedecinInstallation
                .Include(x => x.Medecin)
                .Include(x => x.Installation)
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => (x.RangInstallation.ToString().Contains(search)));
            }
            var items = query
                .OrderBy(x => x.Medecin.Permis).ThenBy(x => x.Medecin.Nom).ThenBy(x => x.Medecin.Prenom)
                .Select(x => new MedecinInstallationListViewModel
                {
                    MedecinInstallationId = x.MedecinInstallationId,
                    MedecinId = x.MedecinId,
                    InstallationId = x.InstallationId,
                    RangInstallation = x.RangInstallation,
                    DateDebut = x.DateDebut,
                    DateFin = x.DateFin,
                    EstPrincipale = x.EstPrincipale,
                    MedecinLibelle = x.Medecin != null ? (x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom) : string.Empty,
                    InstallationLibelle = x.Installation != null ? x.Installation.NomInstallation : string.Empty,
                })
                .ToList();
            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinInstallation
                                .Include(x => x.Medecin)
                                .Include(x => x.Installation)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinInstallationId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new MedecinInstallationDetailsViewModel
            {
            MedecinInstallationId = entity.MedecinInstallationId,
            MedecinId = entity.MedecinId,
            InstallationId = entity.InstallationId,
            RangInstallation = entity.RangInstallation,
            DateDebut = entity.DateDebut,
            DateFin = entity.DateFin,
            EstPrincipale = entity.EstPrincipale,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            InstallationLibelle = (entity.Installation != null ? entity.Installation.NomInstallation : ""),
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new MedecinInstallationFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(MedecinInstallationFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new MedecinInstallation();
            MapToEntity(model, entity);

            db.MedecinInstallation.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinInstallation créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinInstallation.FirstOrDefault(x => x.MedecinInstallationId == id.Value);
            if (entity == null) return HttpNotFound();

            var vm = new MedecinInstallationFormViewModel
            {
                MedecinInstallationId = entity.MedecinInstallationId,
                MedecinId = entity.MedecinId,
                InstallationId = entity.InstallationId,
                RangInstallation = entity.RangInstallation,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                EstPrincipale = entity.EstPrincipale,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(MedecinInstallationFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.MedecinInstallation.FirstOrDefault(x => x.MedecinInstallationId == model.MedecinInstallationId);
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinInstallation modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinInstallation
                                .Include(x => x.Medecin)
                                .Include(x => x.Installation)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinInstallationId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new MedecinInstallationDetailsViewModel
            {
            MedecinInstallationId = entity.MedecinInstallationId,
            MedecinId = entity.MedecinId,
            InstallationId = entity.InstallationId,
            RangInstallation = entity.RangInstallation,
            DateDebut = entity.DateDebut,
            DateFin = entity.DateFin,
            EstPrincipale = entity.EstPrincipale,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            InstallationLibelle = (entity.Installation != null ? entity.Installation.NomInstallation : ""),
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.MedecinInstallation.FirstOrDefault(x => x.MedecinInstallationId == id);
            if (entity == null) return HttpNotFound();

            db.MedecinInstallation.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinInstallation supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private MedecinInstallationFormViewModel BuildFormViewModel(MedecinInstallationFormViewModel model)
        {
            model.MedecinIdOptions = db.Medecin.AsNoTracking()
                .OrderBy(x => (x.Permis + " - " + x.Nom + " " + x.Prenom))
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = (x.Permis + " - " + x.Nom + " " + x.Prenom)
                })
                .ToList();
            model.InstallationIdOptions = db.Installation.AsNoTracking()
                .OrderBy(x => x.NomInstallation)
                .Select(x => new SelectListItem
                {
                    Value = x.InstallationId.ToString(),
                    Text = x.NomInstallation
                })
                .ToList();
            return model;
        }

        private static void MapToEntity(MedecinInstallationFormViewModel model, MedecinInstallation entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.InstallationId = model.InstallationId;
            entity.RangInstallation = (Byte)model.RangInstallation;
            entity.DateDebut = model.DateDebut;
            entity.DateFin = model.DateFin;
            entity.EstPrincipale = model.EstPrincipale;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
