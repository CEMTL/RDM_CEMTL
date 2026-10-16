using Microsoft.Data.SqlClient;
using Solution.Security;

namespace Solution.Services.Identity;

public sealed record AspNetAuthorizationUser(
    string Id,
    string UserName,
    string Email,
    IReadOnlyList<string> Roles);

public sealed record AspNetUserAdminItem(
    string Id,
    string UserName,
    string Email,
    IReadOnlyList<string> Roles);

public interface IAspNetIdentityAuthorizationService
{
    Task<AspNetAuthorizationUser> FindAuthorizedUserAsync(
        string authenticatedWindowsName,
        string submittedWindowsName,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AspNetUserAdminItem>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<AspNetUserAdminItem> GetUserAsync(string id, CancellationToken cancellationToken = default);
    Task<AspNetAuthorizationUser> EnsureBootstrapAdminAsync(string windowsUserName, CancellationToken cancellationToken = default);
    Task<string> CreateUserAsync(string windowsUserName, string email, IEnumerable<string> roles, CancellationToken cancellationToken = default);
    Task UpdateUserRolesAsync(string userId, IEnumerable<string> roles, CancellationToken cancellationToken = default);
    Task RemoveAllRolesAsync(string userId, CancellationToken cancellationToken = default);
    Task DeleteUserWithoutRolesAsync(string userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Utilise les tables ASP.NET Identity déjà présentes dans la base RDM uniquement
/// comme annuaire d'autorisation. Les mots de passe ne sont jamais lus ni écrits ici :
/// l'authentification reste faite par Active Directory.
/// </summary>
public sealed class AspNetIdentityAuthorizationService : IAspNetIdentityAuthorizationService
{
    private readonly string _connectionString;
    private readonly string _domain;
    private readonly ILogger<AspNetIdentityAuthorizationService> _logger;

    public AspNetIdentityAuthorizationService(
        IConfiguration configuration,
        ILogger<AspNetIdentityAuthorizationService> logger)
    {
        _connectionString = configuration.GetConnectionString("RdmConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:RdmConnection est absente.");
        _domain = (configuration["ActiveDirectory:Domain"] ?? string.Empty).Trim();
        _logger = logger;
    }

    public async Task<AspNetAuthorizationUser> FindAuthorizedUserAsync(
        string authenticatedWindowsName,
        string submittedWindowsName,
        CancellationToken cancellationToken = default)
    {
        var candidates = BuildCandidates(authenticatedWindowsName, submittedWindowsName);
        if (candidates.Count == 0)
            return null;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureIdentityTablesAsync(connection, cancellationToken);

        var hasNormalized = await ColumnExistsAsync(connection, "AspNetUsers", "NormalizedUserName", cancellationToken);
        var conditions = new List<string>();
        await using var command = connection.CreateCommand();

        for (var i = 0; i < candidates.Count; i++)
        {
            var parameter = "@u" + i;
            conditions.Add($"UPPER(LTRIM(RTRIM([UserName]))) = UPPER({parameter})");
            if (hasNormalized)
                conditions.Add($"UPPER(LTRIM(RTRIM([NormalizedUserName]))) = UPPER({parameter})");
            command.Parameters.AddWithValue(parameter, candidates[i]);
        }

        command.CommandText = $@"
SELECT TOP (1) [Id], [UserName], {(await ColumnExistsAsync(connection, "AspNetUsers", "Email", cancellationToken) ? "[Email]" : "CAST(NULL AS nvarchar(256)) AS [Email]")}
FROM [dbo].[AspNetUsers]
WHERE {string.Join(" OR ", conditions)};";

        string id = null;
        string userName = null;
        string email = null;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            if (await reader.ReadAsync(cancellationToken))
            {
                id = reader["Id"]?.ToString();
                userName = reader["UserName"]?.ToString();
                email = reader["Email"] == DBNull.Value ? null : reader["Email"]?.ToString();
            }
        }

        if (string.IsNullOrWhiteSpace(id))
            return null;

        var roles = await GetRolesForUserAsync(connection, id, cancellationToken);
        var supportedRoles = roles
            .Select(AppRoles.Normalize)
            .Where(AppRoles.IsSupported)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (supportedRoles.Length == 0)
            return new AspNetAuthorizationUser(id, userName, email, Array.Empty<string>());

        return new AspNetAuthorizationUser(id, userName, email, supportedRoles);
    }

    public async Task<IReadOnlyList<AspNetUserAdminItem>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureIdentityTablesAsync(connection, cancellationToken);
        var hasEmail = await ColumnExistsAsync(connection, "AspNetUsers", "Email", cancellationToken);

        var users = new List<(string Id, string UserName, string Email)>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $@"SELECT [Id], [UserName], {(hasEmail ? "[Email]" : "CAST(NULL AS nvarchar(256)) AS [Email]")} FROM [dbo].[AspNetUsers] ORDER BY [UserName];";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                users.Add((
                    reader["Id"]?.ToString(),
                    reader["UserName"]?.ToString(),
                    reader["Email"] == DBNull.Value ? null : reader["Email"]?.ToString()));
            }
        }

