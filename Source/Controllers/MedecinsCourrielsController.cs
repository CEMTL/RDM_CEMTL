using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Models;
using Solution.ViewModels.MedecinsCourriels;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinsCourrielsController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;

        public MedecinsCourrielsController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }

        public ActionResult Index(string search = null)
        {
            var query = db.MedecinCourriel
                .Include(x => x.Medecin)
                .Include(x => x.TypeCourrielReference)
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    (x.TypeCourrielReference != null && x.TypeCourrielReference.Libelle.Contains(search)) ||
                    (x.TypeCourriel != null && x.TypeCourriel.Contains(search)) ||
                    (x.AdresseCourriel != null && x.AdresseCourriel.Contains(search)));
            }
            var items = query
                .OrderBy(x => x.Medecin.Permis).ThenBy(x => x.Medecin.Nom).ThenBy(x => x.Medecin.Prenom)
                .Select(x => new MedecinCourrielListViewModel
                {
                    MedecinCourrielId = x.MedecinCourrielId,
                    MedecinId = x.MedecinId,
                    TypeCourriel = x.TypeCourrielReference != null ? x.TypeCourrielReference.Libelle : x.TypeCourriel,
                    AdresseCourriel = x.AdresseCourriel,
                    RangSource = x.RangSource,
                    EstPrincipal = x.EstPrincipal,
                    MedecinLibelle = x.Medecin != null ? (x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom) : string.Empty,
                })
                .ToList();
            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return BadRequest();

            var entity = db.MedecinCourriel
                .Include(x => x.Medecin)
                .Include(x => x.TypeCourrielReference)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinCourrielId == id.Value);

            if (entity == null) return NotFound();

            var vm = new MedecinCourrielDetailsViewModel
            {
            MedecinCourrielId = entity.MedecinCourrielId,
            MedecinId = entity.MedecinId,
            TypeCourriel = entity.TypeCourrielReference != null ? entity.TypeCourrielReference.Libelle : entity.TypeCourriel,
            AdresseCourriel = entity.AdresseCourriel,
            RangSource = entity.RangSource,
            EstPrincipal = entity.EstPrincipal,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new MedecinCourrielFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(MedecinCourrielFormViewModel model)
        {
            var typeCourriel = ValiderTypeCourriel(model.TypeCourriel, null, nameof(model.TypeCourriel));
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new MedecinCourriel();
            MapToEntity(model, entity, typeCourriel);

            db.MedecinCourriel.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "Le courriel a été créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return BadRequest();

            var entity = db.MedecinCourriel.FirstOrDefault(x => x.MedecinCourrielId == id.Value);
            if (entity == null) return NotFound();

            var vm = new MedecinCourrielFormViewModel
            {
                MedecinCourrielId = entity.MedecinCourrielId,
                MedecinId = entity.MedecinId,
                TypeCourriel = entity.TypeCourrielReference != null ? entity.TypeCourrielReference.Libelle : entity.TypeCourriel,
                AdresseCourriel = entity.AdresseCourriel,
                RangSource = entity.RangSource,
                EstPrincipal = entity.EstPrincipal,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(MedecinCourrielFormViewModel model)
        {
            var entity = db.MedecinCourriel
                .Include(x => x.TypeCourrielReference)
                .FirstOrDefault(x => x.MedecinCourrielId == model.MedecinCourrielId);
            if (entity == null) return NotFound();

            var typeCourriel = ValiderTypeCourriel(model.TypeCourriel, entity, nameof(model.TypeCourriel));
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            MapToEntity(model, entity, typeCourriel);
            db.SaveChanges();

            TempData["Success"] = "Le courriel a été modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return BadRequest();

            var entity = db.MedecinCourriel
                .Include(x => x.Medecin)
                .Include(x => x.TypeCourrielReference)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinCourrielId == id.Value);

            if (entity == null) return NotFound();

            var vm = new MedecinCourrielDetailsViewModel
            {
            MedecinCourrielId = entity.MedecinCourrielId,
            MedecinId = entity.MedecinId,
            TypeCourriel = entity.TypeCourrielReference != null ? entity.TypeCourrielReference.Libelle : entity.TypeCourriel,
            AdresseCourriel = entity.AdresseCourriel,
            RangSource = entity.RangSource,
            EstPrincipal = entity.EstPrincipal,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.MedecinCourriel.FirstOrDefault(x => x.MedecinCourrielId == id);
            if (entity == null) return NotFound();

            db.MedecinCourriel.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinCourriel supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private MedecinCourrielFormViewModel BuildFormViewModel(MedecinCourrielFormViewModel model)
        {
            model.MedecinIdOptions = db.Medecin.AsNoTracking()
                .OrderBy(x => (x.Permis + " - " + x.Nom + " " + x.Prenom))
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = (x.Permis + " - " + x.Nom + " " + x.Prenom)
                })
                .ToList();

            var typeOptions = db.TypeCourriel
                .AsNoTracking()
                .Where(x => x.EstActif || x.Libelle == model.TypeCourriel)
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.Libelle,
                    Text = x.Libelle + (x.EstActif ? "" : " (inactif)"),
                    Selected = x.Libelle == model.TypeCourriel
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(model.TypeCourriel) &&
                !typeOptions.Any(x => string.Equals(x.Value, model.TypeCourriel, StringComparison.OrdinalIgnoreCase)))
            {
                typeOptions.Add(new SelectListItem
                {
                    Value = model.TypeCourriel,
                    Text = model.TypeCourriel + " (ancienne valeur)",
                    Selected = true
                });
            }

            model.TypeCourrielOptions = typeOptions;
            return model;
        }

        private TypeCourriel ValiderTypeCourriel(string libelle, MedecinCourriel existant, string cleErreur)
        {
            if (string.IsNullOrWhiteSpace(libelle))
                return null;

            var valeur = libelle.Trim();
            var reference = db.TypeCourriel.FirstOrDefault(x => x.Libelle == valeur);
            if (reference != null && (reference.EstActif || existant?.TypeCourrielId == reference.TypeCourrielId))
                return reference;

            bool ancienneValeurInchangee = existant != null &&
                !existant.TypeCourrielId.HasValue &&
                string.Equals(existant.TypeCourriel?.Trim(), valeur, StringComparison.OrdinalIgnoreCase);

            if (!ancienneValeurInchangee)
                ModelState.AddModelError(cleErreur, "Sélectionnez un type de courriel actif dans la liste.");

            return null;
        }

        private static void MapToEntity(MedecinCourrielFormViewModel model, MedecinCourriel entity, TypeCourriel typeCourriel)
        {
            entity.MedecinId = model.MedecinId;
            entity.TypeCourrielId = typeCourriel?.TypeCourrielId;
            entity.TypeCourriel = typeCourriel?.Libelle ?? model.TypeCourriel?.Trim();
            entity.AdresseCourriel = model.AdresseCourriel?.Trim();
            entity.RangSource = model.RangSource.HasValue ? (byte?)model.RangSource.Value : null;
            entity.EstPrincipal = model.EstPrincipal;
        }
}
}
