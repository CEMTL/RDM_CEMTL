using System.ComponentModel.DataAnnotations;

namespace Solution.ViewModels.TitresMedecin
{
    public class TitreMedecinListViewModel
    {
        public int TitreId { get; set; }
        [Display(Name = "Libellé")]
        public string Libelle { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class TitreMedecinFormViewModel
    {
        public int TitreId { get; set; }
        [Display(Name = "Libellé")]
        [Required]
        [StringLength(255)]
        public string Libelle { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class TitreMedecinDetailsViewModel : TitreMedecinListViewModel
    {
    }
}
