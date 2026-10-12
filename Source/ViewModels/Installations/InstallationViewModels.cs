using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Solution.ViewModels.Installations
{
    public class InstallationListViewModel
    {
        [Display(Name = "InstallationId")]
        public int InstallationId { get; set; }
        [Display(Name = "Nom")]
        public string NomInstallation { get; set; }
        [Display(Name = "Code")]
        public string CodeInstallation { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
        [Display(Name = "Services")]
        public int NombreServices { get; set; }
    }

    public class InstallationFormViewModel
    {
        public int InstallationId { get; set; }
        [Display(Name = "Nom")]
        [Required]
        [StringLength(255)]
        public string NomInstallation { get; set; }
        [Display(Name = "Code")]
        public string CodeInstallation { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class InstallationDetailsViewModel : InstallationListViewModel
    {
        public IList<InstallationServiceItemViewModel> Services { get; set; } = new List<InstallationServiceItemViewModel>();
    }

    public class InstallationServiceItemViewModel
    {
        public string NomDepartement { get; set; }
        public string NomService { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstActif { get; set; }
    }
}
