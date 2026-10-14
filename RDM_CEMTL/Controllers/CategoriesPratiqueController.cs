using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Models;
using Solution.ViewModels.CategoriesPratique;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class CategoriesPratiqueController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;

        public CategoriesPratiqueController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(CategoriePratique entity)
        {
            return new
            {
                entity.CategoriePratiqueId,
                entity.Libelle,
                entity.EstActif
            };
        }

        public ActionResult Index(string search = null)
        {
            var query = db.CategoriePratique
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x => x.Libelle != null && x.Libelle.Contains(search));
            }

            var items = query
                .OrderBy(x => x.Libelle)
                .Select(x => new CategoriePratiqueListViewModel
                {
                    CategoriePratiqueId = x.CategoriePratiqueId,
                    Libelle = x.Libelle,
                    EstActif = x.EstActif
                })
                .ToList();

            ViewBag.Search = search;

            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return BadRequest();

            var entity = db.CategoriePratique
                .AsNoTracking()
                .FirstOrDefault(x => x.CategoriePratiqueId == id.Value);

            if (entity == null)
                return NotFound();

            var vm = new CategoriePratiqueDetailsViewModel
            {
                CategoriePratiqueId = entity.CategoriePratiqueId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif,
                NombreMedecinsAssocies = db.Medecin.Count(m => m.CategoriePratiqueId == entity.CategoriePratiqueId)
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new CategoriePratiqueFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(CategoriePratiqueFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new CategoriePratique();
            MapToEntity(model, entity);

            db.CategoriePratique.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "CategoriePratique",
                entity.CategoriePratiqueId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "CategoriePratique créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var entity = db.CategoriePratique.FirstOrDefault(x => x.CategoriePratiqueId == id.Value);
            if (entity == null)
                return NotFound();

            var vm = new CategoriePratiqueFormViewModel
            {
                CategoriePratiqueId = entity.CategoriePratiqueId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(CategoriePratiqueFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.CategoriePratique.FirstOrDefault(x => x.CategoriePratiqueId == model.CategoriePratiqueId);
            if (entity == null)
                return NotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            MapToEntity(model, entity);

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "CategoriePratique",
                entity.CategoriePratiqueId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "CategoriePratique modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var entity = db.CategoriePratique
                .AsNoTracking()
                .FirstOrDefault(x => x.CategoriePratiqueId == id.Value);

            if (entity == null)
                return NotFound();

            var vm = new CategoriePratiqueDetailsViewModel
            {
                CategoriePratiqueId = entity.CategoriePratiqueId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif,
                NombreMedecinsAssocies = db.Medecin.Count(m => m.CategoriePratiqueId == entity.CategoriePratiqueId)
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int CategoriePratiqueId)
        {
            var entity = db.CategoriePratique.FirstOrDefault(x => x.CategoriePratiqueId == CategoriePratiqueId);
            if (entity == null)
                return NotFound();

            var nombreMedecinsAssocies = db.Medecin.Count(m => m.CategoriePratiqueId == CategoriePratiqueId);
            if (nombreMedecinsAssocies > 0)
            {
                TempData["Error"] = nombreMedecinsAssocies == 1
                    ? "Cette catégorie ne peut pas être supprimée, car elle est utilisée par 1 médecin. Réaffectez d'abord ce médecin à une autre catégorie."
                    : $"Cette catégorie ne peut pas être supprimée, car elle est utilisée par {nombreMedecinsAssocies} médecins. Réaffectez d'abord ces médecins à une autre catégorie.";

                return RedirectToAction("Delete", new { id = CategoriePratiqueId });
            }

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.CategoriePratique.Remove(entity);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                // Une référence peut avoir été créée entre la vérification et la suppression.
                db.Entry(entity).State = EntityState.Unchanged;
                TempData["Error"] = "Cette catégorie ne peut pas être supprimée parce qu'elle est encore référencée par des données de l'application.";
                return RedirectToAction("Delete", new { id = CategoriePratiqueId });
            }

            _audit.Enregistrer(
                "CategoriePratique",
                CategoriePratiqueId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "Catégorie pratique supprimée avec succès.";
            return RedirectToAction("Index");
        }

        private CategoriePratiqueFormViewModel BuildFormViewModel(CategoriePratiqueFormViewModel model)
        {
            return model;
        }

        private static void MapToEntity(CategoriePratiqueFormViewModel model, CategoriePratique entity)
        {
            entity.Libelle = model.Libelle;
            entity.EstActif = model.EstActif;
        }
}
}