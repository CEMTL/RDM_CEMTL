using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public MedecinCourrielFormViewModel()
        {
            MedecinIdOptions = new List<SelectListItem>();
            TypeCourrielOptions = new List<SelectListItem>();
        }

        public int MedecinCourrielId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Type")]
        [Required(ErrorMessage = "Le type de courriel est obligatoire.")]
        public string TypeCourriel { get; set; }
        [Display(Name = "Courriel")]
        [Required(ErrorMessage = "L'adresse courriel est obligatoire.")]
        [EmailAddress(ErrorMessage = "L'adresse courriel n'est pas valide.")]
        public string AdresseCourriel { get; set; }
        [Display(Name = "Rang")]
        [Range(0, 255, ErrorMessage = "Le rang doit être compris entre 0 et 255.")]
        public int? RangSource { get; set; }
        [Display(Name = "Principal")]
        public bool EstPrincipal { get; set; }
        public IEnumerable<SelectListItem> MedecinIdOptions { get; set; }
        public IEnumerable<SelectListItem> TypeCourrielOptions { get; set; }
    }

    public class MedecinCourrielDetailsViewModel : MedecinCourrielListViewModel
    {
    }
}