        var result = new List<AspNetUserAdminItem>(users.Count);
        foreach (var user in users)
        {
            var roles = await GetRolesForUserAsync(connection, user.Id, cancellationToken);
            result.Add(new AspNetUserAdminItem(user.Id, user.UserName, user.Email, roles));
        }
        return result;
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureIdentityTablesAsync(connection, cancellationToken);

        // Garantit que les trois rôles RDM sont disponibles même si la base héritée
        // ne les contient pas encore tous sous leur nom canonique.
        foreach (var requiredRole in AppRoles.DocumentRoles)
            await EnsureRoleExistsAsync(connection, requiredRole, cancellationToken);

        var roles = new List<string>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT [Name] FROM [dbo].[AspNetRoles] WHERE [Name] IS NOT NULL ORDER BY [Name];";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var role = reader[0]?.ToString();
            if (!string.IsNullOrWhiteSpace(role) && AppRoles.IsSupported(AppRoles.Normalize(role)))
                roles.Add(role.Trim());
        }
        return roles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public async Task<AspNetUserAdminItem> GetUserAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureIdentityTablesAsync(connection, cancellationToken);
        var hasEmail = await ColumnExistsAsync(connection, "AspNetUsers", "Email", cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $@"SELECT TOP (1) [Id], [UserName], {(hasEmail ? "[Email]" : "CAST(NULL AS nvarchar(256)) AS [Email]")} FROM [dbo].[AspNetUsers] WHERE [Id] = @id;";
        command.Parameters.AddWithValue("@id", id);
        string userId = null, userName = null, email = null;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            if (await reader.ReadAsync(cancellationToken))
            {
                userId = reader["Id"]?.ToString();
                userName = reader["UserName"]?.ToString();
                email = reader["Email"] == DBNull.Value ? null : reader["Email"]?.ToString();
            }
        }
        if (userId == null) return null;
        var roles = await GetRolesForUserAsync(connection, userId, cancellationToken);
        return new AspNetUserAdminItem(userId, userName, email, roles);
    }

    public async Task<AspNetAuthorizationUser> EnsureBootstrapAdminAsync(
        string windowsUserName,
        CancellationToken cancellationToken = default)
    {
        var normalizedUser = NormalizeStoredUserName(windowsUserName);
        if (string.IsNullOrWhiteSpace(normalizedUser))
            throw new InvalidOperationException("Le compte Windows BootstrapAdmin est obligatoire.");

        // Le BootstrapAdmin RDM est un compte de secours explicite, protégé dans
        // l'écran de gestion. Contrairement à un simple « premier admin », il doit
        // rester fonctionnel même lorsqu'un autre administrateur existe déjà.
        await using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await EnsureIdentityTablesAsync(connection, cancellationToken);
            await EnsureRoleExistsAsync(connection, AppRoles.Admin, cancellationToken);
            await EnsureRoleExistsAsync(connection, AppRoles.Utilisateur, cancellationToken);
        }

        string userId;
        await using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            userId = await FindUserIdExactAsync(connection, normalizedUser, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            userId = await CreateUserAsync(
                normalizedUser,
                null,
                new[] { AppRoles.Admin },
                cancellationToken);

            _logger.LogWarning(
                "BootstrapAdmin RDM créé automatiquement dans AspNetUsers pour {UserName}.",
                normalizedUser);
        }
        else
        {
            var current = await FindAuthorizedUserAsync(normalizedUser, normalizedUser, cancellationToken);
            var hasAdmin = current?.Roles.Any(r => r.Equals(AppRoles.Admin, StringComparison.OrdinalIgnoreCase)) == true;

            if (!hasAdmin)
            {
                await UpdateUserRolesAsync(
                    userId,
                    new[] { AppRoles.Admin },
                    cancellationToken);

                _logger.LogWarning(
                    "Rôle Admin restauré automatiquement pour le BootstrapAdmin RDM {UserName}.",
                    normalizedUser);
            }
        }

        return await FindAuthorizedUserAsync(
            normalizedUser,
            normalizedUser,
            cancellationToken);
    }

    public async Task<string> CreateUserAsync(
        string windowsUserName,
        string email,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        var normalizedUser = NormalizeStoredUserName(windowsUserName);
        if (string.IsNullOrWhiteSpace(normalizedUser))
            throw new InvalidOperationException("Le compte Windows est obligatoire.");

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureIdentityTablesAsync(connection, cancellationToken);

        var existing = await FindUserIdExactAsync(connection, normalizedUser, cancellationToken);
        if (!string.IsNullOrWhiteSpace(existing))
            throw new InvalidOperationException("Ce compte Windows existe déjà dans AspNetUsers.");

        var columns = await GetColumnsAsync(connection, "AspNetUsers", cancellationToken);
        var id = Guid.NewGuid().ToString();
        var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = id,
            ["UserName"] = normalizedUser,
            ["NormalizedUserName"] = normalizedUser.ToUpperInvariant(),
            ["Email"] = string.IsNullOrWhiteSpace(email) ? string.Empty : email.Trim(),
            ["NormalizedEmail"] = string.IsNullOrWhiteSpace(email) ? string.Empty : email.Trim().ToUpperInvariant(),
            ["EmailConfirmed"] = false,
            ["PasswordHash"] = DBNull.Value,
            ["SecurityStamp"] = Guid.NewGuid().ToString(),
            ["ConcurrencyStamp"] = Guid.NewGuid().ToString(),
            ["PhoneNumber"] = DBNull.Value,
            ["PhoneNumberConfirmed"] = false,
            ["TwoFactorEnabled"] = false,
            ["LockoutEndDateUtc"] = DBNull.Value,
            ["LockoutEnd"] = DBNull.Value,
            ["LockoutEnabled"] = false,
            ["AccessFailedCount"] = 0,
            ["Nom"] = string.Empty,
            ["Prenom"] = string.Empty
        };

        var selected = values.Where(x => columns.Contains(x.Key)).ToArray();
        if (!selected.Any(x => x.Key.Equals("Id", StringComparison.OrdinalIgnoreCase)) ||
            !selected.Any(x => x.Key.Equals("UserName", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("La structure de dbo.AspNetUsers n'est pas compatible : Id/UserName introuvable.");

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await using (var command = connection.CreateCommand())
            {
                command.Transaction = (SqlTransaction)transaction;
                var names = new List<string>();
                var parameters = new List<string>();
                var i = 0;
                foreach (var item in selected)
                {
                    names.Add("[" + item.Key + "]");
                    var p = "@p" + i++;
                    parameters.Add(p);
                    command.Parameters.AddWithValue(p, item.Value ?? DBNull.Value);
                }
                command.CommandText = $"INSERT INTO [dbo].[AspNetUsers] ({string.Join(",", names)}) VALUES ({string.Join(",", parameters)});";
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await ReplaceRolesAsync(connection, (SqlTransaction)transaction, id, roles, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return id;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateUserRolesAsync(string userId, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureIdentityTablesAsync(connection, cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await ReplaceRolesAsync(connection, (SqlTransaction)transaction, userId, roles, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task RemoveAllRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureIdentityTablesAsync(connection, cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [dbo].[AspNetUserRoles] WHERE [UserId] = @id;";
        command.Parameters.AddWithValue("@id", userId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteUserWithoutRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("L'identifiant de l'utilisateur est obligatoire.", nameof(userId));

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureIdentityTablesAsync(connection, cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using (var exists = connection.CreateCommand())
            {
                exists.Transaction = transaction;
                exists.CommandText = "SELECT COUNT(1) FROM [dbo].[AspNetUsers] WHERE [Id] = @id;";
                exists.Parameters.AddWithValue("@id", userId);
                if (Convert.ToInt32(await exists.ExecuteScalarAsync(cancellationToken)) == 0)
                    throw new InvalidOperationException("Cet utilisateur n'existe plus dans AspNetUsers.");
            }

            await using (var roles = connection.CreateCommand())
            {
                roles.Transaction = transaction;
                roles.CommandText = "SELECT COUNT(1) FROM [dbo].[AspNetUserRoles] WHERE [UserId] = @id;";
                roles.Parameters.AddWithValue("@id", userId);
                if (Convert.ToInt32(await roles.ExecuteScalarAsync(cancellationToken)) > 0)
                    throw new InvalidOperationException("Cet utilisateur possède encore au moins un rôle RDM. Retirez d'abord tous ses rôles avant de le supprimer.");
            }

            foreach (var table in new[] { "AspNetUserClaims", "AspNetUserLogins", "AspNetUserTokens" })
            {
                if (!await TableExistsAsync(connection, table, cancellationToken, transaction))
                    continue;

                await using var childDelete = connection.CreateCommand();
                childDelete.Transaction = transaction;
                childDelete.CommandText = $"DELETE FROM [dbo].[{table}] WHERE [UserId] = @id;";
                childDelete.Parameters.AddWithValue("@id", userId);
                await childDelete.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var delete = connection.CreateCommand())
            {
                delete.Transaction = transaction;
                delete.CommandText = "DELETE FROM [dbo].[AspNetUsers] WHERE [Id] = @id;";
                delete.Parameters.AddWithValue("@id", userId);
                if (await delete.ExecuteNonQueryAsync(cancellationToken) != 1)
                    throw new InvalidOperationException("La suppression de l'utilisateur a échoué.");
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task ReplaceRolesAsync(SqlConnection connection, SqlTransaction transaction, string userId, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        var selectedRoles = (roles ?? Array.Empty<string>())
            .Select(AppRoles.Normalize)
            .Where(x => AppRoles.DocumentRoles.Contains(x, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (selectedRoles.Count == 0)
            throw new InvalidOperationException("Sélectionnez au moins un rôle RDM.");

        selectedRoles.Add(AppRoles.Utilisateur);
        selectedRoles = selectedRoles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var roleIds = new List<string>();
        foreach (var roleName in selectedRoles)
        {
            await using var roleCommand = connection.CreateCommand();
            roleCommand.Transaction = transaction;
            roleCommand.CommandText = "SELECT TOP (1) [Id] FROM [dbo].[AspNetRoles] WHERE UPPER([Name]) = UPPER(@name);";
            roleCommand.Parameters.AddWithValue("@name", roleName);
            var roleId = (await roleCommand.ExecuteScalarAsync(cancellationToken))?.ToString();
            if (string.IsNullOrWhiteSpace(roleId))
                throw new InvalidOperationException($"Le rôle '{roleName}' n'existe pas dans dbo.AspNetRoles.");
            roleIds.Add(roleId);
        }

        await using (var delete = connection.CreateCommand())
        {
            delete.Transaction = transaction;
            delete.CommandText = "DELETE FROM [dbo].[AspNetUserRoles] WHERE [UserId] = @userId;";
            delete.Parameters.AddWithValue("@userId", userId);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var roleId in roleIds.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            await using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = "INSERT INTO [dbo].[AspNetUserRoles] ([UserId],[RoleId]) VALUES (@userId,@roleId);";
            insert.Parameters.AddWithValue("@userId", userId);
            insert.Parameters.AddWithValue("@roleId", roleId);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private async Task<IReadOnlyList<string>> GetRolesForUserAsync(SqlConnection connection, string userId, CancellationToken cancellationToken)
    {
        var roles = new List<string>();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT r.[Name]
FROM [dbo].[AspNetUserRoles] ur
INNER JOIN [dbo].[AspNetRoles] r ON r.[Id] = ur.[RoleId]
WHERE ur.[UserId] = @userId
ORDER BY r.[Name];";
        command.Parameters.AddWithValue("@userId", userId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var role = reader[0]?.ToString();
            if (!string.IsNullOrWhiteSpace(role)) roles.Add(role.Trim());
        }
        return roles;
    }

    private async Task<string> FindUserIdExactAsync(SqlConnection connection, string userName, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT TOP (1) [Id] FROM [dbo].[AspNetUsers] WHERE UPPER(LTRIM(RTRIM([UserName]))) = UPPER(@userName);";
        command.Parameters.AddWithValue("@userName", userName);
        return (await command.ExecuteScalarAsync(cancellationToken))?.ToString();
    }

    private List<string> BuildCandidates(params string[] values)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in values.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var value = raw.Trim().Replace('/', '\\');
            result.Add(value);
            var slash = value.LastIndexOf('\\');
            var at = value.IndexOf('@');
            var shortName = slash >= 0 && slash < value.Length - 1 ? value[(slash + 1)..] : at > 0 ? value[..at] : value;
            if (!string.IsNullOrWhiteSpace(shortName))
            {
                result.Add(shortName);
                if (!string.IsNullOrWhiteSpace(_domain))
                    result.Add(_domain + "\\" + shortName);
            }
        }
        return result.ToList();
    }

    private string NormalizeStoredUserName(string value)
    {
        value = (value ?? string.Empty).Trim().Replace('/', '\\');
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        if (value.Contains('\\') || value.Contains('@') || string.IsNullOrWhiteSpace(_domain)) return value;
        return _domain + "\\" + value;
    }

    private static async Task EnsureRoleExistsAsync(
        SqlConnection connection,
        string roleName,
        CancellationToken cancellationToken)
    {
        await using (var exists = connection.CreateCommand())
        {
            exists.CommandText = "SELECT TOP (1) [Id] FROM [dbo].[AspNetRoles] WHERE UPPER(LTRIM(RTRIM([Name]))) = UPPER(@name);";
            exists.Parameters.AddWithValue("@name", roleName);
            var roleId = (await exists.ExecuteScalarAsync(cancellationToken))?.ToString();
            if (!string.IsNullOrWhiteSpace(roleId))
                return;
        }

        var columns = await GetColumnsAsync(connection, "AspNetRoles", cancellationToken);
        if (!columns.Contains("Id") || !columns.Contains("Name"))
            throw new InvalidOperationException("La structure de dbo.AspNetRoles n'est pas compatible : Id/Name introuvable.");

        var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = Guid.NewGuid().ToString(),
            ["Name"] = roleName,
            ["NormalizedName"] = roleName.ToUpperInvariant(),
            ["ConcurrencyStamp"] = Guid.NewGuid().ToString()
        };

        var selected = values.Where(x => columns.Contains(x.Key)).ToArray();
        await using var insert = connection.CreateCommand();
        var names = new List<string>();
        var parameters = new List<string>();
        for (var i = 0; i < selected.Length; i++)
        {
            names.Add("[" + selected[i].Key + "]");
            var parameter = "@p" + i;
            parameters.Add(parameter);
            insert.Parameters.AddWithValue(parameter, selected[i].Value ?? DBNull.Value);
        }
        insert.CommandText = $"INSERT INTO [dbo].[AspNetRoles] ({string.Join(",", names)}) VALUES ({string.Join(",", parameters)});";
        await insert.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsureIdentityTablesAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        foreach (var table in new[] { "AspNetUsers", "AspNetRoles", "AspNetUserRoles" })
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@table;";
            command.Parameters.AddWithValue("@table", table);
            if (Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) == 0)
                throw new InvalidOperationException($"La table dbo.{table} est introuvable. Les tables ASP.NET Identity doivent exister dans la base RDM.");
        }
    }

    private static async Task<HashSet<string>> GetColumnsAsync(SqlConnection connection, string table, CancellationToken cancellationToken)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@table;";
        command.Parameters.AddWithValue("@table", table);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) result.Add(reader.GetString(0));
        return result;
    }

    private static async Task<bool> TableExistsAsync(SqlConnection connection, string table, CancellationToken cancellationToken, SqlTransaction transaction = null)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@table;";
        command.Parameters.AddWithValue("@table", table);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }

    private static async Task<bool> ColumnExistsAsync(SqlConnection connection, string table, string column, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@table AND COLUMN_NAME=@column;";
        command.Parameters.AddWithValue("@table", table);
        command.Parameters.AddWithValue("@column", column);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }
}
