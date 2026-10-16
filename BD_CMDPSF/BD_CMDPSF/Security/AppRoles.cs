namespace Solution.Security;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string SuperUtilisateur = "SuperUtilisateur";
    public const string Utilisateur = "Utilisateur";

    // Dans RDM, les trois rôles restent attribuables explicitement.
    public static readonly string[] DocumentRoles = { Admin, SuperUtilisateur, Utilisateur };
    public static readonly string[] All = { Admin, SuperUtilisateur, Utilisateur };

    public static bool IsSupported(string role) =>
        !string.IsNullOrWhiteSpace(role) &&
        All.Contains(role, StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string role)
    {
        if (string.IsNullOrWhiteSpace(role)) return string.Empty;
        if (role.Equals(Admin, StringComparison.OrdinalIgnoreCase)) return Admin;
        if (role.Equals(SuperUtilisateur, StringComparison.OrdinalIgnoreCase) ||
            role.Equals("Superutilisateur", StringComparison.OrdinalIgnoreCase)) return SuperUtilisateur;
        if (role.Equals(Utilisateur, StringComparison.OrdinalIgnoreCase)) return Utilisateur;
        return role.Trim();
    }

    public static IReadOnlyCollection<string> ExpandForCompatibility(IEnumerable<string> roles)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in roles ?? Array.Empty<string>())
        {
            var role = Normalize(raw);
            if (IsSupported(role)) result.Add(role);
        }
        return result;
    }

    public static string GetDisplayName(string role) => Normalize(role) switch
    {
        Admin => "Administrateur",
        SuperUtilisateur => "Super-utilisateur",
        Utilisateur => "Utilisateur",
        _ => role
    };
}
