using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.MedecinsUnitesOrganisationnelles;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinsUnitesOrganisationnellesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null)
        {
            var query = db.MedecinUniteOrganisationnelle
                .Include(x => x.Medecin)
                .Include(x => x.UniteOrganisationnelle)
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => (x.RoleOrganisationnel != null && x.RoleOrganisationnel.Contains(search)));
            }
            var items = query
                .OrderBy(x => x.Medecin.Permis).ThenBy(x => x.Medecin.Nom).ThenBy(x => x.Medecin.Prenom)
                .Select(x => new MedecinUniteOrganisationnelleListViewModel
                {
                    MedecinUniteOrganisationnelleId = x.MedecinUniteOrganisationnelleId,
                    MedecinId = x.MedecinId,
                    UniteOrganisationnelleId = x.UniteOrganisationnelleId,
                    RangSource = x.RangSource,
                    RoleOrganisationnel = x.RoleOrganisationnel,
                    DateDebut = x.DateDebut,
                    DateFin = x.DateFin,
                    MedecinLibelle = x.Medecin != null ? (x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom) : string.Empty,
                    UniteOrganisationnelleLibelle = x.UniteOrganisationnelle != null ? x.UniteOrganisationnelle.Libelle : string.Empty,
                })
                .ToList();
            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinUniteOrganisationnelle
                                .Include(x => x.Medecin)
                                .Include(x => x.UniteOrganisationnelle)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinUniteOrganisationnelleId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new MedecinUniteOrganisationnelleDetailsViewModel
            {
            MedecinUniteOrganisationnelleId = entity.MedecinUniteOrganisationnelleId,
            MedecinId = entity.MedecinId,
            UniteOrganisationnelleId = entity.UniteOrganisationnelleId,
            RangSource = entity.RangSource,
            RoleOrganisationnel = entity.RoleOrganisationnel,
            DateDebut = entity.DateDebut,
            DateFin = entity.DateFin,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            UniteOrganisationnelleLibelle = (entity.UniteOrganisationnelle != null ? entity.UniteOrganisationnelle.Libelle : ""),
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new MedecinUniteOrganisationnelleFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(MedecinUniteOrganisationnelleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new MedecinUniteOrganisationnelle();
            MapToEntity(model, entity);

            db.MedecinUniteOrganisationnelle.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinUniteOrganisationnelle créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinUniteOrganisationnelle.FirstOrDefault(x => x.MedecinUniteOrganisationnelleId == id.Value);
            if (entity == null) return HttpNotFound();

            var vm = new MedecinUniteOrganisationnelleFormViewModel
            {
                MedecinUniteOrganisationnelleId = entity.MedecinUniteOrganisationnelleId,
                MedecinId = entity.MedecinId,
                UniteOrganisationnelleId = entity.UniteOrganisationnelleId,
                RangSource = entity.RangSource,
                RoleOrganisationnel = entity.RoleOrganisationnel,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(MedecinUniteOrganisationnelleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.MedecinUniteOrganisationnelle.FirstOrDefault(x => x.MedecinUniteOrganisationnelleId == model.MedecinUniteOrganisationnelleId);
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinUniteOrganisationnelle modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinUniteOrganisationnelle
                                .Include(x => x.Medecin)
                                .Include(x => x.UniteOrganisationnelle)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinUniteOrganisationnelleId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new MedecinUniteOrganisationnelleDetailsViewModel
            {
            MedecinUniteOrganisationnelleId = entity.MedecinUniteOrganisationnelleId,
            MedecinId = entity.MedecinId,
            UniteOrganisationnelleId = entity.UniteOrganisationnelleId,
            RangSource = entity.RangSource,
            RoleOrganisationnel = entity.RoleOrganisationnel,
            DateDebut = entity.DateDebut,
            DateFin = entity.DateFin,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            UniteOrganisationnelleLibelle = (entity.UniteOrganisationnelle != null ? entity.UniteOrganisationnelle.Libelle : ""),
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.MedecinUniteOrganisationnelle.FirstOrDefault(x => x.MedecinUniteOrganisationnelleId == id);
            if (entity == null) return HttpNotFound();

            db.MedecinUniteOrganisationnelle.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinUniteOrganisationnelle supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private MedecinUniteOrganisationnelleFormViewModel BuildFormViewModel(MedecinUniteOrganisationnelleFormViewModel model)
        {
            model.MedecinIdOptions = db.Medecin.AsNoTracking()
                .OrderBy(x => (x.Permis + " - " + x.Nom + " " + x.Prenom))
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = (x.Permis + " - " + x.Nom + " " + x.Prenom)
                })
                .ToList();
            model.UniteOrganisationnelleIdOptions = db.UniteOrganisationnelle.AsNoTracking()
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.UniteOrganisationnelleId.ToString(),
                    Text = x.Libelle
                })
                .ToList();
            return model;
        }

        private static void MapToEntity(MedecinUniteOrganisationnelleFormViewModel model, MedecinUniteOrganisationnelle entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.UniteOrganisationnelleId = model.UniteOrganisationnelleId;
            entity.RangSource = (Byte)model.RangSource;
            entity.RoleOrganisationnel = model.RoleOrganisationnel;
            entity.DateDebut = model.DateDebut;
            entity.DateFin = model.DateFin;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
