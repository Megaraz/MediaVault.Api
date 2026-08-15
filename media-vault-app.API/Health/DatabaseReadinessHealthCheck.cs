using media_vault_app.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace media_vault_app.API.Health;

public sealed class DatabaseReadinessHealthCheck(
    IServiceScopeFactory scopeFactory,
    ILogger<DatabaseReadinessHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!await dbContext.Database.CanConnectAsync(cancellationToken))
            {
                return HealthCheckResult.Unhealthy(
                    "SQLite database cannot be opened.",
                    data: new Dictionary<string, object>
                    {
                        ["database"] = "sqlite",
                        ["connectivity"] = "failed",
                        ["migrationState"] = "unknown"
                    });
            }

            var historyRepository = dbContext.Database.GetService<IHistoryRepository>();
            var pendingMigrations = await historyRepository.ExistsAsync(cancellationToken)
                ? (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken))
                    .ToArray()
                : dbContext.Database.GetMigrations().ToArray();

            if (pendingMigrations.Length > 0)
            {
                return HealthCheckResult.Unhealthy(
                    "SQLite database has pending EF Core migrations.",
                    data: new Dictionary<string, object>
                    {
                        ["database"] = "sqlite",
                        ["connectivity"] = "ok",
                        ["migrationState"] = "pending",
                        ["pendingMigrationCount"] = pendingMigrations.Length
                    });
            }

            return HealthCheckResult.Healthy(
                "SQLite database is reachable and has no pending EF Core migrations.",
                data: new Dictionary<string, object>
                {
                    ["database"] = "sqlite",
                    ["connectivity"] = "ok",
                    ["migrationState"] = "current",
                    ["pendingMigrationCount"] = 0
                });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Database readiness check failed.");

            return HealthCheckResult.Unhealthy(
                "SQLite database readiness check failed.",
                data: new Dictionary<string, object>
                {
                    ["database"] = "sqlite",
                    ["connectivity"] = "failed",
                    ["migrationState"] = "unknown"
                });
        }
    }
}
