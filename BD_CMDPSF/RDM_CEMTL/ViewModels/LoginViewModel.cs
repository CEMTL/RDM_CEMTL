using System.ComponentModel.DataAnnotations;

namespace Solution.ViewModels;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Le compte Windows est obligatoire.")]
    [Display(Name = "Compte Windows")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Le mot de passe Windows est obligatoire.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; }

    public string ReturnUrl { get; set; }
    public string ErrorMessage { get; set; }
}
