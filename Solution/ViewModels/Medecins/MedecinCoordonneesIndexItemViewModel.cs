using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.Medecins
{
    public class MedecinCoordonneesParentInfoViewModel
    {
        public int MedecinId { get; set; }
        public string Permis { get; set; }
        public string NomComplet { get; set; }
    }

    public class MedecinCoordonneesIndexItemViewModel
    {
        public int MedecinId { get; set; }
        public string Permis { get; set; }
        public string NomComplet { get; set; }
        public bool EstActif { get; set; }
        public int NbAdresses { get; set; }
        public int NbTelephones { get; set; }
        public int NbCourriels { get; set; }
        public int NbInstallations { get; set; }
        public int NbUnites { get; set; }
    }

    public class MedecinCoordonneesManageViewModel
    {
        public MedecinCoordonneesManageViewModel()
        {
            Adresses = new List<MedecinAdresseItemViewModel>();
            Telephones = new List<MedecinTelephoneItemViewModel>();
            Courriels = new List<MedecinCourrielItemViewModel>();
            Installations = new List<MedecinInstallationItemViewModel>();
            Unites = new List<MedecinUniteOrganisationnelleItemViewModel>();
            InstallationsDisponibles = new List<SelectListItem>();
            UnitesDisponibles = new List<SelectListItem>();
            NouvelleAdresse = new MedecinAdresseFormViewModel();
            NouveauTelephone = new MedecinTelephoneFormViewModel();
            NouveauCourriel = new MedecinCourrielFormViewModel();
            NouvelleInstallation = new MedecinInstallationFormViewModel();
            NouvelleUnite = new MedecinUniteOrganisationnelleFormViewModel();
        }

        public int MedecinId { get; set; }
        public string Permis { get; set; }
        public string NomComplet { get; set; }
        public bool EstActif { get; set; }

        public IList<MedecinAdresseItemViewModel> Adresses { get; set; }
        public IList<MedecinTelephoneItemViewModel> Telephones { get; set; }
        public IList<MedecinCourrielItemViewModel> Courriels { get; set; }
        public IList<MedecinInstallationItemViewModel> Installations { get; set; }
        public IList<MedecinUniteOrganisationnelleItemViewModel> Unites { get; set; }

        public MedecinAdresseFormViewModel NouvelleAdresse { get; set; }
        public MedecinTelephoneFormViewModel NouveauTelephone { get; set; }
        public MedecinCourrielFormViewModel NouveauCourriel { get; set; }
        public MedecinInstallationFormViewModel NouvelleInstallation { get; set; }
        public MedecinUniteOrganisationnelleFormViewModel NouvelleUnite { get; set; }

        public IList<SelectListItem> InstallationsDisponibles { get; set; }
        public IList<SelectListItem> UnitesDisponibles { get; set; }
    }

    public class MedecinAdresseItemViewModel
    {
        public int MedecinAdresseId { get; set; }
        public string AdresseLigne1 { get; set; }
        public string Ville { get; set; }
        public string Province { get; set; }
        public string CodePostal { get; set; }
        public bool EstPrincipale { get; set; }
    }

    public class MedecinTelephoneItemViewModel
    {
        public int MedecinTelephoneId { get; set; }
        public string TypeTelephone { get; set; }
        public string NumeroTelephone { get; set; }
        public byte? RangSource { get; set; }
        public bool EstPrincipal { get; set; }
    }

    public class MedecinCourrielItemViewModel
    {
        public int MedecinCourrielId { get; set; }
        public string TypeCourriel { get; set; }
        public string AdresseCourriel { get; set; }
        public byte? RangSource { get; set; }
        public bool EstPrincipal { get; set; }
    }

    public class MedecinInstallationItemViewModel
    {
        public int MedecinInstallationId { get; set; }
        public int InstallationId { get; set; }
        public string NomInstallation { get; set; }
        public byte RangInstallation { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstPrincipale { get; set; }
    }

    public class MedecinUniteOrganisationnelleItemViewModel
    {
        public int MedecinUniteOrganisationnelleId { get; set; }
        public int UniteOrganisationnelleId { get; set; }
        public string LibelleUnite { get; set; }
        public string TypeUnite { get; set; }
        public byte? RangSource { get; set; }
        public string RoleOrganisationnel { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }

    public class MedecinAdresseFormViewModel : MedecinCoordonneesParentInfoViewModel
    {
        public int MedecinAdresseId { get; set; }

        [Display(Name = "Adresse")]
        public string AdresseLigne1 { get; set; }

        public string Ville { get; set; }
        public string Province { get; set; }

        [Display(Name = "Code postal")]
        public string CodePostal { get; set; }

        [Display(Name = "Principale")]
        public bool EstPrincipale { get; set; }
    }

    public class MedecinTelephoneFormViewModel : MedecinCoordonneesParentInfoViewModel
    {
        public int MedecinTelephoneId { get; set; }

        [Display(Name = "Type téléphone")]
        public string TypeTelephone { get; set; }

        [Display(Name = "Numéro")]
        public string NumeroTelephone { get; set; }

        [Display(Name = "Rang source")]
        public byte? RangSource { get; set; }

        [Display(Name = "Principal")]
        public bool EstPrincipal { get; set; }
    }

    public class MedecinCourrielFormViewModel : MedecinCoordonneesParentInfoViewModel
    {
        public int MedecinCourrielId { get; set; }

        [Display(Name = "Type courriel")]
        public string TypeCourriel { get; set; }

        [Display(Name = "Adresse courriel")]
        public string AdresseCourriel { get; set; }

        [Display(Name = "Rang source")]
        public byte? RangSource { get; set; }

        [Display(Name = "Principal")]
        public bool EstPrincipal { get; set; }
    }

    public class MedecinInstallationFormViewModel : MedecinCoordonneesParentInfoViewModel
    {
        public MedecinInstallationFormViewModel()
        {
            InstallationsDisponibles = new List<SelectListItem>();
        }

        public int MedecinInstallationId { get; set; }

        [Display(Name = "Installation")]
        public int? InstallationId { get; set; }

        [Display(Name = "Rang")]
        public byte RangInstallation { get; set; }

        [Display(Name = "Date début")]
        public DateTime? DateDebut { get; set; }

        [Display(Name = "Date fin")]
        public DateTime? DateFin { get; set; }

        [Display(Name = "Principale")]
        public bool EstPrincipale { get; set; }

        public IList<SelectListItem> InstallationsDisponibles { get; set; }
    }

    public class MedecinUniteOrganisationnelleFormViewModel : MedecinCoordonneesParentInfoViewModel
    {
        public MedecinUniteOrganisationnelleFormViewModel()
        {
            UnitesDisponibles = new List<SelectListItem>();
        }

        public int MedecinUniteOrganisationnelleId { get; set; }

        [Display(Name = "Unité")]
        public int? UniteOrganisationnelleId { get; set; }

        [Display(Name = "Rang source")]
        public byte? RangSource { get; set; }

        [Display(Name = "Rôle")]
        public string RoleOrganisationnel { get; set; }

        [Display(Name = "Date début")]
        public DateTime? DateDebut { get; set; }

        [Display(Name = "Date fin")]
        public DateTime? DateFin { get; set; }

        public IList<SelectListItem> UnitesDisponibles { get; set; }
    }
}
