using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.ProfilsCEMTL;
using Solution.ViewModels.Shared;

namespace Solution.Controllers
{
    [Authorize]
    public class ProfilsCEMTLController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null, string departement = null, string absence = null, string fdc = null, int page = 1, int pageSize = 25)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 25 : pageSize;

            var query = db.MedecinProfilCEMTL
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
                    (x.Departement1 != null && x.Departement1.Contains(search)) ||
                    (x.Departement2 != null && x.Departement2.Contains(search)) ||
                    (x.ServiceSecteur1 != null && x.ServiceSecteur1.Contains(search)) ||
                    (x.ServiceSecteur2 != null && x.ServiceSecteur2.Contains(search)) ||
                    (x.Courriel1 != null && x.Courriel1.Contains(search)) ||
                    (x.CourrielMed != null && x.CourrielMed.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(departement))
            {
                query = query.Where(x =>
                    (x.Departement1 != null && x.Departement1.Contains(departement)) ||
                    (x.Departement2 != null && x.Departement2.Contains(departement)));
            }

            if (!string.IsNullOrWhiteSpace(absence))
            {
                query = query.Where(x => x.AbsenceTemporaire != null && x.AbsenceTemporaire.Contains(absence));
            }

            if (!string.IsNullOrWhiteSpace(fdc))
            {
                query = query.Where(x => x.EngagementFDC != null && x.EngagementFDC.Contains(fdc));
            }

            var totalItems = query.Count();
            var items = query
                .OrderBy(x => x.Medecin.Nom)
                .ThenBy(x => x.Medecin.Prenom)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new MedecinProfilCEMTLListViewModel
                {
                    MedecinProfilCEMTLId = x.MedecinProfilCEMTLId,
                    MedecinId = x.MedecinId,
                    Telephone1 = x.Telephone1,
                    CellulaireNo = x.CellulaireNo,
                    Telephone2 = x.Telephone2,
                    DomicileNo = x.DomicileNo,
                    Telephone3 = x.Telephone3,
                    PagetteNo = x.PagetteNo,
                    Departement1 = x.Departement1,
                    Departement2 = x.Departement2,
                    ServiceSecteur1 = x.ServiceSecteur1,
                    ServiceSecteur2 = x.ServiceSecteur2,
                    Installation1Source = x.Installation1Source,
                    Installation2Source = x.Installation2Source,
                    Installation3Source = x.Installation3Source,
                    Courriel1 = x.Courriel1,
                    Courriel2 = x.Courriel2,
                    CourrielMed = x.CourrielMed,
                    NominationAdministrativeCourante = x.NominationAdministrativeCourante,
                    DateNominationAdministrative = x.DateNominationAdministrative,
                    DateAvisCongeDepart = x.DateAvisCongeDepart,
                    AbsenceTemporaire = x.AbsenceTemporaire,
                    DateAbsence = x.DateAbsence,
                    DateRetourAbsence = x.DateRetourAbsence,
                    DateDebutPratique = x.DateDebutPratique,
                    DateFinPratique = x.DateFinPratique,
                    EngagementFDC = x.EngagementFDC,
                    DateAdhesionFDC = x.DateAdhesionFDC,
                    DateFinFDC = x.DateFinFDC,
                    PEMVise = x.PEMVise,
                    PEMAilleurs = x.PEMAilleurs,
                    RLS = x.RLS,
                    ListeNomin = x.ListeNomin,
                    Engagement = x.Engagement,
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
                    FitTestCourant = x.FitTestCourant,
                    CommentairesOperationnels = x.CommentairesOperationnels,
                    MedecinNomComplet = x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom
                })
                .ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)System.Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("SuperUtilisateur");
            return View(new Solution.ViewModels.Shared.PagedListViewModel<MedecinProfilCEMTLListViewModel>
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

            var entity = db.MedecinProfilCEMTL
                                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinProfilCEMTLId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new MedecinProfilCEMTLDetailsViewModel
            {
            MedecinProfilCEMTLId = entity.MedecinProfilCEMTLId,
            MedecinId = entity.MedecinId,
            Telephone1 = entity.Telephone1,
            CellulaireNo = entity.CellulaireNo,
            Telephone2 = entity.Telephone2,
            DomicileNo = entity.DomicileNo,
            Telephone3 = entity.Telephone3,
            PagetteNo = entity.PagetteNo,
            Departement1 = entity.Departement1,
            Departement2 = entity.Departement2,
            ServiceSecteur1 = entity.ServiceSecteur1,
            ServiceSecteur2 = entity.ServiceSecteur2,
            Installation1Source = entity.Installation1Source,
            Installation2Source = entity.Installation2Source,
            Installation3Source = entity.Installation3Source,
            Courriel1 = entity.Courriel1,
            Courriel2 = entity.Courriel2,
            CourrielMed = entity.CourrielMed,
            NominationAdministrativeCourante = entity.NominationAdministrativeCourante,
            DateNominationAdministrative = entity.DateNominationAdministrative,
            DateAvisCongeDepart = entity.DateAvisCongeDepart,
            AbsenceTemporaire = entity.AbsenceTemporaire,
            DateAbsence = entity.DateAbsence,
            DateRetourAbsence = entity.DateRetourAbsence,
            DateDebutPratique = entity.DateDebutPratique,
            DateFinPratique = entity.DateFinPratique,
            EngagementFDC = entity.EngagementFDC,
            DateAdhesionFDC = entity.DateAdhesionFDC,
            DateFinFDC = entity.DateFinFDC,
            PEMVise = entity.PEMVise,
            PEMAilleurs = entity.PEMAilleurs,
            RLS = entity.RLS,
            ListeNomin = entity.ListeNomin,
            Engagement = entity.Engagement,
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
            FitTestCourant = entity.FitTestCourant,
            CommentairesOperationnels = entity.CommentairesOperationnels,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new MedecinProfilCEMTLFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(MedecinProfilCEMTLFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new MedecinProfilCEMTL();
            MapToEntity(model, entity);

            db.MedecinProfilCEMTL.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinProfilCEMTL créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinProfilCEMTL.FirstOrDefault(x => x.MedecinProfilCEMTLId == id.Value);
            if (entity == null) return HttpNotFound();

            var vm = new MedecinProfilCEMTLFormViewModel
            {
                MedecinProfilCEMTLId = entity.MedecinProfilCEMTLId,
                MedecinId = entity.MedecinId,
                Telephone1 = entity.Telephone1,
                CellulaireNo = entity.CellulaireNo,
                Telephone2 = entity.Telephone2,
                DomicileNo = entity.DomicileNo,
                Telephone3 = entity.Telephone3,
                PagetteNo = entity.PagetteNo,
                Departement1 = entity.Departement1,
                Departement2 = entity.Departement2,
                ServiceSecteur1 = entity.ServiceSecteur1,
                ServiceSecteur2 = entity.ServiceSecteur2,
                Installation1Source = entity.Installation1Source,
                Installation2Source = entity.Installation2Source,
                Installation3Source = entity.Installation3Source,
                Courriel1 = entity.Courriel1,
                Courriel2 = entity.Courriel2,
                CourrielMed = entity.CourrielMed,
                NominationAdministrativeCourante = entity.NominationAdministrativeCourante,
                DateNominationAdministrative = entity.DateNominationAdministrative,
                DateAvisCongeDepart = entity.DateAvisCongeDepart,
                AbsenceTemporaire = entity.AbsenceTemporaire,
                DateAbsence = entity.DateAbsence,
                DateRetourAbsence = entity.DateRetourAbsence,
                DateDebutPratique = entity.DateDebutPratique,
                DateFinPratique = entity.DateFinPratique,
                EngagementFDC = entity.EngagementFDC,
                DateAdhesionFDC = entity.DateAdhesionFDC,
                DateFinFDC = entity.DateFinFDC,
                PEMVise = entity.PEMVise,
                PEMAilleurs = entity.PEMAilleurs,
                RLS = entity.RLS,
                ListeNomin = entity.ListeNomin,
                Engagement = entity.Engagement,
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
                FitTestCourant = entity.FitTestCourant,
                CommentairesOperationnels = entity.CommentairesOperationnels,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(MedecinProfilCEMTLFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.MedecinProfilCEMTL.FirstOrDefault(x => x.MedecinProfilCEMTLId == model.MedecinProfilCEMTLId);
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinProfilCEMTL modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinProfilCEMTL
                                .Include(x => x.Medecin)
                .AsNoTracking()
                .FirstOrDefault(x => x.MedecinProfilCEMTLId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new MedecinProfilCEMTLDetailsViewModel
            {
            MedecinProfilCEMTLId = entity.MedecinProfilCEMTLId,
            MedecinId = entity.MedecinId,
            Telephone1 = entity.Telephone1,
            CellulaireNo = entity.CellulaireNo,
            Telephone2 = entity.Telephone2,
            DomicileNo = entity.DomicileNo,
            Telephone3 = entity.Telephone3,
            PagetteNo = entity.PagetteNo,
            Departement1 = entity.Departement1,
            Departement2 = entity.Departement2,
            ServiceSecteur1 = entity.ServiceSecteur1,
            ServiceSecteur2 = entity.ServiceSecteur2,
            Installation1Source = entity.Installation1Source,
            Installation2Source = entity.Installation2Source,
            Installation3Source = entity.Installation3Source,
            Courriel1 = entity.Courriel1,
            Courriel2 = entity.Courriel2,
            CourrielMed = entity.CourrielMed,
            NominationAdministrativeCourante = entity.NominationAdministrativeCourante,
            DateNominationAdministrative = entity.DateNominationAdministrative,
            DateAvisCongeDepart = entity.DateAvisCongeDepart,
            AbsenceTemporaire = entity.AbsenceTemporaire,
            DateAbsence = entity.DateAbsence,
            DateRetourAbsence = entity.DateRetourAbsence,
            DateDebutPratique = entity.DateDebutPratique,
            DateFinPratique = entity.DateFinPratique,
            EngagementFDC = entity.EngagementFDC,
            DateAdhesionFDC = entity.DateAdhesionFDC,
            DateFinFDC = entity.DateFinFDC,
            PEMVise = entity.PEMVise,
            PEMAilleurs = entity.PEMAilleurs,
            RLS = entity.RLS,
            ListeNomin = entity.ListeNomin,
            Engagement = entity.Engagement,
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
            FitTestCourant = entity.FitTestCourant,
            CommentairesOperationnels = entity.CommentairesOperationnels,
            MedecinLibelle = (entity.Medecin != null ? (entity.Medecin.Permis + " - " + entity.Medecin.Nom + " " + entity.Medecin.Prenom) : ""),
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.MedecinProfilCEMTL.FirstOrDefault(x => x.MedecinProfilCEMTLId == id);
            if (entity == null) return HttpNotFound();

            db.MedecinProfilCEMTL.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "MedecinProfilCEMTL supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private MedecinProfilCEMTLFormViewModel BuildFormViewModel(MedecinProfilCEMTLFormViewModel model)
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

        private static void MapToEntity(MedecinProfilCEMTLFormViewModel model, MedecinProfilCEMTL entity)
        {
            entity.MedecinId = model.MedecinId;
            entity.Telephone1 = model.Telephone1;
            entity.CellulaireNo = model.CellulaireNo;
            entity.Telephone2 = model.Telephone2;
            entity.DomicileNo = model.DomicileNo;
            entity.Telephone3 = model.Telephone3;
            entity.PagetteNo = model.PagetteNo;
            entity.Departement1 = model.Departement1;
            entity.Departement2 = model.Departement2;
            entity.ServiceSecteur1 = model.ServiceSecteur1;
            entity.ServiceSecteur2 = model.ServiceSecteur2;
            entity.Installation1Source = model.Installation1Source;
            entity.Installation2Source = model.Installation2Source;
            entity.Installation3Source = model.Installation3Source;
            entity.Courriel1 = model.Courriel1;
            entity.Courriel2 = model.Courriel2;
            entity.CourrielMed = model.CourrielMed;
            entity.NominationAdministrativeCourante = model.NominationAdministrativeCourante;
            entity.DateNominationAdministrative = model.DateNominationAdministrative;
            entity.DateAvisCongeDepart = model.DateAvisCongeDepart;
            entity.AbsenceTemporaire = model.AbsenceTemporaire;
            entity.DateAbsence = model.DateAbsence;
            entity.DateRetourAbsence = model.DateRetourAbsence;
            entity.DateDebutPratique = model.DateDebutPratique;
            entity.DateFinPratique = model.DateFinPratique;
            entity.EngagementFDC = model.EngagementFDC;
            entity.DateAdhesionFDC = model.DateAdhesionFDC;
            entity.DateFinFDC = model.DateFinFDC;
            entity.PEMVise = model.PEMVise;
            entity.PEMAilleurs = model.PEMAilleurs;
            entity.RLS = model.RLS;
            entity.ListeNomin = model.ListeNomin;
            entity.Engagement = model.Engagement;
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
            entity.FitTestCourant = model.FitTestCourant;
            entity.CommentairesOperationnels = model.CommentairesOperationnels;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
