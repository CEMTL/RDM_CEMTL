using System.Collections.Generic;

namespace Solution.Models
{
    public class Departement
    {
        public Departement()
        {
            Services = new HashSet<Service>();
            AffectationsOrganisationnelles = new HashSet<AffectationOrganisationnelle>();
            MedecinAffectationsCliniques = new HashSet<MedecinAffectationClinique>();
        }

        public int DepartementId { get; set; }
        public string NomDepartement { get; set; }
        public string CodeDepartement { get; set; }
        public bool EstActif { get; set; }

        public virtual ICollection<Service> Services { get; set; }
        public virtual ICollection<AffectationOrganisationnelle> AffectationsOrganisationnelles { get; set; }
        public virtual ICollection<MedecinAffectationClinique> MedecinAffectationsCliniques { get; set; }
    }
}
