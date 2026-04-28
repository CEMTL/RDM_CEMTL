using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.EntentesFDC
{
    public class EntenteFDCListViewModel
    {
        [Display(Name = "EntenteFDCId")]
        public int EntenteFDCId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Type entente")]
        public string TypeEntenteFDC { get; set; }
        [Display(Name = "Date adhésion")]
        public DateTime? DateAdhesion { get; set; }
        [Display(Name = "Date fin")]
        public DateTime? DateFin { get; set; }
        [Display(Name = "PEM visé")]
        public string PEMVise { get; set; }
        [Display(Name = "Active")]
        public bool EstActive { get; set; }
        public string MedecinLibelle { get; set; }
    }

    public class EntenteFDCFormViewModel
    {
        public int EntenteFDCId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Type entente")]
        public string TypeEntenteFDC { get; set; }
        [Display(Name = "Date adhésion")]
        [DataType(DataType.Date)]
        public DateTime? DateAdhesion { get; set; }
        [Display(Name = "Date fin")]
        [DataType(DataType.Date)]
        public DateTime? DateFin { get; set; }
        [Display(Name = "PEM visé")]
        public string PEMVise { get; set; }
        [Display(Name = "Active")]
        public bool EstActive { get; set; }
        public IEnumerable<SelectListItem> MedecinIdOptions { get; set; }
    }

    public class EntenteFDCDetailsViewModel : EntenteFDCListViewModel
    {
    }
}