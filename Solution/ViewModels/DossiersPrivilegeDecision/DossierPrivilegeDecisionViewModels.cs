using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.DossiersPrivilegeDecision
{
    public class DossierPrivilegeDecisionListViewModel
    {
        [Display(Name = "DossierPrivilegeDecisionId")]
        public int DossierPrivilegeDecisionId { get; set; }
        [Display(Name = "Dossier")]
        public int DossierPrivilegeId { get; set; }
        [Display(Name = "Instance")]
        public int InstanceDecisionId { get; set; }
        [Display(Name = "Texte décision")]
        public string TexteDecision { get; set; }
        public string DossierPrivilegeLibelle { get; set; }
        public string InstanceDecisionLibelle { get; set; }
    }

    public class DossierPrivilegeDecisionFormViewModel
    {
        public int DossierPrivilegeDecisionId { get; set; }
        [Display(Name = "Dossier")]
        public int DossierPrivilegeId { get; set; }
        [Display(Name = "Instance")]
        public int InstanceDecisionId { get; set; }
        [Display(Name = "Texte décision")]
        public string TexteDecision { get; set; }
        public IEnumerable<SelectListItem> DossierPrivilegeIdOptions { get; set; }
        public IEnumerable<SelectListItem> InstanceDecisionIdOptions { get; set; }
    }

    public class DossierPrivilegeDecisionDetailsViewModel : DossierPrivilegeDecisionListViewModel
    {
    }
}