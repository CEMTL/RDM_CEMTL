using System;

namespace Solution.Models
{
    public class AffectationOrganisationnelle
    {
        public int AffectationOrganisationnelleId { get; set; }
        public int? DepartementId { get; set; }
        public int? ServiceId { get; set; }
        public int? MedecinId { get; set; }
        public string RoleCode { get; set; }
        public string NomAffiche { get; set; }
        public string Titre { get; set; }
        public string Courriel { get; set; }
        public string CourrielSource { get; set; }
        public string Telephone { get; set; }
        public string PosteTelephone { get; set; }
        public int? InstallationId { get; set; }
        public string SiteNomSource { get; set; }
        public string RegroupementSource { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstInterim { get; set; }
        public bool EstActive { get; set; }
        public bool EstAValider { get; set; }
        public string SourceDonnees { get; set; }
        public int? SourceLigne { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime DateModification { get; set; }

        public virtual Departement Departement { get; set; }
        public virtual Service Service { get; set; }
        public virtual Medecin Medecin { get; set; }
        public virtual Installation Installation { get; set; }
    }
}
