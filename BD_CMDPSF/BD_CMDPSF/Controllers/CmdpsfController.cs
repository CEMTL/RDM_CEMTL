using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Solution.Models;
using Solution.ViewModels;
using Solution.Services;
using Solution.ViewModels.Shared;
using System.Globalization;

namespace Solution.Controllers;

[Authorize]
public class CmdpsfController : Controller
{
    private readonly HopitalMedecinsDbEntities db;
    private static readonly string[] Codes = { "MEDECIN", "DENTISTE", "PHARMACIEN", "SAGE_FEMME" };
    private readonly IAdminExportDocumentService _exports;
    public CmdpsfController(HopitalMedecinsDbEntities db, IAdminExportDocumentService exports)
    {
        this.db = db;
        _exports = exports;
    }
    private AuditService Audit => new AuditService(db, HttpContext);

    private async Task<int[]> ChargerIdsCategoriesPratiqueAsync(string categoriePratique)
    {
        var categories = await db.CategoriePratique.AsNoTracking()
            .Select(c => new { c.CategoriePratiqueId, c.Libelle })
            .ToListAsync();
        return categories
            .Where(c => MedecinCategorieRegistre.Determiner(c.Libelle) == categoriePratique)
            .Select(c => c.CategoriePratiqueId)
            .ToArray();
    }

    [HttpGet]
    public async Task<IActionResult> Index(string profession = null, string recherche = null, int? statutMedecinId = null, string categoriePratique = null)
    {
        categoriePratique = MedecinCategorieRegistre.NormaliserFiltre(categoriePratique);
        ViewBag.Profession = categoriePratique != null ? "MEDECIN" : profession;
        ViewBag.CategoriePratique = categoriePratique;
        ViewBag.Recherche = recherche;
        // Afficher tous les statuts définis en base, y compris ceux non actifs.
        // Aucun filtrage par EstActif ne s'applique au registre.
        var statuts = await db.StatutMedecin.AsNoTracking()
            .OrderBy(s => s.Libelle)
            .Select(s => new { s.StatutMedecinId, s.Libelle })
            .ToListAsync();
        ViewBag.StatutsMedecin = statuts.Select(s => new SelectListItem
        {
            Value = s.StatutMedecinId.ToString(),
            Text = s.Libelle,
            Selected = statutMedecinId == s.StatutMedecinId
        }).ToList();
        return View();
    }

