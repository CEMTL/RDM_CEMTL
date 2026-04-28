using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.Privileges;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class PrivilegeReferencesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(PrivilegeReference entity)
        {
            return new
            {
                entity.PrivilegeReferenceId,
                entity.CodePrivilege,
                entity.LibellePrivilege,
                entity.Categorie,
                entity.Description,
                entity.NecessiteBlocOperatoire,
                entity.NecessiteInstallation,
                entity.NecessiteUnite,
                entity.OrdreAffichage,
                entity.EstActif
            };
        }

        public ActionResult Index(string search = null, bool? actifsSeulement = null)
        {
            var query = db.PrivilegeReference.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    (x.CodePrivilege != null && x.CodePrivilege.Contains(search)) ||
                    (x.LibellePrivilege != null && x.LibellePrivilege.Contains(search)) ||
                    (x.Categorie != null && x.Categorie.Contains(search)));
            }

            if (actifsSeulement.HasValue && actifsSeulement.Value)
            {
                query = query.Where(x => x.EstActif);
            }

            var items = query
                .OrderBy(x => x.OrdreAffichage)
                .ThenBy(x => x.LibellePrivilege)
                .Select(x => new PrivilegeReferenceListViewModel
                {
                    PrivilegeReferenceId = x.PrivilegeReferenceId,
                    CodePrivilege = x.CodePrivilege,
                    LibellePrivilege = x.LibellePrivilege,
                    Categorie = x.Categorie,
                    NecessiteBlocOperatoire = x.NecessiteBlocOperatoire,
                    NecessiteInstallation = x.NecessiteInstallation,
                    NecessiteUnite = x.NecessiteUnite,
                    OrdreAffichage = x.OrdreAffichage,
                    EstActif = x.EstActif
                })
                .ToList();

            ViewBag.Search = search;
            ViewBag.ActifsSeulement = actifsSeulement.HasValue && actifsSeulement.Value;
            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("SuperUtilisateur");

            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.PrivilegeReference
                .AsNoTracking()
                .FirstOrDefault(x => x.PrivilegeReferenceId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new PrivilegeReferenceDetailsViewModel
            {
                PrivilegeReferenceId = entity.PrivilegeReferenceId,
                CodePrivilege = entity.CodePrivilege,
                LibellePrivilege = entity.LibellePrivilege,
                Categorie = entity.Categorie,
                Description = entity.Description,
                NecessiteBlocOperatoire = entity.NecessiteBlocOperatoire,
                NecessiteInstallation = entity.NecessiteInstallation,
                NecessiteUnite = entity.NecessiteUnite,
                OrdreAffichage = entity.OrdreAffichage,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            return View(new PrivilegeReferenceFormViewModel { EstActif = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(PrivilegeReferenceFormViewModel model)
        {
            ValidateUniqueCode(model);

            if (!ModelState.IsValid)
                return View(model);

            var entity = new PrivilegeReference();
            MapToEntity(model, entity);
            entity.DateCreation = DateTime.Now;

            db.PrivilegeReference.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "PrivilegeReference",
                entity.PrivilegeReferenceId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "Privilège de référence créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.PrivilegeReference.FirstOrDefault(x => x.PrivilegeReferenceId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new PrivilegeReferenceFormViewModel
            {
                PrivilegeReferenceId = entity.PrivilegeReferenceId,
                CodePrivilege = entity.CodePrivilege,
                LibellePrivilege = entity.LibellePrivilege,
                Categorie = entity.Categorie,
                Description = entity.Description,
                NecessiteBlocOperatoire = entity.NecessiteBlocOperatoire,
                NecessiteInstallation = entity.NecessiteInstallation,
                NecessiteUnite = entity.NecessiteUnite,
                OrdreAffichage = entity.OrdreAffichage,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int id, PrivilegeReferenceFormViewModel model)
        {
            if (id != model.PrivilegeReferenceId)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ValidateUniqueCode(model);

            if (!ModelState.IsValid)
                return View(model);

            var entity = db.PrivilegeReference.FirstOrDefault(x => x.PrivilegeReferenceId == id);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            MapToEntity(model, entity);
            entity.DateModification = DateTime.Now;

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "PrivilegeReference",
                entity.PrivilegeReferenceId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "Privilège de référence modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.PrivilegeReference
                .AsNoTracking()
                .FirstOrDefault(x => x.PrivilegeReferenceId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new PrivilegeReferenceDetailsViewModel
            {
                PrivilegeReferenceId = entity.PrivilegeReferenceId,
                CodePrivilege = entity.CodePrivilege,
                LibellePrivilege = entity.LibellePrivilege,
                Categorie = entity.Categorie,
                Description = entity.Description,
                NecessiteBlocOperatoire = entity.NecessiteBlocOperatoire,
                NecessiteInstallation = entity.NecessiteInstallation,
                NecessiteUnite = entity.NecessiteUnite,
                OrdreAffichage = entity.OrdreAffichage,
                EstActif = entity.EstActif
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int PrivilegeReferenceId)
        {
            var isUsed = db.MedecinPrivilege.Any(x => x.PrivilegeReferenceId == PrivilegeReferenceId);
            if (isUsed)
            {
                TempData["Erreur"] = "Impossible de supprimer ce privilège, car il est déjà utilisé par un médecin.";
                return RedirectToAction("Index");
            }

            var entity = db.PrivilegeReference.FirstOrDefault(x => x.PrivilegeReferenceId == PrivilegeReferenceId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.PrivilegeReference.Remove(entity);

            _audit.Enregistrer(
                "PrivilegeReference",
                PrivilegeReferenceId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "Privilège de référence supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private void ValidateUniqueCode(PrivilegeReferenceFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CodePrivilege))
                return;

            var code = model.CodePrivilege.Trim();

            var exists = db.PrivilegeReference.Any(x =>
                x.PrivilegeReferenceId != model.PrivilegeReferenceId &&
                x.CodePrivilege == code);

            if (exists)
                ModelState.AddModelError("CodePrivilege", "Ce code privilège existe déjà.");
        }

        private static void MapToEntity(PrivilegeReferenceFormViewModel model, PrivilegeReference entity)
        {
            entity.CodePrivilege = (model.CodePrivilege ?? "").Trim();
            entity.LibellePrivilege = (model.LibellePrivilege ?? "").Trim();
            entity.Categorie = model.Categorie;
            entity.Description = model.Description;
            entity.NecessiteBlocOperatoire = model.NecessiteBlocOperatoire;
            entity.NecessiteInstallation = model.NecessiteInstallation;
            entity.NecessiteUnite = model.NecessiteUnite;
            entity.OrdreAffichage = model.OrdreAffichage;
            entity.EstActif = model.EstActif;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}