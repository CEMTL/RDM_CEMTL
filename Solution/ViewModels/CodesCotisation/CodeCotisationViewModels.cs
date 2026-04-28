using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.CodesCotisation
{
    public class CodeCotisationListViewModel
    {
        [Display(Name = "CodeCotisationId")]
        public int CodeCotisationId { get; set; }
        [Display(Name = "Code")]
        public string CodeCotisation { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class CodeCotisationFormViewModel
    {
        public int CodeCotisationId { get; set; }
        [Display(Name = "Code")]
        public string CodeCotisation { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }
        [Display(Name = "Actif")]
        public bool EstActif { get; set; }
    }

    public class CodeCotisationDetailsViewModel : CodeCotisationListViewModel
    {
    }
}