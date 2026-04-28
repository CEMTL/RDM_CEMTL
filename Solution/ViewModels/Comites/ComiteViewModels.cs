using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.Comites
{
    public class ComiteListViewModel
    {
        [Display(Name = "ComiteId")]
        public int ComiteId { get; set; }
        [Display(Name = "Numéro source")]
        public int? NumeroSource { get; set; }
        [Display(Name = "Comité")]
        public string NomComite { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class ComiteFormViewModel
    {
        public int ComiteId { get; set; }
        [Display(Name = "Numéro source")]
        public int? NumeroSource { get; set; }
        [Display(Name = "Comité")]
        [Required]
        [StringLength(255)]
        public string NomComite { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class ComiteDetailsViewModel : ComiteListViewModel
    {
    }
}