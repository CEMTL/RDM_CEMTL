using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Solution.ViewModels.AffectationsOrganisationnelles
{
    public class AffectationOrganisationnelleListViewModel
    {
        public int AffectationOrganisationnelleId { get; set; }
        public string RoleCode { get; set; }
        public string RoleLibelle { get; set; }
        public string NomAffiche { get; set; }
        public string NomMedecin { get; set; }
        public string NomDepartement { get; set; }
        public string NomService { get; set; }
        public string Cible { get; set; }
        public string RegroupementSource { get; set; }
        public string Titre { get; set; }
        public string Courriel { get; set; }
        public string Site { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstInterim { get; set; }
        public bool EstActive { get; set; }
        public bool EstAValider { get; set; }
        public string SourceDonnees { get; set; }
        public int? SourceLigne { get; set; }
    }

    public class AffectationOrganisationnelleDetailsViewModel : AffectationOrganisationnelleListViewModel
    {
        public string Telephone { get; set; }
        public string PosteTelephone { get; set; }
        public string CourrielSource { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime DateModification { get; set; }
    }

    public class AffectationOrganisationnelleFormViewModel
    {
        public int AffectationOrganisationnelleId { get; set; }

        [Required(ErrorMessage = "Le rôle est obligatoire.")]
        [StringLength(40)]
        [Display(Name = "Rôle")]
        public string RoleCode { get; set; }

        [Display(Name = "Département")]
        public int? DepartementId { get; set; }

        [Display(Name = "Service")]
        public int? ServiceId { get; set; }

        [Display(Name = "Médecin lié")]
        public int? MedecinId { get; set; }

        [Required(ErrorMessage = "Le nom affiché est obligatoire.")]
        [StringLength(300)]
        [Display(Name = "Nom affiché")]
        public string NomAffiche { get; set; }

        [StringLength(300)]
        [Display(Name = "Titre")]
        public string Titre { get; set; }

        [StringLength(255)]
        [EmailAddress(ErrorMessage = "Le courriel n'est pas valide.")]
        [Display(Name = "Courriel validé")]
        public string Courriel { get; set; }

        [StringLength(50)]
        [Display(Name = "Téléphone")]
        public string Telephone { get; set; }

        [StringLength(20)]
        [Display(Name = "Poste téléphonique")]
        public string PosteTelephone { get; set; }

        [Display(Name = "Installation")]
        public int? InstallationId { get; set; }

        [StringLength(200)]
        [Display(Name = "Nom du site provenant de la source")]
        public string SiteNomSource { get; set; }

        [StringLength(200)]
        [Display(Name = "Regroupement")]
        public string RegroupementSource { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date de début")]
        public DateTime? DateDebut { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date de fin")]
        public DateTime? DateFin { get; set; }

        [Display(Name = "Intérim")]
        public bool EstInterim { get; set; }

        [Display(Name = "Active")]
        public bool EstActive { get; set; }

        [Display(Name = "À valider")]
        public bool EstAValider { get; set; }

        public string CourrielSource { get; set; }
        public string SourceDonnees { get; set; }
        public int? SourceLigne { get; set; }

        public IEnumerable<SelectListItem> RoleOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> DepartementOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ServiceOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> MedecinOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> InstallationOptions { get; set; } = new List<SelectListItem>();
    }

    public class AffectationOrganisationnelleResumeViewModel
    {
        public int AffectationOrganisationnelleId { get; set; }
        public string RoleLibelle { get; set; }
        public string NomAffiche { get; set; }
        public string NomMedecin { get; set; }
        public string Titre { get; set; }
        public string Courriel { get; set; }
        public string Site { get; set; }
        public string RegroupementSource { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstInterim { get; set; }
        public bool EstActive { get; set; }
        public bool EstAValider { get; set; }
    }
}
