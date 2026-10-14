using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Models;
using Solution.ViewModels.MedecinsTelephones;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinsTelephonesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;

        public MedecinsTelephonesController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }

        public ActionResult Index(string search = null, int? medecinId = null)
        {
            var query = db.MedecinTelephone
                .Include(x => x.Medecin)
                .AsNoTracking()
                .AsQueryable();
            if (medecinId.HasValue)
                query = query.Where(x => x.MedecinId == medecinId.Value);
            ViewBag.CmdpsfMedecinId = medecinId;
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => (x.TypeTelephone != null && x.TypeTelephone.Contains(search)) || (x.NumeroTelephone != null && x.NumeroTelephone.Contains(search)));
            }
            var items = query
                .OrderBy(x => x.Medecin.Permis).ThenBy(x => x.Medecin.Nom).ThenBy(x => x.Medecin.Prenom)
                .Select(x => new MedecinTelephoneListViewModel
                {
                    MedecinTelephoneId = x.MedecinTelephoneId,
                    MedecinId = x.MedecinId,
                    TypeTelephone = x.TypeTelephone,
                    NumeroTelephone = x.NumeroTelephone,
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

            var entity = db.MedecinTelephone
                                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinTelephoneId == id.Value);

            if (entity == null) return NotFound();

            var vm = new MedecinTelephoneDetailsViewModel
            {
            MedecinTelephoneId = entity.MedecinTelephoneId,
            MedecinId = entity.MedecinId,
            TypeTelephone = entity.TypeTelephone,
            NumeroTelephone = entity.NumeroTelephone,
            RangSource = entity.RangSource,
            EstPrincipal = entity.EstPrincipal,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(int? medecinId = null)
        {
            var vm = BuildFormViewModel(new MedecinTelephoneFormViewModel { MedecinId = medecinId ?? 0 });
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(MedecinTelephoneFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new MedecinTelephone();
            MapToEntity(model, entity);

            db.MedecinTelephone.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinTelephone créé avec succès.";
            return RetourApresModification(entity.MedecinId);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return BadRequest();

            var entity = db.MedecinTelephone.FirstOrDefault(x => x.MedecinTelephoneId == id.Value);
            if (entity == null) return NotFound();

            var vm = new MedecinTelephoneFormViewModel
            {
                MedecinTelephoneId = entity.MedecinTelephoneId,
                MedecinId = entity.MedecinId,
                TypeTelephone = entity.TypeTelephone,
                NumeroTelephone = entity.NumeroTelephone,
                RangSource = entity.RangSource,
                EstPrincipal = entity.EstPrincipal,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(MedecinTelephoneFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.MedecinTelephone.FirstOrDefault(x => x.MedecinTelephoneId == model.MedecinTelephoneId);
            if (entity == null) return NotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinTelephone modifié avec succès.";
            return RetourApresModification(entity.MedecinId);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return BadRequest();

            var entity = db.MedecinTelephone
                                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinTelephoneId == id.Value);

            if (entity == null) return NotFound();

            var vm = new MedecinTelephoneDetailsViewModel
            {
            MedecinTelephoneId = entity.MedecinTelephoneId,
            MedecinId = entity.MedecinId,
            TypeTelephone = entity.TypeTelephone,
            NumeroTelephone = entity.NumeroTelephone,
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
            var entity = db.MedecinTelephone.FirstOrDefault(x => x.MedecinTelephoneId == id);
            if (entity == null) return NotFound();

            db.MedecinTelephone.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinTelephone supprimé avec succès.";
            return RetourApresModification(entity.MedecinId);
        }

        // Retrouver la fiche du même professionnel après enregistrement.
        private ActionResult RetourApresModification(int medecinId)
        {
            if (Request.Query.ContainsKey("medecinId") && medecinId > 0)
                return RedirectToAction("Details", "Cmdpsf", new { id = medecinId });
            return RedirectToAction("Index");
        }

        private MedecinTelephoneFormViewModel BuildFormViewModel(MedecinTelephoneFormViewModel model)
        {
            model.MedecinIdOptions = db.Medecin.AsNoTracking()
                .OrderBy(x => (x.Permis + " - " + x.Nom + " " + x.Prenom))
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = (x.Permis + " - " + x.Nom + " " + x.Prenom)
                })
                .ToList();
            return model;
        }

        private static void MapToEntity(MedecinTelephoneFormViewModel model, MedecinTelephone entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.TypeTelephone = model.TypeTelephone;
            entity.NumeroTelephone = model.NumeroTelephone;
            entity.RangSource = (Byte)model.RangSource;
            entity.EstPrincipal = model.EstPrincipal;
        }
}
}
