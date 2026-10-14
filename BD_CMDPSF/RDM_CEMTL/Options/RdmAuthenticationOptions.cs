namespace Solution.Options;

public sealed class RdmAuthenticationOptions
{
    public const string SectionName = "Authentication";

    public bool UseActiveDirectory { get; set; } = true;
    public bool AllowLocalAdminOutsideDevelopment { get; set; }
    public string LocalAdminUserName { get; set; } = "RDM\\AdminLocal";
    public string BootstrapAdmin { get; set; }
}
