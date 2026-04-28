using System.ComponentModel.DataAnnotations;

namespace Solution.ViewModels.StatutsMedecin
{
    public class StatutMedecinListViewModel
    {
        public int StatutMedecinId { get; set; }
        [Display(Name = "Libellé")]
        public string Libelle { get; set; }
        [Display(Name = "Code statut")]
        public string CodeStatut { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class StatutMedecinFormViewModel
    {
        public int StatutMedecinId { get; set; }
        [Display(Name = "Code statut")]
        [Required]
        [StringLength(30)]
        public string CodeStatut { get; set; }
        [Display(Name = "Libellé")]
        [Required]
        [StringLength(255)]
        public string Libelle { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class StatutMedecinDetailsViewModel : StatutMedecinListViewModel
    {
    }
}
