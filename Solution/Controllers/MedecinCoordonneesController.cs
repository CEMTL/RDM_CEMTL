using Solution.Models;
using Solution.Services;
using Solution.ViewModels.Medecins;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinCoordonneesController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        public ActionResult Index(string search = null, bool? actifsSeulement = null)
        {
            var query = db.Medecin.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    (x.Permis != null && x.Permis.Contains(search)) ||
                    (x.Nom != null && x.Nom.Contains(search)) ||
                    (x.Prenom != null && x.Prenom.Contains(search)) ||
                    (x.NAM != null && x.NAM.Contains(search)));
            }

            if (actifsSeulement.HasValue && actifsSeulement.Value)
            {
                query = query.Where(x => x.EstActif);
            }

            var items = query
                .OrderBy(x => x.Nom)
                .ThenBy(x => x.Prenom)
                .Select(x => new MedecinCoordonneesIndexItemViewModel
                {
                    MedecinId = x.MedecinId,
                    Permis = x.Permis,
                    NomComplet = (x.Nom ?? "") + " " + (x.Prenom ?? ""),
                    EstActif = x.EstActif,
                    NbAdresses = x.MedecinAdresse.Count(),
                    NbTelephones = x.MedecinTelephone.Count(),
                    NbCourriels = x.MedecinCourriel.Count(),
                    NbInstallations = x.MedecinInstallation.Count(),
                    NbUnites = x.MedecinUniteOrganisationnelle.Count()
                })
                .ToList();

            ViewBag.Search = search;
            ViewBag.ActifsSeulement = actifsSeulement.HasValue && actifsSeulement.Value;
            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("SuperUtilisateur");

            return View(items);
        }

        public ActionResult Manage(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var vm = BuildManageViewModel(id.Value);
            if (vm == null)
                return HttpNotFound();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult AddAdresse(MedecinAdresseFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.AdresseLigne1))
                ModelState.AddModelError("NouvelleAdresse.AdresseLigne1", "L'adresse est obligatoire.");

            if (!ModelState.IsValid)
                return View("Manage", BuildManageViewModel(model.MedecinId));

            if (model.EstPrincipale)
                ResetAdressePrincipale(model.MedecinId);

            var entity = new MedecinAdresse
            {
                MedecinId = model.MedecinId,
                AdresseLigne1 = NullIfWhiteSpace(model.AdresseLigne1),
                Ville = NullIfWhiteSpace(model.Ville),
                Province = NullIfWhiteSpace(model.Province),
                CodePostal = NullIfWhiteSpace(model.CodePostal),
                EstPrincipale = model.EstPrincipale
            };

            db.MedecinAdresse.Add(entity);
            db.SaveChanges();

            _audit.Enregistrer("MedecinAdresse", entity.MedecinAdresseId, "INSERT", null, SnapshotAdresse(entity));
            db.SaveChanges();

            TempData["Success"] = "Adresse ajoutée avec succès.";
            return RedirectToAction("Manage", new { id = model.MedecinId });
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult EditAdresse(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinAdresse.AsNoTracking().FirstOrDefault(x => x.MedecinAdresseId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new MedecinAdresseFormViewModel
            {
                MedecinAdresseId = entity.MedecinAdresseId,
                MedecinId = entity.MedecinId,
                AdresseLigne1 = entity.AdresseLigne1,
                Ville = entity.Ville,
                Province = entity.Province,
                CodePostal = entity.CodePostal,
                EstPrincipale = entity.EstPrincipale
            };

            PopulateParentInfo(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult EditAdresse(MedecinAdresseFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.AdresseLigne1))
                ModelState.AddModelError("AdresseLigne1", "L'adresse est obligatoire.");

            if (!ModelState.IsValid)
            {
                PopulateParentInfo(model);
                return View(model);
            }

            var entity = db.MedecinAdresse.FirstOrDefault(x => x.MedecinAdresseId == model.MedecinAdresseId && x.MedecinId == model.MedecinId);
            if (entity == null)
                return HttpNotFound();

            var oldValues = SnapshotAdresse(entity);

            if (model.EstPrincipale)
                ResetAdressePrincipale(model.MedecinId);

            entity.AdresseLigne1 = NullIfWhiteSpace(model.AdresseLigne1);
            entity.Ville = NullIfWhiteSpace(model.Ville);
            entity.Province = NullIfWhiteSpace(model.Province);
            entity.CodePostal = NullIfWhiteSpace(model.CodePostal);
            entity.EstPrincipale = model.EstPrincipale;

            _audit.Enregistrer("MedecinAdresse", entity.MedecinAdresseId, "UPDATE", oldValues, SnapshotAdresse(entity));
            db.SaveChanges();

            TempData["Success"] = "Adresse modifiée avec succès.";
            return RedirectToAction("Manage", new { id = model.MedecinId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteAdresse(int medecinId, int medecinAdresseId)
        {
            var entity = db.MedecinAdresse.FirstOrDefault(x => x.MedecinAdresseId == medecinAdresseId && x.MedecinId == medecinId);
            if (entity == null)
                return HttpNotFound();

            var oldValues = SnapshotAdresse(entity);
            db.MedecinAdresse.Remove(entity);
            _audit.Enregistrer("MedecinAdresse", medecinAdresseId, "DELETE", oldValues, null);
            db.SaveChanges();

            TempData["Success"] = "Adresse supprimée avec succès.";
            return RedirectToAction("Manage", new { id = medecinId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult AddTelephone(MedecinTelephoneFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TypeTelephone))
                ModelState.AddModelError("NouveauTelephone.TypeTelephone", "Le type est obligatoire.");

            if (string.IsNullOrWhiteSpace(model.NumeroTelephone))
                ModelState.AddModelError("NouveauTelephone.NumeroTelephone", "Le numéro est obligatoire.");

            if (!ModelState.IsValid)
                return View("Manage", BuildManageViewModel(model.MedecinId));

            if (model.EstPrincipal)
                ResetTelephonePrincipal(model.MedecinId);

            var entity = new MedecinTelephone
            {
                MedecinId = model.MedecinId,
                TypeTelephone = model.TypeTelephone.Trim(),
                NumeroTelephone = model.NumeroTelephone.Trim(),
                RangSource = model.RangSource,
                EstPrincipal = model.EstPrincipal
            };

            db.MedecinTelephone.Add(entity);
            db.SaveChanges();

            _audit.Enregistrer("MedecinTelephone", entity.MedecinTelephoneId, "INSERT", null, SnapshotTelephone(entity));
            db.SaveChanges();

            TempData["Success"] = "Téléphone ajouté avec succès.";
            return RedirectToAction("Manage", new { id = model.MedecinId });
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult EditTelephone(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinTelephone.AsNoTracking().FirstOrDefault(x => x.MedecinTelephoneId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new MedecinTelephoneFormViewModel
            {
                MedecinTelephoneId = entity.MedecinTelephoneId,
                MedecinId = entity.MedecinId,
                TypeTelephone = entity.TypeTelephone,
                NumeroTelephone = entity.NumeroTelephone,
                RangSource = entity.RangSource,
                EstPrincipal = entity.EstPrincipal
            };

            PopulateParentInfo(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult EditTelephone(MedecinTelephoneFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TypeTelephone))
                ModelState.AddModelError("TypeTelephone", "Le type est obligatoire.");

            if (string.IsNullOrWhiteSpace(model.NumeroTelephone))
                ModelState.AddModelError("NumeroTelephone", "Le numéro est obligatoire.");

            if (!ModelState.IsValid)
            {
                PopulateParentInfo(model);
                return View(model);
            }

            var entity = db.MedecinTelephone.FirstOrDefault(x => x.MedecinTelephoneId == model.MedecinTelephoneId && x.MedecinId == model.MedecinId);
            if (entity == null)
                return HttpNotFound();

            var oldValues = SnapshotTelephone(entity);

            if (model.EstPrincipal)
                ResetTelephonePrincipal(model.MedecinId);

            entity.TypeTelephone = model.TypeTelephone.Trim();
            entity.NumeroTelephone = model.NumeroTelephone.Trim();
            entity.RangSource = model.RangSource;
            entity.EstPrincipal = model.EstPrincipal;

            _audit.Enregistrer("MedecinTelephone", entity.MedecinTelephoneId, "UPDATE", oldValues, SnapshotTelephone(entity));
            db.SaveChanges();

            TempData["Success"] = "Téléphone modifié avec succès.";
            return RedirectToAction("Manage", new { id = model.MedecinId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteTelephone(int medecinId, int medecinTelephoneId)
        {
            var entity = db.MedecinTelephone.FirstOrDefault(x => x.MedecinTelephoneId == medecinTelephoneId && x.MedecinId == medecinId);
            if (entity == null)
                return HttpNotFound();

            var oldValues = SnapshotTelephone(entity);
            db.MedecinTelephone.Remove(entity);
            _audit.Enregistrer("MedecinTelephone", medecinTelephoneId, "DELETE", oldValues, null);
            db.SaveChanges();

            TempData["Success"] = "Téléphone supprimé avec succès.";
            return RedirectToAction("Manage", new { id = medecinId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult AddCourriel(MedecinCourrielFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TypeCourriel))
                ModelState.AddModelError("NouveauCourriel.TypeCourriel", "Le type est obligatoire.");

            if (string.IsNullOrWhiteSpace(model.AdresseCourriel))
                ModelState.AddModelError("NouveauCourriel.AdresseCourriel", "L'adresse courriel est obligatoire.");

            if (!ModelState.IsValid)
                return View("Manage", BuildManageViewModel(model.MedecinId));

            if (model.EstPrincipal)
                ResetCourrielPrincipal(model.MedecinId);

            var entity = new MedecinCourriel
            {
                MedecinId = model.MedecinId,
                TypeCourriel = model.TypeCourriel.Trim(),
                AdresseCourriel = model.AdresseCourriel.Trim(),
                RangSource = model.RangSource,
                EstPrincipal = model.EstPrincipal
            };

            db.MedecinCourriel.Add(entity);
            db.SaveChanges();

            _audit.Enregistrer("MedecinCourriel", entity.MedecinCourrielId, "INSERT", null, SnapshotCourriel(entity));
            db.SaveChanges();

            TempData["Success"] = "Courriel ajouté avec succès.";
            return RedirectToAction("Manage", new { id = model.MedecinId });
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult EditCourriel(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinCourriel.AsNoTracking().FirstOrDefault(x => x.MedecinCourrielId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new MedecinCourrielFormViewModel
            {
                MedecinCourrielId = entity.MedecinCourrielId,
                MedecinId = entity.MedecinId,
                TypeCourriel = entity.TypeCourriel,
                AdresseCourriel = entity.AdresseCourriel,
                RangSource = entity.RangSource,
                EstPrincipal = entity.EstPrincipal
            };

            PopulateParentInfo(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult EditCourriel(MedecinCourrielFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TypeCourriel))
                ModelState.AddModelError("TypeCourriel", "Le type est obligatoire.");

            if (string.IsNullOrWhiteSpace(model.AdresseCourriel))
                ModelState.AddModelError("AdresseCourriel", "L'adresse courriel est obligatoire.");

            if (!ModelState.IsValid)
            {
                PopulateParentInfo(model);
                return View(model);
            }

            var entity = db.MedecinCourriel.FirstOrDefault(x => x.MedecinCourrielId == model.MedecinCourrielId && x.MedecinId == model.MedecinId);
            if (entity == null)
                return HttpNotFound();

            var oldValues = SnapshotCourriel(entity);

            if (model.EstPrincipal)
                ResetCourrielPrincipal(model.MedecinId);

            entity.TypeCourriel = model.TypeCourriel.Trim();
            entity.AdresseCourriel = model.AdresseCourriel.Trim();
            entity.RangSource = model.RangSource;
            entity.EstPrincipal = model.EstPrincipal;

            _audit.Enregistrer("MedecinCourriel", entity.MedecinCourrielId, "UPDATE", oldValues, SnapshotCourriel(entity));
            db.SaveChanges();

            TempData["Success"] = "Courriel modifié avec succès.";
            return RedirectToAction("Manage", new { id = model.MedecinId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteCourriel(int medecinId, int medecinCourrielId)
        {
            var entity = db.MedecinCourriel.FirstOrDefault(x => x.MedecinCourrielId == medecinCourrielId && x.MedecinId == medecinId);
            if (entity == null)
                return HttpNotFound();

            var oldValues = SnapshotCourriel(entity);
            db.MedecinCourriel.Remove(entity);
            _audit.Enregistrer("MedecinCourriel", medecinCourrielId, "DELETE", oldValues, null);
            db.SaveChanges();

            TempData["Success"] = "Courriel supprimé avec succès.";
            return RedirectToAction("Manage", new { id = medecinId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult AddInstallation(MedecinInstallationFormViewModel model)
        {
            if (!model.InstallationId.HasValue)
                ModelState.AddModelError("NouvelleInstallation.InstallationId", "L'installation est obligatoire.");

            if (!ModelState.IsValid)
                return View("Manage", BuildManageViewModel(model.MedecinId));

            if (model.EstPrincipale)
                ResetInstallationPrincipale(model.MedecinId);

            var entity = new MedecinInstallation
            {
                MedecinId = model.MedecinId,
                InstallationId = model.InstallationId.Value,
                RangInstallation = model.RangInstallation,
                DateDebut = model.DateDebut,
                DateFin = model.DateFin,
                EstPrincipale = model.EstPrincipale
            };

            db.MedecinInstallation.Add(entity);
            db.SaveChanges();

            _audit.Enregistrer("MedecinInstallation", entity.MedecinInstallationId, "INSERT", null, SnapshotInstallation(entity));
            db.SaveChanges();

            TempData["Success"] = "Installation ajoutée avec succès.";
            return RedirectToAction("Manage", new { id = model.MedecinId });
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult EditInstallation(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinInstallation.AsNoTracking().FirstOrDefault(x => x.MedecinInstallationId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new MedecinInstallationFormViewModel
            {
                MedecinInstallationId = entity.MedecinInstallationId,
                MedecinId = entity.MedecinId,
                InstallationId = entity.InstallationId,
                RangInstallation = entity.RangInstallation,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                EstPrincipale = entity.EstPrincipale
            };

            PopulateParentInfo(vm);
            vm.InstallationsDisponibles = GetInstallationsDisponibles();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult EditInstallation(MedecinInstallationFormViewModel model)
        {
            if (!model.InstallationId.HasValue)
                ModelState.AddModelError("InstallationId", "L'installation est obligatoire.");

            if (!ModelState.IsValid)
            {
                PopulateParentInfo(model);
                model.InstallationsDisponibles = GetInstallationsDisponibles();
                return View(model);
            }

            var entity = db.MedecinInstallation.FirstOrDefault(x => x.MedecinInstallationId == model.MedecinInstallationId && x.MedecinId == model.MedecinId);
            if (entity == null)
                return HttpNotFound();

            var oldValues = SnapshotInstallation(entity);

            if (model.EstPrincipale)
                ResetInstallationPrincipale(model.MedecinId);

            entity.InstallationId = model.InstallationId.Value;
            entity.RangInstallation = model.RangInstallation;
            entity.DateDebut = model.DateDebut;
            entity.DateFin = model.DateFin;
            entity.EstPrincipale = model.EstPrincipale;

            _audit.Enregistrer("MedecinInstallation", entity.MedecinInstallationId, "UPDATE", oldValues, SnapshotInstallation(entity));
            db.SaveChanges();

            TempData["Success"] = "Installation modifiée avec succès.";
            return RedirectToAction("Manage", new { id = model.MedecinId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteInstallation(int medecinId, int medecinInstallationId)
        {
            var entity = db.MedecinInstallation.FirstOrDefault(x => x.MedecinInstallationId == medecinInstallationId && x.MedecinId == medecinId);
            if (entity == null)
                return HttpNotFound();

            var oldValues = SnapshotInstallation(entity);
            db.MedecinInstallation.Remove(entity);
            _audit.Enregistrer("MedecinInstallation", medecinInstallationId, "DELETE", oldValues, null);
            db.SaveChanges();

            TempData["Success"] = "Installation supprimée avec succès.";
            return RedirectToAction("Manage", new { id = medecinId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult AddUnite(MedecinUniteOrganisationnelleFormViewModel model)
        {
            if (!model.UniteOrganisationnelleId.HasValue)
                ModelState.AddModelError("NouvelleUnite.UniteOrganisationnelleId", "L'unité est obligatoire.");

            if (!ModelState.IsValid)
                return View("Manage", BuildManageViewModel(model.MedecinId));

            var entity = new MedecinUniteOrganisationnelle
            {
                MedecinId = model.MedecinId,
                UniteOrganisationnelleId = model.UniteOrganisationnelleId.Value,
                RangSource = model.RangSource,
                RoleOrganisationnel = NullIfWhiteSpace(model.RoleOrganisationnel),
                DateDebut = model.DateDebut,
                DateFin = model.DateFin
            };

            db.MedecinUniteOrganisationnelle.Add(entity);
            db.SaveChanges();

            _audit.Enregistrer("MedecinUniteOrganisationnelle", entity.MedecinUniteOrganisationnelleId, "INSERT", null, SnapshotUnite(entity));
            db.SaveChanges();

            TempData["Success"] = "Unité organisationnelle ajoutée avec succès.";
            return RedirectToAction("Manage", new { id = model.MedecinId });
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult EditUnite(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.MedecinUniteOrganisationnelle.AsNoTracking().FirstOrDefault(x => x.MedecinUniteOrganisationnelleId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new MedecinUniteOrganisationnelleFormViewModel
            {
                MedecinUniteOrganisationnelleId = entity.MedecinUniteOrganisationnelleId,
                MedecinId = entity.MedecinId,
                UniteOrganisationnelleId = entity.UniteOrganisationnelleId,
                RangSource = entity.RangSource,
                RoleOrganisationnel = entity.RoleOrganisationnel,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin
            };

            PopulateParentInfo(vm);
            vm.UnitesDisponibles = GetUnitesDisponibles();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult EditUnite(MedecinUniteOrganisationnelleFormViewModel model)
        {
            if (!model.UniteOrganisationnelleId.HasValue)
                ModelState.AddModelError("UniteOrganisationnelleId", "L'unité est obligatoire.");

            if (!ModelState.IsValid)
            {
                PopulateParentInfo(model);
                model.UnitesDisponibles = GetUnitesDisponibles();
                return View(model);
            }

            var entity = db.MedecinUniteOrganisationnelle.FirstOrDefault(x => x.MedecinUniteOrganisationnelleId == model.MedecinUniteOrganisationnelleId && x.MedecinId == model.MedecinId);
            if (entity == null)
                return HttpNotFound();

            var oldValues = SnapshotUnite(entity);

            entity.UniteOrganisationnelleId = model.UniteOrganisationnelleId.Value;
            entity.RangSource = model.RangSource;
            entity.RoleOrganisationnel = NullIfWhiteSpace(model.RoleOrganisationnel);
            entity.DateDebut = model.DateDebut;
            entity.DateFin = model.DateFin;

            _audit.Enregistrer("MedecinUniteOrganisationnelle", entity.MedecinUniteOrganisationnelleId, "UPDATE", oldValues, SnapshotUnite(entity));
            db.SaveChanges();

            TempData["Success"] = "Unité organisationnelle modifiée avec succès.";
            return RedirectToAction("Manage", new { id = model.MedecinId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteUnite(int medecinId, int medecinUniteOrganisationnelleId)
        {
            var entity = db.MedecinUniteOrganisationnelle.FirstOrDefault(x => x.MedecinUniteOrganisationnelleId == medecinUniteOrganisationnelleId && x.MedecinId == medecinId);
            if (entity == null)
                return HttpNotFound();

            var oldValues = SnapshotUnite(entity);
            db.MedecinUniteOrganisationnelle.Remove(entity);
            _audit.Enregistrer("MedecinUniteOrganisationnelle", medecinUniteOrganisationnelleId, "DELETE", oldValues, null);
            db.SaveChanges();

            TempData["Success"] = "Unité organisationnelle supprimée avec succès.";
            return RedirectToAction("Manage", new { id = medecinId });
        }

        private MedecinCoordonneesManageViewModel BuildManageViewModel(int medecinId)
        {
            var medecin = db.Medecin
                .AsNoTracking()
                .Include(x => x.MedecinAdresse)
                .Include(x => x.MedecinTelephone)
                .Include(x => x.MedecinCourriel)
                .Include(x => x.MedecinInstallation.Select(i => i.Installation))
                .Include(x => x.MedecinUniteOrganisationnelle.Select(u => u.UniteOrganisationnelle))
                .FirstOrDefault(x => x.MedecinId == medecinId);

            if (medecin == null)
                return null;

            var vm = new MedecinCoordonneesManageViewModel
            {
                MedecinId = medecin.MedecinId,
                Permis = medecin.Permis,
                NomComplet = (medecin.Nom ?? "") + " " + (medecin.Prenom ?? ""),
                EstActif = medecin.EstActif,

                Adresses = medecin.MedecinAdresse
                    .OrderByDescending(x => x.EstPrincipale)
                    .ThenBy(x => x.AdresseLigne1)
                    .Select(x => new MedecinAdresseItemViewModel
                    {
                        MedecinAdresseId = x.MedecinAdresseId,
                        AdresseLigne1 = x.AdresseLigne1,
                        Ville = x.Ville,
                        Province = x.Province,
                        CodePostal = x.CodePostal,
                        EstPrincipale = x.EstPrincipale
                    })
                    .ToList(),

                Telephones = medecin.MedecinTelephone
                    .OrderByDescending(x => x.EstPrincipal)
                    .ThenBy(x => x.TypeTelephone)
                    .Select(x => new MedecinTelephoneItemViewModel
                    {
                        MedecinTelephoneId = x.MedecinTelephoneId,
                        TypeTelephone = x.TypeTelephone,
                        NumeroTelephone = x.NumeroTelephone,
                        RangSource = x.RangSource,
                        EstPrincipal = x.EstPrincipal
                    })
                    .ToList(),

                Courriels = medecin.MedecinCourriel
                    .OrderByDescending(x => x.EstPrincipal)
                    .ThenBy(x => x.TypeCourriel)
                    .Select(x => new MedecinCourrielItemViewModel
                    {
                        MedecinCourrielId = x.MedecinCourrielId,
                        TypeCourriel = x.TypeCourriel,
                        AdresseCourriel = x.AdresseCourriel,
                        RangSource = x.RangSource,
                        EstPrincipal = x.EstPrincipal
                    })
                    .ToList(),

                Installations = medecin.MedecinInstallation
                    .OrderByDescending(x => x.EstPrincipale)
                    .ThenBy(x => x.RangInstallation)
                    .Select(x => new MedecinInstallationItemViewModel
                    {
                        MedecinInstallationId = x.MedecinInstallationId,
                        InstallationId = x.InstallationId,
                        NomInstallation = x.Installation != null ? x.Installation.NomInstallation : "",
                        RangInstallation = x.RangInstallation,
                        DateDebut = x.DateDebut,
                        DateFin = x.DateFin,
                        EstPrincipale = x.EstPrincipale
                    })
                    .ToList(),

                Unites = medecin.MedecinUniteOrganisationnelle
                    .OrderBy(x => x.RangSource)
                    .ThenBy(x => x.UniteOrganisationnelle != null ? x.UniteOrganisationnelle.Libelle : "")
                    .Select(x => new MedecinUniteOrganisationnelleItemViewModel
                    {
                        MedecinUniteOrganisationnelleId = x.MedecinUniteOrganisationnelleId,
                        UniteOrganisationnelleId = x.UniteOrganisationnelleId,
                        LibelleUnite = x.UniteOrganisationnelle != null ? x.UniteOrganisationnelle.Libelle : "",
                        TypeUnite = x.UniteOrganisationnelle != null ? x.UniteOrganisationnelle.TypeUnite : "",
                        RangSource = x.RangSource,
                        RoleOrganisationnel = x.RoleOrganisationnel,
                        DateDebut = x.DateDebut,
                        DateFin = x.DateFin
                    })
                    .ToList(),

                NouvelleAdresse = new MedecinAdresseFormViewModel { MedecinId = medecin.MedecinId },
                NouveauTelephone = new MedecinTelephoneFormViewModel { MedecinId = medecin.MedecinId },
                NouveauCourriel = new MedecinCourrielFormViewModel { MedecinId = medecin.MedecinId },
                NouvelleInstallation = new MedecinInstallationFormViewModel { MedecinId = medecin.MedecinId, RangInstallation = 1, EstPrincipale = false },
                NouvelleUnite = new MedecinUniteOrganisationnelleFormViewModel { MedecinId = medecin.MedecinId },

                InstallationsDisponibles = GetInstallationsDisponibles(),
                UnitesDisponibles = GetUnitesDisponibles()
            };

            return vm;
        }

        private IList<SelectListItem> GetInstallationsDisponibles()
        {
            return db.Installation
                .Where(x => x.EstActif)
                .OrderBy(x => x.NomInstallation)
                .Select(x => new SelectListItem
                {
                    Value = x.InstallationId.ToString(),
                    Text = x.NomInstallation
                })
                .ToList();
        }

        private IList<SelectListItem> GetUnitesDisponibles()
        {
            return db.UniteOrganisationnelle
                .Where(x => x.EstActif)
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.UniteOrganisationnelleId.ToString(),
                    Text = x.Libelle + (x.TypeUnite != null ? " (" + x.TypeUnite + ")" : "")
                })
                .ToList();
        }

        private void ResetAdressePrincipale(int medecinId)
        {
            var entities = db.MedecinAdresse.Where(x => x.MedecinId == medecinId).ToList();
            foreach (var e in entities)
                e.EstPrincipale = false;
        }

        private void ResetTelephonePrincipal(int medecinId)
        {
            var entities = db.MedecinTelephone.Where(x => x.MedecinId == medecinId).ToList();
            foreach (var e in entities)
                e.EstPrincipal = false;
        }

        private void ResetCourrielPrincipal(int medecinId)
        {
            var entities = db.MedecinCourriel.Where(x => x.MedecinId == medecinId).ToList();
            foreach (var e in entities)
                e.EstPrincipal = false;
        }

        private void ResetInstallationPrincipale(int medecinId)
        {
            var entities = db.MedecinInstallation.Where(x => x.MedecinId == medecinId).ToList();
            foreach (var e in entities)
                e.EstPrincipale = false;
        }

        private void PopulateParentInfo(MedecinCoordonneesParentInfoViewModel model)
        {
            var medecin = db.Medecin.AsNoTracking().FirstOrDefault(x => x.MedecinId == model.MedecinId);
            if (medecin != null)
            {
                model.Permis = medecin.Permis;
                model.NomComplet = (medecin.Nom ?? "") + " " + (medecin.Prenom ?? "");
            }
        }

        private object SnapshotAdresse(MedecinAdresse entity)
        {
            return new
            {
                entity.MedecinAdresseId,
                entity.MedecinId,
                entity.AdresseLigne1,
                entity.Ville,
                entity.Province,
                entity.CodePostal,
                entity.EstPrincipale
            };
        }

        private object SnapshotTelephone(MedecinTelephone entity)
        {
            return new
            {
                entity.MedecinTelephoneId,
                entity.MedecinId,
                entity.TypeTelephone,
                entity.NumeroTelephone,
                entity.RangSource,
                entity.EstPrincipal
            };
        }

        private object SnapshotCourriel(MedecinCourriel entity)
        {
            return new
            {
                entity.MedecinCourrielId,
                entity.MedecinId,
                entity.TypeCourriel,
                entity.AdresseCourriel,
                entity.RangSource,
                entity.EstPrincipal
            };
        }

        private object SnapshotInstallation(MedecinInstallation entity)
        {
            return new
            {
                entity.MedecinInstallationId,
                entity.MedecinId,
                entity.InstallationId,
                entity.RangInstallation,
                entity.DateDebut,
                entity.DateFin,
                entity.EstPrincipale
            };
        }

        private object SnapshotUnite(MedecinUniteOrganisationnelle entity)
        {
            return new
            {
                entity.MedecinUniteOrganisationnelleId,
                entity.MedecinId,
                entity.UniteOrganisationnelleId,
                entity.RangSource,
                entity.RoleOrganisationnel,
                entity.DateDebut,
                entity.DateFin
            };
        }

        private static string NullIfWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
