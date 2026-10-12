using System.DirectoryServices.AccountManagement;
using System.Runtime.Versioning;
using Microsoft.Extensions.Options;
using Solution.Options;

namespace Solution.Security;

/// <summary>
/// Valide un compte/mot de passe auprès d'Active Directory et transforme
/// les groupes configurés dans ActiveDirectory:RoleGroups en rôles RDM.
/// Le mot de passe reçu n'est jamais conservé ni journalisé.
/// </summary>
public sealed class ActiveDirectoryAuthenticationService : IActiveDirectoryAuthenticationService
{
    private readonly ActiveDirectoryRoleOptions _options;
    private readonly ILogger<ActiveDirectoryAuthenticationService> _logger;

    public ActiveDirectoryAuthenticationService(
        IOptions<ActiveDirectoryRoleOptions> options,
        ILogger<ActiveDirectoryAuthenticationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public ActiveDirectoryLoginResult Authenticate(string userName, string password)
    {
        if (!OperatingSystem.IsWindows())
        {
            return ActiveDirectoryLoginResult.Failed(
                "La validation Active Directory des identifiants Windows nécessite un serveur Windows joint au domaine.");
        }

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
            return ActiveDirectoryLoginResult.Failed("Le compte Windows et le mot de passe sont obligatoires.");

        var normalized = NormalizeUserName(userName);
        var domains = GetCandidateDomains(normalized.Domain);

        if (domains.Count == 0)
            return ActiveDirectoryLoginResult.Failed("Aucun domaine Active Directory n'est configuré.");

        var serverUnavailable = 0;
        ActiveDirectoryLoginResult firstValidatedFallback = null;
        var domainWasExplicit = !string.IsNullOrWhiteSpace(normalized.Domain);

        foreach (var domain in domains)
        {
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, domain);

                var valid = context.ValidateCredentials(
                    normalized.AccountName,
                    password,
                    ContextOptions.Negotiate);

                if (!valid)
                    continue;

                // IMPORTANT en environnement multi-domaines : un domaine approuvé peut
                // parfois valider les identifiants d'un compte appartenant à un autre
                // domaine. Pour une saisie sans préfixe, on ne doit donc pas s'arrêter
                // au premier ValidateCredentials() réussi. On privilégie le domaine qui
                // permet réellement de retrouver le profil du compte.
                using var user = FindUser(context, normalized.AccountName, userName, domain);

                if (user?.Enabled == false)
                    return ActiveDirectoryLoginResult.Failed("Ce compte Active Directory est désactivé.");

                if (user == null)
                {
                    var fallbackIdentity = BuildQualifiedUserName(domain, normalized.AccountName);
                    firstValidatedFallback ??= new ActiveDirectoryLoginResult(
                        true,
                        fallbackIdentity,
                        Array.Empty<string>(),
                        null);

                    _logger.LogInformation(
                        "Les identifiants de {Account} ont été validés par {Domain}, mais le profil n'y a pas été résolu. " +
                        "{Action}",
                        normalized.AccountName,
                        domain,
                        domainWasExplicit
                            ? "Le domaine ayant été saisi explicitement, cette identité est conservée."
                            : "RDM poursuit la recherche dans les autres domaines configurés.");

                    if (domainWasExplicit)
                        return firstValidatedFallback;

                    continue;
                }

                var accountName = string.IsNullOrWhiteSpace(user.SamAccountName)
                    ? normalized.AccountName
                    : user.SamAccountName.Trim();

                // Utiliser le domaine réel du profil lorsqu'il est disponible. Ceci évite
                // qu'un compte HMR validé à travers une relation de confiance 06CEMTL soit
                // présenté à AspNetUsers comme 06CEMTL\compte.
                var resolvedDomain = ResolveProfileDomain(user, domain);
                var displayName = BuildQualifiedUserName(resolvedDomain, accountName);

                _logger.LogInformation(
                    "Identifiants Active Directory validés pour {UserName} (contexte testé : {Domain}). Les rôles seront résolus dans ASP.NET Identity.",
                    displayName,
                    domain);

                return new ActiveDirectoryLoginResult(
                    true,
                    displayName,
                    Array.Empty<string>(),
                    null);
            }
            catch (PrincipalServerDownException ex)
            {
                serverUnavailable++;
                _logger.LogWarning(ex, "Contrôleur Active Directory inaccessible pour le domaine {Domain}.", domain);
            }
            catch (PrincipalOperationException ex)
            {
                _logger.LogWarning(ex, "Erreur Active Directory pendant l'authentification de {Account} dans {Domain}.", normalized.AccountName, domain);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue pendant l'authentification Active Directory de {Account} dans {Domain}.", normalized.AccountName, domain);
            }
        }

