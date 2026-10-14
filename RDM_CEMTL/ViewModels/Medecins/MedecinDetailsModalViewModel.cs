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
        public List<MedecinServiceItemVm> Services { get; set; }
        public List<MedecinAffectationOrganisationnelleItemVm> AffectationsOrganisationnelles { get; set; }
        public List<NominationAdministrativeItemVm> NominationsAdministratives { get; set; }

        // Données métier centralisées dans le dossier du médecin.
        // Ces collections sont chargées séparément dans MedecinsController afin
        // d'éviter un produit cartésien EF avec les nombreuses autres collections.
        public List<MedecinPrivilegeDetailsItemVm> Privileges { get; set; }
        public List<MedecinCotisationDetailsItemVm> Cotisations { get; set; }
        public List<MedecinFitTestDetailsItemVm> FitTests { get; set; }
        public List<MedecinEntenteFdcDetailsItemVm> EntentesFDC { get; set; }
        public List<MedecinAffectationCliniqueDetailsItemVm> AffectationsCliniques { get; set; }
        public List<MedecinComiteDetailsItemVm> Comites { get; set; }
        public List<MedecinDocumentDetailsItemVm> Documents { get; set; }
        public List<Solution.ViewModels.Shared.AlerteMedecinItemViewModel> Alertes { get; set; }

        public MedecinDetailsModalViewModel()
        {
            Adresses = new List<MedecinAdresseItemVm>();
            Telephones = new List<MedecinTelephoneItemVm>();
            Courriels = new List<MedecinCourrielItemVm>();
            Installations = new List<MedecinInstallationItemVm>();
            Services = new List<MedecinServiceItemVm>();
            AffectationsOrganisationnelles = new List<MedecinAffectationOrganisationnelleItemVm>();
            NominationsAdministratives = new List<NominationAdministrativeItemVm>();
            Privileges = new List<MedecinPrivilegeDetailsItemVm>();
            Cotisations = new List<MedecinCotisationDetailsItemVm>();
            FitTests = new List<MedecinFitTestDetailsItemVm>();
            EntentesFDC = new List<MedecinEntenteFdcDetailsItemVm>();
            AffectationsCliniques = new List<MedecinAffectationCliniqueDetailsItemVm>();
            Comites = new List<MedecinComiteDetailsItemVm>();
            Documents = new List<MedecinDocumentDetailsItemVm>();
            Alertes = new List<Solution.ViewModels.Shared.AlerteMedecinItemViewModel>();
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

    public class MedecinServiceItemVm
    {
        public string Departement { get; set; }
        public string Service { get; set; }
        public int NombrePrivileges { get; set; }
    }

    public class MedecinAffectationOrganisationnelleItemVm
    {
        public int AffectationOrganisationnelleId { get; set; }
        public string Role { get; set; }
        public string Departement { get; set; }
        public string Service { get; set; }
        public string Regroupement { get; set; }
        public string Titre { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstActive { get; set; }
        public bool EstAValider { get; set; }
    }

    public class NominationAdministrativeItemVm
    {
        public int NominationAdministrativeId { get; set; }
        public string LibelleNomination { get; set; }
        public DateTime? DateNomination { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstActive { get; set; }
    }

    public class MedecinPrivilegeDetailsItemVm
    {
        public int MedecinPrivilegeId { get; set; }
        public string CodePrivilege { get; set; }
        public string PrivilegeLibelle { get; set; }
        public string InstallationLibelle { get; set; }
        public string DepartementLibelle { get; set; }
        public string ServiceLibelle { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public string Etat { get; set; }
        public bool EstActif { get; set; }
    }

    public class MedecinCotisationDetailsItemVm
    {
        public int CotisationAnnuelleId { get; set; }
        public short AnneeExercice { get; set; }
        public string CodeCotisation { get; set; }
        public decimal? MontantInitial { get; set; }
        public decimal? MontantFinal { get; set; }
        public decimal? MontantPaye { get; set; }
        public string EtatCotisation { get; set; }
        public DateTime? DatePaiement { get; set; }
        public string TypePaiement { get; set; }
    }

    public class MedecinFitTestDetailsItemVm
    {
        public int FitTestId { get; set; }
        public string Resultat { get; set; }
        public DateTime? DateFitTest { get; set; }
        public string Commentaire { get; set; }
        public bool EstActif { get; set; }
    }

    public class MedecinEntenteFdcDetailsItemVm
    {
        public int EntenteFDCId { get; set; }
        public string TypeEntenteFDC { get; set; }
        public DateTime? DateAdhesion { get; set; }
        public DateTime? DateFin { get; set; }
        public string PEMVise { get; set; }
        public bool EstActive { get; set; }
    }

    public class MedecinAffectationCliniqueDetailsItemVm
    {
        public int MedecinAffectationCliniqueId { get; set; }
        public string Departement { get; set; }
        public string Service { get; set; }
        public string Installation { get; set; }
        public string TypeAffectation { get; set; }
        public bool EstPrincipale { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstActive { get; set; }
    }

    public class MedecinComiteDetailsItemVm
    {
        public int MedecinComiteId { get; set; }
        public string Comite { get; set; }
        public string RoleMembre { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public string Commentaire { get; set; }
    }

    public class MedecinDocumentDetailsItemVm
    {
        public int DocumentMedecinId { get; set; }
        public string TypeDocument { get; set; }
        public string NomFichier { get; set; }
        public DateTime? DateDocument { get; set; }
        public DateTime? DateExpiration { get; set; }
        public bool Confidentiel { get; set; }
        public bool EstActif { get; set; }
    }
}
