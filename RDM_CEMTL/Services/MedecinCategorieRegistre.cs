using System.Globalization;
using System.Linq;
using System.Text;

namespace Solution.Services;

/// <summary>
/// Classement commun aux compteurs du tableau de bord et au registre CMDPSF.
/// Aucune condition sur Medecin.EstActif, StatutMedecinId ou CategoriePratique.EstActif.
/// </summary>
public static class MedecinCategorieRegistre
{
    public const string Omnipraticien = "OMNIPRATICIEN";
    public const string Specialiste = "SPECIALISTE";

    public static string NormaliserFiltre(string filtre)
    {
        var valeur = (filtre ?? string.Empty).Trim().ToUpperInvariant();
        return valeur == Omnipraticien || valeur == Specialiste ? valeur : null;
    }

    public static string Determiner(string libelle)
    {
        if (string.IsNullOrWhiteSpace(libelle))
            return null;

        var decompose = libelle.Normalize(NormalizationForm.FormD);
        var categorie = new string(decompose
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray())
            .ToUpperInvariant();

        // Garder le même ordre de priorité que le tableau de bord d'origine.
        if (categorie.Contains("OMNIPRAT") || categorie.Contains("GENERAL") ||
            categorie.Contains("FAMIL"))
            return Omnipraticien;
        if (categorie.Contains("SPECIAL"))
            return Specialiste;
        return null;
    }
}
