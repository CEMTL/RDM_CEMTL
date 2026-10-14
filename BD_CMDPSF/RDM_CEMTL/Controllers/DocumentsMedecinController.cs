using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Solution.Models;
using Solution.Services;
using Solution.ViewModels.DocumentsMedecin;

namespace Solution.Controllers
{
    [Authorize]
    public class DocumentsMedecinController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;
        private readonly IConfiguration configuration;
        private AuditService Audit => new AuditService(db, HttpContext);

        private static readonly HashSet<string> ExtensionsAutorisees =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".pdf", ".doc", ".docx", ".xls", ".xlsx",
                ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".txt"
            };

        public DocumentsMedecinController(HopitalMedecinsDbEntities db, IConfiguration configuration)
        {
            this.db = db;
            this.configuration = configuration;
        }

        public ActionResult Index(string search = null, int? medecinId = null, bool actifsSeulement = true)
        {
            var query = db.DocumentMedecin
                .AsNoTracking()
                .Include(x => x.Medecin)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(x =>
                    x.Medecin.Permis.Contains(s) ||
                    x.Medecin.Nom.Contains(s) ||
                    x.Medecin.Prenom.Contains(s) ||
                    x.TypeDocument.Contains(s) ||
                    x.NomFichier.Contains(s) ||
                    (x.Description != null && x.Description.Contains(s)));
            }

            if (medecinId.HasValue)
                query = query.Where(x => x.MedecinId == medecinId.Value);

            if (actifsSeulement)
                query = query.Where(x => x.EstActif);

            var items = query
                .OrderBy(x => x.Medecin.Nom)
                .ThenBy(x => x.Medecin.Prenom)
                .ThenByDescending(x => x.DateDocument)
                .ThenByDescending(x => x.DateDepot)
                .Select(x => new DocumentMedecinListViewModel
                {
                    DocumentMedecinId = x.DocumentMedecinId,
                    MedecinId = x.MedecinId,
                    Medecin = x.Medecin.Permis + " - " + x.Medecin.Nom + " " + x.Medecin.Prenom,
                    TypeDocument = x.TypeDocument,
                    NomFichier = x.NomFichier,
                    TailleOctets = x.TailleOctets,
                    DateDocument = x.DateDocument,
                    DateExpiration = x.DateExpiration,
                    Description = x.Description,
                    Confidentiel = x.Confidentiel,
                    DateDepot = x.DateDepot,
                    DeposePar = x.DeposePar,
                    EstActif = x.EstActif
                }).ToList();

            ViewBag.Search = search;
            ViewBag.ActifsSeulement = actifsSeulement;
            ViewBag.MedecinId = new SelectList(
                db.Medecin.AsNoTracking().OrderBy(x => x.Nom).ThenBy(x => x.Prenom)
                    .Select(x => new
                    {
                        x.MedecinId,
                        Texte = x.Permis + " - " + x.Nom + " " + x.Prenom
                    }),
                "MedecinId", "Texte", medecinId);

            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("SuperUtilisateur");
            return View(items);
        }

        public ActionResult Details(int? id, int? returnMedecinId = null, string returnTab = "documents")
        {
            if (!id.HasValue) return BadRequest();

            var entity = db.DocumentMedecin.AsNoTracking()
                .Include(x => x.Medecin)
                .FirstOrDefault(x => x.DocumentMedecinId == id.Value);

            if (entity == null) return NotFound();

            ViewBag.ReturnMedecinId = returnMedecinId;
            ViewBag.ReturnTab = returnTab;
            return View(entity);
        }

        public ActionResult Download(int id)
        {
            var entity = db.DocumentMedecin.AsNoTracking()
                .FirstOrDefault(x => x.DocumentMedecinId == id && x.EstActif);

            if (entity == null) return NotFound();

            return File(entity.Contenu, entity.ContentType, entity.NomFichier);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(int? medecinId = null, string typeDocument = null, string entiteLiee = null, int? entiteId = null, int? returnMedecinId = null, string returnTab = "documents")
        {
            var model = new DocumentMedecinCreateViewModel
            {
                MedecinId = medecinId ?? 0,
                TypeDocument = string.IsNullOrWhiteSpace(typeDocument) ? "Autre" : typeDocument,
                EntiteLiee = entiteLiee,
                EntiteId = entiteId,
                DateDocument = DateTime.Today,
                ReturnMedecinId = returnMedecinId ?? medecinId,
                ReturnTab = returnTab
            };

            BuildCreateView(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        [RequestSizeLimit(30 * 1024 * 1024)]
        public ActionResult Create(DocumentMedecinCreateViewModel model)
        {
            var maxMo = Math.Max(1, configuration.GetValue<int?>("Documents:MaxFileSizeMo") ?? 25);
            var maxBytes = (long)maxMo * 1024L * 1024L;

            if (model.Fichier == null || model.Fichier.Length <= 0)
                ModelState.AddModelError(nameof(model.Fichier), "Le fichier est requis.");
            else
            {
                if (model.Fichier.Length > maxBytes)
                    ModelState.AddModelError(nameof(model.Fichier), $"La taille maximale permise est de {maxMo} Mo.");

                var extension = Path.GetExtension(model.Fichier.FileName);
                if (string.IsNullOrWhiteSpace(extension) || !ExtensionsAutorisees.Contains(extension))
                    ModelState.AddModelError(nameof(model.Fichier), "Ce type de fichier n'est pas autorisé.");
            }

            if (model.DateExpiration.HasValue && model.DateDocument.HasValue &&
                model.DateExpiration.Value.Date < model.DateDocument.Value.Date)
            {
                ModelState.AddModelError(nameof(model.DateExpiration),
                    "La date d'expiration ne peut pas précéder la date du document.");
            }

            if (!db.Medecin.Any(x => x.MedecinId == model.MedecinId))
                ModelState.AddModelError(nameof(model.MedecinId), "Le médecin sélectionné est introuvable.");

            if (!ModelState.IsValid)
            {
                BuildCreateView(model);
                return View(model);
            }

            byte[] contenu;
            using (var ms = new MemoryStream())
            {
                model.Fichier.CopyTo(ms);
                contenu = ms.ToArray();
            }

            var hash = Convert.ToHexString(SHA256.HashData(contenu));
            var nomFichier = Path.GetFileName(model.Fichier.FileName);

            var entity = new DocumentMedecin
            {
                MedecinId = model.MedecinId,
                TypeDocument = model.TypeDocument.Trim(),
                NomFichier = nomFichier,
                ContentType = string.IsNullOrWhiteSpace(model.Fichier.ContentType)
                    ? "application/octet-stream"
                    : model.Fichier.ContentType,
                TailleOctets = contenu.LongLength,
                HashSHA256 = hash,
                DateDocument = model.DateDocument,
                DateExpiration = model.DateExpiration,
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                EntiteLiee = string.IsNullOrWhiteSpace(model.EntiteLiee) ? null : model.EntiteLiee.Trim(),
                EntiteId = model.EntiteId,
                Confidentiel = model.Confidentiel,
                Contenu = contenu,
                DateDepot = DateTime.UtcNow,
                DeposePar = User?.Identity?.Name,
                EstActif = true
            };

            db.DocumentMedecin.Add(entity);
            db.SaveChanges();

            Audit.Enregistrer(
                "DocumentMedecin",
                entity.DocumentMedecinId,
                "INSERT",
                null,
                new
                {
                    entity.MedecinId,
                    entity.TypeDocument,
                    entity.NomFichier,
                    entity.TailleOctets,
                    entity.HashSHA256,
                    entity.DateDocument,
                    entity.DateExpiration,
                    entity.EntiteLiee,
                    entity.EntiteId,
                    entity.Confidentiel
                });
            db.SaveChanges();

            TempData["Success"] = "Document ajouté avec succès.";
            return RedirectAfterSave(model.ReturnMedecinId, model.ReturnTab);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id, int? returnMedecinId = null, string returnTab = "documents")
        {
            if (!id.HasValue) return BadRequest();

            var entity = db.DocumentMedecin.AsNoTracking()
                .Include(x => x.Medecin)
                .FirstOrDefault(x => x.DocumentMedecinId == id.Value);

            if (entity == null) return NotFound();

            ViewBag.ReturnMedecinId = returnMedecinId ?? entity.MedecinId;
            ViewBag.ReturnTab = returnTab;
            return View(entity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id, int? returnMedecinId = null, string returnTab = "documents")
        {
            var entity = db.DocumentMedecin.FirstOrDefault(x => x.DocumentMedecinId == id);
            if (entity == null) return NotFound();

            var medecinId = returnMedecinId ?? entity.MedecinId;
            var old = new
            {
                entity.MedecinId,
                entity.TypeDocument,
                entity.NomFichier,
                entity.DateExpiration,
                entity.EstActif
            };

            entity.EstActif = false;

            Audit.Enregistrer(
                "DocumentMedecin",
                entity.DocumentMedecinId,
                "UPDATE",
                old,
                new
                {
                    entity.MedecinId,
                    entity.TypeDocument,
                    entity.NomFichier,
                    entity.DateExpiration,
                    entity.EstActif
                });

            db.SaveChanges();
            TempData["Success"] = "Document archivé. Le fichier demeure conservé dans l'historique.";
            return RedirectAfterSave(medecinId, returnTab);
        }

        private void BuildCreateView(DocumentMedecinCreateViewModel model)
        {
            ViewBag.Medecins = db.Medecin.AsNoTracking()
                .Where(x => x.EstActif || x.MedecinId == model.MedecinId)
                .OrderBy(x => x.Nom)
                .ThenBy(x => x.Prenom)
                .Select(x => new SelectListItem
                {
                    Value = x.MedecinId.ToString(),
                    Text = x.Permis + " - " + x.Nom + " " + x.Prenom
                }).ToList();

            ViewBag.TypesDocument = new SelectList(new[]
            {
                "Décision / résolution",
                "Privilège",
                "Nomination",
                "Fit Test",
                "Entente FDC",
                "Cotisation",
                "Correspondance",
                "Formulaire",
                "Preuve / attestation",
                "Autre"
            }, model.TypeDocument);
        }

        private ActionResult RedirectAfterSave(int? medecinId, string tab)
        {
            if (medecinId.HasValue && medecinId.Value > 0)
                return RedirectToAction("Index", "Cmdpsf", new
                {
                    openMedecin = medecinId.Value,
                    tab = string.IsNullOrWhiteSpace(tab) ? "documents" : tab
                });

            return RedirectToAction("Index");
        }
    }
}
