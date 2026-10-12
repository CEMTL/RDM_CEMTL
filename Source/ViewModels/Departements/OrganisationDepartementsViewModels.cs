using System;
using System.Collections.Generic;

namespace Solution.ViewModels.Departements
{
    public class OrganisationDepartementViewModel
    {
        public int DepartementId { get; set; }
        public string NomDepartement { get; set; }
        public bool PeutAjouterChefDepartement { get; set; }
        public IList<OrganisationResponsableViewModel> ChefsDepartement { get; set; } = new List<OrganisationResponsableViewModel>();
        public IList<OrganisationResponsableViewModel> Secretaires { get; set; } = new List<OrganisationResponsableViewModel>();
        public IList<OrganisationResponsableViewModel> AdjointsSantaCabrini { get; set; } = new List<OrganisationResponsableViewModel>();
        public IList<OrganisationServiceViewModel> Services { get; set; } = new List<OrganisationServiceViewModel>();
    }

    public class OrganisationServiceViewModel
    {
        public int ServiceId { get; set; }
        public string NomService { get; set; }
        public string RegroupementSource { get; set; }
        public bool PeutAjouterChefService { get; set; }
        public IList<OrganisationResponsableViewModel> ChefsService { get; set; } = new List<OrganisationResponsableViewModel>();
    }

    public class OrganisationResponsableViewModel
    {
        public int AffectationOrganisationnelleId { get; set; }
        public string NomAffiche { get; set; }
        public string Titre { get; set; }
        public string Courriel { get; set; }
        public DateTime? DateDebut { get; set; }
        public bool EstInterim { get; set; }
        public bool EstAValider { get; set; }
    }
}
