using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.MedecinsTelephones;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinsTelephonesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null)
        {
            var query = db.MedecinTelephone
                .Include(x => x.Medecin)
                .AsNoTracking()
                .AsQueryable();
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
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinTelephone
                                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinTelephoneId == id.Value);

            if (entity == null) return HttpNotFound();

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
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new MedecinTelephoneFormViewModel());
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
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinTelephone.FirstOrDefault(x => x.MedecinTelephoneId == id.Value);
            if (entity == null) return HttpNotFound();

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
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinTelephone modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinTelephone
                                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinTelephoneId == id.Value);

            if (entity == null) return HttpNotFound();

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
            if (entity == null) return HttpNotFound();

            db.MedecinTelephone.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinTelephone supprimé avec succès.";
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
