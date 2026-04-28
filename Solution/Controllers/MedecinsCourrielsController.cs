using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.MedecinsCourriels;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinsCourrielsController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null)
        {
            var query = db.MedecinCourriel
                .Include(x => x.Medecin)
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => (x.TypeCourriel != null && x.TypeCourriel.Contains(search)) || (x.AdresseCourriel != null && x.AdresseCourriel.Contains(search)));
            }
            var items = query
                .OrderBy(x => x.Medecin.Permis).ThenBy(x => x.Medecin.Nom).ThenBy(x => x.Medecin.Prenom)
                .Select(x => new MedecinCourrielListViewModel
                {
                    MedecinCourrielId = x.MedecinCourrielId,
                    MedecinId = x.MedecinId,
                    TypeCourriel = x.TypeCourriel,
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
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinCourriel
                                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinCourrielId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new MedecinCourrielDetailsViewModel
            {
            MedecinCourrielId = entity.MedecinCourrielId,
            MedecinId = entity.MedecinId,
            TypeCourriel = entity.TypeCourriel,
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
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new MedecinCourriel();
            MapToEntity(model, entity);

            db.MedecinCourriel.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinCourriel créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinCourriel.FirstOrDefault(x => x.MedecinCourrielId == id.Value);
            if (entity == null) return HttpNotFound();

            var vm = new MedecinCourrielFormViewModel
            {
                MedecinCourrielId = entity.MedecinCourrielId,
                MedecinId = entity.MedecinId,
                TypeCourriel = entity.TypeCourriel,
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
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.MedecinCourriel.FirstOrDefault(x => x.MedecinCourrielId == model.MedecinCourrielId);
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinCourriel modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinCourriel
                                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinCourrielId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new MedecinCourrielDetailsViewModel
            {
            MedecinCourrielId = entity.MedecinCourrielId,
            MedecinId = entity.MedecinId,
            TypeCourriel = entity.TypeCourriel,
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
            if (entity == null) return HttpNotFound();

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
            return model;
        }

        private static void MapToEntity(MedecinCourrielFormViewModel model, MedecinCourriel entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.TypeCourriel = model.TypeCourriel;
            entity.AdresseCourriel = model.AdresseCourriel;
            entity.RangSource = (Byte) model.RangSource;
            entity.EstPrincipal = model.EstPrincipal;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
