using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Models;
using Solution.ViewModels.Privileges;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinPrivilegesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;

        public MedecinPrivilegesController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }

        public ActionResult Index(string search = null, string etat = null, bool? actifsSeulement = null)
        {
            var today = DateTime.Today;

            // Les filtres déterminent les médecins à afficher. Une fois un médecin
            // sélectionné par les filtres, le popup affiche l'ensemble de ses privilèges
            // afin de permettre leur gestion complète depuis une seule ligne.
            var matchingQuery = db.MedecinPrivilege
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.PrivilegeReference)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                matchingQuery = matchingQuery.Where(x =>
                    (x.Medecin.Permis != null && x.Medecin.Permis.Contains(search)) ||
                    (x.Medecin.Nom != null && x.Medecin.Nom.Contains(search)) ||
                    (x.Medecin.Prenom != null && x.Medecin.Prenom.Contains(search)) ||
                    (x.PrivilegeReference.LibellePrivilege != null && x.PrivilegeReference.LibellePrivilege.Contains(search)) ||
                    (x.PrivilegeReference.CodePrivilege != null && x.PrivilegeReference.CodePrivilege.Contains(search)));
            }


            if (!string.IsNullOrWhiteSpace(etat))
                matchingQuery = matchingQuery.Where(x => x.Etat == etat);

            if (actifsSeulement.HasValue && actifsSeulement.Value)
            {
                matchingQuery = matchingQuery.Where(x =>
                    x.EstActif &&
                    x.Etat == "Actif" &&
                    x.DateDebut <= today &&
                    (!x.DateFin.HasValue || x.DateFin >= today));
            }

            var matchingMedecinIds = matchingQuery
                .Select(x => x.MedecinId)
                .Distinct();

            var privileges = db.MedecinPrivilege
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.PrivilegeReference)
                .Include(x => x.Installation)
                .Include(x => x.Service).ThenInclude(x => x.Departement)
                .Where(x => matchingMedecinIds.Contains(x.MedecinId))
                .OrderBy(x => x.Medecin.Nom)
                .ThenBy(x => x.Medecin.Prenom)
                .ThenBy(x => x.PrivilegeReference.LibellePrivilege)
                .Select(x => new
                {
                    x.MedecinId,
                    x.MedecinPrivilegeId,
                    MedecinNomComplet = x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom,
                    PrivilegeLibelle = x.PrivilegeReference.LibellePrivilege,
                    CodePrivilege = x.PrivilegeReference.CodePrivilege,
                    InstallationLibelle = x.Installation != null ? x.Installation.NomInstallation : "",
                    ServiceLibelle = x.Service != null ? x.Service.NomService : "",
                    DepartementLibelle = x.Service != null && x.Service.Departement != null ? x.Service.Departement.NomDepartement : "",
                    x.DateDebut,
                    x.DateFin,
                    x.Etat,
                    EstActif = x.EstActif && x.Etat == "Actif" &&
                               x.DateDebut <= today &&
                               (!x.DateFin.HasValue || x.DateFin >= today)
                })
                .ToList();

            var items = privileges
                .GroupBy(x => new { x.MedecinId, x.MedecinNomComplet })
                .Select(g => new MedecinPrivilegeGroupViewModel
                {
                    MedecinId = g.Key.MedecinId,
                    MedecinNomComplet = g.Key.MedecinNomComplet,
                    NombrePrivileges = g.Count(),
                    NombrePrivilegesActifs = g.Count(x => x.EstActif),
                    Privileges = g.Select(x => new MedecinPrivilegeListViewModel
                    {
                        MedecinPrivilegeId = x.MedecinPrivilegeId,
                        MedecinNomComplet = x.MedecinNomComplet,
                        PrivilegeLibelle = x.PrivilegeLibelle,
                        CodePrivilege = x.CodePrivilege,
                        InstallationLibelle = x.InstallationLibelle,
                        ServiceLibelle = x.ServiceLibelle,
                        DepartementLibelle = x.DepartementLibelle,
                        DateDebut = x.DateDebut,
                        DateFin = x.DateFin,
                        Etat = x.Etat,
                        EstActif = x.EstActif
                    }).ToList()
                })
                .ToList();

            ViewBag.Search = search;
            ViewBag.Etat = etat;
            ViewBag.ActifsSeulement = actifsSeulement;
            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("SuperUtilisateur");

            return View(items);
        }

        public ActionResult Details(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (id == null)
                return BadRequest();

            var entity = db.MedecinPrivilege
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.PrivilegeReference)
                .Include(x => x.Installation)
                .Include(x => x.Service).ThenInclude(x => x.Departement)
                .FirstOrDefault(x => x.MedecinPrivilegeId == id.Value);

            if (entity == null)
                return NotFound();

            var today = DateTime.Today;

            var vm = new MedecinPrivilegeDetailsViewModel
            {
                MedecinPrivilegeId = entity.MedecinPrivilegeId,
                MedecinId = entity.MedecinId,
                PrivilegeReferenceId = entity.PrivilegeReferenceId,
                InstallationId = entity.InstallationId,
                ServiceId = entity.ServiceId,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                Etat = entity.Etat,
                NatureActivites = entity.NatureActivites,
                ChampActivites = entity.ChampActivites,
                Obligations = entity.Obligations,
                Commentaire = entity.Commentaire,
                NumeroResolutionCA = entity.NumeroResolutionCA,
                DateResolutionCA = entity.DateResolutionCA,
                ReconnaissanceEcriteRecue = entity.ReconnaissanceEcriteRecue,
                EstActif = entity.EstActif && entity.Etat == "Actif" &&
                           entity.DateDebut <= today &&
                           (!entity.DateFin.HasValue || entity.DateFin >= today),
                MedecinNomComplet = entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom,
                PrivilegeLibelle = entity.PrivilegeReference != null ? entity.PrivilegeReference.LibellePrivilege : "",
                CodePrivilege = entity.PrivilegeReference != null ? entity.PrivilegeReference.CodePrivilege : "",
                InstallationLibelle = entity.Installation != null ? entity.Installation.NomInstallation : "",
                ServiceLibelle = entity.Service != null ? entity.Service.NomService : "",
                DepartementLibelle = entity.Service?.Departement != null ? entity.Service.Departement.NomDepartement : ""
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(int? medecinId = null, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            var vm = new MedecinPrivilegeFormViewModel
            {
                MedecinId = medecinId ?? 0,
                DateDebut = DateTime.Today,
                Etat = "Actif",
                EstActif = true,
                ReconnaissanceEcriteRecue = false
            };

            return View(BuildForm(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(MedecinPrivilegeFormViewModel model, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            ValidateBusinessRules(model);

            if (!ModelState.IsValid)
                return View(BuildForm(model));

            var entity = new MedecinPrivilege();
            MapToEntity(model, entity);
            entity.DateCreation = DateTime.Now;

            db.MedecinPrivilege.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "Privilège accordé avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (id == null)
                return BadRequest();

            var entity = db.MedecinPrivilege.FirstOrDefault(x => x.MedecinPrivilegeId == id.Value);
            if (entity == null)
                return NotFound();

            var vm = new MedecinPrivilegeFormViewModel
            {
                MedecinPrivilegeId = entity.MedecinPrivilegeId,
                MedecinId = entity.MedecinId,
                PrivilegeReferenceId = entity.PrivilegeReferenceId,
                InstallationId = entity.InstallationId,
                ServiceId = entity.ServiceId,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                Etat = entity.Etat,
                NatureActivites = entity.NatureActivites,
                ChampActivites = entity.ChampActivites,
                Obligations = entity.Obligations,
                Commentaire = entity.Commentaire,
                NumeroResolutionCA = entity.NumeroResolutionCA,
                DateResolutionCA = entity.DateResolutionCA,
                ReconnaissanceEcriteRecue = entity.ReconnaissanceEcriteRecue,
                EstActif = entity.EstActif
            };

            return View(BuildForm(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int id, MedecinPrivilegeFormViewModel model, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (id != model.MedecinPrivilegeId)
                return BadRequest();

            ValidateBusinessRules(model);

            if (!ModelState.IsValid)
                return View(BuildForm(model));

            var entity = db.MedecinPrivilege.FirstOrDefault(x => x.MedecinPrivilegeId == id);
            if (entity == null)
                return NotFound();

            MapToEntity(model, entity);
            entity.DateModification = DateTime.Now;

            db.SaveChanges();

            TempData["Success"] = "Privilège modifié avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (id == null)
                return BadRequest();

            var entity = db.MedecinPrivilege
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.PrivilegeReference)
                .Include(x => x.Installation)
                .Include(x => x.Service).ThenInclude(x => x.Departement)
                .FirstOrDefault(x => x.MedecinPrivilegeId == id.Value);

            if (entity == null)
                return NotFound();

            var today = DateTime.Today;

            var vm = new MedecinPrivilegeDetailsViewModel
            {
                MedecinPrivilegeId = entity.MedecinPrivilegeId,
                MedecinNomComplet = entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom,
                PrivilegeLibelle = entity.PrivilegeReference != null ? entity.PrivilegeReference.LibellePrivilege : "",
                CodePrivilege = entity.PrivilegeReference != null ? entity.PrivilegeReference.CodePrivilege : "",
                InstallationLibelle = entity.Installation != null ? entity.Installation.NomInstallation : "",
                ServiceLibelle = entity.Service != null ? entity.Service.NomService : "",
                DepartementLibelle = entity.Service?.Departement != null ? entity.Service.Departement.NomDepartement : "",
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                Etat = entity.Etat,
                EstActif = entity.EstActif && entity.Etat == "Actif" &&
                           entity.DateDebut <= today &&
                           (!entity.DateFin.HasValue || entity.DateFin >= today)
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id, int? returnMedecinId = null, string returnTab = null)
        {
            var entity = db.MedecinPrivilege.FirstOrDefault(x => x.MedecinPrivilegeId == id);
            if (entity == null)
                return NotFound();

            db.MedecinPrivilege.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "Privilège supprimé avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        private void PreparerRetour(int? returnMedecinId, string returnTab)
        {
            ViewBag.ReturnMedecinId = returnMedecinId;
            ViewBag.ReturnTab = returnTab;
        }

        private ActionResult RedirigerApresAction(int? returnMedecinId, string returnTab)
        {
            if (returnMedecinId.HasValue)
            {
                return RedirectToAction("Edit", "Medecins", new
                {
                    id = returnMedecinId.Value,
                    tab = string.IsNullOrWhiteSpace(returnTab) ? "privileges" : returnTab
                });
            }

            return RedirectToAction("Index");
        }

        private MedecinPrivilegeFormViewModel BuildForm(MedecinPrivilegeFormViewModel model)
        {
            // En édition, conserver dans chaque liste la valeur déjà liée à
            // l'enregistrement même si le référentiel (ou le médecin) a depuis été
            // désactivé. Sans cela, DropDownListFor ne peut pas retrouver la valeur
            // courante et affiche "-- Sélectionner --".
            model.MedecinOptions = db.Medecin
                .AsNoTracking()
                .Where(x => x.EstActif || x.MedecinId == model.MedecinId)
                .OrderBy(x => x.Nom)
                .ThenBy(x => x.Prenom)
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = x.Permis + " - " + x.Nom + " " + x.Prenom,
                    Selected = x.MedecinId == model.MedecinId
                })
                .ToList();

            model.PrivilegeOptions = db.PrivilegeReference
                .AsNoTracking()
                .Where(x => x.EstActif || x.PrivilegeReferenceId == model.PrivilegeReferenceId)
                .OrderBy(x => x.OrdreAffichage)
                .ThenBy(x => x.LibellePrivilege)
                .Select(x => new SelectListItem
                {
                    Value = x.PrivilegeReferenceId.ToString(),
                    Text = x.CodePrivilege + " - " + x.LibellePrivilege,
                    Selected = x.PrivilegeReferenceId == model.PrivilegeReferenceId
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

            model.ServiceOptions = GetServiceQuery(model.InstallationId, model.ServiceId)
                .OrderBy(x => x.Departement != null ? x.Departement.NomDepartement : "")
                .ThenBy(x => x.NomService)
                .Select(x => new SelectListItem
                {
                    Value = x.ServiceId.ToString(),
                    Text = (x.Departement != null ? x.Departement.NomDepartement + " — " : "") + x.NomService,
                    Selected = model.ServiceId.HasValue && x.ServiceId == model.ServiceId.Value
                })
                .ToList();

            model.EtatOptions = new[]
            {
                new SelectListItem { Value = "Actif", Text = "Actif", Selected = model.Etat == "Actif" },
                new SelectListItem { Value = "Suspendu", Text = "Suspendu", Selected = model.Etat == "Suspendu" },
                new SelectListItem { Value = "Expiré", Text = "Expiré", Selected = model.Etat == "Expiré" },
                new SelectListItem { Value = "En attente", Text = "En attente", Selected = model.Etat == "En attente" },
                new SelectListItem { Value = "Refusé", Text = "Refusé", Selected = model.Etat == "Refusé" },
                new SelectListItem { Value = "Non renouvelé", Text = "Non renouvelé", Selected = model.Etat == "Non renouvelé" }
            };

            return model;
        }

        [HttpGet]
        public JsonResult ServicesForInstallation(int? installationId)
        {
            var items = GetServiceQuery(installationId, null)
                .OrderBy(x => x.Departement != null ? x.Departement.NomDepartement : "")
                .ThenBy(x => x.NomService)
                .Select(x => new
                {
                    value = x.ServiceId.ToString(),
                    text = (x.Departement != null ? x.Departement.NomDepartement + " — " : "") + x.NomService
                })
                .ToList();

            return Json(items);
        }

        private IQueryable<Service> GetServiceQuery(int? installationId, int? selectedServiceId)
        {
            var query = db.Service
                .AsNoTracking()
                .Where(x =>
                    (x.EstActif && (x.DepartementId == null || x.Departement.EstActif)) ||
                    (selectedServiceId.HasValue && x.ServiceId == selectedServiceId.Value));

            if (installationId.HasValue)
            {
                var installationConfiguree = db.InstallationService
                    .AsNoTracking()
                    .Any(x => x.InstallationId == installationId.Value && x.EstActif);

                if (installationConfiguree)
                {
                    query = query.Where(x =>
                        x.InstallationServices.Any(link =>
                            link.InstallationId == installationId.Value && link.EstActif) ||
                        (selectedServiceId.HasValue && x.ServiceId == selectedServiceId.Value));
                }
            }

            return query;
        }

        private void ValidateBusinessRules(MedecinPrivilegeFormViewModel model)
        {
            if (!model.DateDebut.HasValue)
                return;

            if (model.DateFin.HasValue && model.DateFin.Value.Date < model.DateDebut.Value.Date)
            {
                ModelState.AddModelError("DateFin", "La date de fin doit être supérieure ou égale à la date de début.");
            }

            var privilege = db.PrivilegeReference
                .AsNoTracking()
                .FirstOrDefault(x => x.PrivilegeReferenceId == model.PrivilegeReferenceId);

            if (privilege != null)
            {
                if (privilege.NecessiteInstallation && !model.InstallationId.HasValue)
                {
                    ModelState.AddModelError("InstallationId", "Une installation est obligatoire pour ce privilège.");
                }

                if (privilege.NecessiteService && !model.ServiceId.HasValue)
                {
                    ModelState.AddModelError(nameof(model.ServiceId), "Un service est obligatoire pour ce privilège.");
                }
            }

            if (model.InstallationId.HasValue && model.ServiceId.HasValue)
            {
                var installationConfiguree = db.InstallationService
                    .AsNoTracking()
                    .Any(x => x.InstallationId == model.InstallationId.Value && x.EstActif);

                if (installationConfiguree && !db.InstallationService.AsNoTracking().Any(x =>
                    x.InstallationId == model.InstallationId.Value &&
                    x.ServiceId == model.ServiceId.Value &&
                    x.EstActif))
                {
                    ModelState.AddModelError(nameof(model.ServiceId), "Ce service n'est pas rattaché à l'installation sélectionnée.");
                }
            }

            if (model.MedecinId > 0 && model.PrivilegeReferenceId > 0 && model.DateDebut.HasValue)
            {
                var dateDebut = model.DateDebut.Value.Date;
                var dateFin = model.DateFin.HasValue ? model.DateFin.Value.Date : DateTime.MaxValue.Date;

                var overlap = db.MedecinPrivilege.Any(x =>
                    x.MedecinPrivilegeId != model.MedecinPrivilegeId &&
                    x.MedecinId == model.MedecinId &&
                    x.PrivilegeReferenceId == model.PrivilegeReferenceId &&
                    x.InstallationId == model.InstallationId &&
                    x.ServiceId == model.ServiceId &&
                    x.DateDebut <= dateFin &&
                    (!x.DateFin.HasValue || x.DateFin >= dateDebut));

                if (overlap)
                {
                    ModelState.AddModelError("", "Ce médecin possède déjà ce privilège sur une période qui se chevauche.");
                }
            }
        }

        private static void MapToEntity(MedecinPrivilegeFormViewModel model, MedecinPrivilege entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.PrivilegeReferenceId = model.PrivilegeReferenceId;
            entity.InstallationId = model.InstallationId;
            entity.ServiceId = model.ServiceId;
            entity.DateDebut = model.DateDebut;
            entity.DateFin = model.DateFin;
            entity.Etat = model.Etat;
            entity.NatureActivites = model.NatureActivites;
            entity.ChampActivites = model.ChampActivites;
            entity.Obligations = model.Obligations;
            entity.Commentaire = model.Commentaire;
            entity.NumeroResolutionCA = model.NumeroResolutionCA;
            entity.DateResolutionCA = model.DateResolutionCA;
            entity.ReconnaissanceEcriteRecue = model.ReconnaissanceEcriteRecue;
            entity.EstActif = model.EstActif;
        }
}
}
