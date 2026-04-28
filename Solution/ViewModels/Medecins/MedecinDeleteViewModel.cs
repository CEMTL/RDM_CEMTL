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
        public int NbUnitesOrganisationnelles { get; set; }
        public int NbNominationsAdministratives { get; set; }

        public int TotalDependances
        {
            get
            {
                return NbAdresses
                     + NbTelephones
                     + NbCourriels
                     + NbInstallations
                     + NbUnitesOrganisationnelles
                     + NbNominationsAdministratives;
            }
        }
    }
}