using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.Nominations
{
    public class NominationListViewModel
    {
        public int NominationId { get; set; }

        [Display(Name = "Code")]
        public string CodeNomination { get; set; }

        [Display(Name = "Libellé")]
        public string LibelleNomination { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class NominationFormViewModel
    {
        public int NominationId { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Code")]
        public string CodeNomination { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Libellé")]
        public string LibelleNomination { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class NominationDetailsViewModel : NominationFormViewModel
    {
    }

    public class NominationAdministrativeListViewModel
    {
        public int NominationAdministrativeId { get; set; }

        [Display(Name = "Médecin")]
        public string MedecinNomComplet { get; set; }

        [Display(Name = "Nomination")]
        public string LibelleNomination { get; set; }

        [Display(Name = "Type")]
        public string TypeNominationLibelle { get; set; }

        [Display(Name = "Date nomination")]
        public DateTime? DateNomination { get; set; }

        [Display(Name = "Date fin")]
        public DateTime? DateFin { get; set; }

        [Display(Name = "Active")]
        public bool EstActive { get; set; }
    }

    public class NominationAdministrativeFormViewModel
    {
        public int NominationAdministrativeId { get; set; }

        [Required]
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }

        [Display(Name = "Nomination de référence")]
        public int? NominationId { get; set; }

        [StringLength(255)]
        [Display(Name = "Libellé nomination")]
        public string LibelleNomination { get; set; }

        [Display(Name = "Type de nomination")]
        public int? TypeNominationId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date nomination")]
        public DateTime? DateNomination { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date fin")]
        public DateTime? DateFin { get; set; }

        [Display(Name = "Active")]
        public bool EstActive { get; set; }

        public IEnumerable<SelectListItem> MedecinOptions { get; set; }
        public IEnumerable<SelectListItem> NominationOptions { get; set; }
        public IEnumerable<SelectListItem> TypeNominationOptions { get; set; }
    }

    public class NominationAdministrativeDetailsViewModel : NominationAdministrativeFormViewModel
    {
        public string MedecinNomComplet { get; set; }
        public string TypeNominationLibelle { get; set; }
    }
}