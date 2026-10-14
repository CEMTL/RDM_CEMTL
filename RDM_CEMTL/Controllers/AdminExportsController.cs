using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Solution.Services;
using Solution.ViewModels.Shared;

namespace Solution.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("AdminExports")]
    public sealed class AdminExportsController : Controller
    {
        private const int MaxLignes = 50000;
        private const int MaxColonnes = 100;
        private const int MaxCaracteresCellule = 10000;
        private readonly IAdminExportDocumentService _documents;
        private readonly ILogger<AdminExportsController> _logger;

        public AdminExportsController(
            IAdminExportDocumentService documents,
            ILogger<AdminExportsController> logger)
        {
            _documents = documents;
            _logger = logger;
        }

        [HttpPost("Creer")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50_000_000)]
        public IActionResult Creer([FromBody] AdminTableExportRequest demande)
        {
            if (demande == null)
                return BadRequest(new { message = "Aucune donnée n'a été reçue pour l'exportation." });

            if ((demande.Lignes?.Count ?? 0) > MaxLignes)
                return BadRequest(new { message = $"Le fichier contient plus de {MaxLignes} lignes. Réduisez les filtres; export annulé pour éviter une troncature." });
            Normaliser(demande);
            if (demande.Entetes.Count == 0)
                return BadRequest(new { message = "Aucune colonne exportable n'a été trouvée dans cette page." });
            if (demande.Lignes.Count == 0)
                return BadRequest(new { message = "Aucune donnée ne correspond aux filtres actuellement appliqués." });

            try
            {
                var format = (demande.Format ?? string.Empty).Trim().ToLowerInvariant();
                var nomBase = NomFichier(demande.Titre);
                var horodatage = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);

                byte[] contenu;
                string typeContenu;
                string extension;
                switch (format)
                {
                    case "pdf":
                        contenu = _documents.CreerPdf(demande);
                        typeContenu = "application/pdf";
                        extension = ".pdf";
                        break;
                    case "word":
                        contenu = _documents.CreerWord(demande);
                        typeContenu = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        extension = ".docx";
                        break;
                    case "excel":
                        contenu = _documents.CreerExcel(demande);
                        typeContenu = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        extension = ".xlsx";
                        break;
                    default:
                        return BadRequest(new { message = "Le format d'exportation demandé n'est pas pris en charge." });
                }

                _logger.LogInformation(
                    "Export administratif {Format} créé par {Utilisateur} depuis {PageSource} : {NombreLignes} ligne(s), {NombreColonnes} colonne(s).",
                    format,
                    User?.Identity?.Name ?? "Utilisateur inconnu",
                    demande.PageSource,
                    demande.Lignes.Count,
                    demande.Entetes.Count);

                return File(contenu, typeContenu, $"{nomBase}_{horodatage}{extension}");
            }
            catch (Exception exception)
            {
                var reference = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpperInvariant();
                _logger.LogError(exception,
                    "Échec de l'exportation administrative. Référence {Reference}.", reference);
                return StatusCode(500, new
                {
                    message = $"Le document n'a pas pu être créé. Référence : {reference}."
                });
            }
        }

        private static void Normaliser(AdminTableExportRequest demande)
        {
            demande.Titre = Limiter(Nettoyer(demande.Titre), 200);
            if (string.IsNullOrWhiteSpace(demande.Titre))
                demande.Titre = "Exportation RDM";
            demande.Filtres = Limiter(Nettoyer(demande.Filtres), 4000);
            demande.PageSource = Limiter(Nettoyer(demande.PageSource), 500);

            demande.Entetes = (demande.Entetes ?? new List<string>())
                .Take(MaxColonnes)
                .Select(x => Limiter(Nettoyer(x), 250))
                .ToList();

            demande.Lignes = (demande.Lignes ?? new List<List<string>>())
                .Take(MaxLignes)
                .Select(ligne => Enumerable.Range(0, demande.Entetes.Count)
                    .Select(index => index < (ligne?.Count ?? 0)
                        ? Limiter(Nettoyer(ligne[index]), MaxCaracteresCellule)
                        : string.Empty)
                    .ToList())
                .ToList();
        }

        private static string Nettoyer(string valeur)
        {
            if (string.IsNullOrWhiteSpace(valeur))
                return string.Empty;
            return string.Join(" ", valeur
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static string Limiter(string valeur, int maximum) =>
            string.IsNullOrEmpty(valeur) || valeur.Length <= maximum
                ? valeur ?? string.Empty
                : valeur.Substring(0, maximum);

        private static string NomFichier(string titre)
        {
            var normalise = (titre ?? "Exportation_RDM").Normalize(NormalizationForm.FormD);
            var resultat = new StringBuilder();
            foreach (var caractere in normalise)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caractere) == UnicodeCategory.NonSpacingMark)
                    continue;
                if (char.IsLetterOrDigit(caractere))
                    resultat.Append(caractere);
                else if (resultat.Length > 0 && resultat[^1] != '_')
                    resultat.Append('_');
            }

            var nom = resultat.ToString().Trim('_');
            return string.IsNullOrWhiteSpace(nom) ? "Exportation_RDM" : Limiter(nom, 80);
        }
    }
}