        if (firstValidatedFallback != null)
        {
            _logger.LogWarning(
                "Les identifiants de {Account} ont été validés, mais aucun domaine configuré n'a permis de résoudre le profil. " +
                "Utilisation de la première identité validée comme repli.",
                normalized.AccountName);
            return firstValidatedFallback;
        }

        if (serverUnavailable == domains.Count)
        {
            return ActiveDirectoryLoginResult.Failed(
                "Impossible de joindre les domaines Active Directory configurés. Vérifiez le réseau et les contrôleurs de domaine.");
        }

        return ActiveDirectoryLoginResult.Failed("Compte Windows ou mot de passe incorrect.");
    }

    private IReadOnlyList<string> GetCandidateDomains(string submittedDomain)
    {
        // Si l'utilisateur précise explicitement un domaine, respecter ce choix.
        if (!string.IsNullOrWhiteSpace(submittedDomain))
            return new[] { submittedDomain.Trim() };

        // Deux domaines RDM doivent toujours être essayés lorsqu'aucun domaine
        // n'est saisi. On ne dépend donc pas uniquement d'un appsettings publié
        // qui pourrait être incomplet ou écrasé par une configuration d'environnement.
        var candidates = new List<string>
        {
            "06CEMTL",
            "hmr.hmr.qc.ca"
        };

        // Ajouter ensuite les domaines configurés explicitement, sans doublons.
        if (_options.Domains != null)
        {
            candidates.AddRange(_options.Domains
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim()));
        }

        // Compatibilité avec l'ancienne clé ActiveDirectory:Domain.
        if (!string.IsNullOrWhiteSpace(_options.Domain))
            candidates.Add(_options.Domain.Trim());

        return candidates
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    [SupportedOSPlatform("windows")]
    private HashSet<string> ReadAuthorizationGroups(UserPrincipal user)
    {
        var groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using var authorizationGroups = user.GetAuthorizationGroups();
            foreach (var principal in authorizationGroups)
            {
                try
                {
                    AddGroupNames(groups, principal);
                }
                finally
                {
                    principal.Dispose();
                }
            }
        }
        catch (PrincipalOperationException ex)
        {
            // Certains domaines contiennent des groupes orphelins qui peuvent faire échouer
            // GetAuthorizationGroups. On retombe alors sur les groupes directement lisibles.
            _logger.LogWarning(ex, "Lecture complète des groupes AD impossible; utilisation du repli GetGroups().");

            using var directGroups = user.GetGroups();
            foreach (var principal in directGroups)
            {
                try
                {
                    AddGroupNames(groups, principal);
                }
                finally
                {
                    principal.Dispose();
                }
            }
        }

        return groups;
    }

    private HashSet<string> ResolveRoles(ISet<string> groups)
    {
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in _options.RoleGroups)
        {
            var role = NormalizeRole(pair.Key);
            var configuredGroup = pair.Value?.Trim();

            if (role == null || string.IsNullOrWhiteSpace(configuredGroup))
                continue;

            var shortConfiguredGroup = ShortName(configuredGroup);

            if (groups.Contains(configuredGroup) ||
                groups.Contains(shortConfiguredGroup) ||
                groups.Any(g => string.Equals(ShortName(g), shortConfiguredGroup, StringComparison.OrdinalIgnoreCase)))
            {
                roles.Add(role);
            }
        }

        return roles;
    }

    [SupportedOSPlatform("windows")]
    private void AddDirectConfiguredMemberships(
        PrincipalContext context,
        UserPrincipal user,
        ISet<string> roles)
    {
        foreach (var pair in _options.RoleGroups)
        {
            var role = NormalizeRole(pair.Key);
            var configuredGroup = pair.Value?.Trim();

            if (role == null || string.IsNullOrWhiteSpace(configuredGroup) || roles.Contains(role))
                continue;

            var shortConfiguredGroup = ShortName(configuredGroup);
            if (string.IsNullOrWhiteSpace(shortConfiguredGroup))
                continue;

            try
            {
                using var group = GroupPrincipal.FindByIdentity(
                    context,
                    IdentityType.SamAccountName,
                    shortConfiguredGroup);

                if (group != null && user.IsMemberOf(group))
                    roles.Add(role);
            }
            catch (PrincipalOperationException ex)
            {
                _logger.LogDebug(
                    ex,
                    "Vérification directe du groupe AD {Group} impossible pour le rôle {Role}.",
                    configuredGroup,
                    role);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private static UserPrincipal FindUser(
        PrincipalContext context,
        string accountName,
        string originalUserName,
        string domain)
    {
        var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, accountName);
        if (user != null)
            return user;

        var original = (originalUserName ?? string.Empty).Trim();
        if (original.Contains('@'))
        {
            user = UserPrincipal.FindByIdentity(context, IdentityType.UserPrincipalName, original);
            if (user != null)
                return user;
        }

        if (!string.IsNullOrWhiteSpace(domain))
        {
            user = UserPrincipal.FindByIdentity(
                context,
                IdentityType.UserPrincipalName,
                $"{accountName}@{domain}");
            if (user != null)
                return user;
        }

        return UserPrincipal.FindByIdentity(context, accountName);
    }

    [SupportedOSPlatform("windows")]
    private static void AddGroupNames(ISet<string> groups, Principal principal)
    {
        if (!string.IsNullOrWhiteSpace(principal.SamAccountName))
            groups.Add(principal.SamAccountName.Trim());

        if (!string.IsNullOrWhiteSpace(principal.Name))
            groups.Add(principal.Name.Trim());

        if (!string.IsNullOrWhiteSpace(principal.DistinguishedName))
            groups.Add(principal.DistinguishedName.Trim());

        if (principal.Context?.Name is { Length: > 0 } contextName &&
            !string.IsNullOrWhiteSpace(principal.SamAccountName))
        {
            groups.Add(BuildQualifiedUserName(contextName, principal.SamAccountName));
        }
    }


    [SupportedOSPlatform("windows")]
    private static string ResolveProfileDomain(UserPrincipal user, string testedDomain)
    {
        // Le suffixe UPN représente généralement le domaine DNS réel du compte.
        // Exemple : hmr80111@hmr.hmr.qc.ca -> hmr.hmr.qc.ca.
        var upn = user?.UserPrincipalName;
        if (!string.IsNullOrWhiteSpace(upn))
        {
            var at = upn.LastIndexOf('@');
            if (at > 0 && at < upn.Length - 1)
                return upn[(at + 1)..].Trim();
        }

        try
        {
            var contextName = user?.Context?.Name;
            if (!string.IsNullOrWhiteSpace(contextName))
                return contextName.Trim();
        }
        catch
        {
            // Le domaine testé reste un repli sûr si le contexte du principal n'est pas lisible.
        }

        return testedDomain;
    }

    private static (string Domain, string AccountName) NormalizeUserName(string value)
    {
        var userName = value.Trim().Replace('/', '\\');

        var slash = userName.IndexOf('\\');
        if (slash > 0 && slash < userName.Length - 1)
            return (userName[..slash].Trim(), userName[(slash + 1)..].Trim());

        var at = userName.IndexOf('@');
        if (at > 0 && at < userName.Length - 1)
            return (userName[(at + 1)..].Trim(), userName[..at].Trim());

        return (string.Empty, userName);
    }

    private static string BuildQualifiedUserName(string domain, string accountName) =>
        $"{domain.Trim()}\\{accountName.Trim()}";

    private static string ShortName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var normalized = value.Trim().Replace('/', '\\');
        var slash = normalized.LastIndexOf('\\');
        if (slash >= 0 && slash < normalized.Length - 1)
            return normalized[(slash + 1)..];

        // Si on reçoit un DN (CN=Groupe,OU=...), récupérer le CN.
        if (normalized.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
        {
            var comma = normalized.IndexOf(',');
            return comma > 3 ? normalized[3..comma] : normalized[3..];
        }

        return normalized;
    }

    private static string NormalizeRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role)) return null;
        var normalized = AppRoles.Normalize(role);
        return AppRoles.IsSupported(normalized) ? normalized : null;
    }
}