    // Mêmes colonnes et stratégie de pagination SQL que le registre Medecins.
    [HttpPost]
    public async Task<IActionResult> GetRegistre(string profession = null, string recherche = null, int? statutMedecinId = null, string categoriePratique = null)
    {
        var form = Request.Form;
        int.TryParse(form["draw"].FirstOrDefault(), out var draw);
        int.TryParse(form["start"].FirstOrDefault(), out var start);
        int.TryParse(form["length"].FirstOrDefault(), out var length);
        start = Math.Max(0, start);
        length = length < 1 ? 10 : Math.Min(100, length);
        var query = db.Medecin.AsNoTracking().Select(m => new
        {
            m.MedecinId,
            m.Permis,
            m.Nom,
            m.Prenom,
            m.EstActif,
            m.StatutMedecinId,
            m.CategoriePratiqueId,
            CodeProfession = m.Profession != null ? m.Profession.CodeProfession : "MEDECIN",
            Profession = m.Profession != null ? m.Profession.Libelle : "Médecin",
            NomComplet = ((m.TitreMedecin != null ? m.TitreMedecin.Libelle : "") + " " + m.Prenom + " " + m.Nom).Trim(),
            TypePermis = m.TypePermis != null ? m.TypePermis.Libelle : "",
            StatutMedecin = m.StatutMedecin != null ? m.StatutMedecin.Libelle : "",
            CategoriePratique = m.CategoriePratique != null ? m.CategoriePratique.Libelle : "",
            TelephonePrincipal = m.MedecinTelephone.OrderByDescending(t => t.EstPrincipal)
                .ThenBy(t => t.RangSource).Select(t => t.NumeroTelephone).FirstOrDefault() ?? "",
            CourrielPrincipal = m.MedecinCourriel.OrderByDescending(t => t.EstPrincipal)
                .ThenBy(t => t.RangSource).Select(t => t.AdresseCourriel).FirstOrDefault() ?? "",
            InstallationPrincipale = m.MedecinInstallation.OrderByDescending(t => t.EstPrincipale)
                .ThenBy(t => t.RangInstallation).Select(t => t.Installation.NomInstallation).FirstOrDefault() ?? "",
            NominationActive = m.NominationAdministrative.OrderByDescending(t => t.EstActive)
                .ThenByDescending(t => t.DateNomination).Select(t => t.LibelleNomination).FirstOrDefault() ?? ""
        });
        int total = await query.CountAsync();
        if (!string.IsNullOrWhiteSpace(profession) && Codes.Contains(profession))
            query = query.Where(m => m.CodeProfession == profession);
        // Filtrage par catégorie conforme aux compteurs du tableau de bord.
        // La sélection ne tient compte ni de EstActif ni de StatutMedecinId.
        categoriePratique = MedecinCategorieRegistre.NormaliserFiltre(categoriePratique);
        if (categoriePratique != null)
        {
            var idsCategories = await ChargerIdsCategoriesPratiqueAsync(categoriePratique);
            query = query.Where(m => m.CodeProfession == "MEDECIN" &&
                m.CategoriePratiqueId.HasValue && idsCategories.Contains(m.CategoriePratiqueId.Value));
        }
        // Filtre sur l'identifiant de statut du médecin, sans aucune condition EstActif.
        // La valeur null signifie « Tous les statuts » (y compris les statuts non renseignés).
        if (statutMedecinId.HasValue)
            query = query.Where(m => m.StatutMedecinId == statutMedecinId.Value);
        var termes = (recherche ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var terme in termes)
        {
            var motif = "%" + terme.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
            query = query.Where(m =>
                EF.Functions.Like(EF.Functions.Collate(m.Permis, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.NomComplet, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.Profession, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.TypePermis, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.StatutMedecin, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.CategoriePratique, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.TelephonePrincipal, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.CourrielPrincipal, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.InstallationPrincipale, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.NominationActive, "Latin1_General_100_CI_AI"), motif));
        }
        int filtered = await query.CountAsync();
        int.TryParse(form["order[0][column]"].FirstOrDefault(), out var sortColumn);
        bool desc = form["order[0][dir]"].FirstOrDefault() == "desc";
        query = sortColumn switch
        {
            0 => desc ? query.OrderByDescending(x => x.Profession) : query.OrderBy(x => x.Profession),
            1 => desc ? query.OrderByDescending(x => x.Permis) : query.OrderBy(x => x.Permis),
            2 => desc ? query.OrderByDescending(x => x.Nom).ThenByDescending(x => x.Prenom) : query.OrderBy(x => x.Nom).ThenBy(x => x.Prenom),
            3 => desc ? query.OrderByDescending(x => x.TypePermis) : query.OrderBy(x => x.TypePermis),
            4 => desc ? query.OrderByDescending(x => x.StatutMedecin) : query.OrderBy(x => x.StatutMedecin),
            5 => desc ? query.OrderByDescending(x => x.CategoriePratique) : query.OrderBy(x => x.CategoriePratique),
            6 => desc ? query.OrderByDescending(x => x.TelephonePrincipal) : query.OrderBy(x => x.TelephonePrincipal),
            7 => desc ? query.OrderByDescending(x => x.CourrielPrincipal) : query.OrderBy(x => x.CourrielPrincipal),
            8 => desc ? query.OrderByDescending(x => x.InstallationPrincipale) : query.OrderBy(x => x.InstallationPrincipale),
            9 => desc ? query.OrderByDescending(x => x.NominationActive) : query.OrderBy(x => x.NominationActive),
            10 => desc ? query.OrderByDescending(x => x.EstActif) : query.OrderBy(x => x.EstActif),
            _ => query.OrderBy(x => x.Nom).ThenBy(x => x.Prenom)
        };
        var rows = await query.Skip(start).Take(length).ToListAsync();
        return Json(new { draw, recordsTotal = total, recordsFiltered = filtered,
            data = rows.Select(x => new {
                x.MedecinId, x.Profession, x.CodeProfession, x.Permis, x.NomComplet,
                x.TypePermis, x.StatutMedecin, x.CategoriePratique, x.TelephonePrincipal,
                x.CourrielPrincipal, x.InstallationPrincipale, x.NominationActive,
                EstActif = x.EstActif ? "Oui" : "Non", EstActifBool = x.EstActif
            }) });
    }

