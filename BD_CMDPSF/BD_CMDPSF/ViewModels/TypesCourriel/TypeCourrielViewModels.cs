using System.ComponentModel.DataAnnotations;

namespace Solution.ViewModels.TypesCourriel
{
    public class TypeCourrielListViewModel
    {
        public int TypeCourrielId { get; set; }

        [Display(Name = "Libellé")]
        public string Libelle { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }

        [Display(Name = "Courriels associés")]
        public int NombreUtilisations { get; set; }
    }

    public class TypeCourrielFormViewModel
    {
        public int TypeCourrielId { get; set; }

        [Display(Name = "Libellé")]
        [Required(ErrorMessage = "Le libellé est obligatoire.")]
        [StringLength(50, ErrorMessage = "Le libellé ne peut pas dépasser 50 caractères.")]
        public string Libelle { get; set; }

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class TypeCourrielDetailsViewModel : TypeCourrielListViewModel
    {
    }
}
