using System.Data.Common;
using media_vault_app.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace media_vault_app.Tests.Infrastructure;

public sealed class UserIdentifierMigrationTests
{
    [Fact]
    public async Task LatestMigration_CanonicalizesLegacyUserIdentifiers()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var userId = Guid.NewGuid();

        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync("20260703181005_RatingConstraintRemove");
        await InsertLegacyUserAsync(
            context,
            userId,
            " LegacyUser ",
            " LEGACY@Example.COM ");

        await context.Database.MigrateAsync();

        var user = await context.Users.SingleAsync(currentUser => currentUser.Id == userId);
        Assert.Equal("legacyuser", user.Username);
        Assert.Equal("legacy@example.com", user.Email);

        context.Users.Add(new media_vault_app.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Username = "LEGACYUSER",
            Email = "another@example.com",
            PasswordHash = "stored-hash"
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task LatestMigration_RejectsCanonicalizationCollisionsWithoutMergingUsers()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var firstUserId = Guid.NewGuid();
        var secondUserId = Guid.NewGuid();

        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync("20260703181005_RatingConstraintRemove");
        await InsertLegacyUserAsync(
            context,
            firstUserId,
            "LegacyUser",
            "first@example.com");
        await InsertLegacyUserAsync(
            context,
            secondUserId,
            " legacyuser ",
            "second@example.com");

        var exception = await Assert.ThrowsAnyAsync<DbException>(
            () => context.Database.MigrateAsync());

        Assert.Contains("collide after canonicalization", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, await context.Users.CountAsync());
        Assert.Contains(
            await context.Users.Select(user => user.Id).ToListAsync(),
            id => id == firstUserId);
        Assert.Contains(
            await context.Users.Select(user => user.Id).ToListAsync(),
            id => id == secondUserId);
    }

    private static Task<int> InsertLegacyUserAsync(
        AppDbContext context,
        Guid id,
        string username,
        string email) =>
        context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "Users"
                ("Id", "Username", "Email", "PasswordHash", "CreatedAtUtc", "UpdatedAtUtc")
            VALUES ({0}, {1}, {2}, {3}, {4}, {5});
            """,
            id,
            username,
            email,
            "stored-hash",
            DateTime.UtcNow,
            DateTime.UtcNow);
}
