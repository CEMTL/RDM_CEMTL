using System;

namespace Solution.ViewModels.Medecins
{
    public class MedecinDeleteViewModel
    {
        public int MedecinId { get; set; }
        public string Permis { get; set; }
        public string NomComplet { get; set; }
        public string TypePermis { get; set; }
        public string StatutMedecin { get; set; }
        public string CategoriePratique { get; set; }
        public bool EstActif { get; set; }
        public DateTime? DateEntreeFonction { get; set; }
        public DateTime? DateDepart { get; set; }
        public string Commentaires { get; set; }

        public int NbAdresses { get; set; }
        public int NbTelephones { get; set; }
        public int NbCourriels { get; set; }
        public int NbInstallations { get; set; }
        public int NbNominationsAdministratives { get; set; }

        // Données métier/historiques : elles doivent bloquer la suppression du médecin.
        public int NbCotisationsAnnuelles { get; set; }
        public int NbEntentesFDC { get; set; }
        public int NbFitTests { get; set; }
        public int NbComites { get; set; }
        public int NbPrivileges { get; set; }
        public int NbAffectationsOrganisationnelles { get; set; }
        public int NbAffectationsCliniques { get; set; }
        public int NbDocuments { get; set; }

        public int TotalDependancesBloquantes =>
            NbCotisationsAnnuelles
            + NbEntentesFDC
            + NbFitTests
            + NbComites
            + NbPrivileges
            + NbAffectationsOrganisationnelles
            + NbAffectationsCliniques
            + NbDocuments;

        public bool SuppressionBloquee => TotalDependancesBloquantes > 0;

        public int TotalDependances
        {
            get
            {
                return NbAdresses
                     + NbTelephones
                     + NbCourriels
                     + NbInstallations
                     + NbNominationsAdministratives;
            }
        }
    }
}
