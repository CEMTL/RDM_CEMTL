using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Solution.Options;
using Solution.Security;
using Solution.ViewModels;
using Solution.Services.Identity;

namespace Solution.Controllers;

/// <summary>
/// Connexion RDM avec un compte Active Directory saisi par l'utilisateur.
/// Le mot de passe sert uniquement à la validation AD et n'est jamais conservé.
/// </summary>
[AllowAnonymous]
public sealed class AccountController : Controller
{
    private readonly IActiveDirectoryAuthenticationService _activeDirectory;
    private readonly RdmAuthenticationOptions _authenticationOptions;
    private readonly IAspNetIdentityAuthorizationService _identityAuthorization;

    public AccountController(
        IActiveDirectoryAuthenticationService activeDirectory,
        IOptions<RdmAuthenticationOptions> authenticationOptions,
        IAspNetIdentityAuthorizationService identityAuthorization)
    {
        _activeDirectory = activeDirectory;
        _authenticationOptions = authenticationOptions.Value;
        _identityAuthorization = identityAuthorization;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        if (!_authenticationOptions.UseActiveDirectory)
            return RedirectToAction("Index", "Home")!;

        // Si une session RDM par cookie existe déjà, ne pas redemander le mot de passe.
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!_authenticationOptions.UseActiveDirectory)
            return RedirectToAction("Index", "Home")!;

        if (!ModelState.IsValid)
        {
            ClearPassword(model);
            return View(model);
        }

        var result = _activeDirectory.Authenticate(model.UserName!, model.Password!);

        // Ne jamais renvoyer le mot de passe dans le modèle ou ModelState affiché.
        ClearPassword(model);

        if (!result.Success || string.IsNullOrWhiteSpace(result.UserName))
        {
            model.ErrorMessage = result.ErrorMessage ?? "Connexion Active Directory refusée.";
            return View(model);
        }

        var isBootstrapAdmin = IsBootstrapAdmin(result.UserName, model.UserName);

        var localUser = await _identityAuthorization.FindAuthorizedUserAsync(
            result.UserName,
            model.UserName,
            HttpContext.RequestAborted);

        // Comme dans SRSAD, l'authentification reste faite par AD.
        // Le compte BootstrapAdmin est toutefois l'accès de secours protégé :
        // on garantit son existence et son rôle Admin dans ASP.NET Identity,
        // même si d'autres administrateurs RDM existent déjà. On utilise le nom
        // canonique configuré afin que la protection dans UsersController reste stable.
        if (isBootstrapAdmin)
        {
            localUser = await _identityAuthorization.EnsureBootstrapAdminAsync(
                _authenticationOptions.BootstrapAdmin,
                HttpContext.RequestAborted);
        }

        if (localUser == null)
        {
            model.ErrorMessage = "Votre compte Windows est valide, mais aucun accès CMDPSF n'est configuré dans AspNetUsers.";
            return View(model);
        }

        var roles = AppRoles.ExpandForCompatibility(localUser.Roles)
            .Where(AppRoles.IsSupported)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (roles.Length == 0)
        {
            model.ErrorMessage = "Votre compte Windows est valide, mais aucun rôle CMDPSF n'est configuré dans AspNetUserRoles.";
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, localUser.Id),
            new(ClaimTypes.Name, result.UserName),
            new("rdm:identity-user", localUser.UserName ?? result.UserName),
            new("rdm:authentication-mode", "ActiveDirectory+AspNetIdentity")
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();

        if (_authenticationOptions.UseActiveDirectory)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> AccessDenied()
    {
        if (_authenticationOptions.UseActiveDirectory && User.Identity?.IsAuthenticated == true)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var model = new LoginViewModel
        {
            ErrorMessage = "Accès refusé. Votre compte ne possède pas un rôle RDM autorisé."
        };

        return View("Login", model);
    }


    [HttpGet]
    public IActionResult Register() => RedirectToAction(nameof(Login));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(object model) => RedirectToAction(nameof(Login));


    private bool IsBootstrapAdmin(string authenticatedWindowsName, string submittedWindowsName)
    {
        var configured = NormalizeWindowsIdentity(_authenticationOptions.BootstrapAdmin);
        if (string.IsNullOrWhiteSpace(configured))
            return false;

        var authenticated = NormalizeWindowsIdentity(authenticatedWindowsName);
        var submitted = NormalizeWindowsIdentity(submittedWindowsName);

        if (string.Equals(configured, authenticated, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(configured, submitted, StringComparison.OrdinalIgnoreCase))
            return true;

        // AD peut renvoyer le même compte avec un suffixe DNS (UPN) plutôt que le
        // nom NetBIOS configuré, p. ex. 06CEMTL\T2-MERA8300. Pour une saisie sans
        // domaine, accepter le BootstrapAdmin si le SAM account authentifié correspond.
        // Si AD a clairement résolu un AUTRE domaine configuré, ne pas l'accepter.
        var configuredParts = SplitWindowsIdentity(configured);
        var authenticatedParts = SplitWindowsIdentity(authenticated);
        var submittedParts = SplitWindowsIdentity(submitted);

        if (!string.Equals(configuredParts.Account, authenticatedParts.Account, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(configuredParts.Account, submittedParts.Account, StringComparison.OrdinalIgnoreCase))
            return false;

        var knownDomains = new[] { "06CEMTL", "HMR.HMR.QC.CA" };
        var authenticatedDomainIsKnown = knownDomains.Contains(authenticatedParts.Domain, StringComparer.OrdinalIgnoreCase);
        var configuredDomainIsKnown = knownDomains.Contains(configuredParts.Domain, StringComparer.OrdinalIgnoreCase);

        if (authenticatedDomainIsKnown && configuredDomainIsKnown &&
            !string.Equals(authenticatedParts.Domain, configuredParts.Domain, StringComparison.OrdinalIgnoreCase))
            return false;

        return !submitted.Contains('\\');
    }

    private static (string Domain, string Account) SplitWindowsIdentity(string value)
    {
        var normalized = NormalizeWindowsIdentity(value);
        var slash = normalized.LastIndexOf('\\');
        if (slash >= 0 && slash < normalized.Length - 1)
            return (normalized[..slash], normalized[(slash + 1)..]);

        var at = normalized.IndexOf('@');
        if (at > 0 && at < normalized.Length - 1)
            return (normalized[(at + 1)..], normalized[..at]);

        return (string.Empty, normalized);
    }

    private static string NormalizeWindowsIdentity(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Trim().Replace('/', '\\').ToUpperInvariant();
    }

    private void ClearPassword(LoginViewModel model)
    {
        model.Password = null;
        ModelState.Remove(nameof(LoginViewModel.Password));
    }

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home")!;
    }
}
