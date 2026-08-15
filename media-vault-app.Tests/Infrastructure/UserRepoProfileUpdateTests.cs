using media_vault_app.Domain.Entities;
using media_vault_app.Infrastructure;
using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Infrastructure.Repos;
using media_vault_app.Tests.TestHelpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace media_vault_app.Tests.Infrastructure;

public sealed class UserRepoProfileUpdateTests
{
    [Fact]
    public async Task UpdateProfileAsync_PreservesPasswordAndCreatedTimestamp_AndUpdatesOnlyProfileFields()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var userId = Guid.NewGuid();
        var createdAt = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var originalUpdatedAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
            setupContext.Users.Add(new User
            {
                Id = userId,
                Username = "original-user",
                Email = "original@example.com",
                PasswordHash = "stored-password-hash",
                CreatedAtUtc = createdAt,
                UpdatedAtUtc = originalUpdatedAt
            });
            await setupContext.SaveChangesAsync();
        }

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using (var updateContext = new AppDbContext(options))
        {
            var repo = CreateUserRepo(updateContext, loggerFactory);
            var result = await repo.UpdateProfileAsync(
                userId,
                " Updated-User ",
                " UPDATED@Example.COM ");

            Assert.True(result.IsSuccess);
        }

        await using var verificationContext = new AppDbContext(options);
        var updatedUser = await verificationContext.Users.SingleAsync(user => user.Id == userId);

        Assert.Equal("updated-user", updatedUser.Username);
        Assert.Equal("updated@example.com", updatedUser.Email);
        Assert.Equal("stored-password-hash", updatedUser.PasswordHash);
        Assert.Equal(createdAt, updatedUser.CreatedAtUtc);
        Assert.True(updatedUser.UpdatedAtUtc > originalUpdatedAt);
        Assert.True(updatedUser.UpdatedAtUtc <= DateTime.UtcNow);
    }

    [Fact]
    public async Task CheckProfileUpdateAvailabilityAsync_TrimsValues_AndExcludesCurrentUser()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
            setupContext.Users.AddRange(
                new User
                {
                    Id = currentUserId,
                    Username = "current-user",
                    Email = "current@example.com",
                    PasswordHash = "current-hash"
                },
                new User
                {
                    Id = otherUserId,
                    Username = "other-user",
                    Email = "other@example.com",
                    PasswordHash = "other-hash"
                });
            await setupContext.SaveChangesAsync();
        }

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using var queryContext = new AppDbContext(options);
        var repo = CreateUserRepo(queryContext, loggerFactory);

        var selfUpdateResult = await repo.CheckProfileUpdateAvailabilityAsync(
            currentUserId,
            " current-user ",
            " current@example.com ");
        var conflictingUpdateResult = await repo.CheckProfileUpdateAvailabilityAsync(
            currentUserId,
            " other-user ",
            " available@example.com ");

        Assert.True(selfUpdateResult.IsSuccess);
        Assert.True(selfUpdateResult.Value.IsUserNameAvailable);
        Assert.True(selfUpdateResult.Value.IsEmailAvailable);
        Assert.True(conflictingUpdateResult.IsSuccess);
        Assert.False(conflictingUpdateResult.Value.IsUserNameAvailable);
        Assert.True(conflictingUpdateResult.Value.IsEmailAvailable);
    }

    [Fact]
    public async Task CheckRegistrationAvailabilityAsync_UsesCanonicalIdentifiers()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
            setupContext.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Username = "existing-user",
                Email = "existing@example.com",
                PasswordHash = "hash"
            });
            await setupContext.SaveChangesAsync();
        }

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using var queryContext = new AppDbContext(options);
        var repo = CreateUserRepo(queryContext, loggerFactory);

        var result = await repo.CheckRegistrationAvailabilityAsync(
            " EXISTING-USER ",
            " EXISTING@EXAMPLE.COM ");

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsUserNameAvailable);
        Assert.False(result.Value.IsEmailAvailable);
    }

    [Fact]
    public async Task GetByUsernameOrEmailAsync_UsesCanonicalIdentifierLookup()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var userId = Guid.NewGuid();

        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
            setupContext.Users.Add(new User
            {
                Id = userId,
                Username = "existing-user",
                Email = "existing@example.com",
                PasswordHash = "hash"
            });
            await setupContext.SaveChangesAsync();
        }

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using var queryContext = new AppDbContext(options);
        var repo = CreateUserRepo(queryContext, loggerFactory);

        var result = await repo.GetByUsernameOrEmailAsync(" EXISTING@EXAMPLE.COM ");

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value.Id);
    }

    [Fact]
    public async Task RegisterUserAsync_MapsSqliteUniqueViolationToConflict()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
            setupContext.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Username = "existing-user",
                Email = "existing@example.com",
                PasswordHash = "hash"
            });
            await setupContext.SaveChangesAsync();
        }

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using var insertContext = new AppDbContext(options);
        var repo = CreateUserRepo(insertContext, loggerFactory);

        var result = await repo.RegisterUserAsync(new User
        {
            Id = Guid.NewGuid(),
            Username = " EXISTING-USER ",
            Email = "available@example.com",
            PasswordHash = "hash"
        });

        Assert.True(result.IsFailure);
        Assert.Equal(Megaraz.ResultPattern.ErrorType.Conflict, result.PrimaryError.Type);
        Assert.DoesNotContain("SQLite", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Unique User constraint violated.", result.Message);
    }

    private static UserRepo CreateUserRepo(AppDbContext context, ILoggerFactory loggerFactory) =>
        new(
            context,
            new ErrorEventLogger<RepoBase<User, Guid>>(
                loggerFactory.CreateLogger<RepoBase<User, Guid>>(),
                new ErrorEventPolicy(),
                new ErrorDiagnosticsOptions(false)));
}
