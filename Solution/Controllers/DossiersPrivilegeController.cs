using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.Shared;
using Solution.ViewModels.DossiersPrivilege;

namespace Solution.Controllers
{
    [Authorize]
    public class DossiersPrivilegeController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null, string pem = null, bool? actifSeulement = null, bool? absenceSeulement = null, int page = 1, int pageSize = 25)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 25 : pageSize;

            var query = db.DossierPrivilege
                .AsNoTracking()
                .Include(x => x.Medecin)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    (x.Medecin.Permis != null && x.Medecin.Permis.Contains(search)) ||
                    (x.Medecin.Nom != null && x.Medecin.Nom.Contains(search)) ||
                    (x.Medecin.Prenom != null && x.Medecin.Prenom.Contains(search)) ||
                    (x.EngagementFDC != null && x.EngagementFDC.Contains(search)) ||
                    (x.AbsenceTemporaire != null && x.AbsenceTemporaire.Contains(search)) ||
                    (x.PEMVise != null && x.PEMVise.Contains(search)) ||
                    (x.NumeroResolution != null && x.NumeroResolution.Contains(search)));
            }
            if (!string.IsNullOrWhiteSpace(pem)) query = query.Where(x => x.PEMVise != null && x.PEMVise.Contains(pem));
            if (actifSeulement.HasValue && actifSeulement.Value) query = query.Where(x => x.EstDossierActif);
            if (absenceSeulement.HasValue && absenceSeulement.Value) query = query.Where(x => x.AbsenceTemporaire != null && x.AbsenceTemporaire != "");

            var totalItems = query.Count();
            var items = query
                .OrderByDescending(x => x.EstDossierActif)
                .ThenBy(x => x.Medecin.Nom)
                .ThenBy(x => x.Medecin.Prenom)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DossierPrivilegeListViewModel
                {
                    DossierPrivilegeId = x.DossierPrivilegeId,
                    MedecinId = x.MedecinId,
                    EngagementFDC = x.EngagementFDC,
                    DateAdhesionFDC = x.DateAdhesionFDC,
                    DateFinFDC = x.DateFinFDC,
                    DateAvisCongeDepart = x.DateAvisCongeDepart,
                    AbsenceTemporaire = x.AbsenceTemporaire,
                    DateAbsence = x.DateAbsence,
                    DateRetourAbsence = x.DateRetourAbsence,
                    ListeNomin = x.ListeNomin,
                    Engagement = x.Engagement,
                    DateDebutPratique = x.DateDebutPratique,
                    DateFinPrivileges = x.DateFinPrivileges,
                    DateFinPratique = x.DateFinPratique,
                    PEMVise = x.PEMVise,
                    PEMAilleurs = x.PEMAilleurs,
                    RLS = x.RLS,
                    PrivilegesTexte = x.PrivilegesTexte,
                    PrivilegesEnseignement = x.PrivilegesEnseignement,
                    PrivilegesRecherche = x.PrivilegesRecherche,
                    DemandeMSSS = x.DemandeMSSS,
                    ApprobMSSS = x.ApprobMSSS,
                    DemandeUdeM = x.DemandeUdeM,
                    ApprobUdeM = x.ApprobUdeM,
                    DateDMSP = x.DateDMSP,
                    DateCECMDP = x.DateCECMDP,
                    DateCA = x.DateCA,
                    NumeroResolution = x.NumeroResolution,
                    DateRenouvellement = x.DateRenouvellement,
                    EstDossierActif = x.EstDossierActif,
                    MedecinNomComplet = x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom
                })
                .ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)System.Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("SuperUtilisateur");
            return View(new PagedListViewModel<DossierPrivilegeListViewModel>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            });
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.DossierPrivilege
                                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.DossierPrivilegeId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new DossierPrivilegeDetailsViewModel
            {
            DossierPrivilegeId = entity.DossierPrivilegeId,
            MedecinId = entity.MedecinId,
            EngagementFDC = entity.EngagementFDC,
            DateAdhesionFDC = entity.DateAdhesionFDC,
            DateFinFDC = entity.DateFinFDC,
            DateAvisCongeDepart = entity.DateAvisCongeDepart,
            AbsenceTemporaire = entity.AbsenceTemporaire,
            DateAbsence = entity.DateAbsence,
            DateRetourAbsence = entity.DateRetourAbsence,
            ListeNomin = entity.ListeNomin,
            Engagement = entity.Engagement,
            DateDebutPratique = entity.DateDebutPratique,
            DateFinPrivileges = entity.DateFinPrivileges,
            DateFinPratique = entity.DateFinPratique,
            PEMVise = entity.PEMVise,
            PEMAilleurs = entity.PEMAilleurs,
            RLS = entity.RLS,
            PrivilegesTexte = entity.PrivilegesTexte,
            PrivilegesEnseignement = entity.PrivilegesEnseignement,
            PrivilegesRecherche = entity.PrivilegesRecherche,
            DemandeMSSS = entity.DemandeMSSS,
            ApprobMSSS = entity.ApprobMSSS,
            DemandeUdeM = entity.DemandeUdeM,
            ApprobUdeM = entity.ApprobUdeM,
            DateDMSP = entity.DateDMSP,
            DateCECMDP = entity.DateCECMDP,
            DateCA = entity.DateCA,
            NumeroResolution = entity.NumeroResolution,
            DateRenouvellement = entity.DateRenouvellement,
            EstDossierActif = entity.EstDossierActif,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new DossierPrivilegeFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(DossierPrivilegeFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new DossierPrivilege();
            MapToEntity(model, entity);

            db.DossierPrivilege.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "DossierPrivilege créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.DossierPrivilege.FirstOrDefault(x => x.DossierPrivilegeId == id.Value);
            if (entity == null) return HttpNotFound();

            var vm = new DossierPrivilegeFormViewModel
            {
                DossierPrivilegeId = entity.DossierPrivilegeId,
                MedecinId = entity.MedecinId,
                EngagementFDC = entity.EngagementFDC,
                DateAdhesionFDC = entity.DateAdhesionFDC,
                DateFinFDC = entity.DateFinFDC,
                DateAvisCongeDepart = entity.DateAvisCongeDepart,
                AbsenceTemporaire = entity.AbsenceTemporaire,
                DateAbsence = entity.DateAbsence,
                DateRetourAbsence = entity.DateRetourAbsence,
                ListeNomin = entity.ListeNomin,
                Engagement = entity.Engagement,
                DateDebutPratique = entity.DateDebutPratique,
                DateFinPrivileges = entity.DateFinPrivileges,
                DateFinPratique = entity.DateFinPratique,
                PEMVise = entity.PEMVise,
                PEMAilleurs = entity.PEMAilleurs,
                RLS = entity.RLS,
                PrivilegesTexte = entity.PrivilegesTexte,
                PrivilegesEnseignement = entity.PrivilegesEnseignement,
                PrivilegesRecherche = entity.PrivilegesRecherche,
                DemandeMSSS = entity.DemandeMSSS,
                ApprobMSSS = entity.ApprobMSSS,
                DemandeUdeM = entity.DemandeUdeM,
                ApprobUdeM = entity.ApprobUdeM,
                DateDMSP = entity.DateDMSP,
                DateCECMDP = entity.DateCECMDP,
                DateCA = entity.DateCA,
                NumeroResolution = entity.NumeroResolution,
                DateRenouvellement = entity.DateRenouvellement,
                EstDossierActif = entity.EstDossierActif,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(DossierPrivilegeFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.DossierPrivilege.FirstOrDefault(x => x.DossierPrivilegeId == model.DossierPrivilegeId);
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "DossierPrivilege modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.DossierPrivilege
                                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.DossierPrivilegeId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new DossierPrivilegeDetailsViewModel
            {
            DossierPrivilegeId = entity.DossierPrivilegeId,
            MedecinId = entity.MedecinId,
            EngagementFDC = entity.EngagementFDC,
            DateAdhesionFDC = entity.DateAdhesionFDC,
            DateFinFDC = entity.DateFinFDC,
            DateAvisCongeDepart = entity.DateAvisCongeDepart,
            AbsenceTemporaire = entity.AbsenceTemporaire,
            DateAbsence = entity.DateAbsence,
            DateRetourAbsence = entity.DateRetourAbsence,
            ListeNomin = entity.ListeNomin,
            Engagement = entity.Engagement,
            DateDebutPratique = entity.DateDebutPratique,
            DateFinPrivileges = entity.DateFinPrivileges,
            DateFinPratique = entity.DateFinPratique,
            PEMVise = entity.PEMVise,
            PEMAilleurs = entity.PEMAilleurs,
            RLS = entity.RLS,
            PrivilegesTexte = entity.PrivilegesTexte,
            PrivilegesEnseignement = entity.PrivilegesEnseignement,
            PrivilegesRecherche = entity.PrivilegesRecherche,
            DemandeMSSS = entity.DemandeMSSS,
            ApprobMSSS = entity.ApprobMSSS,
            DemandeUdeM = entity.DemandeUdeM,
            ApprobUdeM = entity.ApprobUdeM,
            DateDMSP = entity.DateDMSP,
            DateCECMDP = entity.DateCECMDP,
            DateCA = entity.DateCA,
            NumeroResolution = entity.NumeroResolution,
            DateRenouvellement = entity.DateRenouvellement,
            EstDossierActif = entity.EstDossierActif,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.DossierPrivilege.FirstOrDefault(x => x.DossierPrivilegeId == id);
            if (entity == null) return HttpNotFound();

            db.DossierPrivilege.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "DossierPrivilege supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private DossierPrivilegeFormViewModel BuildFormViewModel(DossierPrivilegeFormViewModel model)
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

        private static void MapToEntity(DossierPrivilegeFormViewModel model, DossierPrivilege entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.EngagementFDC = model.EngagementFDC;
            entity.DateAdhesionFDC = model.DateAdhesionFDC;
            entity.DateFinFDC = model.DateFinFDC;
            entity.DateAvisCongeDepart = model.DateAvisCongeDepart;
            entity.AbsenceTemporaire = model.AbsenceTemporaire;
            entity.DateAbsence = model.DateAbsence;
            entity.DateRetourAbsence = model.DateRetourAbsence;
            entity.ListeNomin = model.ListeNomin;
            entity.Engagement = model.Engagement;
            entity.DateDebutPratique = model.DateDebutPratique;
            entity.DateFinPrivileges = model.DateFinPrivileges;
            entity.DateFinPratique = model.DateFinPratique;
            entity.PEMVise = model.PEMVise;
            entity.PEMAilleurs = model.PEMAilleurs;
            entity.RLS = model.RLS;
            entity.PrivilegesTexte = model.PrivilegesTexte;
            entity.PrivilegesEnseignement = model.PrivilegesEnseignement;
            entity.PrivilegesRecherche = model.PrivilegesRecherche;
            entity.DemandeMSSS = model.DemandeMSSS;
            entity.ApprobMSSS = model.ApprobMSSS;
            entity.DemandeUdeM = model.DemandeUdeM;
            entity.ApprobUdeM = model.ApprobUdeM;
            entity.DateDMSP = model.DateDMSP;
            entity.DateCECMDP = model.DateCECMDP;
            entity.DateCA = model.DateCA;
            entity.NumeroResolution = model.NumeroResolution;
            entity.DateRenouvellement = model.DateRenouvellement;
            entity.EstDossierActif = model.EstDossierActif;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
