using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Models;
using Solution.ViewModels.EntentesFDC;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class EntentesFDCController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;

        public EntentesFDCController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(EntenteFDC entity)
        {
            return new
            {
                entity.EntenteFDCId,
                entity.MedecinId,
                entity.TypeEntenteFDC,
                entity.DateAdhesion,
                entity.DateFin,
                entity.PEMVise,
                entity.EstActive
            };
        }

        [HttpGet]
        public ActionResult Index(string search = null)
        {
            ViewBag.Search = search;
            return View();
        }

        [HttpPost]
        public ActionResult GetEntentesFDC(string searchTop)
        {
            int draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault());
            int start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            int length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());

            string dataTableSearch = Request.Form["search[value]"].FirstOrDefault();
            string termeRecherche = !string.IsNullOrWhiteSpace(searchTop)
                ? searchTop.Trim()
                : (!string.IsNullOrWhiteSpace(dataTableSearch) ? dataTableSearch.Trim() : null);

            var query = db.EntenteFDC
                .AsNoTracking()
                .Include(x => x.Medecin)
                .AsQueryable();

            int recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(termeRecherche))
            {
                query = query.Where(x =>
                    (x.TypeEntenteFDC != null && x.TypeEntenteFDC.Contains(termeRecherche)) ||
                    (x.PEMVise != null && x.PEMVise.Contains(termeRecherche)) ||
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
                    x.EntenteFDCId,
                    MedecinLibelle = x.Medecin != null
                        ? (x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom)
                        : "",
                    x.TypeEntenteFDC,
                    DateAdhesion = x.DateAdhesion.HasValue ? x.DateAdhesion.Value.ToString("yyyy-MM-dd") : "",
                    DateFin = x.DateFin.HasValue ? x.DateFin.Value.ToString("yyyy-MM-dd") : "",
                    x.PEMVise,
                    EstActive = x.EstActive ? "Oui" : "Non"
                });

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            });
        }

        public ActionResult Details(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (id == null)
                return BadRequest();

            var entity = db.EntenteFDC
                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.EntenteFDCId == id.Value);

            if (entity == null)
                return NotFound();

            var vm = new EntenteFDCDetailsViewModel
            {
                EntenteFDCId = entity.EntenteFDCId,
                MedecinId = entity.MedecinId,
                TypeEntenteFDC = entity.TypeEntenteFDC,
                DateAdhesion = entity.DateAdhesion,
                DateFin = entity.DateFin,
                PEMVise = entity.PEMVise,
                EstActive = entity.EstActive,
                MedecinLibelle = entity.Medecin != null
                    ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom)
                    : ""
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(int? medecinId = null, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            var vm = BuildFormViewModel(new EntenteFDCFormViewModel
            {
                MedecinId = medecinId ?? 0
            });
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(EntenteFDCFormViewModel model, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new EntenteFDC();
            MapToEntity(model, entity);

            db.EntenteFDC.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "EntenteFDC",
                entity.EntenteFDCId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "EntenteFDC créée avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (id == null)
                return BadRequest();

            var entity = db.EntenteFDC.FirstOrDefault(x => x.EntenteFDCId == id.Value);
            if (entity == null)
                return NotFound();

            var vm = new EntenteFDCFormViewModel
            {
                EntenteFDCId = entity.EntenteFDCId,
                MedecinId = entity.MedecinId,
                TypeEntenteFDC = entity.TypeEntenteFDC,
                DateAdhesion = entity.DateAdhesion,
                DateFin = entity.DateFin,
                PEMVise = entity.PEMVise,
                EstActive = entity.EstActive
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(EntenteFDCFormViewModel model, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.EntenteFDC.FirstOrDefault(x => x.EntenteFDCId == model.EntenteFDCId);
            if (entity == null)
                return NotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            MapToEntity(model, entity);

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "EntenteFDC",
                entity.EntenteFDCId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "EntenteFDC modifiée avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (id == null)
                return BadRequest();

            var entity = db.EntenteFDC
                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.EntenteFDCId == id.Value);

            if (entity == null)
                return NotFound();

            var vm = new EntenteFDCDetailsViewModel
            {
                EntenteFDCId = entity.EntenteFDCId,
                MedecinId = entity.MedecinId,
                TypeEntenteFDC = entity.TypeEntenteFDC,
                DateAdhesion = entity.DateAdhesion,
                DateFin = entity.DateFin,
                PEMVise = entity.PEMVise,
                EstActive = entity.EstActive,
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
        public ActionResult DeleteConfirmed(int EntenteFDCId, int? returnMedecinId = null, string returnTab = null)
        {
            var entity = db.EntenteFDC.FirstOrDefault(x => x.EntenteFDCId == EntenteFDCId);
            if (entity == null)
                return NotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.EntenteFDC.Remove(entity);

            _audit.Enregistrer(
                "EntenteFDC",
                EntenteFDCId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "EntenteFDC supprimée avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        private void PreparerRetour(int? returnMedecinId, string returnTab)
        {
            ViewBag.ReturnMedecinId = returnMedecinId;
            ViewBag.ReturnTab = returnTab;
        }

        private ActionResult RedirigerApresAction(int? returnMedecinId, string returnTab)
        {
            if (returnMedecinId.HasValue)
            {
                return RedirectToAction("Index", "Medecins", new
                {
                    openMedecin = returnMedecinId.Value,
                    tab = string.IsNullOrWhiteSpace(returnTab) ? "ententes" : returnTab
                });
            }

            return RedirectToAction("Index");
        }

        private EntenteFDCFormViewModel BuildFormViewModel(EntenteFDCFormViewModel model)
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

        private static void MapToEntity(EntenteFDCFormViewModel model, EntenteFDC entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.TypeEntenteFDC = model.TypeEntenteFDC;
            entity.DateAdhesion = model.DateAdhesion;
            entity.DateFin = model.DateFin;
            entity.PEMVise = model.PEMVise;
            entity.EstActive = model.EstActive;
        }
}
}