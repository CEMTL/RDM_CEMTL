using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Solution.ViewModels.AffectationsOrganisationnelles;

namespace Solution.ViewModels.Departements
{
    public class DepartementListViewModel
    {
        public int DepartementId { get; set; }

        [Display(Name = "Département")]
        public string NomDepartement { get; set; }

        [Display(Name = "Code")]
        public string CodeDepartement { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }

        [Display(Name = "Services")]
        public int NombreServices { get; set; }

        [Display(Name = "Chef de département")]
        public string ChefDepartement { get; set; }

        public int NombreAValider { get; set; }
    }

    public class DepartementFormViewModel
    {
        public int DepartementId { get; set; }

        [Required(ErrorMessage = "Le nom du département est obligatoire.")]
        [StringLength(200)]
        [Display(Name = "Département")]
        public string NomDepartement { get; set; }

        [StringLength(50)]
        [Display(Name = "Code")]
        public string CodeDepartement { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class DepartementDetailsViewModel : DepartementListViewModel
    {
        public IList<DepartementServiceItemViewModel> Services { get; set; } = new List<DepartementServiceItemViewModel>();
        public IList<AffectationOrganisationnelleResumeViewModel> Affectations { get; set; } = new List<AffectationOrganisationnelleResumeViewModel>();
    }

    public class DepartementServiceItemViewModel
    {
        public int ServiceId { get; set; }
        public string NomService { get; set; }
        public string CodeService { get; set; }
        public string RegroupementSource { get; set; }
        public string ChefService { get; set; }
        public bool EstActif { get; set; }
    }
}
