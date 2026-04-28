using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.Privileges;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinPrivilegesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null, int? privilegeReferenceId = null, string etat = null, bool? actifsSeulement = null)
        {
            var today = DateTime.Today;

            var query = db.MedecinPrivilege
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.PrivilegeReference)
                .Include(x => x.Installation)
                .Include(x => x.UniteOrganisationnelle)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    (x.Medecin.Permis != null && x.Medecin.Permis.Contains(search)) ||
                    (x.Medecin.Nom != null && x.Medecin.Nom.Contains(search)) ||
                    (x.Medecin.Prenom != null && x.Medecin.Prenom.Contains(search)) ||
                    (x.PrivilegeReference.LibellePrivilege != null && x.PrivilegeReference.LibellePrivilege.Contains(search)) ||
                    (x.PrivilegeReference.CodePrivilege != null && x.PrivilegeReference.CodePrivilege.Contains(search)));
            }

            if (privilegeReferenceId.HasValue)
                query = query.Where(x => x.PrivilegeReferenceId == privilegeReferenceId.Value);

            if (!string.IsNullOrWhiteSpace(etat))
                query = query.Where(x => x.Etat == etat);

            if (actifsSeulement.HasValue && actifsSeulement.Value)
            {
                query = query.Where(x =>
                    x.EstActif &&
                    x.Etat == "Actif" &&
                    x.DateDebut <= today &&
                    (!x.DateFin.HasValue || x.DateFin >= today));
            }

            var items = query
                .OrderBy(x => x.Medecin.Nom)
                .ThenBy(x => x.Medecin.Prenom)
                .ThenBy(x => x.PrivilegeReference.LibellePrivilege)
                .Select(x => new MedecinPrivilegeListViewModel
                {
                    MedecinPrivilegeId = x.MedecinPrivilegeId,
                    MedecinNomComplet = x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom,
                    PrivilegeLibelle = x.PrivilegeReference.LibellePrivilege,
                    CodePrivilege = x.PrivilegeReference.CodePrivilege,
                    InstallationLibelle = x.Installation != null ? x.Installation.NomInstallation : "",
                    UniteLibelle = x.UniteOrganisationnelle != null ? x.UniteOrganisationnelle.Libelle : "",
                    DateDebut = x.DateDebut,
                    DateFin = x.DateFin,
                    Etat = x.Etat,
                    EstActif = x.EstActif && x.Etat == "Actif" &&
                               x.DateDebut <= today &&
                               (!x.DateFin.HasValue || x.DateFin >= today)
                })
                .ToList();

            ViewBag.PrivilegeReferenceId = new SelectList(
                db.PrivilegeReference
                    .AsNoTracking()
                    .Where(x => x.EstActif)
                    .OrderBy(x => x.LibellePrivilege)
                    .ToList(),
                "PrivilegeReferenceId",
                "LibellePrivilege",
                privilegeReferenceId);

            ViewBag.Search = search;
            ViewBag.Etat = etat;
            ViewBag.ActifsSeulement = actifsSeulement;
            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("SuperUtilisateur");

            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinPrivilege
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.PrivilegeReference)
                .Include(x => x.Installation)
                .Include(x => x.UniteOrganisationnelle)
                .FirstOrDefault(x => x.MedecinPrivilegeId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var today = DateTime.Today;

            var vm = new MedecinPrivilegeDetailsViewModel
            {
                MedecinPrivilegeId = entity.MedecinPrivilegeId,
                MedecinId = entity.MedecinId,
                PrivilegeReferenceId = entity.PrivilegeReferenceId,
                InstallationId = entity.InstallationId,
                UniteOrganisationnelleId = entity.UniteOrganisationnelleId,
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
                UniteLibelle = entity.UniteOrganisationnelle != null ? entity.UniteOrganisationnelle.Libelle : ""
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = new MedecinPrivilegeFormViewModel
            {
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
        public ActionResult Create(MedecinPrivilegeFormViewModel model)
        {
            ValidateBusinessRules(model);

            if (!ModelState.IsValid)
                return View(BuildForm(model));

            var entity = new MedecinPrivilege();
            MapToEntity(model, entity);
            entity.DateCreation = DateTime.Now;

            db.MedecinPrivilege.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "Privilège accordé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinPrivilege.FirstOrDefault(x => x.MedecinPrivilegeId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new MedecinPrivilegeFormViewModel
            {
                MedecinPrivilegeId = entity.MedecinPrivilegeId,
                MedecinId = entity.MedecinId,
                PrivilegeReferenceId = entity.PrivilegeReferenceId,
                InstallationId = entity.InstallationId,
                UniteOrganisationnelleId = entity.UniteOrganisationnelleId,
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
        public ActionResult Edit(int id, MedecinPrivilegeFormViewModel model)
        {
            if (id != model.MedecinPrivilegeId)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ValidateBusinessRules(model);

            if (!ModelState.IsValid)
                return View(BuildForm(model));

            var entity = db.MedecinPrivilege.FirstOrDefault(x => x.MedecinPrivilegeId == id);
            if (entity == null)
                return HttpNotFound();

            MapToEntity(model, entity);
            entity.DateModification = DateTime.Now;

            db.SaveChanges();

            TempData["Success"] = "Privilège modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinPrivilege
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.PrivilegeReference)
                .Include(x => x.Installation)
                .Include(x => x.UniteOrganisationnelle)
                .FirstOrDefault(x => x.MedecinPrivilegeId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var today = DateTime.Today;

            var vm = new MedecinPrivilegeDetailsViewModel
            {
                MedecinPrivilegeId = entity.MedecinPrivilegeId,
                MedecinNomComplet = entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom,
                PrivilegeLibelle = entity.PrivilegeReference != null ? entity.PrivilegeReference.LibellePrivilege : "",
                CodePrivilege = entity.PrivilegeReference != null ? entity.PrivilegeReference.CodePrivilege : "",
                InstallationLibelle = entity.Installation != null ? entity.Installation.NomInstallation : "",
                UniteLibelle = entity.UniteOrganisationnelle != null ? entity.UniteOrganisationnelle.Libelle : "",
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
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.MedecinPrivilege.FirstOrDefault(x => x.MedecinPrivilegeId == id);
            if (entity == null)
                return HttpNotFound();

            db.MedecinPrivilege.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "Privilège supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private MedecinPrivilegeFormViewModel BuildForm(MedecinPrivilegeFormViewModel model)
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

            model.PrivilegeOptions = db.PrivilegeReference
                .AsNoTracking()
                .Where(x => x.EstActif)
                .OrderBy(x => x.OrdreAffichage)
                .ThenBy(x => x.LibellePrivilege)
                .Select(x => new SelectListItem
                {
                    Value = x.PrivilegeReferenceId.ToString(),
                    Text = x.CodePrivilege + " - " + x.LibellePrivilege
                })
                .ToList();

            model.InstallationOptions = db.Installation
                .AsNoTracking()
                .Where(x => x.EstActif)
                .OrderBy(x => x.NomInstallation)
                .Select(x => new SelectListItem
                {
                    Value = x.InstallationId.ToString(),
                    Text = x.NomInstallation
                })
                .ToList();

            model.UniteOptions = db.UniteOrganisationnelle
                .AsNoTracking()
                .Where(x => x.EstActif)
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.UniteOrganisationnelleId.ToString(),
                    Text = x.Libelle
                })
                .ToList();

            model.EtatOptions = new[]
            {
                new SelectListItem { Value = "Actif", Text = "Actif" },
                new SelectListItem { Value = "Suspendu", Text = "Suspendu" },
                new SelectListItem { Value = "Expiré", Text = "Expiré" },
                new SelectListItem { Value = "En attente", Text = "En attente" },
                new SelectListItem { Value = "Refusé", Text = "Refusé" },
                new SelectListItem { Value = "Non renouvelé", Text = "Non renouvelé" }
            };

            return model;
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

                if (privilege.NecessiteUnite && !model.UniteOrganisationnelleId.HasValue)
                {
                    ModelState.AddModelError("UniteOrganisationnelleId", "Une unité organisationnelle est obligatoire pour ce privilège.");
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
            entity.UniteOrganisationnelleId = model.UniteOrganisationnelleId;
            entity.DateDebut = model.DateDebut.Value;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}