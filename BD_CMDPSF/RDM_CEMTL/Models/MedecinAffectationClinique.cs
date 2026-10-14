using System;

namespace Solution.Models
{
    /// <summary>
    /// Affectation clinique d'un médecin à un département/service.
    /// Cette information est distincte des responsabilités organisationnelles
    /// (chef, adjoint, secrétaire) et des privilèges cliniques.
    /// </summary>
    public class MedecinAffectationClinique
    {
        public int MedecinAffectationCliniqueId { get; set; }
        public int MedecinId { get; set; }
        public int DepartementId { get; set; }
        public int ServiceId { get; set; }
        public int? InstallationId { get; set; }
        public string TypeAffectation { get; set; }
        public bool EstPrincipale { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstActive { get; set; }
        public string Commentaire { get; set; }
        public string SourceDonnees { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime DateModification { get; set; }

        public virtual Medecin Medecin { get; set; }
        public virtual Departement Departement { get; set; }
        public virtual Service Service { get; set; }
        public virtual Installation Installation { get; set; }
    }
}
