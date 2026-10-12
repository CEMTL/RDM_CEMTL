using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Solution.Models;
using Solution.Services;
using Solution.ViewModels.MedecinAffectationsCliniques;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinAffectationsCliniquesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;
        private AuditService Audit => new AuditService(db, HttpContext);

        public MedecinAffectationsCliniquesController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }

        public ActionResult Index(string search = null, int? departementId = null, bool actifsSeulement = true)
        {
            var query = db.MedecinAffectationClinique
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.Departement)
                .Include(x => x.Service)
                .Include(x => x.Installation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(x =>
                    x.Medecin.Permis.Contains(s) ||
                    x.Medecin.Nom.Contains(s) ||
                    x.Medecin.Prenom.Contains(s) ||
                    x.Departement.NomDepartement.Contains(s) ||
                    x.Service.NomService.Contains(s) ||
                    (x.Installation != null && x.Installation.NomInstallation.Contains(s)) ||
                    (x.TypeAffectation != null && x.TypeAffectation.Contains(s)));
            }

            if (departementId.HasValue)
                query = query.Where(x => x.DepartementId == departementId.Value);

            if (actifsSeulement)
                query = query.Where(x => x.EstActive);

            var items = query
                .OrderBy(x => x.Medecin.Nom)
                .ThenBy(x => x.Medecin.Prenom)
                .ThenByDescending(x => x.EstPrincipale)
                .ThenBy(x => x.Departement.NomDepartement)
                .ThenBy(x => x.Service.NomService)
                .Select(x => new MedecinAffectationCliniqueListViewModel
                {
                    MedecinAffectationCliniqueId = x.MedecinAffectationCliniqueId,
                    MedecinId = x.MedecinId,
                    Medecin = x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom,
                    Departement = x.Departement.NomDepartement,
                    Service = x.Service.NomService,
                    Installation = x.Installation != null ? x.Installation.NomInstallation : "",
                    TypeAffectation = x.TypeAffectation,
                    EstPrincipale = x.EstPrincipale,
                    DateDebut = x.DateDebut,
                    DateFin = x.DateFin,
                    EstActive = x.EstActive
                })
                .ToList();

            ViewBag.Search = search;
            ViewBag.ActifsSeulement = actifsSeulement;
            ViewBag.DepartementId = new SelectList(
                db.Departement.AsNoTracking().OrderBy(x => x.NomDepartement),
                "DepartementId", "NomDepartement", departementId);
            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("SuperUtilisateur");

            return View(items);
        }

        public ActionResult Details(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            if (!id.HasValue) return BadRequest();

            var entity = db.MedecinAffectationClinique
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.Departement)
                .Include(x => x.Service)
                .Include(x => x.Installation)
                .FirstOrDefault(x => x.MedecinAffectationCliniqueId == id.Value);

            if (entity == null) return NotFound();

            ViewBag.ReturnMedecinId = returnMedecinId;
            ViewBag.ReturnTab = returnTab;
            return View(entity);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(int? medecinId = null, int? departementId = null, int? serviceId = null, int? returnMedecinId = null, string returnTab = "organisation")
        {
            var model = new MedecinAffectationCliniqueFormViewModel
            {
                MedecinId = medecinId ?? 0,
                DepartementId = departementId ?? 0,
                ServiceId = serviceId ?? 0,
                TypeAffectation = "Clinique",
                EstActive = true,
                EstPrincipale = false,
                DateDebut = DateTime.Today,
                ReturnMedecinId = returnMedecinId ?? medecinId,
                ReturnTab = returnTab
            };

            return View(BuildForm(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(MedecinAffectationCliniqueFormViewModel model)
        {
            Validate(model, null);

            if (!ModelState.IsValid)
                return View(BuildForm(model));

            var entity = new MedecinAffectationClinique
            {
                DateCreation = DateTime.Now,
                DateModification = DateTime.Now,
                SourceDonnees = "RDM"
            };

            Map(model, entity);
            db.MedecinAffectationClinique.Add(entity);

            try
            {
                db.SaveChanges();
                Audit.Enregistrer("MedecinAffectationClinique", entity.MedecinAffectationCliniqueId, "INSERT", null, Snapshot(entity));
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", UserFriendlyExceptionService.GetMessage(ex, "enregistrer l'affectation clinique"));
                return View(BuildForm(model));
            }

            TempData["Success"] = "Affectation clinique créée avec succès.";
            return RedirectAfterSave(model.ReturnMedecinId, model.ReturnTab);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id, int? returnMedecinId = null, string returnTab = "organisation")
        {
            if (!id.HasValue) return BadRequest();

            var entity = db.MedecinAffectationClinique
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinAffectationCliniqueId == id.Value);

            if (entity == null) return NotFound();

            var model = new MedecinAffectationCliniqueFormViewModel
            {
                MedecinAffectationCliniqueId = entity.MedecinAffectationCliniqueId,
                MedecinId = entity.MedecinId,
                DepartementId = entity.DepartementId,
                ServiceId = entity.ServiceId,
                InstallationId = entity.InstallationId,
                TypeAffectation = entity.TypeAffectation,
                EstPrincipale = entity.EstPrincipale,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                EstActive = entity.EstActive,
                Commentaire = entity.Commentaire,
                ReturnMedecinId = returnMedecinId ?? entity.MedecinId,
                ReturnTab = returnTab
            };

            return View(BuildForm(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(MedecinAffectationCliniqueFormViewModel model)
        {
            Validate(model, model.MedecinAffectationCliniqueId);

            if (!ModelState.IsValid)
                return View(BuildForm(model));

            var entity = db.MedecinAffectationClinique
                .FirstOrDefault(x => x.MedecinAffectationCliniqueId == model.MedecinAffectationCliniqueId);

            if (entity == null) return NotFound();

            var old = Snapshot(entity);
            Map(model, entity);
            entity.DateModification = DateTime.Now;

            Audit.Enregistrer(
                "MedecinAffectationClinique",
                entity.MedecinAffectationCliniqueId,
                "UPDATE",
                old,
                Snapshot(entity));

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", UserFriendlyExceptionService.GetMessage(ex, "modifier l'affectation clinique"));
                return View(BuildForm(model));
            }

            TempData["Success"] = "Affectation clinique modifiée avec succès.";
            return RedirectAfterSave(model.ReturnMedecinId, model.ReturnTab);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id, int? returnMedecinId = null, string returnTab = "organisation")
        {
            if (!id.HasValue) return BadRequest();

            var entity = db.MedecinAffectationClinique
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.Departement)
                .Include(x => x.Service)
                .Include(x => x.Installation)
                .FirstOrDefault(x => x.MedecinAffectationCliniqueId == id.Value);

            if (entity == null) return NotFound();

            ViewBag.ReturnMedecinId = returnMedecinId ?? entity.MedecinId;
            ViewBag.ReturnTab = returnTab;
            return View(entity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id, int? returnMedecinId = null, string returnTab = "organisation")
        {
            var entity = db.MedecinAffectationClinique
                .FirstOrDefault(x => x.MedecinAffectationCliniqueId == id);

            if (entity == null) return NotFound();

            var medecinId = returnMedecinId ?? entity.MedecinId;
            var old = Snapshot(entity);

            // Les affectations cliniques sont historisées : on termine l'affectation
            // au lieu de supprimer physiquement l'enregistrement.
            entity.EstActive = false;
            entity.EstPrincipale = false;
            entity.DateFin ??= DateTime.Today;
            entity.DateModification = DateTime.Now;

            Audit.Enregistrer(
                "MedecinAffectationClinique",
                entity.MedecinAffectationCliniqueId,
                "UPDATE",
                old,
                Snapshot(entity));

            db.SaveChanges();

            TempData["Success"] = "Affectation clinique terminée avec succès.";
            return RedirectAfterSave(medecinId, returnTab);
        }

        private MedecinAffectationCliniqueFormViewModel BuildForm(MedecinAffectationCliniqueFormViewModel model)
        {
            var medecinIds = model.MedecinId > 0
                ? new[] { model.MedecinId }
                : Array.Empty<int>();

            model.Medecins = db.Medecin.AsNoTracking()
                .Where(x => x.EstActif || medecinIds.Contains(x.MedecinId))
                .OrderBy(x => x.Nom).ThenBy(x => x.Prenom)
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = x.Permis + " - " + x.Nom + " " + x.Prenom
                }).ToList();

            model.Departements = db.Departement.AsNoTracking()
                .Where(x => x.EstActif || x.DepartementId == model.DepartementId)
                .OrderBy(x => x.NomDepartement)
                .Select(x => new SelectListItem
                {
                    Value = x.DepartementId.ToString(),
                    Text = x.NomDepartement
                }).ToList();

            model.ServiceOptions = db.Service.AsNoTracking()
                .Where(x => x.EstActif || x.ServiceId == model.ServiceId)
                .OrderBy(x => x.Departement.NomDepartement)
                .ThenBy(x => x.NomService)
                .Select(x => new ServiceOptionViewModel
                {
                    ServiceId = x.ServiceId,
                    DepartementId = x.DepartementId,
                    Libelle = x.NomService,
                    EstActif = x.EstActif
                }).ToList();

            model.Services = model.ServiceOptions.Select(x => new SelectListItem
            {
                Value = x.ServiceId.ToString(),
                Text = x.Libelle,
                Selected = x.ServiceId == model.ServiceId
            }).ToList();

            model.Installations = db.Installation.AsNoTracking()
                .Where(x => x.EstActif || x.InstallationId == model.InstallationId)
                .OrderBy(x => x.NomInstallation)
                .Select(x => new SelectListItem
                {
                    Value = x.InstallationId.ToString(),
                    Text = x.NomInstallation
                }).ToList();

            return model;
        }

        private void Validate(MedecinAffectationCliniqueFormViewModel model, int? currentId)
        {
            if (model.DateFin.HasValue && model.DateDebut.HasValue && model.DateFin.Value.Date < model.DateDebut.Value.Date)
                ModelState.AddModelError(nameof(model.DateFin), "La date de fin ne peut pas précéder la date de début.");

            var service = db.Service.AsNoTracking().FirstOrDefault(x => x.ServiceId == model.ServiceId);
            if (service == null)
                ModelState.AddModelError(nameof(model.ServiceId), "Le service sélectionné est introuvable.");
            else if (service.DepartementId != model.DepartementId)
                ModelState.AddModelError(nameof(model.ServiceId), "Le service sélectionné n'appartient pas au département choisi.");

            if (model.EstPrincipale && model.EstActive && model.MedecinId > 0)
            {
                var existePrincipale = db.MedecinAffectationClinique.Any(x =>
                    x.MedecinId == model.MedecinId &&
                    x.EstPrincipale &&
                    x.EstActive &&
                    (!currentId.HasValue || x.MedecinAffectationCliniqueId != currentId.Value));

                if (existePrincipale)
                    ModelState.AddModelError(nameof(model.EstPrincipale), "Ce médecin possède déjà une affectation clinique principale active.");
            }
        }

        private static void Map(MedecinAffectationCliniqueFormViewModel model, MedecinAffectationClinique entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.DepartementId = model.DepartementId;
            entity.ServiceId = model.ServiceId;
            entity.InstallationId = model.InstallationId;
            entity.TypeAffectation = string.IsNullOrWhiteSpace(model.TypeAffectation) ? "Clinique" : model.TypeAffectation.Trim();
            entity.EstPrincipale = model.EstPrincipale;
            entity.DateDebut = model.DateDebut;
            entity.DateFin = model.DateFin;
            entity.EstActive = model.EstActive;
            entity.Commentaire = string.IsNullOrWhiteSpace(model.Commentaire) ? null : model.Commentaire.Trim();
        }

        private static object Snapshot(MedecinAffectationClinique x) => new
        {
            x.MedecinAffectationCliniqueId,
            x.MedecinId,
            x.DepartementId,
            x.ServiceId,
            x.InstallationId,
            x.TypeAffectation,
            x.EstPrincipale,
            x.DateDebut,
            x.DateFin,
            x.EstActive,
            x.Commentaire
        };

        private ActionResult RedirectAfterSave(int? medecinId, string tab)
        {
            if (medecinId.HasValue && medecinId.Value > 0)
                return RedirectToAction("Index", "Medecins", new
                {
                    openMedecin = medecinId.Value,
                    tab = string.IsNullOrWhiteSpace(tab) ? "organisation" : tab
                });

            return RedirectToAction("Index");
        }
    }
}
