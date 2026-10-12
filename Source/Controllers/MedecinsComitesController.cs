using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Models;
using Solution.Services;
using Solution.ViewModels.MedecinsComites;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinsComitesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;

        public MedecinsComitesController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }

        private AuditService Audit => new AuditService(db, HttpContext);

        [HttpPost]
        public ActionResult GetMedecinsComites(string searchTop)
        {
            int draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault());
            int start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            int length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());

            string dataTableSearch = Request.Form["search[value]"].FirstOrDefault();
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
                draw,
                recordsTotal,
                recordsFiltered,
                data
            });
        }

        [HttpGet]
        public ActionResult Index(string search = null)
        {
            ViewBag.Search = search;
            return View();
        }

        public ActionResult Details(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab);
            if (id == null) return BadRequest();

            var entity = db.MedecinComite
                .Include(x => x.Medecin)
                .Include(x => x.Comite)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinComiteId == id.Value);

            if (entity == null) return NotFound();
            return View(ToDetails(entity));
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(int? medecinId = null, int? returnMedecinId = null, string returnTab = null)
        {
            if (medecinId.HasValue && !returnMedecinId.HasValue)
                returnMedecinId = medecinId;

            PreparerRetour(returnMedecinId, returnTab ?? "comites");
            var vm = BuildFormViewModel(new MedecinComiteFormViewModel
            {
                MedecinId = medecinId ?? 0,
                DateDebut = DateTime.Today
            });
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(MedecinComiteFormViewModel model, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab ?? "comites");
            Valider(model);
            if (!ModelState.IsValid)
                return View(BuildFormViewModel(model));

            var entity = new MedecinComite();
            MapToEntity(model, entity);
            db.MedecinComite.Add(entity);
            db.SaveChanges();

            Audit.Enregistrer("MedecinComite", entity.MedecinComiteId, "INSERT", null, Snapshot(entity));
            db.SaveChanges();

            TempData["Success"] = "Participation au comité créée avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab ?? "comites");
            if (id == null) return BadRequest();

            var entity = db.MedecinComite.FirstOrDefault(x => x.MedecinComiteId == id.Value);
            if (entity == null) return NotFound();

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
        public ActionResult Edit(MedecinComiteFormViewModel model, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab ?? "comites");
            Valider(model);
            if (!ModelState.IsValid)
                return View(BuildFormViewModel(model));

            var entity = db.MedecinComite.FirstOrDefault(x => x.MedecinComiteId == model.MedecinComiteId);
            if (entity == null) return NotFound();

            var oldValue = Snapshot(entity);
            MapToEntity(model, entity);
            var newValue = Snapshot(entity);

            Audit.Enregistrer("MedecinComite", entity.MedecinComiteId, "UPDATE", oldValue, newValue);
            db.SaveChanges();

            TempData["Success"] = "Participation au comité modifiée avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id, int? returnMedecinId = null, string returnTab = null)
        {
            PreparerRetour(returnMedecinId, returnTab ?? "comites");
            if (id == null) return BadRequest();

            var entity = db.MedecinComite
                .Include(x => x.Medecin)
                .Include(x => x.Comite)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinComiteId == id.Value);

            if (entity == null) return NotFound();
            return View(ToDetails(entity));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id, int? returnMedecinId = null, string returnTab = null)
        {
            var entity = db.MedecinComite.FirstOrDefault(x => x.MedecinComiteId == id);
            if (entity == null) return NotFound();

            var oldValue = Snapshot(entity);
            db.MedecinComite.Remove(entity);
            Audit.Enregistrer("MedecinComite", id, "DELETE", oldValue, null);
            db.SaveChanges();

            TempData["Success"] = "Participation au comité supprimée avec succès.";
            return RedirigerApresAction(returnMedecinId, returnTab);
        }

        private void Valider(MedecinComiteFormViewModel model)
        {
            if (model.MedecinId <= 0 || !db.Medecin.Any(x => x.MedecinId == model.MedecinId))
                ModelState.AddModelError(nameof(model.MedecinId), "Le médecin est requis.");

            if (model.ComiteId <= 0 || !db.Comite.Any(x => x.ComiteId == model.ComiteId && x.EstActif))
                ModelState.AddModelError(nameof(model.ComiteId), "Le comité est requis et doit être actif.");

            if (model.DateDebut.HasValue && model.DateFin.HasValue && model.DateFin.Value.Date < model.DateDebut.Value.Date)
                ModelState.AddModelError(nameof(model.DateFin), "La date de fin doit être postérieure ou égale à la date de début.");
        }

        private MedecinComiteFormViewModel BuildFormViewModel(MedecinComiteFormViewModel model)
        {
            model.MedecinIdOptions = db.Medecin.AsNoTracking()
                .Where(x => x.EstActif || x.MedecinId == model.MedecinId)
                .OrderBy(x => x.Permis)
                .ThenBy(x => x.Nom)
                .ThenBy(x => x.Prenom)
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = x.Permis + " - " + x.Nom + " " + x.Prenom,
                    Selected = x.MedecinId == model.MedecinId
                })
                .ToList();

            model.ComiteIdOptions = db.Comite.AsNoTracking()
                .Where(x => x.EstActif || x.ComiteId == model.ComiteId)
                .OrderBy(x => x.NomComite)
                .Select(x => new SelectListItem
                {
                    Value = x.ComiteId.ToString(),
                    Text = x.NomComite,
                    Selected = x.ComiteId == model.ComiteId
                })
                .ToList();
            return model;
        }

        private static MedecinComiteDetailsViewModel ToDetails(MedecinComite entity)
        {
            return new MedecinComiteDetailsViewModel
            {
                MedecinComiteId = entity.MedecinComiteId,
                MedecinId = entity.MedecinId,
                ComiteId = entity.ComiteId,
                RoleMembre = entity.RoleMembre,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                Commentaire = entity.Commentaire,
                MedecinLibelle = entity.Medecin != null
                    ? entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom
                    : "",
                ComiteLibelle = entity.Comite?.NomComite ?? ""
            };
        }

        private static object Snapshot(MedecinComite entity)
        {
            return new
            {
                entity.MedecinComiteId,
                entity.MedecinId,
                entity.ComiteId,
                entity.RoleMembre,
                entity.DateDebut,
                entity.DateFin,
                entity.Commentaire
            };
        }

        private static void MapToEntity(MedecinComiteFormViewModel model, MedecinComite entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.ComiteId = model.ComiteId;
            entity.RoleMembre = string.IsNullOrWhiteSpace(model.RoleMembre) ? null : model.RoleMembre.Trim();
            entity.DateDebut = model.DateDebut;
            entity.DateFin = model.DateFin;
            entity.Commentaire = string.IsNullOrWhiteSpace(model.Commentaire) ? null : model.Commentaire.Trim();
        }

        private void PreparerRetour(int? returnMedecinId, string returnTab)
        {
            ViewBag.ReturnMedecinId = returnMedecinId;
            ViewBag.ReturnTab = string.IsNullOrWhiteSpace(returnTab) ? "comites" : returnTab;
        }

        private ActionResult RedirigerApresAction(int? returnMedecinId, string returnTab)
        {
            if (returnMedecinId.HasValue)
            {
                return RedirectToAction("Index", "Medecins", new
                {
                    openMedecin = returnMedecinId.Value,
                    tab = string.IsNullOrWhiteSpace(returnTab) ? "comites" : returnTab
                });
            }

            return RedirectToAction("Index");
        }
    }
}
