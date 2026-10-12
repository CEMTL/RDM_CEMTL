using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;
using System.Xml;
using Solution.ViewModels.Shared;

namespace Solution.Services
{
    public interface IAdminExportDocumentService
    {
        byte[] CreerPdf(AdminTableExportRequest demande);
        byte[] CreerWord(AdminTableExportRequest demande);
        byte[] CreerExcel(AdminTableExportRequest demande);
    }

    public sealed class AdminExportDocumentService : IAdminExportDocumentService
    {
        private const int MaxCellulesParLigne = 100;

        public byte[] CreerExcel(AdminTableExportRequest demande)
        {
            var entetes = Entetes(demande);
            var lignes = Lignes(demande, entetes.Count);
            var derniereColonne = ReferenceColonne(entetes.Count);
            var dateGeneration = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.GetCultureInfo("fr-CA"));

            using var flux = new MemoryStream();
            using (var archive = new ZipArchive(flux, ZipArchiveMode.Create, true, Encoding.UTF8))
            {
                AjouterEntree(archive, "[Content_Types].xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                    "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                    "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                    "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                    "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                    "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" +
                    "</Types>");

                AjouterEntree(archive, "_rels/.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                    "</Relationships>");

                AjouterEntree(archive, "xl/workbook.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                    "<sheets><sheet name=\"Données\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");

                AjouterEntree(archive, "xl/_rels/workbook.xml.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                    "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>" +
                    "</Relationships>");

                AjouterEntree(archive, "xl/styles.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                    "<fonts count=\"2\"><font><sz val=\"11\"/><name val=\"Calibri\"/></font><font><b/><sz val=\"11\"/><name val=\"Calibri\"/></font></fonts>" +
                    "<fills count=\"3\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill><fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFD9EAF7\"/><bgColor indexed=\"64\"/></patternFill></fill></fills>" +
                    "<borders count=\"1\"><border><left/><right/><top/><bottom/><diagonal/></border></borders>" +
                    "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
                    "<cellXfs count=\"3\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>" +
                    "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\"/>" +
                    "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\"/></cellXfs>" +
                    "<cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles></styleSheet>");

                var feuille = new StringBuilder(64 * 1024);
                feuille.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                feuille.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
                feuille.Append("<sheetViews><sheetView workbookViewId=\"0\"><pane ySplit=\"4\" topLeftCell=\"A5\" activePane=\"bottomLeft\" state=\"frozen\"/></sheetView></sheetViews>");
                feuille.Append("<sheetFormatPr defaultRowHeight=\"15\"/><cols>");
                for (var col = 1; col <= entetes.Count; col++)
                    feuille.Append($"<col min=\"{col}\" max=\"{col}\" width=\"22\" customWidth=\"1\"/>");
                feuille.Append("</cols><sheetData>");

                AjouterLigneExcel(feuille, 1, new[] { demande.Titre }, 1);
                AjouterLigneExcel(feuille, 2, new[] { "Filtres : " + (string.IsNullOrWhiteSpace(demande.Filtres) ? "Aucun filtre appliqué" : demande.Filtres) }, 0);
                AjouterLigneExcel(feuille, 3, new[] { "Généré le " + dateGeneration }, 0);
                AjouterLigneExcel(feuille, 4, entetes, 2);

                for (var index = 0; index < lignes.Count; index++)
                    AjouterLigneExcel(feuille, index + 5, lignes[index], 0);

                feuille.Append("</sheetData>");
                if (entetes.Count > 1)
                {
                    feuille.Append($"<mergeCells count=\"3\"><mergeCell ref=\"A1:{derniereColonne}1\"/><mergeCell ref=\"A2:{derniereColonne}2\"/><mergeCell ref=\"A3:{derniereColonne}3\"/></mergeCells>");
                }
                feuille.Append($"<autoFilter ref=\"A4:{derniereColonne}{Math.Max(4, lignes.Count + 4)}\"/>");
                feuille.Append("</worksheet>");
                AjouterEntree(archive, "xl/worksheets/sheet1.xml", feuille.ToString());
            }

            return flux.ToArray();
        }

        public byte[] CreerWord(AdminTableExportRequest demande)
        {
            var entetes = Entetes(demande);
            var lignes = Lignes(demande, entetes.Count);
            var dateGeneration = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.GetCultureInfo("fr-CA"));

