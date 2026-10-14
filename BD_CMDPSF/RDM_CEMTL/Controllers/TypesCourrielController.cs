using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Models;
using Solution.Services;
using Solution.ViewModels.TypesCourriel;
using System;
using System.Linq;

namespace Solution.Controllers
{
    [Authorize]
    public class TypesCourrielController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;

        public TypesCourrielController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }

        private AuditService Audit => new AuditService(db, HttpContext);

        public ActionResult Index(string search = null)
        {
            var query = db.TypeCourriel.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x => x.Libelle.Contains(search));
            }

            var model = query
                .OrderBy(x => x.Libelle)
                .Select(x => new TypeCourrielListViewModel
                {
                    TypeCourrielId = x.TypeCourrielId,
                    Libelle = x.Libelle,
                    EstActif = x.EstActif,
                    NombreUtilisations = x.MedecinCourriels.Count()
                })
                .ToList();

            ViewBag.Search = search;
            return View(model);
        }

        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
                return BadRequest();

            var model = db.TypeCourriel
                .AsNoTracking()
                .Where(x => x.TypeCourrielId == id.Value)
                .Select(x => new TypeCourrielDetailsViewModel
                {
                    TypeCourrielId = x.TypeCourrielId,
                    Libelle = x.Libelle,
                    EstActif = x.EstActif,
                    NombreUtilisations = x.MedecinCourriels.Count()
                })
                .FirstOrDefault();

            return model == null ? NotFound() : View(model);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            return View(new TypeCourrielFormViewModel { EstActif = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(TypeCourrielFormViewModel model)
        {
            ValiderLibelleUnique(model);
            if (!ModelState.IsValid)
                return View(model);

            var entity = new TypeCourriel
            {
                Libelle = model.Libelle.Trim(),
                EstActif = model.EstActif
            };

            db.TypeCourriel.Add(entity);
            db.SaveChanges();
            Audit.Enregistrer("TypeCourriel", entity.TypeCourrielId, "INSERT", null, Snapshot(entity));
            db.SaveChanges();

            TempData["Success"] = "Le type de courriel a été créé.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
                return BadRequest();

            var entity = db.TypeCourriel.AsNoTracking().FirstOrDefault(x => x.TypeCourrielId == id.Value);
            if (entity == null)
                return NotFound();

            return View(new TypeCourrielFormViewModel
            {
                TypeCourrielId = entity.TypeCourrielId,
                Libelle = entity.Libelle,
                EstActif = entity.EstActif
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(TypeCourrielFormViewModel model)
        {
            ValiderLibelleUnique(model);
            if (!ModelState.IsValid)
                return View(model);

            var entity = db.TypeCourriel.FirstOrDefault(x => x.TypeCourrielId == model.TypeCourrielId);
            if (entity == null)
                return NotFound();

            var ancienneValeur = Snapshot(entity);
            entity.Libelle = model.Libelle.Trim();
            entity.EstActif = model.EstActif;

            // Le texte historique reste synchronisé afin que les exports et les anciens
            // écrans continuent d'afficher le libellé canonique du référentiel.
            foreach (var courriel in db.MedecinCourriel.Where(x => x.TypeCourrielId == entity.TypeCourrielId))
                courriel.TypeCourriel = entity.Libelle;

            Audit.Enregistrer("TypeCourriel", entity.TypeCourrielId, "UPDATE", ancienneValeur, Snapshot(entity));
            db.SaveChanges();

            TempData["Success"] = "Le type de courriel a été modifié.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
                return BadRequest();

            var model = db.TypeCourriel
                .AsNoTracking()
                .Where(x => x.TypeCourrielId == id.Value)
                .Select(x => new TypeCourrielDetailsViewModel
                {
                    TypeCourrielId = x.TypeCourrielId,
                    Libelle = x.Libelle,
                    EstActif = x.EstActif,
                    NombreUtilisations = x.MedecinCourriels.Count()
                })
                .FirstOrDefault();

            return model == null ? NotFound() : View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int TypeCourrielId)
        {
            var entity = db.TypeCourriel.FirstOrDefault(x => x.TypeCourrielId == TypeCourrielId);
            if (entity == null)
                return NotFound();

            if (db.MedecinCourriel.Any(x => x.TypeCourrielId == TypeCourrielId))
            {
                TempData["Error"] = "Ce type est utilisé par au moins un courriel. Désactivez-le au lieu de le supprimer.";
                return RedirectToAction("Delete", new { id = TypeCourrielId });
            }

            var ancienneValeur = Snapshot(entity);
            db.TypeCourriel.Remove(entity);
            Audit.Enregistrer("TypeCourriel", TypeCourrielId, "DELETE", ancienneValeur, null);
            db.SaveChanges();

            TempData["Success"] = "Le type de courriel a été supprimé.";
            return RedirectToAction("Index");
        }

        private void ValiderLibelleUnique(TypeCourrielFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Libelle))
                return;

            var libelle = model.Libelle.Trim();
            if (db.TypeCourriel.Any(x => x.TypeCourrielId != model.TypeCourrielId && x.Libelle == libelle))
                ModelState.AddModelError(nameof(model.Libelle), "Un type de courriel porte déjà ce libellé.");
        }

        private static object Snapshot(TypeCourriel entity)
        {
            return new
            {
                entity.TypeCourrielId,
                entity.Libelle,
                entity.EstActif
            };
        }
    }
}
