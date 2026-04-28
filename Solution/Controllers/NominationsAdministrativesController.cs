using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.NominationsAdministratives;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class NominationsAdministrativesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(NominationAdministrative entity)
        {
            return new
            {
                entity.NominationAdministrativeId,
                entity.MedecinId,
                entity.LibelleNomination,
                entity.DateNomination,
                entity.DateFin,
                entity.EstActive
            };
        }

        [HttpGet]
        public ActionResult Index(string search = null)
        {
            ViewBag.Search = search;
            return View();
        }

        [HttpPost]
        public ActionResult GetNominationsAdministratives(string searchTop)
        {
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);

            string dataTableSearch = Request["search[value]"];
            string termeRecherche = !string.IsNullOrWhiteSpace(searchTop)
                ? searchTop.Trim()
                : (!string.IsNullOrWhiteSpace(dataTableSearch) ? dataTableSearch.Trim() : null);

            var query = db.NominationAdministrative
                .AsNoTracking()
                .Include(x => x.Medecin)
                .AsQueryable();

            int recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(termeRecherche))
            {
                query = query.Where(x =>
                    (x.LibelleNomination != null && x.LibelleNomination.Contains(termeRecherche)) ||
                    (x.Medecin != null && x.Medecin.Permis != null && x.Medecin.Permis.Contains(termeRecherche)) ||
                    (x.Medecin != null && x.Medecin.Nom != null && x.Medecin.Nom.Contains(termeRecherche)) ||
                    (x.Medecin != null && x.Medecin.Prenom != null && x.Medecin.Prenom.Contains(termeRecherche)));
            }

            int recordsFiltered = query.Count();

            var data = query
                .OrderBy(x => x.Medecin.Permis)
                .ThenBy(x => x.Medecin.Nom)
                .ThenBy(x => x.Medecin.Prenom)
                .Skip(start)
                .Take(length)
                .ToList()
                .Select(x => new
                {
                    x.NominationAdministrativeId,
                    MedecinLibelle = x.Medecin != null
                        ? (x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom)
                        : "",
                    x.LibelleNomination,
                    DateNomination = x.DateNomination.HasValue ? x.DateNomination.Value.ToString("yyyy-MM-dd") : "",
                    DateFin = x.DateFin.HasValue ? x.DateFin.Value.ToString("yyyy-MM-dd") : "",
                    EstActive = x.EstActive ? "Oui" : "Non"
                });

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.NominationAdministrative
                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.NominationAdministrativeId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new NominationAdministrativeDetailsViewModel
            {
                NominationAdministrativeId = entity.NominationAdministrativeId,
                MedecinId = entity.MedecinId,
                LibelleNomination = entity.LibelleNomination,
                DateNomination = entity.DateNomination,
                DateFin = entity.DateFin,
                EstActive = entity.EstActive,
                MedecinLibelle = entity.Medecin != null
                    ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom)
                    : ""
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new NominationAdministrativeFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(NominationAdministrativeFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new NominationAdministrative();
            MapToEntity(model, entity);

            db.NominationAdministrative.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "NominationAdministrative",
                entity.NominationAdministrativeId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "NominationAdministrative créée avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.NominationAdministrative.FirstOrDefault(x => x.NominationAdministrativeId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new NominationAdministrativeFormViewModel
            {
                NominationAdministrativeId = entity.NominationAdministrativeId,
                MedecinId = entity.MedecinId,
                LibelleNomination = entity.LibelleNomination,
                DateNomination = entity.DateNomination,
                DateFin = entity.DateFin,
                EstActive = entity.EstActive
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(NominationAdministrativeFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.NominationAdministrative.FirstOrDefault(x => x.NominationAdministrativeId == model.NominationAdministrativeId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            MapToEntity(model, entity);

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "NominationAdministrative",
                entity.NominationAdministrativeId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "NominationAdministrative modifiée avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.NominationAdministrative
                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.NominationAdministrativeId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new NominationAdministrativeDetailsViewModel
            {
                NominationAdministrativeId = entity.NominationAdministrativeId,
                MedecinId = entity.MedecinId,
                LibelleNomination = entity.LibelleNomination,
                DateNomination = entity.DateNomination,
                DateFin = entity.DateFin,
                EstActive = entity.EstActive,
                MedecinLibelle = entity.Medecin != null
                    ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom)
                    : ""
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int NominationAdministrativeId)
        {
            var entity = db.NominationAdministrative.FirstOrDefault(x => x.NominationAdministrativeId == NominationAdministrativeId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.NominationAdministrative.Remove(entity);

            _audit.Enregistrer(
                "NominationAdministrative",
                NominationAdministrativeId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "NominationAdministrative supprimée avec succès.";
            return RedirectToAction("Index");
        }

        private NominationAdministrativeFormViewModel BuildFormViewModel(NominationAdministrativeFormViewModel model)
        {
            model.MedecinIdOptions = db.Medecin
                .AsNoTracking()
                .OrderBy(x => x.Permis)
                .ThenBy(x => x.Nom)
                .ThenBy(x => x.Prenom)
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = x.Permis + " - " + x.Nom + " " + x.Prenom
                })
                .ToList();

            return model;
        }

        private static void MapToEntity(NominationAdministrativeFormViewModel model, NominationAdministrative entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.LibelleNomination = model.LibelleNomination;
            entity.DateNomination = model.DateNomination;
            entity.DateFin = model.DateFin;
            entity.EstActive = model.EstActive;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}