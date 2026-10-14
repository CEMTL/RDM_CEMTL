namespace Solution.Options;

public sealed class ActiveDirectoryRoleOptions
{
    public const string SectionName = "ActiveDirectory";

    /// <summary>
    /// Domaine historique conservé pour compatibilité avec les anciennes configurations.
    /// </summary>
    public string Domain { get; set; } = "06CEMTL";

    /// <summary>
    /// Domaines Active Directory essayés, dans l'ordre, lorsque l'utilisateur
    /// saisit uniquement son compte sans préciser de domaine.
    /// </summary>
    public List<string> Domains { get; set; } = new()
    {
        "06CEMTL",
        "hmr.hmr.qc.ca"
    };

    /// <summary>
    /// Conservé pour compatibilité avec les anciennes configurations RDM.
    /// Les rôles applicatifs sont désormais résolus dans AspNetUsers/AspNetUserRoles/AspNetRoles.
    /// </summary>
    public Dictionary<string, string> RoleGroups { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
