using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.Privileges
{
    public class PrivilegeReferenceListViewModel
    {
        public int PrivilegeReferenceId { get; set; }

        [Display(Name = "Code")]
        public string CodePrivilege { get; set; }

        [Display(Name = "Libellé")]
        public string LibellePrivilege { get; set; }

        [Display(Name = "Catégorie")]
        public string Categorie { get; set; }

        [Display(Name = "Bloc opératoire")]
        public bool NecessiteBlocOperatoire { get; set; }

        [Display(Name = "Installation")]
        public bool NecessiteInstallation { get; set; }

        [Display(Name = "Département")]
        public bool NecessiteUnite { get; set; }

        [Display(Name = "Ordre")]
        public int? OrdreAffichage { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class PrivilegeReferenceFormViewModel
    {
        public int PrivilegeReferenceId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Code")]
        public string CodePrivilege { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Libellé")]
        public string LibellePrivilege { get; set; }

        [StringLength(100)]
        [Display(Name = "Catégorie")]
        public string Categorie { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Nécessite le bloc opératoire")]
        public bool NecessiteBlocOperatoire { get; set; }

        [Display(Name = "Nécessite une installation")]
        public bool NecessiteInstallation { get; set; }

        [Display(Name = "Nécessite une unité organisationnelle")]
        public bool NecessiteUnite { get; set; }

        [Display(Name = "Ordre d'affichage")]
        public int? OrdreAffichage { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class PrivilegeReferenceDetailsViewModel : PrivilegeReferenceFormViewModel
    {
    }

    public class MedecinPrivilegeListViewModel
    {
        public int MedecinPrivilegeId { get; set; }

        [Display(Name = "Médecin")]
        public string MedecinNomComplet { get; set; }

        [Display(Name = "Privilège")]
        public string PrivilegeLibelle { get; set; }

        [Display(Name = "Code")]
        public string CodePrivilege { get; set; }

        [Display(Name = "Installation")]
        public string InstallationLibelle { get; set; }

        [Display(Name = "Département")]
        public string UniteLibelle { get; set; }

        [Display(Name = "Date début")]
        public DateTime DateDebut { get; set; }

        [Display(Name = "Date fin")]
        public DateTime? DateFin { get; set; }

        [Display(Name = "État")]
        public string Etat { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class MedecinPrivilegeFormViewModel
    {
        public int MedecinPrivilegeId { get; set; }

        [Required]
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }

        [Required]
        [Display(Name = "Privilège")]
        public int PrivilegeReferenceId { get; set; }

        [Display(Name = "Installation")]
        public int? InstallationId { get; set; }

        [Display(Name = "Département")]
        public int? UniteOrganisationnelleId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date début")]
        public DateTime? DateDebut { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date fin")]
        public DateTime? DateFin { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "État")]
        public string Etat { get; set; }

        [StringLength(500)]
        [Display(Name = "Nature des activités")]
        public string NatureActivites { get; set; }

        [StringLength(500)]
        [Display(Name = "Champ des activités")]
        public string ChampActivites { get; set; }

        [Display(Name = "Obligations")]
        public string Obligations { get; set; }

        [StringLength(1000)]
        [Display(Name = "Commentaire")]
        public string Commentaire { get; set; }

        [StringLength(50)]
        [Display(Name = "No résolution CA")]
        public string NumeroResolutionCA { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date résolution CA")]
        public DateTime? DateResolutionCA { get; set; }

        [Display(Name = "Reconnaissance écrite reçue")]
        public bool ReconnaissanceEcriteRecue { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }

        public IEnumerable<SelectListItem> MedecinOptions { get; set; }
        public IEnumerable<SelectListItem> PrivilegeOptions { get; set; }
        public IEnumerable<SelectListItem> InstallationOptions { get; set; }
        public IEnumerable<SelectListItem> UniteOptions { get; set; }
        public IEnumerable<SelectListItem> EtatOptions { get; set; }
    }

    public class MedecinPrivilegeDetailsViewModel : MedecinPrivilegeFormViewModel
    {
        public string MedecinNomComplet { get; set; }
        public string PrivilegeLibelle { get; set; }
        public string CodePrivilege { get; set; }
        public string InstallationLibelle { get; set; }
        public string UniteLibelle { get; set; }
    }
}