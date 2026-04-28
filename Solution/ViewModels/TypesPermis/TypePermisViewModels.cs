using System.ComponentModel.DataAnnotations;

namespace Solution.ViewModels.TypesPermis
{
    public class TypePermisListViewModel
    {
        public int TypePermisId { get; set; }
        [Display(Name = "Libellé")]
        public string Libelle { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class TypePermisFormViewModel
    {
        public int TypePermisId { get; set; }
        [Display(Name = "Libellé")]
        [Required]
        [StringLength(255)]
        public string Libelle { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class TypePermisDetailsViewModel : TypePermisListViewModel
    {
    }
}
