using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.TypesPaiement
{
    public class TypePaiementListViewModel
    {
        [Display(Name = "TypePaiementId")]
        public int TypePaiementId { get; set; }
        [Display(Name = "Libellé")]
        public string Libelle { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class TypePaiementFormViewModel
    {
        public int TypePaiementId { get; set; }
        [Display(Name = "Libellé")]
        [Required]
        [StringLength(255)]
        public string Libelle { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class TypePaiementDetailsViewModel : TypePaiementListViewModel
    {
    }
}