            using var flux = new MemoryStream();
            using (var archive = new ZipArchive(flux, ZipArchiveMode.Create, true, Encoding.UTF8))
            {
                AjouterEntree(archive, "[Content_Types].xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                    "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                    "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                    "<Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/>" +
                    "</Types>");
                AjouterEntree(archive, "_rels/.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/>" +
                    "</Relationships>");

                var document = new StringBuilder(128 * 1024);
                document.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                document.Append("<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body>");
                AjouterParagrapheWord(document, demande.Titre, true, 32);
                AjouterParagrapheWord(document, "Filtres : " + (string.IsNullOrWhiteSpace(demande.Filtres) ? "Aucun filtre appliqué" : demande.Filtres), false, 18);
                AjouterParagrapheWord(document, "Généré le " + dateGeneration, false, 18);
                document.Append("<w:tbl><w:tblPr><w:tblStyle w:val=\"TableGrid\"/><w:tblW w:w=\"0\" w:type=\"auto\"/><w:tblBorders><w:top w:val=\"single\" w:sz=\"4\" w:color=\"808080\"/><w:left w:val=\"single\" w:sz=\"4\" w:color=\"808080\"/><w:bottom w:val=\"single\" w:sz=\"4\" w:color=\"808080\"/><w:right w:val=\"single\" w:sz=\"4\" w:color=\"808080\"/><w:insideH w:val=\"single\" w:sz=\"4\" w:color=\"BFBFBF\"/><w:insideV w:val=\"single\" w:sz=\"4\" w:color=\"BFBFBF\"/></w:tblBorders></w:tblPr>");
                document.Append("<w:tr><w:trPr><w:tblHeader/></w:trPr>");
                foreach (var entete in entetes)
                    AjouterCelluleWord(document, entete, true);
                document.Append("</w:tr>");

                foreach (var ligne in lignes)
                {
                    document.Append("<w:tr>");
                    foreach (var cellule in ligne)
                        AjouterCelluleWord(document, cellule, false);
                    document.Append("</w:tr>");
                }

                document.Append("</w:tbl><w:sectPr><w:pgSz w:w=\"16838\" w:h=\"11906\" w:orient=\"landscape\"/><w:pgMar w:top=\"720\" w:right=\"720\" w:bottom=\"720\" w:left=\"720\" w:header=\"360\" w:footer=\"360\" w:gutter=\"0\"/></w:sectPr></w:body></w:document>");
                AjouterEntree(archive, "word/document.xml", document.ToString());
            }

            return flux.ToArray();
        }

        public byte[] CreerPdf(AdminTableExportRequest demande)
        {
            var entetes = Entetes(demande);
            var lignes = Lignes(demande, entetes.Count);
            const double largeurPage = 842;
            const double hauteurPage = 595;
            const double marge = 30;
            const double hauteurLigne = 14;
            const int lignesParPage = 31;
            var pages = Math.Max(1, (int)Math.Ceiling(lignes.Count / (double)lignesParPage));
            var objets = new Dictionary<int, byte[]>();
            var numerosPages = new List<int>();
            var numeroObjet = 5;

            for (var page = 0; page < pages; page++)
            {
                var numeroPage = numeroObjet++;
                var numeroContenu = numeroObjet++;
                numerosPages.Add(numeroPage);
                var debut = page * lignesParPage;
                var lignesPage = lignes.Skip(debut).Take(lignesParPage).ToList();
                var contenu = ConstruirePagePdf(demande, entetes, lignesPage, page + 1, pages, largeurPage, hauteurPage, marge, hauteurLigne);
                var octetsContenu = Encoding.Latin1.GetBytes(contenu);
                objets[numeroContenu] = Combiner(
                    Encoding.ASCII.GetBytes($"<< /Length {octetsContenu.Length} >>\nstream\n"),
                    octetsContenu,
                    Encoding.ASCII.GetBytes("\nendstream"));
                objets[numeroPage] = Encoding.ASCII.GetBytes(
                    $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {largeurPage:0} {hauteurPage:0}] " +
                    $"/Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {numeroContenu} 0 R >>");
            }

            objets[1] = Encoding.ASCII.GetBytes("<< /Type /Catalog /Pages 2 0 R >>");
            objets[2] = Encoding.ASCII.GetBytes($"<< /Type /Pages /Kids [{string.Join(" ", numerosPages.Select(x => $"{x} 0 R"))}] /Count {pages} >>");
            objets[3] = Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
            objets[4] = Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");

            using var flux = new MemoryStream();
            EcrireLatin1(flux, "%PDF-1.4\n%âãÏÓ\n");
            var offsets = new long[numeroObjet];
            for (var index = 1; index < numeroObjet; index++)
            {
                offsets[index] = flux.Position;
                EcrireLatin1(flux, $"{index} 0 obj\n");
                flux.Write(objets[index], 0, objets[index].Length);
                EcrireLatin1(flux, "\nendobj\n");
            }

