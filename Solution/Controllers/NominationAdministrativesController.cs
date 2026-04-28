using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.Nominations;

namespace Solution.Controllers
{
    [Authorize]
    public class NominationAdministrativesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null, int? medecinId = null, int? typeNominationId = null, bool? activesSeulement = null)
        {
            var query = db.NominationAdministrative
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.Nomination)
                .Include(x => x.TypeNomination)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    (x.Medecin.Permis != null && x.Medecin.Permis.Contains(search)) ||
                    (x.Medecin.Nom != null && x.Medecin.Nom.Contains(search)) ||
                    (x.Medecin.Prenom != null && x.Medecin.Prenom.Contains(search)) ||
                    (x.LibelleNomination != null && x.LibelleNomination.Contains(search)) ||
                    (x.Nomination != null && x.Nomination.LibelleNomination.Contains(search)));
            }

            if (medecinId.HasValue)
                query = query.Where(x => x.MedecinId == medecinId.Value);

            if (typeNominationId.HasValue)
                query = query.Where(x => x.TypeNominationId == typeNominationId.Value);

            if (activesSeulement.HasValue && activesSeulement.Value)
                query = query.Where(x => x.EstActive);

            var items = query
                .OrderBy(x => x.Medecin.Nom)
                .ThenBy(x => x.Medecin.Prenom)
                .ThenByDescending(x => x.DateNomination)
                .Select(x => new NominationAdministrativeListViewModel
                {
                    NominationAdministrativeId = x.NominationAdministrativeId,
                    MedecinNomComplet = x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom,
                    LibelleNomination = x.Nomination != null && x.Nomination.LibelleNomination != null && x.Nomination.LibelleNomination != ""
                        ? x.Nomination.LibelleNomination
                        : x.LibelleNomination,
                    TypeNominationLibelle = x.TypeNomination != null ? x.TypeNomination.LibelleTypeNomination : "",
                    DateNomination = x.DateNomination,
                    DateFin = x.DateFin,
                    EstActive = x.EstActive
                })
                .ToList();

            ViewBag.MedecinId = new SelectList(
                db.Medecin.AsNoTracking()
                    .Where(x => x.EstActif)
                    .OrderBy(x => x.Nom)
                    .ThenBy(x => x.Prenom)
                    .ToList(),
                "MedecinId",
                "Nom",
                medecinId);

            ViewBag.TypeNominationId = new SelectList(
                db.TypeNomination.AsNoTracking()
                    .Where(x => x.EstActif)
                    .OrderBy(x => x.LibelleTypeNomination)
                    .ToList(),
                "TypeNominationId",
                "LibelleTypeNomination",
                typeNominationId);

            ViewBag.Search = search;
            ViewBag.ActivesSeulement = activesSeulement;
            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("SuperUtilisateur");

            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.NominationAdministrative
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.Nomination)
                .Include(x => x.TypeNomination)
                .FirstOrDefault(x => x.NominationAdministrativeId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new NominationAdministrativeDetailsViewModel
            {
                NominationAdministrativeId = entity.NominationAdministrativeId,
                MedecinId = entity.MedecinId,
                NominationId = entity.NominationId,
                LibelleNomination = !string.IsNullOrWhiteSpace(entity.LibelleNomination)
                    ? entity.LibelleNomination
                    : (entity.Nomination != null ? entity.Nomination.LibelleNomination : ""),
                TypeNominationId = entity.TypeNominationId,
                DateNomination = entity.DateNomination,
                DateFin = entity.DateFin,
                EstActive = entity.EstActive,
                MedecinNomComplet = entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom,
                TypeNominationLibelle = entity.TypeNomination != null ? entity.TypeNomination.LibelleTypeNomination : ""
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = new NominationAdministrativeFormViewModel
            {
                DateNomination = DateTime.Today,
                EstActive = true
            };

            return View(BuildForm(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(NominationAdministrativeFormViewModel model)
        {
            ValidateBusinessRules(model);

            if (!ModelState.IsValid)
                return View(BuildForm(model));

            var entity = new NominationAdministrative();
            MapToEntity(model, entity);

            db.NominationAdministrative.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "Nomination administrative créée avec succès.";
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
                NominationId = entity.NominationId,
                LibelleNomination = entity.LibelleNomination,
                TypeNominationId = entity.TypeNominationId,
                DateNomination = entity.DateNomination,
                DateFin = entity.DateFin,
                EstActive = entity.EstActive
            };

            return View(BuildForm(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int id, NominationAdministrativeFormViewModel model)
        {
            if (id != model.NominationAdministrativeId)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ValidateBusinessRules(model);

            if (!ModelState.IsValid)
                return View(BuildForm(model));

            var entity = db.NominationAdministrative.FirstOrDefault(x => x.NominationAdministrativeId == id);
            if (entity == null)
                return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "Nomination administrative modifiée avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.NominationAdministrative
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.Nomination)
                .Include(x => x.TypeNomination)
                .FirstOrDefault(x => x.NominationAdministrativeId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new NominationAdministrativeDetailsViewModel
            {
                NominationAdministrativeId = entity.NominationAdministrativeId,
                LibelleNomination = !string.IsNullOrWhiteSpace(entity.LibelleNomination)
                    ? entity.LibelleNomination
                    : (entity.Nomination != null ? entity.Nomination.LibelleNomination : ""),
                DateNomination = entity.DateNomination,
                DateFin = entity.DateFin,
                EstActive = entity.EstActive,
                MedecinNomComplet = entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom,
                TypeNominationLibelle = entity.TypeNomination != null ? entity.TypeNomination.LibelleTypeNomination : ""
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.NominationAdministrative.FirstOrDefault(x => x.NominationAdministrativeId == id);
            if (entity == null)
                return HttpNotFound();

            db.NominationAdministrative.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "Nomination administrative supprimée avec succès.";
            return RedirectToAction("Index");
        }

        private NominationAdministrativeFormViewModel BuildForm(NominationAdministrativeFormViewModel model)
        {
            model.MedecinOptions = db.Medecin
                .AsNoTracking()
                .Where(x => x.EstActif)
                .OrderBy(x => x.Nom)
                .ThenBy(x => x.Prenom)
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = x.Permis + " - " + x.Nom + " " + x.Prenom
                })
                .ToList();

            model.NominationOptions = db.Nomination
                .AsNoTracking()
                .Where(x => x.EstActif)
                .OrderBy(x => x.LibelleNomination)
                .Select(x => new SelectListItem
                {
                    Value = x.NominationId.ToString(),
                    Text = x.CodeNomination + " - " + x.LibelleNomination
                })
                .ToList();

            model.TypeNominationOptions = db.TypeNomination
                .AsNoTracking()
                .Where(x => x.EstActif)
                .OrderBy(x => x.LibelleTypeNomination)
                .Select(x => new SelectListItem
                {
                    Value = x.TypeNominationId.ToString(),
                    Text = x.LibelleTypeNomination
                })
                .ToList();

            return model;
        }

        private void ValidateBusinessRules(NominationAdministrativeFormViewModel model)
        {
            if (!model.NominationId.HasValue && string.IsNullOrWhiteSpace(model.LibelleNomination))
            {
                ModelState.AddModelError("", "Sélectionnez une nomination ou saisissez un libellé.");
            }

            if (model.DateNomination.HasValue && model.DateFin.HasValue && model.DateFin.Value.Date < model.DateNomination.Value.Date)
            {
                ModelState.AddModelError("DateFin", "La date de fin doit être supérieure ou égale à la date de nomination.");
            }
        }

        private void MapToEntity(NominationAdministrativeFormViewModel model, NominationAdministrative entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.NominationId = model.NominationId;
            entity.TypeNominationId = model.TypeNominationId;
            entity.DateNomination = model.DateNomination;
            entity.DateFin = model.DateFin;
            entity.EstActive = model.EstActive;

            string libelleRef = null;
            if (model.NominationId.HasValue)
            {
                libelleRef = db.Nomination
                    .AsNoTracking()
                    .Where(x => x.NominationId == model.NominationId.Value)
                    .Select(x => x.LibelleNomination)
                    .FirstOrDefault();
            }

            entity.LibelleNomination = !string.IsNullOrWhiteSpace(model.LibelleNomination)
                ? model.LibelleNomination.Trim()
                : libelleRef;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}