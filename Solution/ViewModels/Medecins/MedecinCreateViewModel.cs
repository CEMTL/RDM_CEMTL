using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.Medecins
{
    public class MedecinCreateViewModel
    {
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

        [Display(Name = "Commentaires")]
        [StringLength(4000)]
        public string Commentaires { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }

        public IEnumerable<SelectListItem> Titres { get; set; }
        public IEnumerable<SelectListItem> TypesPermis { get; set; }
        public IEnumerable<SelectListItem> StatutsMedecin { get; set; }
        public IEnumerable<SelectListItem> CategoriesPratique { get; set; }
        public IEnumerable<SelectListItem> Genres { get; set; }

        public MedecinCreateViewModel()
        {
            EstActif = true;
            Titres = new List<SelectListItem>();
            TypesPermis = new List<SelectListItem>();
            StatutsMedecin = new List<SelectListItem>();
            CategoriesPratique = new List<SelectListItem>();
            Genres = new List<SelectListItem>();
        }
    }
}