using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.MedecinsTelephones
{
    public class MedecinTelephoneListViewModel
    {
        [Display(Name = "MedecinTelephoneId")]
        public int MedecinTelephoneId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Type")]
        public string TypeTelephone { get; set; }
        [Display(Name = "Numéro")]
        public string NumeroTelephone { get; set; }
        [Display(Name = "Rang")]
        public int? RangSource { get; set; }
        [Display(Name = "Principal")]
        public bool EstPrincipal { get; set; }
        public string MedecinLibelle { get; set; }
    }

    public class MedecinTelephoneFormViewModel
    {
        public int MedecinTelephoneId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Type")]
        public string TypeTelephone { get; set; }
        [Display(Name = "Numéro")]
        public string NumeroTelephone { get; set; }
        [Display(Name = "Rang")]
        public int? RangSource { get; set; }
        [Display(Name = "Principal")]
        public bool EstPrincipal { get; set; }
        public IEnumerable<SelectListItem> MedecinIdOptions { get; set; }
    }

    public class MedecinTelephoneDetailsViewModel : MedecinTelephoneListViewModel
    {
    }
}