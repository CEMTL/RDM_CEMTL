using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.TypesInstanceDecision
{
    public class TypeInstanceDecisionListViewModel
    {
        [Display(Name = "TypeInstanceDecisionId")]
        public int TypeInstanceDecisionId { get; set; }
        [Display(Name = "Code")]
        public string CodeType { get; set; }
        [Display(Name = "Libellé")]
        public string Libelle { get; set; }
    }

    public class TypeInstanceDecisionFormViewModel
    {
        public int TypeInstanceDecisionId { get; set; }
        [Display(Name = "Code")]
        [Required]
        [StringLength(255)]
        public string CodeType { get; set; }
        [Display(Name = "Libellé")]
        [Required]
        [StringLength(255)]
        public string Libelle { get; set; }
    }

    public class TypeInstanceDecisionDetailsViewModel : TypeInstanceDecisionListViewModel
    {
    }
}