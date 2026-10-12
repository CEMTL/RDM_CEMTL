using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Solution.Services
{
    /// <summary>
    /// Transforme les exceptions techniques EF Core / SQL Server en messages adaptés aux utilisateurs.
    /// Les détails techniques doivent rester dans les journaux de l'application et ne jamais être
    /// affichés directement dans les vues.
    /// </summary>
    public static class UserFriendlyExceptionService
    {
        private static readonly IReadOnlyDictionary<string, string> ForeignKeyMessages =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["FK_CotisationAnnuelle_Medecin"] = "Ce médecin ne peut pas être supprimé, car une ou plusieurs cotisations annuelles lui sont associées.",
                ["FK_EntenteFDC_Medecin"] = "Ce médecin ne peut pas être supprimé, car une ou plusieurs ententes FDC lui sont associées.",
                ["FK_FitTest_Medecin"] = "Ce médecin ne peut pas être supprimé, car un ou plusieurs Fit Tests lui sont associés.",
                ["FK_MedecinComite_Medecin"] = "Ce médecin ne peut pas être supprimé, car il est encore associé à un ou plusieurs comités.",
                ["FK_MedecinPrivilege_Medecin"] = "Ce médecin ne peut pas être supprimé, car un ou plusieurs privilèges lui sont associés.",
                ["FK_Medecin_CategoriePratique"] = "Cette catégorie ne peut pas être supprimée, car elle est encore utilisée par un ou plusieurs médecins.",
                ["FK_Medecin_StatutMedecin"] = "Ce statut ne peut pas être supprimé, car il est encore utilisé par un ou plusieurs médecins.",
                ["FK_Medecin_TypePermis"] = "Ce type de permis ne peut pas être supprimé, car il est encore utilisé par un ou plusieurs médecins.",
                ["FK_Medecin_TitreMedecin"] = "Ce titre ne peut pas être supprimé, car il est encore utilisé par un ou plusieurs médecins.",
                ["FK_Medecin_Genre"] = "Ce genre ne peut pas être supprimé, car il est encore utilisé par un ou plusieurs médecins.",
                ["FK_MedecinCourriel_TypeCourriel"] = "Ce type de courriel ne peut pas être supprimé, car il est encore utilisé par un ou plusieurs courriels."
            };

        public static string GetMessage(Exception exception, string action = null)
        {
            if (exception is DbUpdateConcurrencyException)
            {
                return "Les données ont été modifiées par un autre utilisateur. Rechargez la page puis recommencez l'opération.";
            }

            var sqlException = FindSqlException(exception);
            if (sqlException != null)
            {
                return GetSqlMessage(sqlException, action);
            }

            if (exception is DbUpdateException)
            {
                return BuildGenericSaveMessage(action);
            }

            return "Une erreur inattendue est survenue. Veuillez réessayer. Si le problème persiste, communiquez avec le soutien informatique en indiquant la référence affichée.";
        }

        private static string GetSqlMessage(SqlException sqlException, string action)
        {
            // 547 : conflit de clé étrangère / contrainte référentielle.
            if (sqlException.Number == 547)
            {
                foreach (var item in ForeignKeyMessages)
                {
                    if (sqlException.Message.Contains(item.Key, StringComparison.OrdinalIgnoreCase))
                        return item.Value + " Vous devez d'abord retirer ou réaffecter les données associées.";
                }

                if (sqlException.Message.Contains("DELETE", StringComparison.OrdinalIgnoreCase))
                {
                    return "Suppression impossible : cet élément est encore utilisé par d'autres données de l'application. Retirez ou réaffectez d'abord les éléments associés.";
                }

                return "Impossible d'enregistrer cette modification, car une donnée sélectionnée n'est plus valide ou est encore liée à un autre élément.";
            }

            // 2601 / 2627 : index ou contrainte UNIQUE.
            if (sqlException.Number == 2601 || sqlException.Number == 2627)
            {
                return "Enregistrement impossible : un élément avec les mêmes informations existe déjà.";
            }

            // 515 : valeur NULL dans une colonne obligatoire.
            if (sqlException.Number == 515)
            {
                return "Enregistrement impossible : une information obligatoire est manquante. Vérifiez les champs requis.";
            }

            // 2628 / 8152 : chaîne ou données trop longues pour la colonne.
            if (sqlException.Number == 2628 || sqlException.Number == 8152)
            {
                return "Enregistrement impossible : une des valeurs saisies est trop longue. Réduisez le contenu du champ concerné puis recommencez.";
            }

            // -2 : délai SQL dépassé.
            if (sqlException.Number == -2)
            {
                return "La base de données met trop de temps à répondre. Veuillez réessayer dans quelques instants.";
            }

            // 1205 : deadlock SQL.
            if (sqlException.Number == 1205)
            {
                return "L'opération n'a pas pu être terminée parce que les données étaient utilisées simultanément. Veuillez réessayer.";
            }

            return BuildGenericSaveMessage(action);
        }

        private static string BuildGenericSaveMessage(string action)
        {
            if (!string.IsNullOrWhiteSpace(action))
                return $"Impossible de {action}. Vérifiez les informations saisies puis réessayez. Si le problème persiste, communiquez avec le soutien informatique.";

            return "L'opération n'a pas pu être enregistrée. Vérifiez les informations saisies puis réessayez. Si le problème persiste, communiquez avec le soutien informatique.";
        }

        private static SqlException FindSqlException(Exception exception)
        {
            Exception current = exception;
            while (current != null)
            {
                if (current is SqlException sqlException)
                    return sqlException;

                current = current.InnerException;
            }

            return null;
        }
    }
}
