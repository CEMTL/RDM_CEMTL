using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.InstancesDecision
{
    public class InstanceDecisionListViewModel
    {
        [Display(Name = "InstanceDecisionId")]
        public int InstanceDecisionId { get; set; }
        [Display(Name = "Type instance")]
        public int TypeInstanceDecisionId { get; set; }
        [Display(Name = "Code instance")]
        public string CodeInstance { get; set; }
        [Display(Name = "Date instance")]
        public DateTime? DateInstance { get; set; }
        [Display(Name = "Libellé instance")]
        public string LibelleInstance { get; set; }
        public string TypeInstanceDecisionLibelle { get; set; }
    }

    public class InstanceDecisionFormViewModel
    {
        public int InstanceDecisionId { get; set; }
        [Display(Name = "Type instance")]
        public int TypeInstanceDecisionId { get; set; }
        [Display(Name = "Code instance")]
        [Required]
        [StringLength(255)]
        public string CodeInstance { get; set; }
        [Display(Name = "Date instance")]
        [DataType(DataType.Date)]
        public DateTime? DateInstance { get; set; }
        [Display(Name = "Libellé instance")]
        public string LibelleInstance { get; set; }
        public IEnumerable<SelectListItem> TypeInstanceDecisionIdOptions { get; set; }
    }

    public class InstanceDecisionDetailsViewModel : InstanceDecisionListViewModel
    {
    }
}