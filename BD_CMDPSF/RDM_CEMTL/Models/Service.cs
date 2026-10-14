using System.Collections.Generic;

namespace Solution.Models
{
    public class Service
    {
        public Service()
        {
            InstallationServices = new HashSet<InstallationService>();
            MedecinPrivileges = new HashSet<MedecinPrivilege>();
            AffectationsOrganisationnelles = new HashSet<AffectationOrganisationnelle>();
            MedecinAffectationsCliniques = new HashSet<MedecinAffectationClinique>();
        }

        public int ServiceId { get; set; }
        public int? DepartementId { get; set; }
        public string NomService { get; set; }
        public string CodeService { get; set; }
        public string RegroupementSource { get; set; }
        public bool EstActif { get; set; }

        public virtual Departement Departement { get; set; }
        public virtual ICollection<InstallationService> InstallationServices { get; set; }
        public virtual ICollection<MedecinPrivilege> MedecinPrivileges { get; set; }
        public virtual ICollection<AffectationOrganisationnelle> AffectationsOrganisationnelles { get; set; }
        public virtual ICollection<MedecinAffectationClinique> MedecinAffectationsCliniques { get; set; }
    }
}
