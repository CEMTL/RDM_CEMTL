namespace Solution.Security;

public interface IActiveDirectoryAuthenticationService
{
    ActiveDirectoryLoginResult Authenticate(string userName, string password);
}

public sealed record ActiveDirectoryLoginResult(
    bool Success,
    string UserName,
    IReadOnlyCollection<string> Roles,
    string ErrorMessage)
{
    public static ActiveDirectoryLoginResult Failed(string message) =>
        new(false, null, Array.Empty<string>(), message);
}
