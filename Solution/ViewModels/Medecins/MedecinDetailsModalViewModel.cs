using System;
using System.Collections.Generic;

namespace Solution.ViewModels.Medecins
{
    public class MedecinDetailsModalViewModel
    {
        public int MedecinId { get; set; }
        public string Permis { get; set; }
        public string Titre { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string NomComplet { get; set; }
        public string TypePermis { get; set; }
        public string StatutMedecin { get; set; }
        public string CategoriePratique { get; set; }
        public string TypeNomination { get; set; }
        public string Genre { get; set; }
        public DateTime? DateNaissance { get; set; }
        public string NAM { get; set; }
        public DateTime? DateEntreeFonction { get; set; }
        public DateTime? DateDepart { get; set; }
        public bool EstActif { get; set; }
        public string Commentaires { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime DateModification { get; set; }

        public string AdressePrincipale { get; set; }
        public string TelephonePrincipal { get; set; }
        public string CourrielPrincipal { get; set; }
        public string InstallationPrincipale { get; set; }
        public string NominationActive { get; set; }

        public List<MedecinAdresseItemVm> Adresses { get; set; }
        public List<MedecinTelephoneItemVm> Telephones { get; set; }
        public List<MedecinCourrielItemVm> Courriels { get; set; }
        public List<MedecinInstallationItemVm> Installations { get; set; }
        public List<MedecinUniteOrganisationnelleItemVm> UnitesOrganisationnelles { get; set; }
        public List<NominationAdministrativeItemVm> NominationsAdministratives { get; set; }

        public MedecinDetailsModalViewModel()
        {
            Adresses = new List<MedecinAdresseItemVm>();
            Telephones = new List<MedecinTelephoneItemVm>();
            Courriels = new List<MedecinCourrielItemVm>();
            Installations = new List<MedecinInstallationItemVm>();
            UnitesOrganisationnelles = new List<MedecinUniteOrganisationnelleItemVm>();
            NominationsAdministratives = new List<NominationAdministrativeItemVm>();
        }
    }

    public class MedecinAdresseItemVm
    {
        public string AdresseLigne1 { get; set; }
        public string Ville { get; set; }
        public string Province { get; set; }
        public string CodePostal { get; set; }
        public bool EstPrincipale { get; set; }
    }

    public class MedecinTelephoneItemVm
    {
        public string TypeTelephone { get; set; }
        public string NumeroTelephone { get; set; }
        public bool EstPrincipal { get; set; }
        public byte? RangSource { get; set; }
    }

    public class MedecinCourrielItemVm
    {
        public string TypeCourriel { get; set; }
        public string AdresseCourriel { get; set; }
        public bool EstPrincipal { get; set; }
        public byte? RangSource { get; set; }
    }

    public class MedecinInstallationItemVm
    {
        public string NomInstallation { get; set; }
        public string CodeInstallation { get; set; }
        public byte RangInstallation { get; set; }
        public bool EstPrincipale { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }

    public class MedecinUniteOrganisationnelleItemVm
    {
        public string Unite { get; set; }
        public string TypeUnite { get; set; }
        public string RoleOrganisationnel { get; set; }
        public byte? RangSource { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }

    public class NominationAdministrativeItemVm
    {
        public string LibelleNomination { get; set; }
        public DateTime? DateNomination { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstActive { get; set; }
    }
}