            var debutXref = flux.Position;
            EcrireLatin1(flux, $"xref\n0 {numeroObjet}\n0000000000 65535 f \n");
            for (var index = 1; index < numeroObjet; index++)
                EcrireLatin1(flux, $"{offsets[index]:0000000000} 00000 n \n");
            EcrireLatin1(flux, $"trailer\n<< /Size {numeroObjet} /Root 1 0 R >>\nstartxref\n{debutXref}\n%%EOF");
            return flux.ToArray();
        }

        private static string ConstruirePagePdf(
            AdminTableExportRequest demande,
            IReadOnlyList<string> entetes,
            IReadOnlyList<List<string>> lignes,
            int page,
            int totalPages,
            double largeurPage,
            double hauteurPage,
            double marge,
            double hauteurLigne)
        {
            var contenu = new StringBuilder(32 * 1024);
            var largeurTable = largeurPage - (2 * marge);
            var largeurColonne = largeurTable / entetes.Count;
            var y = hauteurPage - marge;

            TextePdf(contenu, marge, y, 14, demande.Titre, true, 105);
            y -= 18;
            var filtres = string.IsNullOrWhiteSpace(demande.Filtres) ? "Aucun filtre appliqué" : demande.Filtres;
            TextePdf(contenu, marge, y, 8, "Filtres : " + filtres, false, 180);
            y -= 13;
            TextePdf(contenu, marge, y, 8,
                $"Généré le {DateTime.Now:yyyy-MM-dd HH:mm} — Page {page}/{totalPages}", false, 180);
            y -= 18;

            contenu.Append("0.88 g ")
                .Append(NombrePdf(marge)).Append(' ').Append(NombrePdf(y - hauteurLigne + 3)).Append(' ')
                .Append(NombrePdf(largeurTable)).Append(' ').Append(NombrePdf(hauteurLigne)).Append(" re f\n0 g\n");
            DessinerLignePdf(contenu, marge, y + 3, marge + largeurTable, y + 3);
            DessinerLignePdf(contenu, marge, y - hauteurLigne + 3, marge + largeurTable, y - hauteurLigne + 3);
            for (var col = 0; col <= entetes.Count; col++)
            {
                var x = marge + (col * largeurColonne);
                DessinerLignePdf(contenu, x, y + 3, x, y - hauteurLigne + 3);
            }

            for (var col = 0; col < entetes.Count; col++)
            {
                var maximum = Math.Max(3, (int)(largeurColonne / 4.0) - 1);
                TextePdf(contenu, marge + (col * largeurColonne) + 2, y - 7, 7, entetes[col], true, maximum);
            }

            y -= hauteurLigne;
            foreach (var ligne in lignes)
            {
                DessinerLignePdf(contenu, marge, y - hauteurLigne + 3, marge + largeurTable, y - hauteurLigne + 3);
                for (var col = 0; col <= entetes.Count; col++)
                {
                    var x = marge + (col * largeurColonne);
                    DessinerLignePdf(contenu, x, y + 3, x, y - hauteurLigne + 3);
                }

                for (var col = 0; col < entetes.Count; col++)
                {
                    var maximum = Math.Max(3, (int)(largeurColonne / 3.7) - 1);
                    TextePdf(contenu, marge + (col * largeurColonne) + 2, y - 7, 6.5, ligne[col], false, maximum);
                }
                y -= hauteurLigne;
            }

            return contenu.ToString();
        }

        private static void DessinerLignePdf(StringBuilder contenu, double x1, double y1, double x2, double y2)
        {
            contenu.Append("0.70 G 0.35 w ")
                .Append(NombrePdf(x1)).Append(' ').Append(NombrePdf(y1)).Append(" m ")
                .Append(NombrePdf(x2)).Append(' ').Append(NombrePdf(y2)).Append(" l S\n0 G\n");
        }

        private static void TextePdf(StringBuilder contenu, double x, double y, double taille, string texte, bool gras, int maximum)
        {
            contenu.Append("BT /").Append(gras ? "F2" : "F1").Append(' ')
                .Append(NombrePdf(taille)).Append(" Tf ")
                .Append(NombrePdf(x)).Append(' ').Append(NombrePdf(y)).Append(" Td (")
                .Append(EchapperPdf(Raccourcir(texte, maximum))).Append(") Tj ET\n");
        }

        private static string EchapperPdf(string valeur)
        {
            if (string.IsNullOrEmpty(valeur))
                return string.Empty;

            var resultat = new StringBuilder(valeur.Length);
            foreach (var caractereOriginal in valeur)
            {
                var caractere = caractereOriginal switch
                {
                    'œ' => 'o',
                    'Œ' => 'O',
                    '–' or '—' => '-',
                    '‘' or '’' => '\'',
                    '“' or '”' => '"',
                    _ => caractereOriginal
                };

                if (caractere > 255 || char.IsControl(caractere))
                    caractere = ' ';
                if (caractere is '\\' or '(' or ')')
                    resultat.Append('\\');
                resultat.Append(caractere);
            }
            return resultat.ToString();
        }

        private static string NombrePdf(double valeur) =>
            valeur.ToString("0.##", CultureInfo.InvariantCulture);

        private static void EcrireLatin1(Stream flux, string valeur)
        {
            var octets = Encoding.Latin1.GetBytes(valeur);
            flux.Write(octets, 0, octets.Length);
        }

        private static byte[] Combiner(params byte[][] blocs)
        {
            using var flux = new MemoryStream();
            foreach (var bloc in blocs)
                flux.Write(bloc, 0, bloc.Length);
            return flux.ToArray();
        }

        private static void AjouterLigneExcel(StringBuilder feuille, int numero, IEnumerable<string> cellules, int style)
        {
            feuille.Append($"<row r=\"{numero}\">");
            var colonne = 1;
            foreach (var cellule in cellules.Take(MaxCellulesParLigne))
            {
                var valeur = Raccourcir(NettoyerXml(cellule), 32767);
                feuille.Append($"<c r=\"{ReferenceColonne(colonne)}{numero}\" t=\"inlineStr\" s=\"{style}\"><is><t xml:space=\"preserve\">{EchapperXml(valeur)}</t></is></c>");
                colonne++;
            }
            feuille.Append("</row>");
        }

        private static string ReferenceColonne(int colonne)
        {
            colonne = Math.Max(1, colonne);
            var resultat = string.Empty;
            while (colonne > 0)
            {
                colonne--;
                resultat = (char)('A' + (colonne % 26)) + resultat;
                colonne /= 26;
            }
            return resultat;
        }

        private static void AjouterParagrapheWord(StringBuilder document, string texte, bool gras, int taille)
        {
            document.Append("<w:p><w:r><w:rPr>");
            if (gras)
                document.Append("<w:b/>");
            document.Append($"<w:sz w:val=\"{taille}\"/></w:rPr><w:t xml:space=\"preserve\">{EchapperXml(NettoyerXml(texte))}</w:t></w:r></w:p>");
        }

        private static void AjouterCelluleWord(StringBuilder document, string texte, bool entete)
        {
            document.Append("<w:tc><w:tcPr>");
            if (entete)
                document.Append("<w:shd w:val=\"clear\" w:fill=\"D9EAF7\"/>");
            document.Append("</w:tcPr><w:p><w:r><w:rPr>");
            if (entete)
                document.Append("<w:b/>");
            document.Append("<w:sz w:val=\"16\"/></w:rPr><w:t xml:space=\"preserve\">")
                .Append(EchapperXml(Raccourcir(NettoyerXml(texte), 10000)))
                .Append("</w:t></w:r></w:p></w:tc>");
        }

        private static void AjouterEntree(ZipArchive archive, string chemin, string contenu)
        {
            var entree = archive.CreateEntry(chemin, CompressionLevel.Fastest);
            using var flux = entree.Open();
            using var writer = new StreamWriter(flux, new UTF8Encoding(false));
            writer.Write(contenu);
        }

        private static List<string> Entetes(AdminTableExportRequest demande)
        {
            var entetes = (demande?.Entetes ?? new List<string>())
                .Take(MaxCellulesParLigne)
                .Select(x => string.IsNullOrWhiteSpace(x) ? "Colonne" : x.Trim())
                .ToList();
            return entetes.Count > 0 ? entetes : new List<string> { "Données" };
        }

        private static List<List<string>> Lignes(AdminTableExportRequest demande, int nombreColonnes)
        {
            return (demande?.Lignes ?? new List<List<string>>())
                .Select(ligne => Enumerable.Range(0, nombreColonnes)
                    .Select(index => index < (ligne?.Count ?? 0) ? ligne[index] ?? string.Empty : string.Empty)
                    .ToList())
                .ToList();
        }

        private static string NettoyerXml(string valeur)
        {
            if (string.IsNullOrEmpty(valeur))
                return string.Empty;
            return new string(valeur.Where(XmlConvert.IsXmlChar).ToArray())
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ");
        }

        private static string EchapperXml(string valeur) => SecurityElement.Escape(valeur) ?? string.Empty;

        private static string Raccourcir(string valeur, int maximum)
        {
            valeur ??= string.Empty;
            if (valeur.Length <= maximum)
                return valeur;
            return maximum <= 3 ? valeur.Substring(0, maximum) : valeur.Substring(0, maximum - 3) + "...";
        }
    }
}
