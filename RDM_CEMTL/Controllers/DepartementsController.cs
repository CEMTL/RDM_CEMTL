using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Models;
using Solution.ViewModels.AffectationsOrganisationnelles;
using Solution.ViewModels.Departements;

namespace Solution.Controllers
{
    [Authorize]
    public class DepartementsController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;

        public DepartementsController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }

        public ActionResult Index(string search = null)
        {
            var query = db.Departement.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x =>
                    x.NomDepartement.Contains(term) ||
                    (x.CodeDepartement != null && x.CodeDepartement.Contains(term)) ||
                    x.Services.Any(s => s.NomService.Contains(term)) ||
                    x.AffectationsOrganisationnelles.Any(a => a.NomAffiche.Contains(term)));
            }

            var items = query
                .OrderBy(x => x.NomDepartement)
                .Select(x => new DepartementListViewModel
                {
                    DepartementId = x.DepartementId,
                    NomDepartement = x.NomDepartement,
                    CodeDepartement = x.CodeDepartement,
                    EstActif = x.EstActif,
                    NombreServices = x.Services.Count(),
                    ChefDepartement = x.AffectationsOrganisationnelles
                        .Where(a => a.RoleCode == RoleAffectationOrganisationnelle.ChefDepartement &&
                                    a.RegroupementSource == null &&
                                    a.EstActive &&
                                    (!a.DateDebut.HasValue || a.DateDebut.Value <= DateTime.Today) &&
                                    (!a.DateFin.HasValue || a.DateFin.Value >= DateTime.Today))
                        .OrderByDescending(a => a.DateDebut)
                        .Select(a => a.NomAffiche)
                        .FirstOrDefault()
                })
                .ToList();

            return View(items);
        }

        public ActionResult Organisation()
        {
            var aujourdhui = DateTime.Today;
            var departements = db.Departement
                .AsNoTracking()
                .Where(x => x.EstActif)
                .Include(x => x.AffectationsOrganisationnelles)
                .Include(x => x.Services)
                    .ThenInclude(x => x.AffectationsOrganisationnelles)
                .AsSplitQuery()
                .OrderBy(x => x.NomDepartement)
                .ToList();

            var model = departements
                .Select(departement => new OrganisationDepartementViewModel
                {
                    DepartementId = departement.DepartementId,
                    NomDepartement = departement.NomDepartement,
                    PeutAjouterChefDepartement = !departement.AffectationsOrganisationnelles.Any(x =>
                        x.RoleCode == RoleAffectationOrganisationnelle.ChefDepartement && x.EstActive),
                    ChefsDepartement = ToOrganisationResponsables(
                        departement.AffectationsOrganisationnelles.Where(x => x.RegroupementSource == null),
                        RoleAffectationOrganisationnelle.ChefDepartement,
                        aujourdhui),
                    Secretaires = ToOrganisationResponsables(
                        departement.AffectationsOrganisationnelles,
                        RoleAffectationOrganisationnelle.SecretaireDepartement,
                        aujourdhui),
                    AdjointsSantaCabrini = ToOrganisationResponsables(
                        departement.AffectationsOrganisationnelles.Where(x =>
                            !string.IsNullOrWhiteSpace(x.SiteNomSource) &&
                            x.SiteNomSource.Contains("Santa Cabrini", StringComparison.OrdinalIgnoreCase)),
                        RoleAffectationOrganisationnelle.AdjointSite,
                        aujourdhui),
                    Services = departement.Services
                        .Where(service => service.EstActif)
                        .OrderBy(service => service.NomService)
                        .Select(service => new OrganisationServiceViewModel
                        {
                            ServiceId = service.ServiceId,
                            NomService = service.NomService,
                            RegroupementSource = service.RegroupementSource,
                            PeutAjouterChefService = !service.AffectationsOrganisationnelles.Any(x =>
                                x.RoleCode == RoleAffectationOrganisationnelle.ChefService && x.EstActive),
                            ChefsService = ToOrganisationResponsables(
                                service.AffectationsOrganisationnelles,
                                RoleAffectationOrganisationnelle.ChefService,
                                aujourdhui)
                        })
                        .ToList()
                })
                .ToList();

            return View(model);
        }

        public ActionResult Details(int? id)
        {
            if (!id.HasValue) return BadRequest();

            var entity = db.Departement
                .AsNoTracking()
                .Include(x => x.Services).ThenInclude(x => x.AffectationsOrganisationnelles)
                .Include(x => x.AffectationsOrganisationnelles).ThenInclude(x => x.Medecin)
                .Include(x => x.AffectationsOrganisationnelles).ThenInclude(x => x.Installation)
                .FirstOrDefault(x => x.DepartementId == id.Value);
            if (entity == null) return NotFound();

            return View(ToDetails(entity));
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            return View(new DepartementFormViewModel { EstActif = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(DepartementFormViewModel model)
        {
            Normalize(model);
            ValidateUniqueName(model);
            if (!ModelState.IsValid) return View(model);

            var entity = new Departement();
            MapToEntity(model, entity);
            db.Departement.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "Département créé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue) return BadRequest();

            var entity = db.Departement.AsNoTracking().FirstOrDefault(x => x.DepartementId == id.Value);
            if (entity == null) return NotFound();

            return View(new DepartementFormViewModel
            {
                DepartementId = entity.DepartementId,
                NomDepartement = entity.NomDepartement,
                CodeDepartement = entity.CodeDepartement,
                EstActif = entity.EstActif
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(DepartementFormViewModel model)
        {
            Normalize(model);
            ValidateUniqueName(model);
            if (!ModelState.IsValid) return View(model);

            var entity = db.Departement.FirstOrDefault(x => x.DepartementId == model.DepartementId);
            if (entity == null) return NotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "Département modifié avec succès.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue) return BadRequest();

            var entity = db.Departement
                .AsNoTracking()
                .Include(x => x.Services).ThenInclude(x => x.AffectationsOrganisationnelles)
                .Include(x => x.AffectationsOrganisationnelles).ThenInclude(x => x.Medecin)
                .Include(x => x.AffectationsOrganisationnelles).ThenInclude(x => x.Installation)
                .FirstOrDefault(x => x.DepartementId == id.Value);
            if (entity == null) return NotFound();

            return View(ToDetails(entity));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.Departement
                .Include(x => x.Services)
                .Include(x => x.AffectationsOrganisationnelles)
                .FirstOrDefault(x => x.DepartementId == id);
            if (entity == null) return NotFound();

            if (entity.Services.Any() || entity.AffectationsOrganisationnelles.Any())
            {
                TempData["Error"] = "Ce département ne peut pas être supprimé tant que des services ou des affectations lui sont rattachés. Désactivez-le au besoin.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            db.Departement.Remove(entity);
            db.SaveChanges();
            TempData["Success"] = "Département supprimé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        private void ValidateUniqueName(DepartementFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NomDepartement)) return;

            if (db.Departement.Any(x =>
                x.DepartementId != model.DepartementId &&
                x.NomDepartement == model.NomDepartement))
            {
                ModelState.AddModelError(nameof(model.NomDepartement), "Un département portant ce nom existe déjà.");
            }
        }

        private static void Normalize(DepartementFormViewModel model)
        {
            model.NomDepartement = model.NomDepartement?.Trim();
            model.CodeDepartement = string.IsNullOrWhiteSpace(model.CodeDepartement) ? null : model.CodeDepartement.Trim();
        }

        private static void MapToEntity(DepartementFormViewModel model, Departement entity)
        {
            entity.NomDepartement = model.NomDepartement;
            entity.CodeDepartement = model.CodeDepartement;
            entity.EstActif = model.EstActif;
        }

        private static DepartementDetailsViewModel ToDetails(Departement entity)
        {
            return new DepartementDetailsViewModel
            {
                DepartementId = entity.DepartementId,
                NomDepartement = entity.NomDepartement,
                CodeDepartement = entity.CodeDepartement,
                EstActif = entity.EstActif,
                NombreServices = entity.Services.Count,
                ChefDepartement = entity.AffectationsOrganisationnelles
                    .Where(x => x.RoleCode == RoleAffectationOrganisationnelle.ChefDepartement &&
                                x.RegroupementSource == null && x.EstActive &&
                                (!x.DateDebut.HasValue || x.DateDebut.Value.Date <= DateTime.Today) &&
                                (!x.DateFin.HasValue || x.DateFin.Value.Date >= DateTime.Today))
                    .OrderByDescending(x => x.DateDebut)
                    .Select(x => x.NomAffiche)
                    .FirstOrDefault(),
                NombreAValider = entity.AffectationsOrganisationnelles.Count(x => x.EstActive && x.EstAValider),
                Services = entity.Services
                    .OrderBy(x => x.NomService)
                    .Select(x => new DepartementServiceItemViewModel
                    {
                        ServiceId = x.ServiceId,
                        NomService = x.NomService,
                        CodeService = x.CodeService,
                        RegroupementSource = x.RegroupementSource,
                        ChefService = x.AffectationsOrganisationnelles
                            .Where(a => a.RoleCode == RoleAffectationOrganisationnelle.ChefService && a.EstActive &&
                                        (!a.DateDebut.HasValue || a.DateDebut.Value.Date <= DateTime.Today) &&
                                        (!a.DateFin.HasValue || a.DateFin.Value.Date >= DateTime.Today))
                            .OrderByDescending(a => a.DateDebut)
                            .Select(a => a.NomAffiche)
                            .FirstOrDefault(),
                        EstActif = x.EstActif
                    })
                    .ToList(),
                Affectations = entity.AffectationsOrganisationnelles
                    .OrderByDescending(x => x.EstActive)
                    .ThenBy(x => x.RoleCode)
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

        private static List<OrganisationResponsableViewModel> ToOrganisationResponsables(
            IEnumerable<AffectationOrganisationnelle> affectations,
            string roleCode,
            DateTime dateReference)
        {
            return affectations
                .Where(x =>
                    x.RoleCode == roleCode &&
                    x.EstActive &&
                    (!x.DateDebut.HasValue || x.DateDebut.Value.Date <= dateReference) &&
                    (!x.DateFin.HasValue || x.DateFin.Value.Date >= dateReference))
                .OrderByDescending(x => x.DateDebut)
                .ThenBy(x => x.NomAffiche)
                .Select(x => new OrganisationResponsableViewModel
                {
                    AffectationOrganisationnelleId = x.AffectationOrganisationnelleId,
                    NomAffiche = x.NomAffiche,
                    Titre = x.Titre,
                    Courriel = string.IsNullOrWhiteSpace(x.Courriel) ? x.CourrielSource : x.Courriel,
                    DateDebut = x.DateDebut,
                    EstInterim = x.EstInterim,
                    EstAValider = x.EstAValider
                })
                .ToList();
        }
    }
}
