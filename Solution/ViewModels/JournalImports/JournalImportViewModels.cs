using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.JournalImports
{
    public class JournalImportListViewModel
    {
        [Display(Name = "JournalImportId")]
        public int JournalImportId { get; set; }
        [Display(Name = "Nom fichier")]
        public string NomFichier { get; set; }
        [Display(Name = "Date import")]
        public DateTime DateImport { get; set; }
        [Display(Name = "Lignes importées")]
        public int? LignesImportees { get; set; }
        [Display(Name = "Lignes rejetées")]
        public int? LignesRejetees { get; set; }
        [Display(Name = "Commentaire")]
        public string Commentaire { get; set; }
    }

    public class JournalImportFormViewModel
    {
        public int JournalImportId { get; set; }
        [Display(Name = "Nom fichier")]
        [Required]
        [StringLength(255)]
        public string NomFichier { get; set; }
        [Display(Name = "Date import")]
        [DataType(DataType.Date)]
        public DateTime DateImport { get; set; }
        [Display(Name = "Lignes importées")]
        public int? LignesImportees { get; set; }
        [Display(Name = "Lignes rejetées")]
        public int? LignesRejetees { get; set; }
        [Display(Name = "Commentaire")]
        public string Commentaire { get; set; }
    }

    public class JournalImportDetailsViewModel : JournalImportListViewModel
    {
    }
}