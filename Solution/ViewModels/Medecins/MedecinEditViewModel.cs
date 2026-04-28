using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Solution.ViewModels.Medecins
{
    public class MedecinEditViewModel : IValidatableObject
    {
        public int MedecinId { get; set; }

        [Display(Name = "Titre")]
        public int? TitreId { get; set; }

        [Required(ErrorMessage = "Le permis est obligatoire.")]
        [StringLength(20)]
        [Display(Name = "Permis")]
        public string Permis { get; set; }

        [StringLength(20)]
        [Display(Name = "NAM")]
        public string NAM { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(150)]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [StringLength(150)]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; }

        [Display(Name = "Type de permis")]
        public int? TypePermisId { get; set; }

        [Display(Name = "Statut")]
        public int? StatutMedecinId { get; set; }

        [Display(Name = "Catégorie de pratique")]
        public int? CategoriePratiqueId { get; set; }

        //[StringLength(150)]
        //[Display(Name = "Type de nomination")]
        //public string TypeNomination { get; set; }



        [Display(Name = "Genre")]
        public int? GenreId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date de naissance")]
        public DateTime? DateNaissance { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date d'entrée en fonction")]
        public DateTime? DateEntreeFonction { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date de départ")]
        public DateTime? DateDepart { get; set; }

        [StringLength(4000)]
        [Display(Name = "Commentaires")]
        public string Commentaires { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }

        public DateTime? DateCreation { get; set; }
        public DateTime? DateModification { get; set; }

        public List<MedecinAdresseEditItemViewModel> Adresses { get; set; }
        public List<MedecinTelephoneEditItemViewModel> Telephones { get; set; }
        public List<MedecinCourrielEditItemViewModel> Courriels { get; set; }
        public List<MedecinInstallationEditItemViewModel> Installations { get; set; }
        public List<MedecinUniteOrganisationnelleEditItemViewModel> UnitesOrganisationnelles { get; set; }
        public List<NominationAdministrativeEditItemViewModel> NominationsAdministratives { get; set; }

        public IEnumerable<SelectListItem> Titres { get; set; }
        public IEnumerable<SelectListItem> TypesPermis { get; set; }
        public IEnumerable<SelectListItem> StatutsMedecin { get; set; }
        public IEnumerable<SelectListItem> CategoriesPratique { get; set; }
        public IEnumerable<SelectListItem> Genres { get; set; }
        public IEnumerable<SelectListItem> TypesTelephoneDisponibles { get; set; }
        public IEnumerable<SelectListItem> TypesCourrielDisponibles { get; set; }
        public IEnumerable<SelectListItem> InstallationsDisponibles { get; set; }
        public IEnumerable<SelectListItem> UnitesOrganisationnellesDisponibles { get; set; }
        public List<SelectListItem> NominationsDisponibles { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> TypesNominationDisponibles { get; set; } = new List<SelectListItem>();



        public MedecinEditViewModel()
        {
            Adresses = new List<MedecinAdresseEditItemViewModel>();
            Telephones = new List<MedecinTelephoneEditItemViewModel>();
            Courriels = new List<MedecinCourrielEditItemViewModel>();
            Installations = new List<MedecinInstallationEditItemViewModel>();
            UnitesOrganisationnelles = new List<MedecinUniteOrganisationnelleEditItemViewModel>();
            NominationsAdministratives = new List<NominationAdministrativeEditItemViewModel>();

            Titres = new List<SelectListItem>();
            TypesPermis = new List<SelectListItem>();
            StatutsMedecin = new List<SelectListItem>();
            CategoriesPratique = new List<SelectListItem>();
            Genres = new List<SelectListItem>();
            TypesTelephoneDisponibles = new List<SelectListItem>();
            TypesCourrielDisponibles = new List<SelectListItem>();
            InstallationsDisponibles = new List<SelectListItem>();
            UnitesOrganisationnellesDisponibles = new List<SelectListItem>();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateDepart.HasValue && DateEntreeFonction.HasValue && DateDepart.Value < DateEntreeFonction.Value)
            {
                yield return new ValidationResult("La date de départ ne peut pas être antérieure à la date d'entrée en fonction.",
                    new[] { "DateDepart", "DateEntreeFonction" });
            }

            if (DateNaissance.HasValue && DateNaissance.Value > DateTime.Today)
            {
                yield return new ValidationResult("La date de naissance ne peut pas être dans le futur.",
                    new[] { "DateNaissance" });
            }

            var telephonesActifs = Telephones.Where(t => !t.Supprimer && !string.IsNullOrWhiteSpace(t.NumeroTelephone)).ToList();
            if (telephonesActifs.Count(t => t.EstPrincipal) > 1)
            {
                yield return new ValidationResult("Un seul téléphone principal est permis.", new[] { "Telephones" });
            }

            var courrielsActifs = Courriels.Where(c => !c.Supprimer && !string.IsNullOrWhiteSpace(c.AdresseCourriel)).ToList();
            if (courrielsActifs.Count(c => c.EstPrincipal) > 1)
            {
                yield return new ValidationResult("Un seul courriel principal est permis.", new[] { "Courriels" });
            }

            var installationsActives = Installations.Where(i => !i.Supprimer && i.InstallationId > 0).ToList();
            if (installationsActives.Count(i => i.EstPrincipale) > 1)
            {
                yield return new ValidationResult("Une seule installation principale est permise.", new[] { "Installations" });
            }
        }
    }

    public class MedecinAdresseEditItemViewModel
    {
        public int MedecinAdresseId { get; set; }

        [StringLength(255)]
        public string AdresseLigne1 { get; set; }

        [StringLength(100)]
        public string Ville { get; set; }

        [StringLength(100)]
        public string Province { get; set; }

        [StringLength(20)]
        public string CodePostal { get; set; }

        public bool EstPrincipale { get; set; }
        public bool Supprimer { get; set; }
    }

    public class MedecinTelephoneEditItemViewModel
    {
        public int MedecinTelephoneId { get; set; }

        [StringLength(50)]
        public string TypeTelephone { get; set; }

        [StringLength(50)]
        public string NumeroTelephone { get; set; }

        public byte? RangSource { get; set; }
        public bool EstPrincipal { get; set; }
        public bool Supprimer { get; set; }
    }

    public class MedecinCourrielEditItemViewModel
    {
        public int MedecinCourrielId { get; set; }

        [StringLength(50)]
        public string TypeCourriel { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string AdresseCourriel { get; set; }

        public byte? RangSource { get; set; }
        public bool EstPrincipal { get; set; }
        public bool Supprimer { get; set; }
    }

    public class MedecinInstallationEditItemViewModel
    {
        public int MedecinInstallationId { get; set; }
        public int InstallationId { get; set; }

        [Range(1, 255)]
        public byte RangInstallation { get; set; }

        public bool EstPrincipale { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool Supprimer { get; set; }
    }

    public class MedecinUniteOrganisationnelleEditItemViewModel
    {
        public int MedecinUniteOrganisationnelleId { get; set; }
        public int UniteOrganisationnelleId { get; set; }

        [StringLength(50)]
        public string RoleOrganisationnel { get; set; }

        public byte? RangSource { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool Supprimer { get; set; }
    }

    public class NominationAdministrativeEditItemViewModel
    {
        public int NominationAdministrativeId { get; set; }

        [StringLength(255)]
        public string LibelleNomination { get; set; }

        public DateTime? DateNomination { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstActive { get; set; }
        public bool Supprimer { get; set; }

        public int ? TypeNominationId { get; set; }
        public int ? NominationId { get; set; }

    }
}