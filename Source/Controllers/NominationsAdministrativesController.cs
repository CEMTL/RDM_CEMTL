using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Solution.Models;
using Solution.Services;
using Solution.ViewModels.Nominations;
using System;
using System.Linq;

namespace Solution.Controllers
{
    [Authorize]
    public class NominationsAdministrativesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;

        public NominationsAdministrativesController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }

        private AuditService Audit => new AuditService(db, HttpContext);

        [HttpGet]
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
                    (x.Medecin != null && x.Medecin.Permis != null && x.Medecin.Permis.Contains(search)) ||
                    (x.Medecin != null && x.Medecin.Nom != null && x.Medecin.Nom.Contains(search)) ||
                    (x.Medecin != null && x.Medecin.Prenom != null && x.Medecin.Prenom.Contains(search)) ||
                    (x.LibelleNomination != null && x.LibelleNomination.Contains(search)) ||
                    (x.Nomination != null && x.Nomination.LibelleNomination.Contains(search)) ||
                    (x.TypeNomination != null && x.TypeNomination.LibelleTypeNomination.Contains(search)));
            }

            if (medecinId.HasValue)
                query = query.Where(x => x.MedecinId == medecinId.Value);

            if (typeNominationId.HasValue)
                query = query.Where(x => x.TypeNominationId == typeNominationId.Value);

            if (activesSeulement == true)
                query = query.Where(x => x.EstActive && (!x.DateFin.HasValue || x.DateFin.Value.Date >= DateTime.Today));

            var items = query
                .OrderBy(x => x.Medecin.Nom)
                .ThenBy(x => x.Medecin.Prenom)
                .ThenByDescending(x => x.DateNomination)
                .Select(x => new NominationAdministrativeListViewModel
                {
                    NominationAdministrativeId = x.NominationAdministrativeId,
                    MedecinNomComplet = x.Medecin != null
                        ? x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom
                        : "",
                    LibelleNomination = x.Nomination != null && !string.IsNullOrEmpty(x.Nomination.LibelleNomination)
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
                    .Select(x => new { x.MedecinId, Libelle = x.Permis + " - " + x.Nom + " " + x.Prenom })
                    .ToList(),
                "MedecinId", "Libelle", medecinId);

            ViewBag.TypeNominationId = new SelectList(
                db.TypeNomination.AsNoTracking()
                    .Where(x => x.EstActif)
                    .OrderBy(x => x.LibelleTypeNomination)
                    .ToList(),
                "TypeNominationId", "LibelleTypeNomination", typeNominationId);

            ViewBag.Search = search;
            ViewBag.ActivesSeulement = activesSeulement == true;
            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("SuperUtilisateur");
            return View(items);
        }

        public ActionResult Details(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (id == null) return BadRequest();

            var entity = Charger(id.Value, true);
            if (entity == null) return NotFound();
            return View(ToDetails(entity));
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(int? medecinId = null, int? returnMedecinId = null, string returnTab = null)
        {
            if (medecinId.HasValue && !returnMedecinId.HasValue)
                returnMedecinId = medecinId;

            PreparerRetour(returnMedecinId, returnTab ?? "nominations");
            var vm = new NominationAdministrativeFormViewModel
            {
                MedecinId = medecinId ?? 0,
                DateNomination = DateTime.Today,
                EstActive = true
            };
            return View(BuildForm(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(NominationAdministrativeFormViewModel model, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab ?? "nominations");
            ValidateBusinessRules(model);
            if (!ModelState.IsValid)
                return View(BuildForm(model));

            var entity = new NominationAdministrative();
            MapToEntity(model, entity);
            db.NominationAdministrative.Add(entity);
            db.SaveChanges();

            Audit.Enregistrer("NominationAdministrative", entity.NominationAdministrativeId, "INSERT", null, Snapshot(entity));
            db.SaveChanges();

            TempData["Success"] = "Nomination administrative créée avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab ?? "nominations");
            if (id == null) return BadRequest();

            var entity = db.NominationAdministrative.FirstOrDefault(x => x.NominationAdministrativeId == id.Value);
            if (entity == null) return NotFound();

            return View(BuildForm(new NominationAdministrativeFormViewModel
            {
                NominationAdministrativeId = entity.NominationAdministrativeId,
                MedecinId = entity.MedecinId,
                NominationId = entity.NominationId,
                LibelleNomination = entity.LibelleNomination,
                TypeNominationId = entity.TypeNominationId,
                DateNomination = entity.DateNomination,
                DateFin = entity.DateFin,
                EstActive = entity.EstActive
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int id, NominationAdministrativeFormViewModel model, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab ?? "nominations");
            if (id != model.NominationAdministrativeId) return BadRequest();

            ValidateBusinessRules(model);
            if (!ModelState.IsValid)
                return View(BuildForm(model));

            var entity = db.NominationAdministrative.FirstOrDefault(x => x.NominationAdministrativeId == id);
            if (entity == null) return NotFound();

            var oldValue = Snapshot(entity);
            MapToEntity(model, entity);
            var newValue = Snapshot(entity);
            Audit.Enregistrer("NominationAdministrative", entity.NominationAdministrativeId, "UPDATE", oldValue, newValue);
            db.SaveChanges();

            TempData["Success"] = "Nomination administrative modifiée avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab ?? "nominations");
            if (id == null) return BadRequest();

            var entity = Charger(id.Value, true);
            if (entity == null) return NotFound();
            return View(ToDetails(entity));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id, int? returnMedecinId = null, string returnTab = null)
        {
            var entity = db.NominationAdministrative.FirstOrDefault(x => x.NominationAdministrativeId == id);
            if (entity == null) return NotFound();

            var oldValue = Snapshot(entity);
            entity.EstActive = false;
            if (!entity.DateFin.HasValue)
                entity.DateFin = DateTime.Today;

            Audit.Enregistrer("NominationAdministrative", entity.NominationAdministrativeId, "UPDATE", oldValue, Snapshot(entity));
            db.SaveChanges();

            TempData["Success"] = "Nomination administrative terminée et conservée dans l'historique.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        private NominationAdministrative Charger(int id, bool noTracking)
        {
            var query = db.NominationAdministrative
                .Include(x => x.Medecin)
                .Include(x => x.Nomination)
                .Include(x => x.TypeNomination)
                .AsQueryable();
            if (noTracking) query = query.AsNoTracking();
            return query.FirstOrDefault(x => x.NominationAdministrativeId == id);
        }

        private NominationAdministrativeFormViewModel BuildForm(NominationAdministrativeFormViewModel model)
        {
            model.MedecinOptions = db.Medecin.AsNoTracking()
                .Where(x => x.EstActif || x.MedecinId == model.MedecinId)
                .OrderBy(x => x.Nom).ThenBy(x => x.Prenom)
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = x.Permis + " - " + x.Nom + " " + x.Prenom,
                    Selected = x.MedecinId == model.MedecinId
                }).ToList();

            model.NominationOptions = db.Nomination.AsNoTracking()
                .Where(x => x.EstActif || x.NominationId == model.NominationId)
                .OrderBy(x => x.LibelleNomination)
                .Select(x => new SelectListItem
                {
                    Value = x.NominationId.ToString(),
                    Text = x.CodeNomination + " - " + x.LibelleNomination,
                    Selected = x.NominationId == model.NominationId
                }).ToList();

            model.TypeNominationOptions = db.TypeNomination.AsNoTracking()
                .Where(x => x.EstActif || x.TypeNominationId == model.TypeNominationId)
                .OrderBy(x => x.LibelleTypeNomination)
                .Select(x => new SelectListItem
                {
                    Value = x.TypeNominationId.ToString(),
                    Text = x.LibelleTypeNomination,
                    Selected = x.TypeNominationId == model.TypeNominationId
                }).ToList();
            return model;
        }

        private void ValidateBusinessRules(NominationAdministrativeFormViewModel model)
        {
            if (model.MedecinId <= 0 || !db.Medecin.Any(x => x.MedecinId == model.MedecinId))
                ModelState.AddModelError(nameof(model.MedecinId), "Le médecin est requis.");

            if (!model.NominationId.HasValue && string.IsNullOrWhiteSpace(model.LibelleNomination))
                ModelState.AddModelError("", "Sélectionnez une nomination ou saisissez un libellé.");

            if (model.NominationId.HasValue && !db.Nomination.Any(x => x.NominationId == model.NominationId.Value))
                ModelState.AddModelError(nameof(model.NominationId), "La nomination sélectionnée est invalide.");

            if (model.TypeNominationId.HasValue && !db.TypeNomination.Any(x => x.TypeNominationId == model.TypeNominationId.Value))
                ModelState.AddModelError(nameof(model.TypeNominationId), "Le type de nomination sélectionné est invalide.");

            if (model.DateNomination.HasValue && model.DateFin.HasValue && model.DateFin.Value.Date < model.DateNomination.Value.Date)
                ModelState.AddModelError(nameof(model.DateFin), "La date de fin doit être supérieure ou égale à la date de nomination.");
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
                libelleRef = db.Nomination.AsNoTracking()
                    .Where(x => x.NominationId == model.NominationId.Value)
                    .Select(x => x.LibelleNomination)
                    .FirstOrDefault();
            }

            entity.LibelleNomination = !string.IsNullOrWhiteSpace(model.LibelleNomination)
                ? model.LibelleNomination.Trim()
                : libelleRef;
        }

        private static object Snapshot(NominationAdministrative entity) => new
        {
            entity.NominationAdministrativeId,
            entity.MedecinId,
            entity.NominationId,
            entity.TypeNominationId,
            entity.LibelleNomination,
            entity.DateNomination,
            entity.DateFin,
            entity.EstActive
        };

        private static NominationAdministrativeDetailsViewModel ToDetails(NominationAdministrative entity)
        {
            return new NominationAdministrativeDetailsViewModel
            {
                NominationAdministrativeId = entity.NominationAdministrativeId,
                MedecinId = entity.MedecinId,
                NominationId = entity.NominationId,
                TypeNominationId = entity.TypeNominationId,
                LibelleNomination = !string.IsNullOrWhiteSpace(entity.LibelleNomination)
                    ? entity.LibelleNomination
                    : entity.Nomination?.LibelleNomination,
                DateNomination = entity.DateNomination,
                DateFin = entity.DateFin,
                EstActive = entity.EstActive,
                MedecinNomComplet = entity.Medecin != null
                    ? entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom
                    : "",
                TypeNominationLibelle = entity.TypeNomination?.LibelleTypeNomination ?? ""
            };
        }

        private void PreparerRetour(int? returnMedecinId, string returnTab)
        {
            ViewBag.ReturnMedecinId = returnMedecinId;
            ViewBag.ReturnTab = string.IsNullOrWhiteSpace(returnTab) ? "nominations" : returnTab;
        }

        private ActionResult RedirigerApresAction(int? returnMedecinId, string returnTab)
        {
            if (returnMedecinId.HasValue)
            {
                return RedirectToAction("Index", "Medecins", new
                {
                    openMedecin = returnMedecinId.Value,
                    tab = string.IsNullOrWhiteSpace(returnTab) ? "nominations" : returnTab
                });
            }
            return RedirectToAction("Index");
        }
    }
}
