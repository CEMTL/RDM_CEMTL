using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.MedecinsAdresses
{
    public class MedecinAdresseListViewModel
    {
        [Display(Name = "MedecinAdresseId")]
        public int MedecinAdresseId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Adresse")]
        public string AdresseLigne1 { get; set; }
        [Display(Name = "Ville")]
        public string Ville { get; set; }
        [Display(Name = "Province")]
        public string Province { get; set; }
        [Display(Name = "Code postal")]
        public string CodePostal { get; set; }
        [Display(Name = "Principale")]
        public bool EstPrincipale { get; set; }
        public string MedecinLibelle { get; set; }
    }

    public class MedecinAdresseFormViewModel
    {
        public int MedecinAdresseId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Adresse")]
        public string AdresseLigne1 { get; set; }
        [Display(Name = "Ville")]
        public string Ville { get; set; }
        [Display(Name = "Province")]
        public string Province { get; set; }
        [Display(Name = "Code postal")]
        public string CodePostal { get; set; }
        [Display(Name = "Principale")]
        public bool EstPrincipale { get; set; }
        public IEnumerable<SelectListItem> MedecinIdOptions { get; set; }
    }

    public class MedecinAdresseDetailsViewModel : MedecinAdresseListViewModel
    {
    }
}