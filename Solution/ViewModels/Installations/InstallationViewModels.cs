using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

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
    }
}