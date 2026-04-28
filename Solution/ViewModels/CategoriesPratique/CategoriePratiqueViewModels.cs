using System.ComponentModel.DataAnnotations;

namespace Solution.ViewModels.CategoriesPratique
{
    public class CategoriePratiqueListViewModel
    {
        public int CategoriePratiqueId { get; set; }
        [Display(Name = "Libellé")]
        public string Libelle { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class CategoriePratiqueFormViewModel
    {
        public int CategoriePratiqueId { get; set; }
        [Display(Name = "Libellé")]
        [Required]
        [StringLength(255)]
        public string Libelle { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class CategoriePratiqueDetailsViewModel : CategoriePratiqueListViewModel
    {
    }
}