    // L'export relit les résultats depuis SQL Server, et ne dépend pas des 10/25/50 lignes affichées.
    // Aucune pagination DataTables n’est appliquée à l’export (toutes les lignes filtrées).
    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Exporter(string format, string profession = null,
        string recherche = null, int? statutMedecinId = null, string categoriePratique = null)
    {
        format = (format ?? string.Empty).Trim().ToLowerInvariant();
        if (format != "pdf" && format != "word" && format != "excel")
            return BadRequest("Format d'export non reconnu.");

        var query = db.Medecin.AsNoTracking().Select(m => new
        {
            m.MedecinId,
            m.Permis,
            m.Nom,
            m.Prenom,
            m.EstActif,
            m.StatutMedecinId,
            m.CategoriePratiqueId,
            CodeProfession = m.Profession != null ? m.Profession.CodeProfession : "MEDECIN",
            Profession = m.Profession != null ? m.Profession.Libelle : "Médecin",
            NomComplet = ((m.TitreMedecin != null ? m.TitreMedecin.Libelle : "") + " " + m.Prenom + " " + m.Nom).Trim(),
            TypePermis = m.TypePermis != null ? m.TypePermis.Libelle : "",
            StatutMedecin = m.StatutMedecin != null ? m.StatutMedecin.Libelle : "",
            CategoriePratique = m.CategoriePratique != null ? m.CategoriePratique.Libelle : "",
            TelephonePrincipal = m.MedecinTelephone.OrderByDescending(t => t.EstPrincipal)
                .ThenBy(t => t.RangSource).Select(t => t.NumeroTelephone).FirstOrDefault() ?? "",
            CourrielPrincipal = m.MedecinCourriel.OrderByDescending(t => t.EstPrincipal)
                .ThenBy(t => t.RangSource).Select(t => t.AdresseCourriel).FirstOrDefault() ?? "",
            InstallationPrincipale = m.MedecinInstallation.OrderByDescending(t => t.EstPrincipale)
                .ThenBy(t => t.RangInstallation).Select(t => t.Installation.NomInstallation).FirstOrDefault() ?? "",
            NominationActive = m.NominationAdministrative.OrderByDescending(t => t.EstActive)
                .ThenByDescending(t => t.DateNomination).Select(t => t.LibelleNomination).FirstOrDefault() ?? ""
        });
        if (!string.IsNullOrWhiteSpace(profession) && Codes.Contains(profession))
            query = query.Where(m => m.CodeProfession == profession);
        // Filtrage par catégorie conforme aux compteurs du tableau de bord.
        // La sélection ne tient compte ni de EstActif ni de StatutMedecinId.
        categoriePratique = MedecinCategorieRegistre.NormaliserFiltre(categoriePratique);
        if (categoriePratique != null)
        {
            var idsCategories = await ChargerIdsCategoriesPratiqueAsync(categoriePratique);
            query = query.Where(m => m.CodeProfession == "MEDECIN" &&
                m.CategoriePratiqueId.HasValue && idsCategories.Contains(m.CategoriePratiqueId.Value));
        }
        // Filtre sur l'identifiant de statut du médecin, sans aucune condition EstActif.
        // La valeur null signifie « Tous les statuts » (y compris les statuts non renseignés).
        if (statutMedecinId.HasValue)
            query = query.Where(m => m.StatutMedecinId == statutMedecinId.Value);
        var termes = (recherche ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var terme in termes)
        {
            var motif = "%" + terme.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
            query = query.Where(m =>
                EF.Functions.Like(EF.Functions.Collate(m.Permis, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.NomComplet, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.Profession, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.TypePermis, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.StatutMedecin, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.CategoriePratique, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.TelephonePrincipal, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.CourrielPrincipal, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.InstallationPrincipale, "Latin1_General_100_CI_AI"), motif) ||
                EF.Functions.Like(EF.Functions.Collate(m.NominationActive, "Latin1_General_100_CI_AI"), motif));
        }
        var nombre = await query.CountAsync();
        if (nombre == 0) return BadRequest("Aucune donnée à exporter pour les filtres demandés.");

        // Contrairement à GetRegistre, aucun Skip/Take : toutes les lignes filtrées.
        var rows = await query.OrderBy(x => x.Nom).ThenBy(x => x.Prenom)
            .ThenBy(x => x.MedecinId).ToListAsync();
        // Sécurité : ne jamais produire un export partiel silencieusement.
        if (rows.Count != nombre)
            return StatusCode(409, $"Le nombre de dossiers a changé durant l'export : attendus {nombre}, obtenus {rows.Count}. Relancez l'export.");
        string libelleStatut = "Tous les statuts";
        if (statutMedecinId.HasValue)
        {
            var libelle = await db.StatutMedecin.AsNoTracking()
                .Where(s => s.StatutMedecinId == statutMedecinId.Value)
                .Select(s => s.Libelle)
                .FirstOrDefaultAsync();
            libelleStatut = !string.IsNullOrWhiteSpace(libelle)
                ? $"{libelle} (ID {statutMedecinId.Value})"
                : $"ID {statutMedecinId.Value}";
        }
        var demande = new AdminTableExportRequest
        {
            Format = format,
            Titre = "Registre CMDPSF",
            PageSource = "Cmdpsf/Index",
            Filtres = $"Profession : {profession ?? "Toutes"} ; Catégorie de pratique : {categoriePratique ?? "Toutes"} ; Recherche : {recherche ?? ""} ; Statut médecin : {libelleStatut}",
            Entetes = new List<string> {
                "Profession", "Permis", "Professionnel", "Type permis", "Statut",
                "Catégorie", "Téléphone", "Courriel", "Installation", "Nomination", "Actif"
            },
            Lignes = rows.Select(x => new List<string> {
                x.Profession ?? "", x.Permis ?? "", x.NomComplet ?? "",
                x.TypePermis ?? "", x.StatutMedecin ?? "", x.CategoriePratique ?? "",
                x.TelephonePrincipal ?? "", x.CourrielPrincipal ?? "",
                x.InstallationPrincipale ?? "", x.NominationActive ?? "",
                x.EstActif ? "Oui" : "Non"
            }).ToList()
        };
        byte[] contenu;
        string mediaType, extension;
        switch (format)
        {
            case "pdf":
                contenu = _exports.CreerPdf(demande);
                mediaType = "application/pdf";
                extension = "pdf";
                break;
            case "word":
                contenu = _exports.CreerWord(demande);
                mediaType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                extension = "docx";
                break;
            default:
                contenu = _exports.CreerExcel(demande);
                mediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                extension = "xlsx";
                break;
        }
        Response.Headers["X-Export-Row-Count"] = rows.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Response.Headers["Cache-Control"] = "no-store";
        return File(contenu, mediaType,
            $"CMDPSF_Registre_{rows.Count}_lignes_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}");
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var entity = await db.Medecin.AsNoTracking().Include(x => x.Profession)
            .FirstOrDefaultAsync(x => x.MedecinId == id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,SuperUtilisateur")]
    public async Task<IActionResult> ChangerEtat(int id, bool actif)
    {
        var entity = await db.Medecin.Include(x => x.Profession).FirstOrDefaultAsync(x => x.MedecinId == id);
        if (entity == null) return NotFound();
        if ((entity.Profession?.CodeProfession == "MEDECIN" || entity.Profession == null))
            return BadRequest("Utilisez les procédures du registre médical pour les médecins.");
        if (entity.EstActif != actif)
        {
            var avant = new { entity.EstActif, entity.DateModification };
            entity.EstActif = actif;
            entity.DateModification = DateTime.Now;
            Audit.Enregistrer("Medecin", id, "UPDATE", avant,
                new { entity.EstActif, entity.DateModification });
            await db.SaveChangesAsync();
        }
        TempData["Success"] = actif ? "Dossier réactivé." : "Dossier désactivé.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Suppression logique volontaire : aucun effacement des cotisations, privilèges,
    // documents ou affectations historiquement reliés au MedecinId.
    // Accessible uniquement au rôle Admin, avec POST et jeton antiforgery.
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Supprimer(int id)
    {
        var entity = await db.Medecin.FirstOrDefaultAsync(x => x.MedecinId == id);
        if (entity == null) return NotFound();

        if (entity.EstActif)
        {
            var avant = new { entity.EstActif, entity.DateModification };
            entity.EstActif = false;
            entity.DateModification = DateTime.Now;
            Audit.Enregistrer("Medecin", id, "UPDATE", avant,
                new { entity.EstActif, entity.DateModification });
            await db.SaveChangesAsync();
        }

        TempData["Success"] = "Dossier retiré du registre actif et conservé dans l'historique. Aucune donnée associée n'a été supprimée.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet, Authorize(Roles = "Admin,SuperUtilisateur")]
    public IActionResult Creer(string profession = "DENTISTE")
    {
        profession = (profession ?? "DENTISTE").Trim().ToUpperInvariant();
        if (!Codes.Contains(profession)) profession = "DENTISTE";
        return View("Fiche", new CmdpsfProfessionViewModel { CodeProfession = profession });
    }

    [HttpGet, Authorize(Roles = "Admin,SuperUtilisateur")]
    public async Task<IActionResult> Modifier(int id)
    {
        var e = await db.Medecin.Include(x => x.Profession).FirstOrDefaultAsync(x => x.MedecinId == id);
        if (e == null) return NotFound();
        // Les médecins conservent leur écran historique.
        if ((e.Profession?.CodeProfession == "MEDECIN" || e.Profession == null)) return RedirectToAction("Edit", "Medecins", new { id });
        return View("Fiche", new CmdpsfProfessionViewModel {
            MedecinId = e.MedecinId, CodeProfession = e.Profession.CodeProfession,
            Permis = e.Permis, Nom = e.Nom, Prenom = e.Prenom, EstActif = e.EstActif,
            DateEntreeFonction = e.DateEntreeFonction, DateDepart = e.DateDepart,
            DateObtentionPermis = e.DateObtentionPermis, DateExpirationPermis = e.DateExpirationPermis,
            NumeroMembreOrdre = e.NumeroMembreOrdre, DateDerniereVerificationOrdre = e.DateDerniereVerificationOrdre,
            ReferenceVerificationOrdre = e.ReferenceVerificationOrdre, Commentaires = e.Commentaires
        });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,SuperUtilisateur")]
    public async Task<IActionResult> Enregistrer(CmdpsfProfessionViewModel model)
    {
        if (!Codes.Contains(model.CodeProfession))
            ModelState.AddModelError(nameof(model.CodeProfession), "Profession non autorisée dans ce formulaire.");
        var profession = await db.Professions.FirstOrDefaultAsync(x => x.CodeProfession == model.CodeProfession && x.EstActif);
        if (profession == null) ModelState.AddModelError(nameof(model.CodeProfession), "Profession introuvable ou inactive.");
        if (model.DateDepart.HasValue && model.DateEntreeFonction.HasValue && model.DateDepart < model.DateEntreeFonction)
            ModelState.AddModelError(nameof(model.DateDepart), "La date de départ précède la date d'entrée.");
        if (model.DateExpirationPermis.HasValue && model.DateObtentionPermis.HasValue && model.DateExpirationPermis < model.DateObtentionPermis)
            ModelState.AddModelError(nameof(model.DateExpirationPermis), "Date d'expiration antérieure à l'obtention.");
        model.Permis = model.Permis?.Trim(); model.Nom = model.Nom?.Trim(); model.Prenom = model.Prenom?.Trim();
        if (profession != null && !string.IsNullOrWhiteSpace(model.Permis) &&
            await db.Medecin.AnyAsync(x => x.ProfessionId == profession.ProfessionId && x.Permis == model.Permis && x.MedecinId != model.MedecinId))
            ModelState.AddModelError(nameof(model.Permis), "Un dossier de cette profession possède déjà ce permis.");
        Medecin entity = null;
        if (model.MedecinId != 0)
        {
            entity = await db.Medecin.Include(x => x.Profession).FirstOrDefaultAsync(x => x.MedecinId == model.MedecinId);
            if (entity == null) return NotFound();
            if ((entity.Profession?.CodeProfession == "MEDECIN" || entity.Profession == null)) return BadRequest("Modifier les médecins depuis le registre médical.");
            if (entity.ProfessionId != profession?.ProfessionId)
                ModelState.AddModelError(nameof(model.CodeProfession), "Un changement de profession exige une procédure administrative distincte.");
        }
        if (!ModelState.IsValid) return View("Fiche", model);
        var oldValues = entity == null ? null : new {
            entity.Permis, entity.Nom, entity.Prenom, entity.ProfessionId, entity.EstActif,
            entity.DateEntreeFonction, entity.DateDepart, entity.DateObtentionPermis,
            entity.DateExpirationPermis, entity.NumeroMembreOrdre,
            entity.DateDerniereVerificationOrdre, entity.ReferenceVerificationOrdre,
            entity.Commentaires
        };
        if (entity == null)
        {
            entity = new Medecin { DateCreation = DateTime.Now, ProfessionId = profession.ProfessionId };
            db.Medecin.Add(entity);
        }
        entity.Permis = model.Permis; entity.Nom = model.Nom; entity.Prenom = model.Prenom;
        entity.EstActif = model.EstActif; entity.DateEntreeFonction = model.DateEntreeFonction;
        entity.DateDepart = model.DateDepart; entity.DateObtentionPermis = model.DateObtentionPermis;
        entity.DateExpirationPermis = model.DateExpirationPermis; entity.NumeroMembreOrdre = model.NumeroMembreOrdre;
        entity.DateDerniereVerificationOrdre = model.DateDerniereVerificationOrdre;
        entity.ReferenceVerificationOrdre = model.ReferenceVerificationOrdre; entity.Commentaires = model.Commentaires;
        entity.DateModification = DateTime.Now;
        try
        {
            await db.SaveChangesAsync();
            Audit.Enregistrer("Medecin", entity.MedecinId,
                model.MedecinId == 0 ? "INSERT" : "UPDATE", oldValues,
                new {
                    entity.Permis, entity.Nom, entity.Prenom, entity.ProfessionId, entity.EstActif,
                    entity.DateEntreeFonction, entity.DateDepart, entity.DateObtentionPermis,
                    entity.DateExpirationPermis, entity.NumeroMembreOrdre,
                    entity.DateDerniereVerificationOrdre, entity.ReferenceVerificationOrdre,
                    entity.Commentaires
                });
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Enregistrement impossible. Vérifiez notamment l'unicité du permis.");
            return View("Fiche", model);
        }
        TempData["Success"] = "Fiche enregistrée avec succès.";
        // Après création d'un médecin, ouvrir l'édition médicale complète et ses onglets.
        // Les trois autres professions accèdent à leur fiche CMDPSF.
        if (model.MedecinId == 0 && model.CodeProfession == "MEDECIN")
            return RedirectToAction("Edit", "Medecins", new { id = entity.MedecinId });
        return RedirectToAction(nameof(Details), new { id = entity.MedecinId });
    }
}
