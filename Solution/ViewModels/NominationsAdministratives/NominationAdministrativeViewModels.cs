using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.NominationsAdministratives
{
    public class NominationAdministrativeListViewModel
    {
        [Display(Name = "NominationAdministrativeId")]
        public int NominationAdministrativeId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Nomination")]
        public string LibelleNomination { get; set; }
        [Display(Name = "Date nomination")]
        public DateTime? DateNomination { get; set; }
        [Display(Name = "Date fin")]
        public DateTime? DateFin { get; set; }
        [Display(Name = "Active")]
        public bool EstActive { get; set; }
        public string MedecinLibelle { get; set; }
    }

    public class NominationAdministrativeFormViewModel
    {
        public int NominationAdministrativeId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Nomination")]
        public string LibelleNomination { get; set; }
        [Display(Name = "Date nomination")]
        [DataType(DataType.Date)]
        public DateTime? DateNomination { get; set; }
        [Display(Name = "Date fin")]
        [DataType(DataType.Date)]
        public DateTime? DateFin { get; set; }
        [Display(Name = "Active")]
        public bool EstActive { get; set; }
        public int NominationId { get; set; }
        public IEnumerable<SelectListItem> MedecinIdOptions { get; set; }
    }

    public class NominationAdministrativeDetailsViewModel : NominationAdministrativeListViewModel
    {
    }
}