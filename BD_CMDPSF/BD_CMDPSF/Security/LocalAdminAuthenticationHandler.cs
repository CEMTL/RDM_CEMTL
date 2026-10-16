using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Solution.Options;

namespace Solution.Security;

public sealed class LocalAdminAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "RDM.LocalAdmin";
    private readonly RdmAuthenticationOptions _authenticationOptions;

    public LocalAdminAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<RdmAuthenticationOptions> authenticationOptions)
        : base(options, logger, encoder)
    {
        _authenticationOptions = authenticationOptions.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userName = string.IsNullOrWhiteSpace(_authenticationOptions.LocalAdminUserName)
            ? "RDM\\AdminLocal"
            : _authenticationOptions.LocalAdminUserName.Trim();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Role, AppRoles.Admin),
            new(ClaimTypes.Role, AppRoles.Utilisateur),
            new("rdm:authentication-mode", "LocalAdmin")
        };

        var identity = new ClaimsIdentity(claims, SchemeName, ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
