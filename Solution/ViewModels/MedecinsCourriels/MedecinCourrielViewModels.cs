using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.MedecinsCourriels
{
    public class MedecinCourrielListViewModel
    {
        [Display(Name = "MedecinCourrielId")]
        public int MedecinCourrielId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Type")]
        public string TypeCourriel { get; set; }
        [Display(Name = "Courriel")]
        public string AdresseCourriel { get; set; }
        [Display(Name = "Rang")]
        public int? RangSource { get; set; }
        [Display(Name = "Principal")]
        public bool EstPrincipal { get; set; }
        public string MedecinLibelle { get; set; }
    }

    public class MedecinCourrielFormViewModel
    {
        public int MedecinCourrielId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Type")]
        [EmailAddress]
        public string TypeCourriel { get; set; }
        [Display(Name = "Courriel")]
        [EmailAddress]
        public string AdresseCourriel { get; set; }
        [Display(Name = "Rang")]
        public int? RangSource { get; set; }
        [Display(Name = "Principal")]
        public bool EstPrincipal { get; set; }
        public IEnumerable<SelectListItem> MedecinIdOptions { get; set; }
    }

    public class MedecinCourrielDetailsViewModel : MedecinCourrielListViewModel
    {
    }
}