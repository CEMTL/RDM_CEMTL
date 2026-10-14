using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Solution.ViewModels.DocumentsMedecin
{
    public class DocumentMedecinListViewModel
    {
        public int DocumentMedecinId { get; set; }
        public int MedecinId { get; set; }
        public string Medecin { get; set; }
        public string TypeDocument { get; set; }
        public string NomFichier { get; set; }
        public long TailleOctets { get; set; }
        public DateTime? DateDocument { get; set; }
        public DateTime? DateExpiration { get; set; }
        public string Description { get; set; }
        public bool Confidentiel { get; set; }
        public DateTime DateDepot { get; set; }
        public string DeposePar { get; set; }
        public bool EstActif { get; set; }
    }

    public class DocumentMedecinCreateViewModel
    {
        [Required(ErrorMessage = "Le médecin est requis.")]
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }

        [Required(ErrorMessage = "Le type de document est requis.")]
        [Display(Name = "Type de document")]
        [StringLength(100)]
        public string TypeDocument { get; set; }

        [Required(ErrorMessage = "Le fichier est requis.")]
        [Display(Name = "Fichier")]
        public IFormFile Fichier { get; set; }

        [Display(Name = "Date du document")]
        [DataType(DataType.Date)]
        public DateTime? DateDocument { get; set; }

        [Display(Name = "Date d'expiration")]
        [DataType(DataType.Date)]
        public DateTime? DateExpiration { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Confidentiel")]
        public bool Confidentiel { get; set; }

        public string EntiteLiee { get; set; }
        public int? EntiteId { get; set; }
        public int? ReturnMedecinId { get; set; }
        public string ReturnTab { get; set; }
    }
}
