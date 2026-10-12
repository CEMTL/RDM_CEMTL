using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Solution.ViewModels.AffectationsOrganisationnelles;

namespace Solution.ViewModels.Services
{
    public class ServiceListViewModel
    {
        public int ServiceId { get; set; }

        [Display(Name = "Service")]
        public string NomService { get; set; }

        [Display(Name = "Code")]
        public string CodeService { get; set; }

        [Display(Name = "Département")]
        public string NomDepartement { get; set; }

        [Display(Name = "Regroupement")]
        public string RegroupementSource { get; set; }

        [Display(Name = "Chef de service")]
        public string ChefService { get; set; }

        public bool ChefServiceEstAValider { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class ServiceFormViewModel
    {
        public int ServiceId { get; set; }

        [Display(Name = "Département")]
        public int? DepartementId { get; set; }

        [Required(ErrorMessage = "Le nom du service est obligatoire.")]
        [StringLength(250)]
        [Display(Name = "Service")]
        public string NomService { get; set; }

        [StringLength(50)]
        [Display(Name = "Code")]
        public string CodeService { get; set; }

        [StringLength(200)]
        [Display(Name = "Regroupement")]
        public string RegroupementSource { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }

        public IEnumerable<SelectListItem> DepartementOptions { get; set; } = new List<SelectListItem>();
    }

    public class ServiceDetailsViewModel : ServiceListViewModel
    {
        public int? DepartementId { get; set; }
        public int NombrePrivileges { get; set; }
        public IList<ServiceInstallationItemViewModel> Installations { get; set; } = new List<ServiceInstallationItemViewModel>();
        public IList<AffectationOrganisationnelleResumeViewModel> Affectations { get; set; } = new List<AffectationOrganisationnelleResumeViewModel>();
    }

    public class ServiceInstallationItemViewModel
    {
        public string NomInstallation { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstActif { get; set; }
    }
}
