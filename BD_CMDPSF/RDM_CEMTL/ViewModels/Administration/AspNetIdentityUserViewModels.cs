using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Solution.ViewModels.Administration;

public sealed class AspNetIdentityUserListViewModel
{
    public IReadOnlyList<AspNetIdentityUserRowViewModel> Utilisateurs { get; set; } = Array.Empty<AspNetIdentityUserRowViewModel>();
}

public sealed class AspNetIdentityUserRowViewModel
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public bool IsBootstrapAdmin { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}

public sealed class AspNetIdentityUserCreateViewModel
{
    [Required(ErrorMessage = "Le compte Windows est obligatoire.")]
    [Display(Name = "Compte Windows")]
    public string UserName { get; set; }

    [EmailAddress(ErrorMessage = "Le courriel n'est pas valide.")]
    public string Email { get; set; }

    [Display(Name = "Rôles RDM")]
    public List<string> RolesSelectionnes { get; set; } = new();
    public IReadOnlyList<SelectListItem> RolesDisponibles { get; set; } = Array.Empty<SelectListItem>();
}

public sealed class AspNetIdentityUserEditViewModel
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public List<string> RolesSelectionnes { get; set; } = new();
    public IReadOnlyList<SelectListItem> RolesDisponibles { get; set; } = Array.Empty<SelectListItem>();
}
