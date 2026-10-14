using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Solution.Options;
using Solution.Security;
using Solution.Services;
using Solution.Services.Identity;
using Solution.ViewModels.Administration;

namespace Solution.Controllers;

/// <summary>
/// Administration des accès RDM dans AspNetUsers / AspNetRoles / AspNetUserRoles.
/// Active Directory reste uniquement responsable de valider le compte et le mot de passe.
/// Aucune écriture Active Directory n'est effectuée par ce contrôleur.
/// </summary>
[Authorize(Roles = AppRoles.Admin)]
public sealed class UsersController : Controller
{
    private readonly IAspNetIdentityAuthorizationService _identity;
    private readonly ILogger<UsersController> _logger;
    private readonly RdmAuthenticationOptions _authenticationOptions;

    public UsersController(
        IAspNetIdentityAuthorizationService identity,
        ILogger<UsersController> logger,
        IOptions<RdmAuthenticationOptions> authenticationOptions)
    {
        _identity = identity;
        _logger = logger;
        _authenticationOptions = authenticationOptions.Value;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _identity.GetUsersAsync(cancellationToken);
            return View(new AspNetIdentityUserListViewModel
            {
                Utilisateurs = users.Select(x => new AspNetIdentityUserRowViewModel
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    IsBootstrapAdmin = IsBootstrapAdmin(x.UserName),
                    Roles = x.Roles
                        .Select(AppRoles.Normalize)
                        .Where(AppRoles.IsSupported)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray()
                }).ToArray()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Impossible de charger les utilisateurs ASP.NET Identity RDM.");
            TempData["Erreur"] = UserFriendlyExceptionService.GetMessage(ex, "charger les utilisateurs");
            return View(new AspNetIdentityUserListViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var model = new AspNetIdentityUserCreateViewModel();
        await PopulateRolesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AspNetIdentityUserCreateViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.RolesSelectionnes == null || model.RolesSelectionnes.Count == 0)
            ModelState.AddModelError(nameof(model.RolesSelectionnes), "Sélectionnez au moins un rôle RDM.");

        if (!ModelState.IsValid)
        {
            await PopulateRolesAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            await _identity.CreateUserAsync(model.UserName, model.Email, model.RolesSelectionnes, cancellationToken);
            TempData["Success"] = $"Le compte {model.UserName} est maintenant autorisé dans RDM.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Création de l'accès RDM impossible pour {UserName}.", model.UserName);
            ModelState.AddModelError(
                string.Empty,
                UserFriendlyExceptionService.GetMessage(ex, "créer l'accès RDM"));
            await PopulateRolesAsync(model, cancellationToken);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
    {
        var user = await _identity.GetUserAsync(id, cancellationToken);
        if (user == null) return NotFound();

        if (IsBootstrapAdmin(user.UserName))
        {
            TempData["Erreur"] = "Les rôles du compte BootstrapAdmin sont protégés et ne peuvent pas être modifiés.";
            return RedirectToAction(nameof(Index));
        }

        var model = new AspNetIdentityUserEditViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            RolesSelectionnes = user.Roles
                .Select(AppRoles.Normalize)
                .Where(x => AppRoles.DocumentRoles.Contains(x, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
        await PopulateRolesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AspNetIdentityUserEditViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.RolesSelectionnes == null || model.RolesSelectionnes.Count == 0)
            ModelState.AddModelError(nameof(model.RolesSelectionnes), "Sélectionnez au moins un rôle RDM.");

        if (!ModelState.IsValid)
        {
            await PopulateRolesAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            var user = await _identity.GetUserAsync(model.Id, cancellationToken);
            if (user == null)
                return NotFound();

            if (IsBootstrapAdmin(user.UserName))
            {
                TempData["Erreur"] = "Les rôles du compte BootstrapAdmin sont protégés et ne peuvent pas être modifiés.";
                return RedirectToAction(nameof(Index));
            }

            await _identity.UpdateUserRolesAsync(model.Id, model.RolesSelectionnes, cancellationToken);
            TempData["Success"] = $"Les rôles de {model.UserName} ont été mis à jour.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Mise à jour des rôles RDM impossible pour {UserId}.", model.Id);
            ModelState.AddModelError(
                string.Empty,
                UserFriendlyExceptionService.GetMessage(ex, "mettre à jour les rôles RDM"));
            await PopulateRolesAsync(model, cancellationToken);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        var user = await _identity.GetUserAsync(id, cancellationToken);
        if (user == null) return NotFound();

        if (IsBootstrapAdmin(user.UserName))
        {
            TempData["Erreur"] = "Les rôles du compte BootstrapAdmin sont protégés. Ses accès RDM ne peuvent pas être retirés.";
            return RedirectToAction(nameof(Index));
        }

        await _identity.RemoveAllRolesAsync(id, cancellationToken);
        TempData["Success"] = "Les accès RDM de cet utilisateur ont été retirés. Le compte Active Directory n'a pas été modifié.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        try
        {
            var user = await _identity.GetUserAsync(id, cancellationToken);
            if (user == null)
            {
                TempData["Erreur"] = "Cet utilisateur n'existe plus.";
                return RedirectToAction(nameof(Index));
            }

            if (IsBootstrapAdmin(user.UserName))
            {
                TempData["Erreur"] = "Le compte BootstrapAdmin est protégé et ne peut pas être supprimé depuis la gestion des utilisateurs.";
                return RedirectToAction(nameof(Index));
            }

            await _identity.DeleteUserWithoutRolesAsync(id, cancellationToken);
            TempData["Success"] = $"L'utilisateur {user.UserName} a été supprimé de RDM. Son compte Active Directory n'a pas été modifié.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Suppression de l'utilisateur RDM impossible pour {UserId}.", id);
            TempData["Erreur"] = UserFriendlyExceptionService.GetMessage(ex, "supprimer l'utilisateur RDM");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRolesAsync(AspNetIdentityUserCreateViewModel model, CancellationToken ct)
    {
        var roles = await _identity.GetRolesAsync(ct);
        model.RolesDisponibles = BuildRoleList(roles, model.RolesSelectionnes);
    }

    private async Task PopulateRolesAsync(AspNetIdentityUserEditViewModel model, CancellationToken ct)
    {
        var roles = await _identity.GetRolesAsync(ct);
        model.RolesDisponibles = BuildRoleList(roles, model.RolesSelectionnes);
    }

    private bool IsBootstrapAdmin(string userName)
    {
        var configured = NormalizeWindowsIdentity(_authenticationOptions.BootstrapAdmin);
        if (string.IsNullOrWhiteSpace(configured))
            return false;

        return string.Equals(configured, NormalizeWindowsIdentity(userName), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeWindowsIdentity(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Trim().Replace('/', '\\').ToUpperInvariant();
    }

    private static IReadOnlyList<SelectListItem> BuildRoleList(IEnumerable<string> roles, IEnumerable<string> selected)
    {
        var selectedSet = new HashSet<string>((selected ?? Array.Empty<string>()).Select(AppRoles.Normalize), StringComparer.OrdinalIgnoreCase);
        return roles
            .Select(AppRoles.Normalize)
            .Where(x => AppRoles.DocumentRoles.Contains(x, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => Array.IndexOf(AppRoles.DocumentRoles, x))
            .Select(x => new SelectListItem(AppRoles.GetDisplayName(x), x, selectedSet.Contains(x)))
            .ToArray();
    }
}
