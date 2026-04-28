using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.FitTests;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class FitTestsController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(FitTest entity)
        {
            return new
            {
                entity.FitTestId,
                entity.MedecinId,
                entity.Resultat,
                entity.DateFitTest,
                entity.Commentaire,
                entity.EstActif
            };
        }

        [HttpGet]
        public ActionResult Index(string search = null)
        {
            ViewBag.Search = search;
            return View();
        }

        [HttpPost]
        public ActionResult GetFitTests(string searchTop)
        {
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);

            string dataTableSearch = Request["search[value]"];
            string termeRecherche = !string.IsNullOrWhiteSpace(searchTop)
                ? searchTop.Trim()
                : (!string.IsNullOrWhiteSpace(dataTableSearch) ? dataTableSearch.Trim() : null);

            var query = db.FitTest
                .AsNoTracking()
                .Include(x => x.Medecin)
                .AsQueryable();

            int recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(termeRecherche))
            {
                query = query.Where(x =>
                    (x.Resultat != null && x.Resultat.Contains(termeRecherche)) ||
                    (x.Commentaire != null && x.Commentaire.Contains(termeRecherche)) ||
                    (x.Medecin != null && x.Medecin.Permis != null && x.Medecin.Permis.Contains(termeRecherche)) ||
                    (x.Medecin != null && x.Medecin.Nom != null && x.Medecin.Nom.Contains(termeRecherche)) ||
                    (x.Medecin != null && x.Medecin.Prenom != null && x.Medecin.Prenom.Contains(termeRecherche)));
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
                    x.FitTestId,
                    MedecinLibelle = x.Medecin != null
                        ? (x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom)
                        : "",
                    x.Resultat,
                    DateFitTest = x.DateFitTest.HasValue ? x.DateFitTest.Value.ToString("yyyy-MM-dd") : "",
                    EstActif = x.EstActif ? "Oui" : "Non"
                });

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.FitTest
                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.FitTestId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new FitTestDetailsViewModel
            {
                FitTestId = entity.FitTestId,
                MedecinId = entity.MedecinId,
                Resultat = entity.Resultat,
                DateFitTest = entity.DateFitTest,
                Commentaire = entity.Commentaire,
                EstActif = entity.EstActif,
                MedecinLibelle = entity.Medecin != null
                    ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom)
                    : ""
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new FitTestFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(FitTestFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new FitTest();
            MapToEntity(model, entity);

            db.FitTest.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "FitTest",
                entity.FitTestId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "FitTest créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.FitTest.FirstOrDefault(x => x.FitTestId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new FitTestFormViewModel
            {
                FitTestId = entity.FitTestId,
                MedecinId = entity.MedecinId,
                Resultat = entity.Resultat,
                DateFitTest = entity.DateFitTest,
                Commentaire = entity.Commentaire,
                EstActif = entity.EstActif
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(FitTestFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.FitTest.FirstOrDefault(x => x.FitTestId == model.FitTestId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            MapToEntity(model, entity);

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "FitTest",
                entity.FitTestId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "FitTest modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.FitTest
                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.FitTestId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new FitTestDetailsViewModel
            {
                FitTestId = entity.FitTestId,
                MedecinId = entity.MedecinId,
                Resultat = entity.Resultat,
                DateFitTest = entity.DateFitTest,
                Commentaire = entity.Commentaire,
                EstActif = entity.EstActif,
                MedecinLibelle = entity.Medecin != null
                    ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom)
                    : ""
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int FitTestId)
        {
            var entity = db.FitTest.FirstOrDefault(x => x.FitTestId == FitTestId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.FitTest.Remove(entity);

            _audit.Enregistrer(
                "FitTest",
                FitTestId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "FitTest supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private FitTestFormViewModel BuildFormViewModel(FitTestFormViewModel model)
        {
            model.MedecinIdOptions = db.Medecin
                .AsNoTracking()
                .OrderBy(x => x.Permis)
                .ThenBy(x => x.Nom)
                .ThenBy(x => x.Prenom)
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = x.Permis + " - " + x.Nom + " " + x.Prenom
                })
                .ToList();

            return model;
        }

        private static void MapToEntity(FitTestFormViewModel model, FitTest entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.Resultat = model.Resultat;
            entity.DateFitTest = model.DateFitTest;
            entity.Commentaire = model.Commentaire;
            entity.EstActif = model.EstActif;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}