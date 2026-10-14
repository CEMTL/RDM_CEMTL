using Microsoft.Extensions.Diagnostics.HealthChecks;
using Solution.Models;

namespace Solution.Services;

/// <summary>
/// Vérifie que l'application peut joindre SQL Server sans exposer de détail
/// technique dans la réponse HTTP de la sonde de disponibilité.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseHealthCheck> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<HopitalMedecinsDbEntities>();

            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("La base de données ne répond pas.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Échec de la sonde de disponibilité SQL Server.");
            return HealthCheckResult.Unhealthy("La base de données ne répond pas.");
        }
    }
}
