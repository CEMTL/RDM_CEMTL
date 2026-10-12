using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public List<NominationAdministrativeEditItemViewModel> NominationsAdministratives { get; set; }

        // Vue complète du dossier utilisée en édition afin de présenter
        // les mêmes volets métier que la fenêtre Détails.
        public MedecinDetailsModalViewModel Dossier { get; set; } = new MedecinDetailsModalViewModel();

        // Liste explicite des champs/sections en erreur affichée en haut de Medecins/Edit.
        // Elle est reconstruite par le contrôleur à partir de ModelState afin que
        // l'utilisateur sache exactement quoi corriger, même pour les collections.
        public List<MedecinEditValidationIssueViewModel> ErreursValidation { get; set; } = new List<MedecinEditValidationIssueViewModel>();

        public IEnumerable<SelectListItem> Titres { get; set; }
        public IEnumerable<SelectListItem> TypesPermis { get; set; }
        public IEnumerable<SelectListItem> StatutsMedecin { get; set; }
        public IEnumerable<SelectListItem> CategoriesPratique { get; set; }
        public IEnumerable<SelectListItem> Genres { get; set; }
        public IEnumerable<SelectListItem> TypesTelephoneDisponibles { get; set; }
        public IEnumerable<SelectListItem> TypesCourrielDisponibles { get; set; }
        public IEnumerable<SelectListItem> InstallationsDisponibles { get; set; }
        public List<SelectListItem> NominationsDisponibles { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> TypesNominationDisponibles { get; set; } = new List<SelectListItem>();



        public MedecinEditViewModel()
        {
            Adresses = new List<MedecinAdresseEditItemViewModel>();
            Telephones = new List<MedecinTelephoneEditItemViewModel>();
            Courriels = new List<MedecinCourrielEditItemViewModel>();
            Installations = new List<MedecinInstallationEditItemViewModel>();
            NominationsAdministratives = new List<NominationAdministrativeEditItemViewModel>();

            Titres = new List<SelectListItem>();
            TypesPermis = new List<SelectListItem>();
            StatutsMedecin = new List<SelectListItem>();
            CategoriesPratique = new List<SelectListItem>();
            Genres = new List<SelectListItem>();
            TypesTelephoneDisponibles = new List<SelectListItem>();
            TypesCourrielDisponibles = new List<SelectListItem>();
            InstallationsDisponibles = new List<SelectListItem>();
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

            var telephonesActifs = Telephones
                .Select((item, index) => new { item, index })
                .Where(x => !x.item.Supprimer &&
                            (!string.IsNullOrWhiteSpace(x.item.TypeTelephone) ||
                             !string.IsNullOrWhiteSpace(x.item.NumeroTelephone) ||
                             x.item.RangSource.HasValue ||
                             x.item.EstPrincipal))
                .ToList();

            foreach (var x in telephonesActifs)
            {
                if (string.IsNullOrWhiteSpace(x.item.TypeTelephone))
                {
                    yield return new ValidationResult(
                        "Le type de téléphone est obligatoire lorsque le numéro est renseigné.",
                        new[] { $"Telephones[{x.index}].TypeTelephone" });
                }

                if (string.IsNullOrWhiteSpace(x.item.NumeroTelephone))
                {
                    yield return new ValidationResult(
                        "Le numéro de téléphone est obligatoire lorsque le type est renseigné.",
                        new[] { $"Telephones[{x.index}].NumeroTelephone" });
                }
            }

            if (telephonesActifs.Count(x => x.item.EstPrincipal && !string.IsNullOrWhiteSpace(x.item.NumeroTelephone)) > 1)
            {
                yield return new ValidationResult("Un seul téléphone principal est permis.", new[] { "Telephones" });
            }

            var courrielsActifs = Courriels
                .Select((item, index) => new { item, index })
                .Where(x => !x.item.Supprimer &&
                            (!string.IsNullOrWhiteSpace(x.item.TypeCourriel) ||
                             !string.IsNullOrWhiteSpace(x.item.AdresseCourriel) ||
                             x.item.RangSource.HasValue ||
                             x.item.EstPrincipal))
                .ToList();

            foreach (var x in courrielsActifs)
            {
                if (string.IsNullOrWhiteSpace(x.item.TypeCourriel))
                {
                    yield return new ValidationResult(
                        "Le type de courriel est obligatoire lorsque l'adresse est renseignée.",
                        new[] { $"Courriels[{x.index}].TypeCourriel" });
                }

                if (string.IsNullOrWhiteSpace(x.item.AdresseCourriel))
                {
                    yield return new ValidationResult(
                        "L'adresse courriel est obligatoire lorsque le type est renseigné.",
                        new[] { $"Courriels[{x.index}].AdresseCourriel" });
                }
            }

            if (courrielsActifs.Count(x => x.item.EstPrincipal && !string.IsNullOrWhiteSpace(x.item.AdresseCourriel)) > 1)
            {
                yield return new ValidationResult("Un seul courriel principal est permis.", new[] { "Courriels" });
            }

            var installationsRenseignees = Installations
                .Select((item, index) => new { item, index })
                .Where(x => !x.item.Supprimer &&
                            ((x.item.InstallationId.HasValue && x.item.InstallationId.Value > 0) ||
                             x.item.EstPrincipale ||
                             x.item.DateDebut.HasValue ||
                             x.item.DateFin.HasValue ||
                             (x.item.RangInstallation.HasValue && x.item.RangInstallation.Value != 1)))
                .ToList();

            foreach (var x in installationsRenseignees)
            {
                if (!x.item.InstallationId.HasValue || x.item.InstallationId.Value <= 0)
                {
                    yield return new ValidationResult(
                        "L'installation est obligatoire lorsque la ligne contient d'autres informations.",
                        new[] { $"Installations[{x.index}].InstallationId" });
                }

                if (x.item.InstallationId.HasValue && x.item.InstallationId.Value > 0 &&
                    !x.item.RangInstallation.HasValue)
                {
                    yield return new ValidationResult(
                        "Le rang de l'installation est obligatoire.",
                        new[] { $"Installations[{x.index}].RangInstallation" });
                }

                if (x.item.DateDebut.HasValue && x.item.DateFin.HasValue &&
                    x.item.DateFin.Value.Date < x.item.DateDebut.Value.Date)
                {
                    yield return new ValidationResult(
                        "La date de fin ne peut pas être antérieure à la date de début.",
                        new[]
                        {
                            $"Installations[{x.index}].DateDebut",
                            $"Installations[{x.index}].DateFin"
                        });
                }
            }

            var installationsActives = installationsRenseignees
                .Where(x => x.item.InstallationId.HasValue && x.item.InstallationId.Value > 0)
                .ToList();

            if (installationsActives.Count(x => x.item.EstPrincipale) > 1)
            {
                yield return new ValidationResult("Une seule installation principale est permise.", new[] { "Installations" });
            }

            var installationsEnDouble = installationsActives
                .Where(x => x.item.RangInstallation.HasValue)
                .GroupBy(x => new
                {
                    InstallationId = x.item.InstallationId.Value,
                    RangInstallation = x.item.RangInstallation.Value
                })
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToList();

            foreach (var x in installationsEnDouble)
            {
                yield return new ValidationResult(
                    "Cette combinaison installation / rang est déjà présente pour ce médecin.",
                    new[]
                    {
                        $"Installations[{x.index}].InstallationId",
                        $"Installations[{x.index}].RangInstallation"
                    });
            }

            foreach (var x in NominationsAdministratives.Select((item, index) => new { item, index })
                         .Where(x => !x.item.Supprimer))
            {
                if (x.item.DateNomination.HasValue && x.item.DateFin.HasValue &&
                    x.item.DateFin.Value.Date < x.item.DateNomination.Value.Date)
                {
                    yield return new ValidationResult(
                        "La date de fin de la nomination ne peut pas être antérieure à la date de nomination.",
                        new[]
                        {
                            $"NominationsAdministratives[{x.index}].DateNomination",
                            $"NominationsAdministratives[{x.index}].DateFin"
                        });
                }
            }
        }
    }

    public class MedecinEditValidationIssueViewModel
    {
        public string Cle { get; set; }
        public string Champ { get; set; }
        public string Message { get; set; }
        public string Onglet { get; set; }
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

        [EmailAddress(ErrorMessage = "L'adresse courriel n'est pas valide.")]
        [StringLength(255)]
        public string AdresseCourriel { get; set; }

        public byte? RangSource { get; set; }
        public bool EstPrincipal { get; set; }
        public bool Supprimer { get; set; }
    }

    public class MedecinInstallationEditItemViewModel
    {
        public int MedecinInstallationId { get; set; }
        public int? InstallationId { get; set; }

        [Range(1, 3, ErrorMessage = "Le rang de l'installation doit être compris entre 1 et 3.")]
        public byte? RangInstallation { get; set; }

        public bool EstPrincipale { get; set; }
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

        public int? TypeNominationId { get; set; }
        public int? NominationId { get; set; }

    }
}
