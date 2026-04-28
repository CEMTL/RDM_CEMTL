using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.FitTests
{
    public class FitTestListViewModel
    {
        [Display(Name = "FitTestId")]
        public int FitTestId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Résultat")]
        public string Resultat { get; set; }
        [Display(Name = "Date fit test")]
        public DateTime? DateFitTest { get; set; }
        [Display(Name = "Commentaire")]
        public string Commentaire { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
        public string MedecinLibelle { get; set; }
    }

    public class FitTestFormViewModel
    {
        public int FitTestId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Résultat")]
        [Required]
        [StringLength(255)]
        public string Resultat { get; set; }
        [Display(Name = "Date fit test")]
        [DataType(DataType.Date)]
        public DateTime? DateFitTest { get; set; }
        [Display(Name = "Commentaire")]
        public string Commentaire { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
        public IEnumerable<SelectListItem> MedecinIdOptions { get; set; }
    }

    public class FitTestDetailsViewModel : FitTestListViewModel
    {
    }
}