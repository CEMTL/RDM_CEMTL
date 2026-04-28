using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.UnitesOrganisationnelles
{
    public class UniteOrganisationnelleListViewModel
    {
        [Display(Name = "UniteOrganisationnelleId")]
        public int UniteOrganisationnelleId { get; set; }
        [Display(Name = "Libellé")]
        public string Libelle { get; set; }
        [Display(Name = "Type")]
        public string TypeUnite { get; set; }
        [Display(Name = "Département parent")]
        public int? UniteParentId { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
        public string UniteParentLibelle { get; set; }
    }

    public class UniteOrganisationnelleFormViewModel
    {
        public int UniteOrganisationnelleId { get; set; }
        [Display(Name = "Libellé")]
        [Required]
        [StringLength(255)]
        public string Libelle { get; set; }
        [Display(Name = "Type")]
        public string TypeUnite { get; set; }
        [Display(Name = "Département parent")]
        public int? UniteParentId { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
        public IEnumerable<SelectListItem> UniteParentIdOptions { get; set; }
    }

    public class UniteOrganisationnelleDetailsViewModel : UniteOrganisationnelleListViewModel
    {
    }
}