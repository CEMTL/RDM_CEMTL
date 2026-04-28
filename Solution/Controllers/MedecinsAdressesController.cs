using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.MedecinsAdresses;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinsAdressesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null)
        {
            var query = db.MedecinAdresse
                .Include(x => x.Medecin)
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => (x.AdresseLigne1 != null && x.AdresseLigne1.Contains(search)) || (x.Ville != null && x.Ville.Contains(search)) || (x.Province != null && x.Province.Contains(search)) || (x.CodePostal != null && x.CodePostal.Contains(search)));
            }
            var items = query
                .OrderBy(x => x.Medecin.Permis).ThenBy(x => x.Medecin.Nom).ThenBy(x => x.Medecin.Prenom)
                .Select(x => new MedecinAdresseListViewModel
                {
                    MedecinAdresseId = x.MedecinAdresseId,
                    MedecinId = x.MedecinId,
                    AdresseLigne1 = x.AdresseLigne1,
                    Ville = x.Ville,
                    Province = x.Province,
                    CodePostal = x.CodePostal,
                    EstPrincipale = x.EstPrincipale,
                    MedecinLibelle = x.Medecin != null ? (x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom) : string.Empty,
                })
                .ToList();
            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinAdresse
                                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinAdresseId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new MedecinAdresseDetailsViewModel
            {
            MedecinAdresseId = entity.MedecinAdresseId,
            MedecinId = entity.MedecinId,
            AdresseLigne1 = entity.AdresseLigne1,
            Ville = entity.Ville,
            Province = entity.Province,
            CodePostal = entity.CodePostal,
            EstPrincipale = entity.EstPrincipale,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new MedecinAdresseFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(MedecinAdresseFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new MedecinAdresse();
            MapToEntity(model, entity);

            db.MedecinAdresse.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinAdresse créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinAdresse.FirstOrDefault(x => x.MedecinAdresseId == id.Value);
            if (entity == null) return HttpNotFound();

            var vm = new MedecinAdresseFormViewModel
            {
                MedecinAdresseId = entity.MedecinAdresseId,
                MedecinId = entity.MedecinId,
                AdresseLigne1 = entity.AdresseLigne1,
                Ville = entity.Ville,
                Province = entity.Province,
                CodePostal = entity.CodePostal,
                EstPrincipale = entity.EstPrincipale,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(MedecinAdresseFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.MedecinAdresse.FirstOrDefault(x => x.MedecinAdresseId == model.MedecinAdresseId);
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinAdresse modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinAdresse
                                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinAdresseId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new MedecinAdresseDetailsViewModel
            {
            MedecinAdresseId = entity.MedecinAdresseId,
            MedecinId = entity.MedecinId,
            AdresseLigne1 = entity.AdresseLigne1,
            Ville = entity.Ville,
            Province = entity.Province,
            CodePostal = entity.CodePostal,
            EstPrincipale = entity.EstPrincipale,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.MedecinAdresse.FirstOrDefault(x => x.MedecinAdresseId == id);
            if (entity == null) return HttpNotFound();

            db.MedecinAdresse.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinAdresse supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private MedecinAdresseFormViewModel BuildFormViewModel(MedecinAdresseFormViewModel model)
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

        private static void MapToEntity(MedecinAdresseFormViewModel model, MedecinAdresse entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.AdresseLigne1 = model.AdresseLigne1;
            entity.Ville = model.Ville;
            entity.Province = model.Province;
            entity.CodePostal = model.CodePostal;
            entity.EstPrincipale = model.EstPrincipale;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
