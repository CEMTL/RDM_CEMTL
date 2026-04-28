using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.ProfilsCEMTL
{
    public class MedecinProfilCEMTLListViewModel
    {
        [Display(Name = "MedecinProfilCEMTLId")]
        public int MedecinProfilCEMTLId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Téléphone 1")]
        public string Telephone1 { get; set; }
        [Display(Name = "Cellulaire")]
        public string CellulaireNo { get; set; }
        [Display(Name = "Téléphone 2")]
        public string Telephone2 { get; set; }
        [Display(Name = "Domicile")]
        public string DomicileNo { get; set; }
        [Display(Name = "Téléphone 3")]
        public string Telephone3 { get; set; }
        [Display(Name = "Pagette")]
        public string PagetteNo { get; set; }
        [Display(Name = "Département 1")]
        public string Departement1 { get; set; }
        [Display(Name = "Département 2")]
        public string Departement2 { get; set; }
        [Display(Name = "Service/Secteur 1")]
        public string ServiceSecteur1 { get; set; }
        [Display(Name = "Service/Secteur 2")]
        public string ServiceSecteur2 { get; set; }
        [Display(Name = "Installation 1")]
        public string Installation1Source { get; set; }
        [Display(Name = "Installation 2")]
        public string Installation2Source { get; set; }
        [Display(Name = "Installation 3")]
        public string Installation3Source { get; set; }
        [Display(Name = "Courriel 1")]
        public string Courriel1 { get; set; }
        [Display(Name = "Courriel 2")]
        public string Courriel2 { get; set; }
        [Display(Name = "Courriel médical")]
        public string CourrielMed { get; set; }
        [Display(Name = "Nomination administrative")]
        public string NominationAdministrativeCourante { get; set; }
        [Display(Name = "Date nomination administrative")]
        public DateTime? DateNominationAdministrative { get; set; }
        [Display(Name = "Date avis congé/départ")]
        public DateTime? DateAvisCongeDepart { get; set; }
        [Display(Name = "Absence temporaire")]
        public string AbsenceTemporaire { get; set; }
        [Display(Name = "Date absence")]
        public DateTime? DateAbsence { get; set; }
        [Display(Name = "Date retour absence")]
        public DateTime? DateRetourAbsence { get; set; }
        [Display(Name = "Date début pratique")]
        public DateTime? DateDebutPratique { get; set; }
        [Display(Name = "Date fin pratique")]
        public DateTime? DateFinPratique { get; set; }
        [Display(Name = "Engagement FDC")]
        public string EngagementFDC { get; set; }
        [Display(Name = "Date adhésion FDC")]
        public DateTime? DateAdhesionFDC { get; set; }
        [Display(Name = "Date fin FDC")]
        public DateTime? DateFinFDC { get; set; }
        [Display(Name = "PEM visé")]
        public string PEMVise { get; set; }
        [Display(Name = "PEM ailleurs")]
        public string PEMAilleurs { get; set; }
        [Display(Name = "RLS")]
        public string RLS { get; set; }
        [Display(Name = "Liste nomin")]
        public string ListeNomin { get; set; }
        [Display(Name = "Engagement")]
        public string Engagement { get; set; }
        [Display(Name = "Privilèges")]
        public string PrivilegesTexte { get; set; }
        [Display(Name = "Privilèges enseignement")]
        public string PrivilegesEnseignement { get; set; }
        [Display(Name = "Privilèges recherche")]
        public string PrivilegesRecherche { get; set; }
        [Display(Name = "Demande MSSS")]
        public DateTime? DemandeMSSS { get; set; }
        [Display(Name = "Approbation MSSS")]
        public DateTime? ApprobMSSS { get; set; }
        [Display(Name = "Demande UdeM")]
        public DateTime? DemandeUdeM { get; set; }
        [Display(Name = "Approbation UdeM")]
        public DateTime? ApprobUdeM { get; set; }
        [Display(Name = "Date DMSP")]
        public DateTime? DateDMSP { get; set; }
        [Display(Name = "Date CECMDP")]
        public DateTime? DateCECMDP { get; set; }
        [Display(Name = "Date CA")]
        public DateTime? DateCA { get; set; }
        [Display(Name = "Résolution")]
        public string NumeroResolution { get; set; }
        [Display(Name = "Date renouvellement")]
        public DateTime? DateRenouvellement { get; set; }
        [Display(Name = "Fit test courant")]
        public string FitTestCourant { get; set; }
        [Display(Name = "Commentaires opérationnels")]
        public string CommentairesOperationnels { get; set; }
        public string MedecinNomComplet { get; set; }
    }

    public class MedecinProfilCEMTLFormViewModel
    {
        public int MedecinProfilCEMTLId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Téléphone 1")]
        public string Telephone1 { get; set; }
        [Display(Name = "Cellulaire")]
        public string CellulaireNo { get; set; }
        [Display(Name = "Téléphone 2")]
        public string Telephone2 { get; set; }
        [Display(Name = "Domicile")]
        public string DomicileNo { get; set; }
        [Display(Name = "Téléphone 3")]
        public string Telephone3 { get; set; }
        [Display(Name = "Pagette")]
        public string PagetteNo { get; set; }
        [Display(Name = "Département 1")]
        public string Departement1 { get; set; }
        [Display(Name = "Département 2")]
        public string Departement2 { get; set; }
        [Display(Name = "Service/Secteur 1")]
        public string ServiceSecteur1 { get; set; }
        [Display(Name = "Service/Secteur 2")]
        public string ServiceSecteur2 { get; set; }
        [Display(Name = "Installation 1")]
        public string Installation1Source { get; set; }
        [Display(Name = "Installation 2")]
        public string Installation2Source { get; set; }
        [Display(Name = "Installation 3")]
        public string Installation3Source { get; set; }
        [Display(Name = "Courriel 1")]
        [EmailAddress]
        public string Courriel1 { get; set; }
        [Display(Name = "Courriel 2")]
        [EmailAddress]
        public string Courriel2 { get; set; }
        [Display(Name = "Courriel médical")]
        [EmailAddress]
        public string CourrielMed { get; set; }
        [Display(Name = "Nomination administrative")]
        public string NominationAdministrativeCourante { get; set; }
        [Display(Name = "Date nomination administrative")]
        [DataType(DataType.Date)]
        public DateTime? DateNominationAdministrative { get; set; }
        [Display(Name = "Date avis congé/départ")]
        [DataType(DataType.Date)]
        public DateTime? DateAvisCongeDepart { get; set; }
        [Display(Name = "Absence temporaire")]
        public string AbsenceTemporaire { get; set; }
        [Display(Name = "Date absence")]
        [DataType(DataType.Date)]
        public DateTime? DateAbsence { get; set; }
        [Display(Name = "Date retour absence")]
        [DataType(DataType.Date)]
        public DateTime? DateRetourAbsence { get; set; }
        [Display(Name = "Date début pratique")]
        [DataType(DataType.Date)]
        public DateTime? DateDebutPratique { get; set; }
        [Display(Name = "Date fin pratique")]
        [DataType(DataType.Date)]
        public DateTime? DateFinPratique { get; set; }
        [Display(Name = "Engagement FDC")]
        public string EngagementFDC { get; set; }
        [Display(Name = "Date adhésion FDC")]
        [DataType(DataType.Date)]
        public DateTime? DateAdhesionFDC { get; set; }
        [Display(Name = "Date fin FDC")]
        [DataType(DataType.Date)]
        public DateTime? DateFinFDC { get; set; }
        [Display(Name = "PEM visé")]
        public string PEMVise { get; set; }
        [Display(Name = "PEM ailleurs")]
        public string PEMAilleurs { get; set; }
        [Display(Name = "RLS")]
        public string RLS { get; set; }
        [Display(Name = "Liste nomin")]
        public string ListeNomin { get; set; }
        [Display(Name = "Engagement")]
        public string Engagement { get; set; }
        [Display(Name = "Privilèges")]
        public string PrivilegesTexte { get; set; }
        [Display(Name = "Privilèges enseignement")]
        public string PrivilegesEnseignement { get; set; }
        [Display(Name = "Privilèges recherche")]
        public string PrivilegesRecherche { get; set; }
        [Display(Name = "Demande MSSS")]
        [DataType(DataType.Date)]
        public DateTime? DemandeMSSS { get; set; }
        [Display(Name = "Approbation MSSS")]
        [DataType(DataType.Date)]
        public DateTime? ApprobMSSS { get; set; }
        [Display(Name = "Demande UdeM")]
        [DataType(DataType.Date)]
        public DateTime? DemandeUdeM { get; set; }
        [Display(Name = "Approbation UdeM")]
        [DataType(DataType.Date)]
        public DateTime? ApprobUdeM { get; set; }
        [Display(Name = "Date DMSP")]
        [DataType(DataType.Date)]
        public DateTime? DateDMSP { get; set; }
        [Display(Name = "Date CECMDP")]
        [DataType(DataType.Date)]
        public DateTime? DateCECMDP { get; set; }
        [Display(Name = "Date CA")]
        [DataType(DataType.Date)]
        public DateTime? DateCA { get; set; }
        [Display(Name = "Résolution")]
        public string NumeroResolution { get; set; }
        [Display(Name = "Date renouvellement")]
        [DataType(DataType.Date)]
        public DateTime? DateRenouvellement { get; set; }
        [Display(Name = "Fit test courant")]
        public string FitTestCourant { get; set; }
        [Display(Name = "Commentaires opérationnels")]
        public string CommentairesOperationnels { get; set; }
        public IEnumerable<SelectListItem> MedecinIdOptions { get; set; }
    }

    public class MedecinProfilCEMTLDetailsViewModel : MedecinProfilCEMTLListViewModel
    {
        public string MedecinLibelle { get; set; }
    }
}