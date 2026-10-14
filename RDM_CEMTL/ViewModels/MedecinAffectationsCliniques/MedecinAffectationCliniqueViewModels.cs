using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Solution.ViewModels.MedecinAffectationsCliniques
{
    public class MedecinAffectationCliniqueListViewModel
    {
        public int MedecinAffectationCliniqueId { get; set; }
        public int MedecinId { get; set; }
        public string Medecin { get; set; }
        public string Departement { get; set; }
        public string Service { get; set; }
        public string Installation { get; set; }
        public string TypeAffectation { get; set; }
        public bool EstPrincipale { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstActive { get; set; }
    }

    public class MedecinAffectationCliniqueFormViewModel
    {
        public int MedecinAffectationCliniqueId { get; set; }

        [Required(ErrorMessage = "Le médecin est requis.")]
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }

        [Required(ErrorMessage = "Le département est requis.")]
        [Display(Name = "Département")]
        public int DepartementId { get; set; }

        [Required(ErrorMessage = "Le service est requis.")]
        [Display(Name = "Service")]
        public int ServiceId { get; set; }

        [Display(Name = "Installation")]
        public int? InstallationId { get; set; }

        [Display(Name = "Type d'affectation")]
        [StringLength(100)]
        public string TypeAffectation { get; set; }

        [Display(Name = "Affectation principale")]
        public bool EstPrincipale { get; set; }

        [Display(Name = "Date début")]
        [DataType(DataType.Date)]
        public DateTime? DateDebut { get; set; }

        [Display(Name = "Date fin")]
        [DataType(DataType.Date)]
        public DateTime? DateFin { get; set; }

        [Display(Name = "Active")]
        public bool EstActive { get; set; }

        [Display(Name = "Commentaire")]
        [StringLength(500)]
        public string Commentaire { get; set; }

        public int? ReturnMedecinId { get; set; }
        public string ReturnTab { get; set; }

        public IEnumerable<SelectListItem> Medecins { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Departements { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Services { get; set; } = Array.Empty<SelectListItem>();
        public List<ServiceOptionViewModel> ServiceOptions { get; set; } = new List<ServiceOptionViewModel>();
        public IEnumerable<SelectListItem> Installations { get; set; } = Array.Empty<SelectListItem>();
    }

    public class ServiceOptionViewModel
    {
        public int ServiceId { get; set; }
        public int? DepartementId { get; set; }
        public string Libelle { get; set; }
        public bool EstActif { get; set; }
    }
}
