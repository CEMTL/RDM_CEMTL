using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace Solution.ViewModels
{
    // ─── Utilisateurs ────────────────────────────────────────────────────────

    public class UserIndexViewModel
    {
        public string Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Roles { get; set; }
        public bool EstActif { get; set; }
    }

    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Obligatoire")]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Obligatoire")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "Obligatoire")]
        [EmailAddress(ErrorMessage = "Courriel invalide")]
        [Display(Name = "Courriel")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Obligatoire")]
        [Display(Name = "Nom d'utilisateur")]
        public string UserName { get; set; }

        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$",
            ErrorMessage = "Téléphone invalide")]
        [Display(Name = "Téléphone")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Obligatoire")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Minimum 6 caractères")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Obligatoire")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
        [Display(Name = "Confirmer")]
        public string ConfirmPassword { get; set; }

        // AspNetUserRoles
        public List<string> RolesSelectionnes { get; set; } = new List<string>();
        public List<SelectListItem> RolesDisponibles { get; set; } = new List<SelectListItem>();
    }

    public class UserEditViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Obligatoire")]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Obligatoire")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "Obligatoire")]
        [EmailAddress(ErrorMessage = "Courriel invalide")]
        [Display(Name = "Courriel")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Obligatoire")]
        [Display(Name = "Nom d'utilisateur")]
        public string UserName { get; set; }

        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$",
            ErrorMessage = "Téléphone invalide")]
        [Display(Name = "Téléphone")]
        public string PhoneNumber { get; set; }

        // AspNetUserRoles
        public List<string> RolesSelectionnes { get; set; } = new List<string>();
        public List<SelectListItem> RolesDisponibles { get; set; } = new List<SelectListItem>();
    }

    // ─── Rôles ───────────────────────────────────────────────────────────────

    public class RoleIndexViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int NbUtilisateurs { get; set; }   // nb de lignes dans AspNetUserRoles
    }

    public class RoleFormViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Obligatoire")]
        [StringLength(50)]
        [Display(Name = "Nom du rôle")]
        public string Name { get; set; }
    }

    public class RoleDeleteViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int NbUtilisateurs { get; set; }  // nb de lignes dans AspNetUserRoles
    }
}
