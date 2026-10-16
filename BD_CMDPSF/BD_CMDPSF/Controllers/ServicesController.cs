using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Solution.Models;
using Solution.ViewModels.AffectationsOrganisationnelles;
using Solution.ViewModels.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class ServicesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;

        public ServicesController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }

        public ActionResult Index(string search = null, int? departementId = null)
        {
            var query = db.Service.AsNoTracking().AsQueryable();
            if (departementId.HasValue)
                query = query.Where(x => x.DepartementId == departementId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x =>
                    x.NomService.Contains(term) ||
                    (x.CodeService != null && x.CodeService.Contains(term)) ||
                    x.AffectationsOrganisationnelles.Any(a => a.NomAffiche.Contains(term)) ||
                    (x.Departement != null && x.Departement.NomDepartement.Contains(term)));
            }

            var items = query
                .OrderBy(x => x.Departement != null ? x.Departement.NomDepartement : "")
                .ThenBy(x => x.NomService)
                .Select(x => new ServiceListViewModel
                {
                    ServiceId = x.ServiceId,
                    NomService = x.NomService,
                    CodeService = x.CodeService,
                    NomDepartement = x.Departement != null ? x.Departement.NomDepartement : "Sans département",
                    EstActif = x.EstActif,
                    ChefService = x.AffectationsOrganisationnelles
                        .Where(a => a.RoleCode == RoleAffectationOrganisationnelle.ChefService && a.EstActive &&
                                    (!a.DateDebut.HasValue || a.DateDebut.Value <= DateTime.Today) &&
                                    (!a.DateFin.HasValue || a.DateFin.Value >= DateTime.Today))
                        .OrderByDescending(a => a.DateDebut)
                        .Select(a => a.NomAffiche)
                        .FirstOrDefault(),
                    ChefServiceEstAValider = x.AffectationsOrganisationnelles
                        .Any(a => a.RoleCode == RoleAffectationOrganisationnelle.ChefService && a.EstActive && a.EstAValider)
                })
                .ToList();

            ViewBag.DepartementId = departementId;
            ViewBag.Departements = BuildDepartementOptions(departementId);
            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (!id.HasValue) return BadRequest();
            var entity = db.Service
                .AsNoTracking()
                .Include(x => x.Departement)
                .Include(x => x.InstallationServices).ThenInclude(x => x.Installation)
                .Include(x => x.MedecinPrivileges)
                .Include(x => x.AffectationsOrganisationnelles).ThenInclude(x => x.Medecin)
                .Include(x => x.AffectationsOrganisationnelles).ThenInclude(x => x.Installation)
                .FirstOrDefault(x => x.ServiceId == id.Value);
            if (entity == null) return NotFound();

            return View(ToDetails(entity));
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(int? departementId = null)
        {
            return View(BuildForm(new ServiceFormViewModel
            {
                DepartementId = departementId,
                EstActif = true
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(ServiceFormViewModel model)
        {
            Normalize(model);
            Validate(model);
            if (!ModelState.IsValid) return View(BuildForm(model));

            var entity = new Service();
            MapToEntity(model, entity);
            db.Service.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "Service créé avec succès.";
            return RedirectToAction(nameof(Index), new { departementId = entity.DepartementId });
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue) return BadRequest();
            var entity = db.Service.AsNoTracking().FirstOrDefault(x => x.ServiceId == id.Value);
            if (entity == null) return NotFound();

            return View(BuildForm(new ServiceFormViewModel
            {
                ServiceId = entity.ServiceId,
                DepartementId = entity.DepartementId,
                NomService = entity.NomService,
                CodeService = entity.CodeService,
                RegroupementSource = entity.RegroupementSource,
                EstActif = entity.EstActif
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(ServiceFormViewModel model)
        {
            Normalize(model);
            Validate(model);
            if (!ModelState.IsValid) return View(BuildForm(model));

            var entity = db.Service.FirstOrDefault(x => x.ServiceId == model.ServiceId);
            if (entity == null) return NotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "Service modifié avec succès.";
            return RedirectToAction(nameof(Index), new { departementId = entity.DepartementId });
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue) return BadRequest();
            var entity = db.Service
                .AsNoTracking()
                .Include(x => x.Departement)
                .Include(x => x.InstallationServices).ThenInclude(x => x.Installation)
                .Include(x => x.MedecinPrivileges)
                .Include(x => x.AffectationsOrganisationnelles).ThenInclude(x => x.Medecin)
                .Include(x => x.AffectationsOrganisationnelles).ThenInclude(x => x.Installation)
                .FirstOrDefault(x => x.ServiceId == id.Value);
            if (entity == null) return NotFound();

            return View(ToDetails(entity));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.Service
                .Include(x => x.InstallationServices)
                .Include(x => x.MedecinPrivileges)
                .Include(x => x.AffectationsOrganisationnelles)
                .FirstOrDefault(x => x.ServiceId == id);
            if (entity == null) return NotFound();

            if (entity.InstallationServices.Any() || entity.MedecinPrivileges.Any() || entity.AffectationsOrganisationnelles.Any())
            {
                TempData["Error"] = "Ce service est utilisé par une installation, un privilège médical ou une affectation organisationnelle. Désactivez-le plutôt que de le supprimer.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            var departementId = entity.DepartementId;
            db.Service.Remove(entity);
            db.SaveChanges();
            TempData["Success"] = "Service supprimé avec succès.";
            return RedirectToAction(nameof(Index), new { departementId });
        }

        private ServiceFormViewModel BuildForm(ServiceFormViewModel model)
        {
            model.DepartementOptions = BuildDepartementOptions(model.DepartementId);
            return model;
        }

        private IList<SelectListItem> BuildDepartementOptions(int? selectedId)
        {
            return db.Departement
                .AsNoTracking()
                .Where(x => x.EstActif || (selectedId.HasValue && x.DepartementId == selectedId.Value))
                .OrderBy(x => x.NomDepartement)
                .Select(x => new SelectListItem
                {
                    Value = x.DepartementId.ToString(),
                    Text = x.NomDepartement,
                    Selected = selectedId.HasValue && x.DepartementId == selectedId.Value
                })
                .ToList();
        }

        private void Validate(ServiceFormViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.NomService) && db.Service.Any(x =>
                x.ServiceId != model.ServiceId && x.NomService == model.NomService))
            {
                ModelState.AddModelError(nameof(model.NomService), "Un service portant ce nom existe déjà.");
            }

            if (model.DepartementId.HasValue && !db.Departement.Any(x => x.DepartementId == model.DepartementId.Value))
            {
                ModelState.AddModelError(nameof(model.DepartementId), "Le département sélectionné n'existe plus.");
            }
        }

        private static void Normalize(ServiceFormViewModel model)
        {
            model.NomService = model.NomService?.Trim();
            model.CodeService = string.IsNullOrWhiteSpace(model.CodeService) ? null : model.CodeService.Trim();
            model.RegroupementSource = string.IsNullOrWhiteSpace(model.RegroupementSource) ? null : model.RegroupementSource.Trim();
        }

        private static void MapToEntity(ServiceFormViewModel model, Service entity)
        {
            entity.DepartementId = model.DepartementId;
            entity.NomService = model.NomService;
            entity.CodeService = model.CodeService;
            entity.RegroupementSource = model.RegroupementSource;
            entity.EstActif = model.EstActif;
        }

        private static ServiceDetailsViewModel ToDetails(Service entity)
        {
            return new ServiceDetailsViewModel
            {
                ServiceId = entity.ServiceId,
                DepartementId = entity.DepartementId,
                NomService = entity.NomService,
                CodeService = entity.CodeService,
                RegroupementSource = entity.RegroupementSource,
                NomDepartement = entity.Departement?.NomDepartement ?? "Sans département",
                EstActif = entity.EstActif,
                NombrePrivileges = entity.MedecinPrivileges.Count,
                ChefService = entity.AffectationsOrganisationnelles
                    .Where(x => x.RoleCode == RoleAffectationOrganisationnelle.ChefService && x.EstActive &&
                                (!x.DateDebut.HasValue || x.DateDebut.Value.Date <= DateTime.Today) &&
                                (!x.DateFin.HasValue || x.DateFin.Value.Date >= DateTime.Today))
                    .OrderByDescending(x => x.DateDebut)
                    .Select(x => x.NomAffiche)
                    .FirstOrDefault(),
                ChefServiceEstAValider = entity.AffectationsOrganisationnelles
                    .Any(x => x.RoleCode == RoleAffectationOrganisationnelle.ChefService && x.EstActive && x.EstAValider),
                Installations = entity.InstallationServices
                    .OrderByDescending(x => x.EstActif)
                    .ThenBy(x => x.Installation != null ? x.Installation.NomInstallation : "")
                    .Select(x => new ServiceInstallationItemViewModel
                    {
                        NomInstallation = x.Installation?.NomInstallation ?? "",
                        DateDebut = x.DateDebut,
                        DateFin = x.DateFin,
                        EstActif = x.EstActif
                    })
                    .ToList(),
                Affectations = entity.AffectationsOrganisationnelles
                    .OrderByDescending(x => x.EstActive)
                    .ThenByDescending(x => x.DateDebut)
                    .Select(ToResume)
                    .ToList()
            };
        }

        private static AffectationOrganisationnelleResumeViewModel ToResume(AffectationOrganisationnelle entity)
        {
            return new AffectationOrganisationnelleResumeViewModel
            {
                AffectationOrganisationnelleId = entity.AffectationOrganisationnelleId,
                RoleLibelle = RoleAffectationOrganisationnelle.Libelle(entity.RoleCode),
                NomAffiche = entity.NomAffiche,
                NomMedecin = entity.Medecin == null ? null : entity.Medecin.Nom + ", " + entity.Medecin.Prenom,
                Titre = entity.Titre,
                Courriel = entity.Courriel,
                Site = entity.Installation?.NomInstallation ?? entity.SiteNomSource,
                RegroupementSource = entity.RegroupementSource,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                EstInterim = entity.EstInterim,
                EstActive = entity.EstActive,
                EstAValider = entity.EstAValider
            };
        }
    }
}
