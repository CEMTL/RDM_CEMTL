using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.MedecinsComites
{
    public class MedecinComiteListViewModel
    {
        [Display(Name = "MedecinComiteId")]
        public int MedecinComiteId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Comité")]
        public int ComiteId { get; set; }
        [Display(Name = "Rôle")]
        public string RoleMembre { get; set; }
        [Display(Name = "Date début")]
        public DateTime? DateDebut { get; set; }
        [Display(Name = "Date fin")]
        public DateTime? DateFin { get; set; }
        [Display(Name = "Commentaire")]
        public string Commentaire { get; set; }
        public string MedecinLibelle { get; set; }
        public string ComiteLibelle { get; set; }
    }

    public class MedecinComiteFormViewModel
    {
        public int MedecinComiteId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Comité")]
        public int ComiteId { get; set; }
        [Display(Name = "Rôle")]
        public string RoleMembre { get; set; }
        [Display(Name = "Date début")]
        [DataType(DataType.Date)]
        public DateTime? DateDebut { get; set; }
        [Display(Name = "Date fin")]
        [DataType(DataType.Date)]
        public DateTime? DateFin { get; set; }
        [Display(Name = "Commentaire")]
        public string Commentaire { get; set; }
        public IEnumerable<SelectListItem> MedecinIdOptions { get; set; }
        public IEnumerable<SelectListItem> ComiteIdOptions { get; set; }
    }

    public class MedecinComiteDetailsViewModel : MedecinComiteListViewModel
    {
    }
}