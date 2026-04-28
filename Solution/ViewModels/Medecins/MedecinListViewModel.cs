using System;

namespace Solution.ViewModels.Medecins
{
    public class MedecinListViewModel
    {
        public int MedecinId { get; set; }
        public string Permis { get; set; }
        public string NomComplet { get; set; }
        public string TypePermis { get; set; }
        public string StatutMedecin { get; set; }
        public string CategoriePratique { get; set; }
        public string TelephonePrincipal { get; set; }
        public string CourrielPrincipal { get; set; }
        public string InstallationPrincipale { get; set; }
        public string NominationActive { get; set; }
        public bool EstActif { get; set; }
    }
}