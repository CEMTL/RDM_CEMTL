using System.ComponentModel.DataAnnotations;

namespace Solution.ViewModels.Genres
{
    public class GenreListViewModel
    {
        public int GenreId { get; set; }
        [Display(Name = "Code genre")]
        public string CodeGenre { get; set; }
        [Display(Name = "Libellé")]
        public string Libelle { get; set; }
    }

    public class GenreFormViewModel
    {
        public int GenreId { get; set; }
        [Display(Name = "Code genre")]
        [Required]
        [StringLength(10)]
        public string CodeGenre { get; set; }
        [Display(Name = "Libellé")]
        [StringLength(50)]
        public string Libelle { get; set; }
    }

    public class GenreDetailsViewModel : GenreListViewModel
    {
    }
}
