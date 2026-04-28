using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.MedecinsComites;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinsComitesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        [HttpPost]
        public ActionResult GetMedecinsComites(string searchTop)
        {
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);

            string dataTableSearch = Request["search[value]"];
            string termeRecherche = !string.IsNullOrWhiteSpace(searchTop)
                ? searchTop.Trim()
                : (!string.IsNullOrWhiteSpace(dataTableSearch) ? dataTableSearch.Trim() : null);

            var query = db.MedecinComite
                .AsNoTracking()
                .Include(x => x.Medecin)
                .Include(x => x.Comite)
                .AsQueryable();

            int recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(termeRecherche))
            {
                query = query.Where(x =>
                    (x.RoleMembre != null && x.RoleMembre.Contains(termeRecherche)) ||
                    (x.Commentaire != null && x.Commentaire.Contains(termeRecherche)) ||
                    (x.Medecin != null && x.Medecin.Permis != null && x.Medecin.Permis.Contains(termeRecherche)) ||
                    (x.Medecin != null && x.Medecin.Nom != null && x.Medecin.Nom.Contains(termeRecherche)) ||
                    (x.Medecin != null && x.Medecin.Prenom != null && x.Medecin.Prenom.Contains(termeRecherche)) ||
                    (x.Comite != null && x.Comite.NomComite != null && x.Comite.NomComite.Contains(termeRecherche)));
            }

            int recordsFiltered = query.Count();

            var data = query
                .OrderBy(x => x.Medecin.Permis)
                .ThenBy(x => x.Medecin.Nom)
                .ThenBy(x => x.Medecin.Prenom)
                .Skip(start)
                .Take(length)
                .ToList()
                .Select(x => new
                {
                    x.MedecinComiteId,
                    MedecinLibelle = x.Medecin != null
                        ? (x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom)
                        : "",
                    ComiteLibelle = x.Comite != null ? x.Comite.NomComite : "",
                    x.RoleMembre,
                    DateDebut = x.DateDebut.HasValue ? x.DateDebut.Value.ToString("yyyy-MM-dd") : "",
                    DateFin = x.DateFin.HasValue ? x.DateFin.Value.ToString("yyyy-MM-dd") : ""
                });

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Index(string search = null)
        {
            ViewBag.Search = search;
            return View();
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinComite
                                .Include(x => x.Medecin)
                                .Include(x => x.Comite)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinComiteId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new MedecinComiteDetailsViewModel
            {
            MedecinComiteId = entity.MedecinComiteId,
            MedecinId = entity.MedecinId,
            ComiteId = entity.ComiteId,
            RoleMembre = entity.RoleMembre,
            DateDebut = entity.DateDebut,
            DateFin = entity.DateFin,
            Commentaire = entity.Commentaire,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            ComiteLibelle = (entity.Comite != null ? entity.Comite.NomComite : ""),
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new MedecinComiteFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(MedecinComiteFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new MedecinComite();
            MapToEntity(model, entity);

            db.MedecinComite.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinComite créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinComite.FirstOrDefault(x => x.MedecinComiteId == id.Value);
            if (entity == null) return HttpNotFound();

            var vm = new MedecinComiteFormViewModel
            {
                MedecinComiteId = entity.MedecinComiteId,
                MedecinId = entity.MedecinId,
                ComiteId = entity.ComiteId,
                RoleMembre = entity.RoleMembre,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                Commentaire = entity.Commentaire,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(MedecinComiteFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.MedecinComite.FirstOrDefault(x => x.MedecinComiteId == model.MedecinComiteId);
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinComite modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinComite
                                .Include(x => x.Medecin)
                                .Include(x => x.Comite)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinComiteId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new MedecinComiteDetailsViewModel
            {
            MedecinComiteId = entity.MedecinComiteId,
            MedecinId = entity.MedecinId,
            ComiteId = entity.ComiteId,
            RoleMembre = entity.RoleMembre,
            DateDebut = entity.DateDebut,
            DateFin = entity.DateFin,
            Commentaire = entity.Commentaire,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            ComiteLibelle = (entity.Comite != null ? entity.Comite.NomComite : ""),
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.MedecinComite.FirstOrDefault(x => x.MedecinComiteId == id);
            if (entity == null) return HttpNotFound();

            db.MedecinComite.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinComite supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private MedecinComiteFormViewModel BuildFormViewModel(MedecinComiteFormViewModel model)
        {
            model.MedecinIdOptions = db.Medecin.AsNoTracking()
                .OrderBy(x => (x.Permis + " - " + x.Nom + " " + x.Prenom))
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = (x.Permis + " - " + x.Nom + " " + x.Prenom)
                })
                .ToList();
            model.ComiteIdOptions = db.Comite.AsNoTracking()
                .OrderBy(x => x.NomComite)
                .Select(x => new SelectListItem
                {
                    Value = x.ComiteId.ToString(),
                    Text = x.NomComite
                })
                .ToList();
            return model;
        }

        private static void MapToEntity(MedecinComiteFormViewModel model, MedecinComite entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.ComiteId = model.ComiteId;
            entity.RoleMembre = model.RoleMembre;
            entity.DateDebut = model.DateDebut;
            entity.DateFin = model.DateFin;
            entity.Commentaire = model.Commentaire;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
