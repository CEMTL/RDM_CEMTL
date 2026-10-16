using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Solution.Models;
using Solution.ViewModels.AffectationsOrganisationnelles;

namespace Solution.Controllers
{
    [Authorize]
    public class AffectationsOrganisationnellesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;

        public AffectationsOrganisationnellesController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }

        public ActionResult Index(
            string search = null,
            string roleCode = null,
            bool seulementAValider = false,
            bool inclureInactives = false)
        {
            var query = db.AffectationOrganisationnelle
                .AsNoTracking()
                .Include(x => x.Departement)
                .Include(x => x.Service).ThenInclude(x => x.Departement)
                .Include(x => x.Medecin)
                .Include(x => x.Installation)
                .AsQueryable();

            if (!inclureInactives)
                query = query.Where(x => x.EstActive);

            if (seulementAValider)
                query = query.Where(x => x.EstAValider);

            if (!string.IsNullOrWhiteSpace(roleCode))
                query = query.Where(x => x.RoleCode == roleCode);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x =>
                    x.NomAffiche.Contains(term) ||
                    (x.Titre != null && x.Titre.Contains(term)) ||
                    (x.Departement != null && x.Departement.NomDepartement.Contains(term)) ||
                    (x.Service != null && x.Service.NomService.Contains(term)) ||
                    (x.RegroupementSource != null && x.RegroupementSource.Contains(term)) ||
                    (x.Medecin != null && (x.Medecin.Nom.Contains(term) || x.Medecin.Prenom.Contains(term))));
            }

            var entities = query
                .OrderByDescending(x => x.EstAValider)
                .ThenBy(x => x.RoleCode)
                .ThenBy(x => x.Departement != null ? x.Departement.NomDepartement : x.Service.Departement.NomDepartement)
                .ThenBy(x => x.Service != null ? x.Service.NomService : "")
                .ThenBy(x => x.NomAffiche)
                .ToList();

            ViewBag.RoleCode = roleCode;
            ViewBag.RoleOptions = BuildRoleOptions(roleCode);
            ViewBag.SeulementAValider = seulementAValider;
            ViewBag.InclureInactives = inclureInactives;

            return View(entities.Select(ToListItem).ToList());
        }

        public ActionResult Details(int? id)
        {
            if (!id.HasValue) return BadRequest();

            var entity = FindEntity(id.Value, true);
            if (entity == null) return NotFound();

            return View(ToDetails(entity));
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(
            int? departementId = null,
            int? serviceId = null,
            int? medecinId = null,
            string roleCode = null)
        {
            roleCode = roleCode?.Trim().ToUpperInvariant();
            var rolePreselectionne = serviceId.HasValue
                ? RoleAffectationOrganisationnelle.ChefService
                : RoleAffectationOrganisationnelle.Tous.Contains(roleCode) &&
                  roleCode != RoleAffectationOrganisationnelle.ChefService
                    ? roleCode
                    : RoleAffectationOrganisationnelle.ChefDepartement;

            var model = new AffectationOrganisationnelleFormViewModel
            {
                DepartementId = serviceId.HasValue ? null : departementId,
                ServiceId = serviceId,
                MedecinId = medecinId,
                RoleCode = rolePreselectionne,
                EstActive = true
            };

            return View(BuildForm(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(AffectationOrganisationnelleFormViewModel model)
        {
            Normalize(model);
            Validate(model, null);
            if (!ModelState.IsValid) return View(BuildForm(model));

            var entity = new AffectationOrganisationnelle
            {
                DateCreation = DateTime.UtcNow,
                DateModification = DateTime.UtcNow
            };
            MapToEntity(model, entity);
            db.AffectationOrganisationnelle.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "Affectation organisationnelle créée avec succès.";
            return RedirectToAction(nameof(Details), new { id = entity.AffectationOrganisationnelleId });
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue) return BadRequest();

            var entity = FindEntity(id.Value, true);
            if (entity == null) return NotFound();

            return View(BuildForm(ToForm(entity)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(AffectationOrganisationnelleFormViewModel model)
        {
            var entity = db.AffectationOrganisationnelle
                .FirstOrDefault(x => x.AffectationOrganisationnelleId == model.AffectationOrganisationnelleId);
            if (entity == null) return NotFound();

            Normalize(model);
            Validate(model, entity);
            model.SourceDonnees = entity.SourceDonnees;
            model.SourceLigne = entity.SourceLigne;
            model.CourrielSource = entity.CourrielSource;
            if (!ModelState.IsValid) return View(BuildForm(model));

            MapToEntity(model, entity);
            entity.DateModification = DateTime.UtcNow;
            db.SaveChanges();

            TempData["Success"] = "Affectation organisationnelle modifiée avec succès.";
            return RedirectToAction(nameof(Details), new { id = entity.AffectationOrganisationnelleId });
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue) return BadRequest();

            var entity = FindEntity(id.Value, true);
            if (entity == null) return NotFound();

            return View(ToDetails(entity));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.AffectationOrganisationnelle
                .FirstOrDefault(x => x.AffectationOrganisationnelleId == id);
            if (entity == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(entity.SourceDonnees))
            {
                TempData["Error"] = "Une affectation provenant d'une source importée ne peut pas être supprimée. Désactivez-la pour préserver la traçabilité.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            db.AffectationOrganisationnelle.Remove(entity);
            db.SaveChanges();
            TempData["Success"] = "Affectation organisationnelle supprimée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        private AffectationOrganisationnelle FindEntity(int id, bool asNoTracking)
        {
            var query = db.AffectationOrganisationnelle
                .Include(x => x.Departement)
                .Include(x => x.Service).ThenInclude(x => x.Departement)
                .Include(x => x.Medecin)
                .Include(x => x.Installation)
                .AsQueryable();

            if (asNoTracking) query = query.AsNoTracking();
            return query.FirstOrDefault(x => x.AffectationOrganisationnelleId == id);
        }

        private AffectationOrganisationnelleFormViewModel BuildForm(AffectationOrganisationnelleFormViewModel model)
        {
            model.RoleOptions = BuildRoleOptions(model.RoleCode);
            model.DepartementOptions = db.Departement
                .AsNoTracking()
                .Where(x => x.EstActif || (model.DepartementId.HasValue && x.DepartementId == model.DepartementId.Value))
                .OrderBy(x => x.NomDepartement)
                .Select(x => new SelectListItem
                {
                    Value = x.DepartementId.ToString(),
                    Text = x.NomDepartement,
                    Selected = model.DepartementId.HasValue && x.DepartementId == model.DepartementId.Value
                })
                .ToList();

            model.ServiceOptions = db.Service
                .AsNoTracking()
                .Where(x => x.EstActif || (model.ServiceId.HasValue && x.ServiceId == model.ServiceId.Value))
                .OrderBy(x => x.Departement != null ? x.Departement.NomDepartement : "")
                .ThenBy(x => x.RegroupementSource)
                .ThenBy(x => x.NomService)
                .Select(x => new
                {
                    x.ServiceId,
                    x.NomService,
                    x.RegroupementSource,
                    Departement = x.Departement != null ? x.Departement.NomDepartement : "Sans département"
                })
                .AsEnumerable()
                .Select(x => new SelectListItem
                {
                    Value = x.ServiceId.ToString(),
                    Text = x.Departement + " — " +
                        (string.IsNullOrWhiteSpace(x.RegroupementSource) ? "" : x.RegroupementSource + " — ") +
                        x.NomService,
                    Selected = model.ServiceId.HasValue && x.ServiceId == model.ServiceId.Value
                })
                .ToList();

            model.MedecinOptions = db.Medecin
                .AsNoTracking()
                .Where(x => x.EstActif || (model.MedecinId.HasValue && x.MedecinId == model.MedecinId.Value))
                .OrderBy(x => x.Nom)
                .ThenBy(x => x.Prenom)
                .Select(x => new { x.MedecinId, x.Nom, x.Prenom, x.Permis })
                .AsEnumerable()
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = x.Nom + ", " + x.Prenom +
                        (string.IsNullOrWhiteSpace(x.Permis) ? "" : " (" + x.Permis + ")"),
                    Selected = model.MedecinId.HasValue && x.MedecinId == model.MedecinId.Value
                })
                .ToList();

            model.InstallationOptions = db.Installation
                .AsNoTracking()
                .Where(x => x.EstActif || (model.InstallationId.HasValue && x.InstallationId == model.InstallationId.Value))
                .OrderBy(x => x.NomInstallation)
                .Select(x => new SelectListItem
                {
                    Value = x.InstallationId.ToString(),
                    Text = x.NomInstallation,
                    Selected = model.InstallationId.HasValue && x.InstallationId == model.InstallationId.Value
                })
                .ToList();

            return model;
        }

        private static IList<SelectListItem> BuildRoleOptions(string selectedRole)
        {
            return RoleAffectationOrganisationnelle.Tous
                .Select(x => new SelectListItem
                {
                    Value = x,
                    Text = RoleAffectationOrganisationnelle.Libelle(x),
                    Selected = x == selectedRole
                })
                .ToList();
        }

        private void Validate(
            AffectationOrganisationnelleFormViewModel model,
            AffectationOrganisationnelle entityOriginale)
        {
            if (!RoleAffectationOrganisationnelle.Tous.Contains(model.RoleCode))
            {
                ModelState.AddModelError(nameof(model.RoleCode), "Le rôle sélectionné n'est pas reconnu.");
                return;
            }

            if (model.RoleCode == RoleAffectationOrganisationnelle.ChefService)
            {
                if (!model.ServiceId.HasValue)
                    ModelState.AddModelError(nameof(model.ServiceId), "Le service est obligatoire pour un chef de service.");
                if (model.DepartementId.HasValue)
                    ModelState.AddModelError(nameof(model.DepartementId), "Un chef de service doit être rattaché au service, sans cible département distincte.");
            }
            else
            {
                if (!model.DepartementId.HasValue)
                    ModelState.AddModelError(nameof(model.DepartementId), "Le département est obligatoire pour ce rôle.");
                if (model.ServiceId.HasValue)
                    ModelState.AddModelError(nameof(model.ServiceId), "Ce rôle doit être rattaché au département, sans cible service.");
            }

            if (model.DepartementId.HasValue && !db.Departement.Any(x => x.DepartementId == model.DepartementId.Value))
                ModelState.AddModelError(nameof(model.DepartementId), "Le département sélectionné n'existe plus.");

            if (model.ServiceId.HasValue && !db.Service.Any(x => x.ServiceId == model.ServiceId.Value))
                ModelState.AddModelError(nameof(model.ServiceId), "Le service sélectionné n'existe plus.");

            if (model.MedecinId.HasValue && !db.Medecin.Any(x => x.MedecinId == model.MedecinId.Value))
                ModelState.AddModelError(nameof(model.MedecinId), "Le médecin sélectionné n'existe plus.");

            if (model.InstallationId.HasValue && !db.Installation.Any(x => x.InstallationId == model.InstallationId.Value))
                ModelState.AddModelError(nameof(model.InstallationId), "L'installation sélectionnée n'existe plus.");

            if (model.DateDebut.HasValue && model.DateFin.HasValue && model.DateFin.Value.Date < model.DateDebut.Value.Date)
                ModelState.AddModelError(nameof(model.DateFin), "La date de fin doit être postérieure ou égale à la date de début.");

            ValidateChefUnique(model, entityOriginale);
        }

        private void ValidateChefUnique(
            AffectationOrganisationnelleFormViewModel model,
            AffectationOrganisationnelle entityOriginale)
        {
            if (!model.EstActive ||
                (model.RoleCode != RoleAffectationOrganisationnelle.ChefDepartement &&
                 model.RoleCode != RoleAffectationOrganisationnelle.ChefService))
            {
                return;
            }

            var memeAffectationChef = entityOriginale != null &&
                entityOriginale.EstActive &&
                entityOriginale.RoleCode == model.RoleCode &&
                entityOriginale.DepartementId == model.DepartementId &&
                entityOriginale.ServiceId == model.ServiceId;

            // Une modification descriptive d'un chef existant n'ajoute pas un second chef.
            // Cette exception permet aussi de corriger les données historiques déjà importées.
            if (memeAffectationChef)
                return;

            var idAExclure = entityOriginale?.AffectationOrganisationnelleId ?? 0;

            if (model.RoleCode == RoleAffectationOrganisationnelle.ChefDepartement && model.DepartementId.HasValue)
            {
                var existeDeja = db.AffectationOrganisationnelle.Any(x =>
                    x.AffectationOrganisationnelleId != idAExclure &&
                    x.DepartementId == model.DepartementId.Value &&
                    x.RoleCode == RoleAffectationOrganisationnelle.ChefDepartement &&
                    x.EstActive);

                if (existeDeja)
                {
                    ModelState.AddModelError(
                        nameof(model.RoleCode),
                        "Ce département possède déjà un chef actif. Désactivez l'affectation existante avant d'en ajouter une nouvelle.");
                }
            }

            if (model.RoleCode == RoleAffectationOrganisationnelle.ChefService && model.ServiceId.HasValue)
            {
                var existeDeja = db.AffectationOrganisationnelle.Any(x =>
                    x.AffectationOrganisationnelleId != idAExclure &&
                    x.ServiceId == model.ServiceId.Value &&
                    x.RoleCode == RoleAffectationOrganisationnelle.ChefService &&
                    x.EstActive);

                if (existeDeja)
                {
                    ModelState.AddModelError(
                        nameof(model.RoleCode),
                        "Ce service possède déjà un chef actif. Désactivez l'affectation existante avant d'en ajouter une nouvelle.");
                }
            }
        }

        private static void Normalize(AffectationOrganisationnelleFormViewModel model)
        {
            model.RoleCode = model.RoleCode?.Trim().ToUpperInvariant();
            model.NomAffiche = model.NomAffiche?.Trim();
            model.Titre = NormalizeText(model.Titre);
            model.Courriel = NormalizeText(model.Courriel);
            model.Telephone = NormalizeText(model.Telephone);
            model.PosteTelephone = NormalizeText(model.PosteTelephone);
            model.SiteNomSource = NormalizeText(model.SiteNomSource);
            model.RegroupementSource = NormalizeText(model.RegroupementSource);
            model.DateDebut = model.DateDebut?.Date;
            model.DateFin = model.DateFin?.Date;
        }

        private static string NormalizeText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static void MapToEntity(AffectationOrganisationnelleFormViewModel model, AffectationOrganisationnelle entity)
        {
            var cibleService = model.RoleCode == RoleAffectationOrganisationnelle.ChefService;
            entity.DepartementId = cibleService ? null : model.DepartementId;
            entity.ServiceId = cibleService ? model.ServiceId : null;
            entity.MedecinId = model.MedecinId;
            entity.RoleCode = model.RoleCode;
            entity.NomAffiche = model.NomAffiche;
            entity.Titre = model.Titre;
            entity.Courriel = model.Courriel;
            entity.Telephone = model.Telephone;
            entity.PosteTelephone = model.PosteTelephone;
            entity.InstallationId = model.InstallationId;
            entity.SiteNomSource = model.SiteNomSource;
            entity.RegroupementSource = model.RegroupementSource;
            entity.DateDebut = model.DateDebut;
            entity.DateFin = model.DateFin;
            entity.EstInterim = model.EstInterim;
            entity.EstActive = model.EstActive;
            entity.EstAValider = model.EstAValider;
        }

        private static AffectationOrganisationnelleFormViewModel ToForm(AffectationOrganisationnelle entity)
        {
            return new AffectationOrganisationnelleFormViewModel
            {
                AffectationOrganisationnelleId = entity.AffectationOrganisationnelleId,
                DepartementId = entity.DepartementId,
                ServiceId = entity.ServiceId,
                MedecinId = entity.MedecinId,
                RoleCode = entity.RoleCode,
                NomAffiche = entity.NomAffiche,
                Titre = entity.Titre,
                Courriel = entity.Courriel,
                CourrielSource = entity.CourrielSource,
                Telephone = entity.Telephone,
                PosteTelephone = entity.PosteTelephone,
                InstallationId = entity.InstallationId,
                SiteNomSource = entity.SiteNomSource,
                RegroupementSource = entity.RegroupementSource,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                EstInterim = entity.EstInterim,
                EstActive = entity.EstActive,
                EstAValider = entity.EstAValider,
                SourceDonnees = entity.SourceDonnees,
                SourceLigne = entity.SourceLigne
            };
        }

        private static AffectationOrganisationnelleListViewModel ToListItem(AffectationOrganisationnelle entity)
        {
            return new AffectationOrganisationnelleListViewModel
            {
                AffectationOrganisationnelleId = entity.AffectationOrganisationnelleId,
                RoleCode = entity.RoleCode,
                RoleLibelle = RoleAffectationOrganisationnelle.Libelle(entity.RoleCode),
                NomAffiche = entity.NomAffiche,
                NomMedecin = entity.Medecin == null ? null : entity.Medecin.Nom + ", " + entity.Medecin.Prenom,
                NomDepartement = entity.Departement?.NomDepartement ?? entity.Service?.Departement?.NomDepartement,
                NomService = entity.Service?.NomService,
                Cible = entity.Service != null
                    ? entity.Service.NomService
                    : entity.Departement?.NomDepartement ?? "",
                RegroupementSource = entity.RegroupementSource,
                Titre = entity.Titre,
                Courriel = entity.Courriel,
                Site = entity.Installation?.NomInstallation ?? entity.SiteNomSource,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                EstInterim = entity.EstInterim,
                EstActive = entity.EstActive,
                EstAValider = entity.EstAValider,
                SourceDonnees = entity.SourceDonnees,
                SourceLigne = entity.SourceLigne
            };
        }

        private static AffectationOrganisationnelleDetailsViewModel ToDetails(AffectationOrganisationnelle entity)
        {
            var item = ToListItem(entity);
            return new AffectationOrganisationnelleDetailsViewModel
            {
                AffectationOrganisationnelleId = item.AffectationOrganisationnelleId,
                RoleCode = item.RoleCode,
                RoleLibelle = item.RoleLibelle,
                NomAffiche = item.NomAffiche,
                NomMedecin = item.NomMedecin,
                NomDepartement = item.NomDepartement,
                NomService = item.NomService,
                Cible = item.Cible,
                RegroupementSource = item.RegroupementSource,
                Titre = item.Titre,
                Courriel = item.Courriel,
                CourrielSource = entity.CourrielSource,
                Telephone = entity.Telephone,
                PosteTelephone = entity.PosteTelephone,
                Site = item.Site,
                DateDebut = item.DateDebut,
                DateFin = item.DateFin,
                EstInterim = item.EstInterim,
                EstActive = item.EstActive,
                EstAValider = item.EstAValider,
                SourceDonnees = item.SourceDonnees,
                SourceLigne = item.SourceLigne,
                DateCreation = entity.DateCreation,
                DateModification = entity.DateModification
            };
        }
    }
}
