using media_vault_app.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace media_vault_app.Tests.Infrastructure;

public sealed class OptimisticConcurrencyMigrationTests
{
    [Fact]
    public async Task LatestMigration_BackfillsExistingRowsAndDefaultsNewRowsToVersionOne()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var ownerId = Guid.NewGuid();
        var entryId = Guid.NewGuid();

        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync("20260815184002_CanonicalizeUserIdentifiers");
        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "Users"
                ("Id", "Username", "Email", "PasswordHash", "CreatedAtUtc", "UpdatedAtUtc")
            VALUES ({0}, {1}, {2}, {3}, {4}, {5});
            """,
            ownerId,
            "existing-user",
            "existing@example.com",
            "hash",
            DateTime.UtcNow,
            DateTime.UtcNow);

        await context.Database.MigrateAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "MediaEntries"
                ("Id", "OwnerId", "Status", "Title", "Rating", "Genres", "MediaType", "CreatedAtUtc", "UpdatedAtUtc")
            VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8});
            """,
            entryId,
            ownerId,
            2,
            "New movie",
            4m,
            "[]",
            0,
            DateTime.UtcNow,
            DateTime.UtcNow);

        var userVersion = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == ownerId)
            .Select(user => user.Version)
            .SingleAsync();
        var mediaVersion = await context.MediaEntries
            .AsNoTracking()
            .Where(entry => entry.Id == entryId)
            .Select(entry => entry.Version)
            .SingleAsync();

        Assert.Equal(1, userVersion);
        Assert.Equal(1, mediaVersion);
    }
}
