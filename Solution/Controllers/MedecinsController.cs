using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Solution.Models;
using Solution.ViewModels.Medecins;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class MedecinsController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        [HttpPost]
        public ActionResult GetMedecins(string searchTop, bool? actifsSeulement)
        {
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);

            string dataTableSearch = Request["search[value]"];
            string termeRecherche = !string.IsNullOrWhiteSpace(searchTop)
                ? searchTop.Trim()
                : (!string.IsNullOrWhiteSpace(dataTableSearch) ? dataTableSearch.Trim() : null);

            bool filtreActifs = actifsSeulement ?? false;

            var query = db.Medecin
                .AsNoTracking()
                .Include(m => m.TitreMedecin)
                .Include(m => m.TypePermis)
                .Include(m => m.StatutMedecin)
                .Include(m => m.CategoriePratique)
                .Include(m => m.MedecinTelephone)
                .Include(m => m.MedecinCourriel)
                .Include(m => m.MedecinInstallation.Select(i => i.Installation))
                .Include(m => m.NominationAdministrative)
                .AsQueryable();

            int recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(termeRecherche))
            {
                query = query.Where(m =>
                    (m.Permis != null && m.Permis.Contains(termeRecherche)) ||
                    (m.Nom != null && m.Nom.Contains(termeRecherche)) ||
                    (m.Prenom != null && m.Prenom.Contains(termeRecherche)) ||
                    (m.NAM != null && m.NAM.Contains(termeRecherche)));
            }

            if (filtreActifs)
            {
                query = query.Where(m => m.EstActif);
            }

            int recordsFiltered = query.Count();

            var data = query
                .OrderBy(m => m.Nom)
                .ThenBy(m => m.Prenom)
                .Skip(start)
                .Take(length)
                .ToList()
                .Select(m => new
                {
                    m.MedecinId,
                    m.Permis,
                    NomComplet = ((m.TitreMedecin != null ? m.TitreMedecin.Libelle + " " : "") + (m.Prenom ?? "") + " " + (m.Nom ?? "")).Trim(),
                    TypePermis = m.TypePermis != null ? m.TypePermis.Libelle : "",
                    StatutMedecin = m.StatutMedecin != null ? m.StatutMedecin.Libelle : "",
                    CategoriePratique = m.CategoriePratique != null ? m.CategoriePratique.Libelle : "",
                    TelephonePrincipal = m.MedecinTelephone
                        .OrderByDescending(t => t.EstPrincipal)
                        .ThenBy(t => t.RangSource)
                        .Select(t => t.NumeroTelephone)
                        .FirstOrDefault() ?? "",
                    CourrielPrincipal = m.MedecinCourriel
                        .OrderByDescending(c => c.EstPrincipal)
                        .ThenBy(c => c.RangSource)
                        .Select(c => c.AdresseCourriel)
                        .FirstOrDefault() ?? "",
                    InstallationPrincipale = m.MedecinInstallation
                        .OrderByDescending(i => i.EstPrincipale)
                        .ThenBy(i => i.RangInstallation)
                        .Select(i => i.Installation != null ? i.Installation.NomInstallation : "")
                        .FirstOrDefault() ?? "",
                    NominationActive = m.NominationAdministrative
                        .OrderByDescending(n => n.EstActive)
                        .ThenByDescending(n => n.DateNomination)
                        .Select(n => n.LibelleNomination)
                        .FirstOrDefault() ?? "",
                    EstActif = m.EstActif ? "Oui" : "Non"
                });

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            }, JsonRequestBehavior.AllowGet);
        }
        private object CreerSnapshotAudit(Medecin entity)
        {
            return new
            {
                entity.MedecinId,
                entity.TitreId,
                entity.Permis,
                entity.NAM,
                entity.Nom,
                entity.Prenom,
                entity.TypePermisId,
                entity.StatutMedecinId,
                entity.CategoriePratiqueId,
                entity.GenreId,
                entity.DateNaissance,
                entity.DateEntreeFonction,
                entity.DateDepart,
                entity.Commentaires,
                entity.EstActif,
                entity.DateCreation,
                entity.DateModification
            };
        }

        [HttpGet]
        public ActionResult Index(string search, bool? actifsSeulement)
        {
            ViewBag.Search = search;
            ViewBag.ActifsSeulement = actifsSeulement ?? false;
            return View();
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        [HttpGet]
        public ActionResult Create()
        {
            return RedirectToAction("Edit", new { id = 0 });
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue || id.Value == 0)
            {
                var nouveau = new MedecinEditViewModel
                {
                    EstActif = true
                };

                nouveau.Adresses.Add(new MedecinAdresseEditItemViewModel());
                nouveau.Telephones.Add(new MedecinTelephoneEditItemViewModel());
                nouveau.Courriels.Add(new MedecinCourrielEditItemViewModel());
                nouveau.Installations.Add(new MedecinInstallationEditItemViewModel { RangInstallation = 1 });
                nouveau.UnitesOrganisationnelles.Add(new MedecinUniteOrganisationnelleEditItemViewModel());
                nouveau.NominationsAdministratives.Add(new NominationAdministrativeEditItemViewModel());

                ChargerListesEdit(nouveau);
                return View(nouveau);
            }

            var entity = db.Medecin
                .Include(m => m.MedecinAdresse)
                .Include(m => m.MedecinTelephone)
                .Include(m => m.MedecinCourriel)
                .Include(m => m.MedecinInstallation)
                .Include(m => m.MedecinUniteOrganisationnelle)
                .Include(m => m.NominationAdministrative)
                .FirstOrDefault(m => m.MedecinId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var model = MapEdit(entity);
            ChargerListesEdit(model);
            return View(model);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MedecinEditViewModel model)
        {
            model.Adresses = model.Adresses ?? new List<MedecinAdresseEditItemViewModel>();
            model.Telephones = model.Telephones ?? new List<MedecinTelephoneEditItemViewModel>();
            model.Courriels = model.Courriels ?? new List<MedecinCourrielEditItemViewModel>();
            model.Installations = model.Installations ?? new List<MedecinInstallationEditItemViewModel>();
            model.UnitesOrganisationnelles = model.UnitesOrganisationnelles ?? new List<MedecinUniteOrganisationnelleEditItemViewModel>();
            model.NominationsAdministratives = model.NominationsAdministratives ?? new List<NominationAdministrativeEditItemViewModel>();

            if (!string.IsNullOrWhiteSpace(model.Permis))
            {
                var permis = model.Permis.Trim();
                bool existe = db.Medecin.Any(m => m.Permis == permis && m.MedecinId != model.MedecinId);
                if (existe)
                {
                    ModelState.AddModelError("Permis", "Un autre médecin possède déjà ce permis.");
                }
            }

            if (!ModelState.IsValid)
            {
                ChargerListesEdit(model);
                return View(model);
            }

            Medecin entity;
            bool creation = model.MedecinId == 0;
            object ancienneValeur = null;

            if (creation)
            {
                entity = new Medecin
                {
                    DateCreation = DateTime.Now
                };

                db.Medecin.Add(entity);
            }
            else
            {
                entity = db.Medecin
                    .Include(m => m.MedecinAdresse)
                    .Include(m => m.MedecinTelephone)
                    .Include(m => m.MedecinCourriel)
                    .Include(m => m.MedecinInstallation)
                    .Include(m => m.MedecinUniteOrganisationnelle)
                    .Include(m => m.NominationAdministrative)
                    .FirstOrDefault(m => m.MedecinId == model.MedecinId);

                if (entity == null)
                    return HttpNotFound();

                ancienneValeur = CreerSnapshotAudit(entity);
            }

            entity.TitreId = model.TitreId;
            entity.Permis = (model.Permis ?? "").Trim();
            entity.NAM = NullIfWhiteSpace(model.NAM);
            entity.Nom = (model.Nom ?? "").Trim();
            entity.Prenom = (model.Prenom ?? "").Trim();
            entity.TypePermisId = model.TypePermisId;
            entity.StatutMedecinId = model.StatutMedecinId;
            entity.CategoriePratiqueId = model.CategoriePratiqueId;
            entity.GenreId = model.GenreId;
            entity.DateNaissance = model.DateNaissance;
            entity.DateEntreeFonction = model.DateEntreeFonction;
            entity.DateDepart = model.DateDepart;
            entity.Commentaires = NullIfWhiteSpace(model.Commentaires);
            entity.EstActif = model.EstActif;
            entity.DateModification = DateTime.Now;

            try
            {
                if (creation)
                {
                    db.SaveChanges();
                    model.MedecinId = entity.MedecinId;
                }

                SynchroniserAdresses(entity, model);
                SynchroniserTelephones(entity, model);
                SynchroniserCourriels(entity, model);
                SynchroniserInstallations(entity, model);
                SynchroniserUnitesOrganisationnelles(entity, model);
                SynchroniserNominations(entity, model);

                var nouvelleValeur = CreerSnapshotAudit(entity);

                _audit.Enregistrer(
                    "Medecin",
                    entity.MedecinId,
                    creation ? "INSERT" : "UPDATE",
                    ancienneValeur,
                    nouvelleValeur);

                db.SaveChanges();

                TempData["Success"] = creation
                    ? "Le médecin a été créé avec succès."
                    : "Le médecin a été modifié avec succès.";

                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    string nomEntite = eve.Entry.Entity.GetType().Name;

                    foreach (var ve in eve.ValidationErrors)
                    {
                        var message = "Entité: " + nomEntite +
                                      " | Champ: " + ve.PropertyName +
                                      " | Erreur: " + ve.ErrorMessage;

                        ModelState.AddModelError("", message);
                        System.Diagnostics.Debug.WriteLine(message);
                    }
                }

                ChargerListesEdit(model);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Une erreur est survenue lors de l'enregistrement : " + ex.Message);
                ChargerListesEdit(model);
                return View(model);
            }
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.Medecin
                .AsNoTracking()
                .Include(m => m.TitreMedecin)
                .Include(m => m.TypePermis)
                .Include(m => m.StatutMedecin)
                .Include(m => m.CategoriePratique)
                .Include(m => m.MedecinAdresse)
                .Include(m => m.MedecinTelephone)
                .Include(m => m.MedecinCourriel)
                .Include(m => m.MedecinInstallation)
                .Include(m => m.MedecinUniteOrganisationnelle)
                .Include(m => m.NominationAdministrative)
                .FirstOrDefault(m => m.MedecinId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var model = new MedecinDeleteViewModel
            {
                MedecinId = entity.MedecinId,
                Permis = entity.Permis,
                NomComplet = ((entity.TitreMedecin != null ? entity.TitreMedecin.Libelle + " " : "") + entity.Prenom + " " + entity.Nom).Trim(),
                TypePermis = entity.TypePermis != null ? entity.TypePermis.Libelle : "",
                StatutMedecin = entity.StatutMedecin != null ? entity.StatutMedecin.Libelle : "",
                CategoriePratique = entity.CategoriePratique != null ? entity.CategoriePratique.Libelle : "",
                EstActif = entity.EstActif,
                DateEntreeFonction = entity.DateEntreeFonction,
                DateDepart = entity.DateDepart,
                Commentaires = entity.Commentaires,
                NbAdresses = entity.MedecinAdresse.Count,
                NbTelephones = entity.MedecinTelephone.Count,
                NbCourriels = entity.MedecinCourriel.Count,
                NbInstallations = entity.MedecinInstallation.Count,
                NbUnitesOrganisationnelles = entity.MedecinUniteOrganisationnelle.Count,
                NbNominationsAdministratives = entity.NominationAdministrative.Count
            };

            return View(model);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int MedecinId)
        {
            var entity = db.Medecin
                .Include(m => m.MedecinAdresse)
                .Include(m => m.MedecinTelephone)
                .Include(m => m.MedecinCourriel)
                .Include(m => m.MedecinInstallation)
                .Include(m => m.MedecinUniteOrganisationnelle)
                .Include(m => m.NominationAdministrative)
                .FirstOrDefault(m => m.MedecinId == MedecinId);

            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = new
            {
                Medecin = CreerSnapshotAudit(entity),
                NbAdresses = entity.MedecinAdresse.Count,
                NbTelephones = entity.MedecinTelephone.Count,
                NbCourriels = entity.MedecinCourriel.Count,
                NbInstallations = entity.MedecinInstallation.Count,
                NbUnitesOrganisationnelles = entity.MedecinUniteOrganisationnelle.Count,
                NbNominationsAdministratives = entity.NominationAdministrative.Count
            };

            if (entity.NominationAdministrative.Any())
                db.NominationAdministrative.RemoveRange(entity.NominationAdministrative.ToList());

            if (entity.MedecinUniteOrganisationnelle.Any())
                db.MedecinUniteOrganisationnelle.RemoveRange(entity.MedecinUniteOrganisationnelle.ToList());

            if (entity.MedecinInstallation.Any())
                db.MedecinInstallation.RemoveRange(entity.MedecinInstallation.ToList());

            if (entity.MedecinCourriel.Any())
                db.MedecinCourriel.RemoveRange(entity.MedecinCourriel.ToList());

            if (entity.MedecinTelephone.Any())
                db.MedecinTelephone.RemoveRange(entity.MedecinTelephone.ToList());

            if (entity.MedecinAdresse.Any())
                db.MedecinAdresse.RemoveRange(entity.MedecinAdresse.ToList());

            db.Medecin.Remove(entity);

            _audit.Enregistrer(
                "Medecin",
                MedecinId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "Le médecin et ses données liées ont été supprimés avec succès.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DetailsModal(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var medecin = db.Medecin
                .AsNoTracking()
                .Include(m => m.TitreMedecin)
                .Include(m => m.TypePermis)
                .Include(m => m.StatutMedecin)
                .Include(m => m.CategoriePratique)
                .Include(m => m.Genre)
                .Include(m => m.MedecinAdresse)
                .Include(m => m.MedecinTelephone)
                .Include(m => m.MedecinCourriel)
                .Include(m => m.MedecinInstallation.Select(i => i.Installation))
                .Include(m => m.MedecinUniteOrganisationnelle.Select(u => u.UniteOrganisationnelle))
                .Include(m => m.NominationAdministrative)
                .FirstOrDefault(m => m.MedecinId == id.Value);

            if (medecin == null)
                return HttpNotFound();

            var model = new MedecinDetailsModalViewModel
            {
                MedecinId = medecin.MedecinId,
                Permis = medecin.Permis,
                Titre = medecin.TitreMedecin != null ? medecin.TitreMedecin.Libelle : "",
                Nom = medecin.Nom,
                Prenom = medecin.Prenom,
                NomComplet = ((medecin.TitreMedecin != null ? medecin.TitreMedecin.Libelle + " " : "") + medecin.Prenom + " " + medecin.Nom).Trim(),
                TypePermis = medecin.TypePermis != null ? medecin.TypePermis.Libelle : "",
                StatutMedecin = medecin.StatutMedecin != null ? medecin.StatutMedecin.Libelle : "",
                CategoriePratique = medecin.CategoriePratique != null ? medecin.CategoriePratique.Libelle : "",
                Genre = medecin.Genre != null ? medecin.Genre.Libelle : "",
                DateNaissance = medecin.DateNaissance,
                NAM = medecin.NAM,
                DateEntreeFonction = medecin.DateEntreeFonction,
                DateDepart = medecin.DateDepart,
                EstActif = medecin.EstActif,
                Commentaires = medecin.Commentaires,
                DateCreation = medecin.DateCreation,
                DateModification = medecin.DateModification
            };

            model.Adresses = medecin.MedecinAdresse
                .OrderByDescending(a => a.EstPrincipale)
                .Select(a => new MedecinAdresseItemVm
                {
                    AdresseLigne1 = a.AdresseLigne1,
                    Ville = a.Ville,
                    Province = a.Province,
                    CodePostal = a.CodePostal,
                    EstPrincipale = a.EstPrincipale
                }).ToList();

            model.Telephones = medecin.MedecinTelephone
                .OrderByDescending(t => t.EstPrincipal)
                .ThenBy(t => t.RangSource)
                .Select(t => new MedecinTelephoneItemVm
                {
                    TypeTelephone = t.TypeTelephone,
                    NumeroTelephone = t.NumeroTelephone,
                    EstPrincipal = t.EstPrincipal,
                    RangSource = t.RangSource
                }).ToList();

            model.Courriels = medecin.MedecinCourriel
                .OrderByDescending(c => c.EstPrincipal)
                .ThenBy(c => c.RangSource)
                .Select(c => new MedecinCourrielItemVm
                {
                    TypeCourriel = c.TypeCourriel,
                    AdresseCourriel = c.AdresseCourriel,
                    EstPrincipal = c.EstPrincipal,
                    RangSource = c.RangSource
                }).ToList();

            model.Installations = medecin.MedecinInstallation
                .OrderByDescending(i => i.EstPrincipale)
                .ThenBy(i => i.RangInstallation)
                .Select(i => new MedecinInstallationItemVm
                {
                    NomInstallation = i.Installation != null ? i.Installation.NomInstallation : "",
                    CodeInstallation = i.Installation != null ? i.Installation.CodeInstallation : "",
                    RangInstallation = i.RangInstallation,
                    EstPrincipale = i.EstPrincipale,
                    DateDebut = i.DateDebut,
                    DateFin = i.DateFin
                }).ToList();

            model.UnitesOrganisationnelles = medecin.MedecinUniteOrganisationnelle
                .OrderBy(u => u.RangSource)
                .Select(u => new MedecinUniteOrganisationnelleItemVm
                {
                    Unite = u.UniteOrganisationnelle != null ? u.UniteOrganisationnelle.Libelle : "",
                    TypeUnite = u.UniteOrganisationnelle != null ? u.UniteOrganisationnelle.TypeUnite : "",
                    RoleOrganisationnel = u.RoleOrganisationnel,
                    RangSource = u.RangSource,
                    DateDebut = u.DateDebut,
                    DateFin = u.DateFin
                }).ToList();

            model.NominationsAdministratives = medecin.NominationAdministrative
                .OrderByDescending(n => n.EstActive)
                .ThenByDescending(n => n.DateNomination)
                .Select(n => new NominationAdministrativeItemVm
                {
                    LibelleNomination = n.LibelleNomination,
                    DateNomination = n.DateNomination,
                    DateFin = n.DateFin,
                    EstActive = n.EstActive
                }).ToList();

            var adressePrincipale = model.Adresses.FirstOrDefault(a => a.EstPrincipale) ?? model.Adresses.FirstOrDefault();
            if (adressePrincipale != null)
            {
                model.AdressePrincipale = string.Join(", ",
                    new[] { adressePrincipale.AdresseLigne1, adressePrincipale.Ville, adressePrincipale.Province, adressePrincipale.CodePostal }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            model.TelephonePrincipal = model.Telephones.FirstOrDefault(t => t.EstPrincipal)?.NumeroTelephone ?? model.Telephones.FirstOrDefault()?.NumeroTelephone;
            model.CourrielPrincipal = model.Courriels.FirstOrDefault(c => c.EstPrincipal)?.AdresseCourriel ?? model.Courriels.FirstOrDefault()?.AdresseCourriel;
            model.InstallationPrincipale = model.Installations.FirstOrDefault(i => i.EstPrincipale)?.NomInstallation ?? model.Installations.FirstOrDefault()?.NomInstallation;
            model.NominationActive = model.NominationsAdministratives.FirstOrDefault(n => n.EstActive)?.LibelleNomination ?? model.NominationsAdministratives.FirstOrDefault()?.LibelleNomination;

            return PartialView("_DetailsModal", model);
        }

        private MedecinEditViewModel MapEdit(Medecin entity)
        {
            return new MedecinEditViewModel
            {
                MedecinId = entity.MedecinId,
                TitreId = entity.TitreId,
                Permis = entity.Permis,
                NAM = entity.NAM,
                Nom = entity.Nom,
                Prenom = entity.Prenom,
                TypePermisId = entity.TypePermisId,
                StatutMedecinId = entity.StatutMedecinId,
                CategoriePratiqueId = entity.CategoriePratiqueId,
                GenreId = entity.GenreId,
                DateNaissance = entity.DateNaissance,
                DateEntreeFonction = entity.DateEntreeFonction,
                DateDepart = entity.DateDepart,
                Commentaires = entity.Commentaires,
                EstActif = entity.EstActif,
                DateCreation = entity.DateCreation,
                DateModification = entity.DateModification,
                Adresses = entity.MedecinAdresse
                    .OrderByDescending(a => a.EstPrincipale)
                    .ThenBy(a => a.MedecinAdresseId)
                    .Select(a => new MedecinAdresseEditItemViewModel
                    {
                        MedecinAdresseId = a.MedecinAdresseId,
                        AdresseLigne1 = a.AdresseLigne1,
                        Ville = a.Ville,
                        Province = a.Province,
                        CodePostal = a.CodePostal,
                        EstPrincipale = a.EstPrincipale
                    }).ToList(),
                Telephones = entity.MedecinTelephone
                    .OrderByDescending(t => t.EstPrincipal)
                    .ThenBy(t => t.RangSource)
                    .ThenBy(t => t.MedecinTelephoneId)
                    .Select(t => new MedecinTelephoneEditItemViewModel
                    {
                        MedecinTelephoneId = t.MedecinTelephoneId,
                        TypeTelephone = t.TypeTelephone,
                        NumeroTelephone = t.NumeroTelephone,
                        RangSource = t.RangSource,
                        EstPrincipal = t.EstPrincipal
                    }).ToList(),
                Courriels = entity.MedecinCourriel
                    .OrderByDescending(c => c.EstPrincipal)
                    .ThenBy(c => c.RangSource)
                    .ThenBy(c => c.MedecinCourrielId)
                    .Select(c => new MedecinCourrielEditItemViewModel
                    {
                        MedecinCourrielId = c.MedecinCourrielId,
                        TypeCourriel = c.TypeCourriel,
                        AdresseCourriel = c.AdresseCourriel,
                        RangSource = c.RangSource,
                        EstPrincipal = c.EstPrincipal
                    }).ToList(),
                Installations = entity.MedecinInstallation
                    .OrderByDescending(i => i.EstPrincipale)
                    .ThenBy(i => i.RangInstallation)
                    .ThenBy(i => i.MedecinInstallationId)
                    .Select(i => new MedecinInstallationEditItemViewModel
                    {
                        MedecinInstallationId = i.MedecinInstallationId,
                        InstallationId = i.InstallationId,
                        RangInstallation = i.RangInstallation,
                        EstPrincipale = i.EstPrincipale,
                        DateDebut = i.DateDebut,
                        DateFin = i.DateFin
                    }).ToList(),
                UnitesOrganisationnelles = entity.MedecinUniteOrganisationnelle
                    .OrderBy(u => u.RangSource)
                    .ThenBy(u => u.MedecinUniteOrganisationnelleId)
                    .Select(u => new MedecinUniteOrganisationnelleEditItemViewModel
                    {
                        MedecinUniteOrganisationnelleId = u.MedecinUniteOrganisationnelleId,
                        UniteOrganisationnelleId = u.UniteOrganisationnelleId,
                        RoleOrganisationnel = u.RoleOrganisationnel,
                        RangSource = u.RangSource,
                        DateDebut = u.DateDebut,
                        DateFin = u.DateFin
                    }).ToList(),
                NominationsAdministratives = entity.NominationAdministrative
                    .OrderByDescending(n => n.EstActive)
                    .ThenByDescending(n => n.DateNomination)
                    .ThenBy(n => n.NominationAdministrativeId)
                    .Select(n => new NominationAdministrativeEditItemViewModel
                    {
                        NominationAdministrativeId = n.NominationAdministrativeId,
                        NominationId = n.NominationId,
                        TypeNominationId = n.TypeNominationId,
                        LibelleNomination = n.LibelleNomination,
                        DateNomination = n.DateNomination,
                        DateFin = n.DateFin,
                        EstActive = n.EstActive
                    }).ToList()
            };
        }

        private void SynchroniserAdresses(Medecin entity, MedecinEditViewModel model)
        {
            var idsModel = model.Adresses.Where(x => x.MedecinAdresseId > 0).Select(x => x.MedecinAdresseId).ToList();
            var aSupprimerParAbsence = entity.MedecinAdresse.Where(x => !idsModel.Contains(x.MedecinAdresseId)).ToList();

            foreach (var item in aSupprimerParAbsence)
                db.MedecinAdresse.Remove(item);

            foreach (var item in model.Adresses)
            {
                bool vide = string.IsNullOrWhiteSpace(item.AdresseLigne1)
                    && string.IsNullOrWhiteSpace(item.Ville)
                    && string.IsNullOrWhiteSpace(item.Province)
                    && string.IsNullOrWhiteSpace(item.CodePostal);

                if (item.Supprimer || vide)
                {
                    if (item.MedecinAdresseId > 0)
                    {
                        var existant = entity.MedecinAdresse.FirstOrDefault(x => x.MedecinAdresseId == item.MedecinAdresseId);
                        if (existant != null)
                            db.MedecinAdresse.Remove(existant);
                    }
                    continue;
                }

                MedecinAdresse adresse;
                if (item.MedecinAdresseId > 0)
                {
                    adresse = entity.MedecinAdresse.First(x => x.MedecinAdresseId == item.MedecinAdresseId);
                }
                else
                {
                    adresse = new MedecinAdresse { MedecinId = entity.MedecinId };
                    db.MedecinAdresse.Add(adresse);
                }

                adresse.AdresseLigne1 = NullIfWhiteSpace(item.AdresseLigne1);
                adresse.Ville = NullIfWhiteSpace(item.Ville);
                adresse.Province = NullIfWhiteSpace(item.Province);
                adresse.CodePostal = NullIfWhiteSpace(item.CodePostal);
                adresse.EstPrincipale = item.EstPrincipale;
            }
        }

        private void SynchroniserTelephones(Medecin entity, MedecinEditViewModel model)
        {
            var idsModel = model.Telephones.Where(x => x.MedecinTelephoneId > 0).Select(x => x.MedecinTelephoneId).ToList();
            var aSupprimerParAbsence = entity.MedecinTelephone.Where(x => !idsModel.Contains(x.MedecinTelephoneId)).ToList();

            foreach (var item in aSupprimerParAbsence)
                db.MedecinTelephone.Remove(item);

            foreach (var item in model.Telephones)
            {
                bool vide = string.IsNullOrWhiteSpace(item.TypeTelephone) && string.IsNullOrWhiteSpace(item.NumeroTelephone);

                if (item.Supprimer || vide)
                {
                    if (item.MedecinTelephoneId > 0)
                    {
                        var existant = entity.MedecinTelephone.FirstOrDefault(x => x.MedecinTelephoneId == item.MedecinTelephoneId);
                        if (existant != null)
                            db.MedecinTelephone.Remove(existant);
                    }
                    continue;
                }

                MedecinTelephone tel;
                if (item.MedecinTelephoneId > 0)
                {
                    tel = entity.MedecinTelephone.First(x => x.MedecinTelephoneId == item.MedecinTelephoneId);
                }
                else
                {
                    tel = new MedecinTelephone { MedecinId = entity.MedecinId };
                    db.MedecinTelephone.Add(tel);
                }

                tel.TypeTelephone = NullIfWhiteSpace(item.TypeTelephone);
                tel.NumeroTelephone = NullIfWhiteSpace(item.NumeroTelephone);
                tel.RangSource = item.RangSource;
                tel.EstPrincipal = item.EstPrincipal;
            }
        }

        private void SynchroniserCourriels(Medecin entity, MedecinEditViewModel model)
        {
            var idsModel = model.Courriels.Where(x => x.MedecinCourrielId > 0).Select(x => x.MedecinCourrielId).ToList();
            var aSupprimerParAbsence = entity.MedecinCourriel.Where(x => !idsModel.Contains(x.MedecinCourrielId)).ToList();

            foreach (var item in aSupprimerParAbsence)
                db.MedecinCourriel.Remove(item);

            foreach (var item in model.Courriels)
            {
                bool vide = string.IsNullOrWhiteSpace(item.TypeCourriel) && string.IsNullOrWhiteSpace(item.AdresseCourriel);

                if (item.Supprimer || vide)
                {
                    if (item.MedecinCourrielId > 0)
                    {
                        var existant = entity.MedecinCourriel.FirstOrDefault(x => x.MedecinCourrielId == item.MedecinCourrielId);
                        if (existant != null)
                            db.MedecinCourriel.Remove(existant);
                    }
                    continue;
                }

                MedecinCourriel courriel;
                if (item.MedecinCourrielId > 0)
                {
                    courriel = entity.MedecinCourriel.First(x => x.MedecinCourrielId == item.MedecinCourrielId);
                }
                else
                {
                    courriel = new MedecinCourriel { MedecinId = entity.MedecinId };
                    db.MedecinCourriel.Add(courriel);
                }

                courriel.TypeCourriel = NullIfWhiteSpace(item.TypeCourriel);
                courriel.AdresseCourriel = NullIfWhiteSpace(item.AdresseCourriel);
                courriel.RangSource = item.RangSource;
                courriel.EstPrincipal = item.EstPrincipal;
            }
        }

        private void SynchroniserInstallations(Medecin entity, MedecinEditViewModel model)
        {
            var idsModel = model.Installations.Where(x => x.MedecinInstallationId > 0).Select(x => x.MedecinInstallationId).ToList();
            var aSupprimerParAbsence = entity.MedecinInstallation.Where(x => !idsModel.Contains(x.MedecinInstallationId)).ToList();

            foreach (var item in aSupprimerParAbsence)
                db.MedecinInstallation.Remove(item);

            foreach (var item in model.Installations)
            {
                bool vide = item.InstallationId <= 0;

                if (item.Supprimer || vide)
                {
                    if (item.MedecinInstallationId > 0)
                    {
                        var existant = entity.MedecinInstallation.FirstOrDefault(x => x.MedecinInstallationId == item.MedecinInstallationId);
                        if (existant != null)
                            db.MedecinInstallation.Remove(existant);
                    }
                    continue;
                }

                MedecinInstallation installation;
                if (item.MedecinInstallationId > 0)
                {
                    installation = entity.MedecinInstallation.First(x => x.MedecinInstallationId == item.MedecinInstallationId);
                }
                else
                {
                    installation = new MedecinInstallation { MedecinId = entity.MedecinId };
                    db.MedecinInstallation.Add(installation);
                }

                installation.InstallationId = item.InstallationId;
                installation.RangInstallation = item.RangInstallation;
                installation.EstPrincipale = item.EstPrincipale;
                installation.DateDebut = item.DateDebut;
                installation.DateFin = item.DateFin;
            }
        }

        private void SynchroniserUnitesOrganisationnelles(Medecin entity, MedecinEditViewModel model)
        {
            var idsModel = model.UnitesOrganisationnelles.Where(x => x.MedecinUniteOrganisationnelleId > 0).Select(x => x.MedecinUniteOrganisationnelleId).ToList();
            var aSupprimerParAbsence = entity.MedecinUniteOrganisationnelle.Where(x => !idsModel.Contains(x.MedecinUniteOrganisationnelleId)).ToList();

            foreach (var item in aSupprimerParAbsence)
                db.MedecinUniteOrganisationnelle.Remove(item);

            foreach (var item in model.UnitesOrganisationnelles)
            {
                bool vide = item.UniteOrganisationnelleId <= 0 && string.IsNullOrWhiteSpace(item.RoleOrganisationnel);

                if (item.Supprimer || vide)
                {
                    if (item.MedecinUniteOrganisationnelleId > 0)
                    {
                        var existant = entity.MedecinUniteOrganisationnelle.FirstOrDefault(x => x.MedecinUniteOrganisationnelleId == item.MedecinUniteOrganisationnelleId);
                        if (existant != null)
                            db.MedecinUniteOrganisationnelle.Remove(existant);
                    }
                    continue;
                }

                MedecinUniteOrganisationnelle unite;
                if (item.MedecinUniteOrganisationnelleId > 0)
                {
                    unite = entity.MedecinUniteOrganisationnelle.First(x => x.MedecinUniteOrganisationnelleId == item.MedecinUniteOrganisationnelleId);
                }
                else
                {
                    unite = new MedecinUniteOrganisationnelle { MedecinId = entity.MedecinId };
                    db.MedecinUniteOrganisationnelle.Add(unite);
                }

                unite.UniteOrganisationnelleId = item.UniteOrganisationnelleId;
                unite.RoleOrganisationnel = NullIfWhiteSpace(item.RoleOrganisationnel);
                unite.RangSource = item.RangSource;
                unite.DateDebut = item.DateDebut;
                unite.DateFin = item.DateFin;
            }
        }

        private void SynchroniserNominations(Medecin entity, MedecinEditViewModel model)
        {
            var idsModel = model.NominationsAdministratives
                .Where(x => x.NominationAdministrativeId > 0)
                .Select(x => x.NominationAdministrativeId)
                .ToList();

            var aSupprimerParAbsence = entity.NominationAdministrative
                .Where(x => !idsModel.Contains(x.NominationAdministrativeId))
                .ToList();

            foreach (var item in aSupprimerParAbsence)
                db.NominationAdministrative.Remove(item);

            foreach (var item in model.NominationsAdministratives)
            {
                bool vide =
                    (!item.NominationId.HasValue || item.NominationId.Value <= 0) &&
                    (!item.TypeNominationId.HasValue || item.TypeNominationId.Value <= 0) &&
                    string.IsNullOrWhiteSpace(item.LibelleNomination) &&
                    !item.DateNomination.HasValue &&
                    !item.DateFin.HasValue;

                if (item.Supprimer || vide)
                {
                    if (item.NominationAdministrativeId > 0)
                    {
                        var existant = entity.NominationAdministrative
                            .FirstOrDefault(x => x.NominationAdministrativeId == item.NominationAdministrativeId);

                        if (existant != null)
                            db.NominationAdministrative.Remove(existant);
                    }
                    continue;
                }

                NominationAdministrative nomination;
                if (item.NominationAdministrativeId > 0)
                {
                    nomination = entity.NominationAdministrative
                        .First(x => x.NominationAdministrativeId == item.NominationAdministrativeId);
                }
                else
                {
                    nomination = new NominationAdministrative
                    {
                        MedecinId = entity.MedecinId
                    };
                    db.NominationAdministrative.Add(nomination);
                }

                nomination.NominationId = item.NominationId;
                nomination.TypeNominationId = item.TypeNominationId;
                nomination.LibelleNomination = NullIfWhiteSpace(item.LibelleNomination);
                nomination.DateNomination = item.DateNomination;
                nomination.DateFin = item.DateFin;
                nomination.EstActive = item.EstActive;
            }
        }

        private void ChargerListesEdit(MedecinEditViewModel model)
        {
            model.Titres = db.TitreMedecin
                .Where(x => x.EstActif)
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.TitreId.ToString(),
                    Text = x.Libelle,
                    Selected = model.TitreId.HasValue && x.TitreId == model.TitreId.Value
                }).ToList();

            model.TypesPermis = db.TypePermis
                .Where(x => x.EstActif)
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.TypePermisId.ToString(),
                    Text = x.Libelle,
                    Selected = model.TypePermisId.HasValue && x.TypePermisId == model.TypePermisId.Value
                }).ToList();

            model.StatutsMedecin = db.StatutMedecin
                .Where(x => x.EstActif)
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.StatutMedecinId.ToString(),
                    Text = x.Libelle,
                    Selected = model.StatutMedecinId.HasValue && x.StatutMedecinId == model.StatutMedecinId.Value
                }).ToList();

            model.CategoriesPratique = db.CategoriePratique
                .Where(x => x.EstActif)
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.CategoriePratiqueId.ToString(),
                    Text = x.Libelle,
                    Selected = model.CategoriePratiqueId.HasValue && x.CategoriePratiqueId == model.CategoriePratiqueId.Value
                }).ToList();

            model.Genres = db.Genre
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.GenreId.ToString(),
                    Text = x.Libelle,
                    Selected = model.GenreId.HasValue && x.GenreId == model.GenreId.Value
                }).ToList();

            model.TypesTelephoneDisponibles = new[]
 {
    "Personnel",
    "Professionnel",
    "Cellulaire",
    "Bureau",
    "Clinique",
    "Hospitalier",
    "Résidence",
    "Secrétariat",
    "Urgence",
    "Télécopieur"
}
 .Select(x => new SelectListItem
 {
     Value = x,
     Text = x
 })
 .ToList();

            model.TypesCourrielDisponibles = new[]
            {
    "Personnel",
    "Professionnel",
    "Clinique",
    "Hospitalier",
    "Universitaire",
    "Recherche",
    "Administratif",
    "Secrétariat"
}
            .Select(x => new SelectListItem
            {
                Value = x,
                Text = x
            })
            .ToList();

             

            model.InstallationsDisponibles = db.Installation
                .Where(x => x.EstActif)
                .OrderBy(x => x.NomInstallation)
                .Select(x => new SelectListItem
                {
                    Value = x.InstallationId.ToString(),
                    Text = x.NomInstallation
                }).ToList();

            model.UnitesOrganisationnellesDisponibles = db.UniteOrganisationnelle
                .Where(x => x.EstActif)
                .OrderBy(x => x.Libelle)
                .Select(x => new SelectListItem
                {
                    Value = x.UniteOrganisationnelleId.ToString(),
                    Text = x.Libelle + " (" + x.TypeUnite + ")"
                }).ToList();

            model.NominationsDisponibles = db.Nomination
                .Where(x => x.EstActif)
                .OrderBy(x => x.LibelleNomination)
                .Select(x => new SelectListItem
                {
                    Value = x.NominationId.ToString(),
                    Text = x.LibelleNomination
                }).ToList();

            model.TypesNominationDisponibles = db.TypeNomination
                .Where(x => x.EstActif)
                .OrderBy(x => x.LibelleTypeNomination)
                .Select(x => new SelectListItem
                {
                    Value = x.TypeNominationId.ToString(),
                    Text = x.LibelleTypeNomination
                }).ToList();
        }

        private string NullIfWhiteSpace(string value)
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
