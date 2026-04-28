using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.MedecinsUnitesOrganisationnelles
{
    public class MedecinUniteOrganisationnelleListViewModel
    {
        [Display(Name = "MedecinUniteOrganisationnelleId")]
        public int MedecinUniteOrganisationnelleId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Unité")]
        public int UniteOrganisationnelleId { get; set; }
        [Display(Name = "Rang")]
        public int? RangSource { get; set; }
        [Display(Name = "Rôle")]
        public string RoleOrganisationnel { get; set; }
        [Display(Name = "Date début")]
        public DateTime? DateDebut { get; set; }
        [Display(Name = "Date fin")]
        public DateTime? DateFin { get; set; }
        public string MedecinLibelle { get; set; }
        public string UniteOrganisationnelleLibelle { get; set; }
    }

    public class MedecinUniteOrganisationnelleFormViewModel
    {
        public int MedecinUniteOrganisationnelleId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Département")]
        public int UniteOrganisationnelleId { get; set; }
        [Display(Name = "Rang")]
        public int? RangSource { get; set; }
        [Display(Name = "Rôle")]
        public string RoleOrganisationnel { get; set; }
        [Display(Name = "Date début")]
        [DataType(DataType.Date)]
        public DateTime? DateDebut { get; set; }
        [Display(Name = "Date fin")]
        [DataType(DataType.Date)]
        public DateTime? DateFin { get; set; }
        public IEnumerable<SelectListItem> MedecinIdOptions { get; set; }
        public IEnumerable<SelectListItem> UniteOrganisationnelleIdOptions { get; set; }
    }

    public class MedecinUniteOrganisationnelleDetailsViewModel : MedecinUniteOrganisationnelleListViewModel
    {
    }
}