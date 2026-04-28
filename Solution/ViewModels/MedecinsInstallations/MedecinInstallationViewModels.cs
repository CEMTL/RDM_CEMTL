using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.MedecinsInstallations
{
    public class MedecinInstallationListViewModel
    {
        [Display(Name = "MedecinInstallationId")]
        public int MedecinInstallationId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Installation")]
        public int InstallationId { get; set; }
        [Display(Name = "Rang")]
        public int RangInstallation { get; set; }
        [Display(Name = "Date début")]
        public DateTime? DateDebut { get; set; }
        [Display(Name = "Date fin")]
        public DateTime? DateFin { get; set; }
        [Display(Name = "Principale")]
        public bool EstPrincipale { get; set; }
        public string MedecinLibelle { get; set; }
        public string InstallationLibelle { get; set; }
    }

    public class MedecinInstallationFormViewModel
    {
        public int MedecinInstallationId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Installation")]
        public int InstallationId { get; set; }
        [Display(Name = "Rang")]
        public int RangInstallation { get; set; }
        [Display(Name = "Date début")]
        [DataType(DataType.Date)]
        public DateTime? DateDebut { get; set; }
        [Display(Name = "Date fin")]
        [DataType(DataType.Date)]
        public DateTime? DateFin { get; set; }
        [Display(Name = "Principale")]
        public bool EstPrincipale { get; set; }
        public IEnumerable<SelectListItem> MedecinIdOptions { get; set; }
        public IEnumerable<SelectListItem> InstallationIdOptions { get; set; }
    }

    public class MedecinInstallationDetailsViewModel : MedecinInstallationListViewModel
    {
    }
}