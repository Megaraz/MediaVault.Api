using media_vault_app.Domain.Entities;
using media_vault_app.Infrastructure;
using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Infrastructure.Repos;
using media_vault_app.Infrastructure.Timestamps;
using media_vault_app.Tests.TestHelpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace media_vault_app.Tests.Infrastructure;

public sealed class ServerTimestampLifecycleTests
{
    [Fact]
    public async Task UserCreateUpdateNoOpAndCancellation_UseServerOwnedTimestamps()
    {
        var initial = new DateTimeOffset(2026, 8, 21, 10, 0, 0, TimeSpan.Zero);
        var clock = new MutableTimeProvider(initial);
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using (var context = new AppDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
            var repo = CreateUserRepo(context, loggerFactory, clock);
            var result = await repo.RegisterUserAsync(new User
            {
                Id = Guid.NewGuid(),
                Username = "timestamp-user",
                Email = "timestamp@example.com",
                PasswordHash = "hash",
                CreatedAtUtc = DateTime.MinValue,
                UpdatedAtUtc = DateTime.MinValue
            });

            Assert.True(result.IsSuccess);
        }

        var userId = await ReadUserIdAsync(options);
        var createdAt = initial.UtcDateTime;

        clock.Advance(TimeSpan.FromMinutes(5));
        await using (var context = new AppDbContext(options))
        {
            var repo = CreateUserRepo(context, loggerFactory, clock);
            var result = await repo.UpdateProfileAsync(userId, "updated-user", "updated@example.com", 1);

            Assert.True(result.IsSuccess);
        }

        var updatedAt = await ReadUserAsync(options);
        Assert.Equal(createdAt, updatedAt.CreatedAtUtc);
        Assert.Equal(initial.AddMinutes(5).UtcDateTime, updatedAt.UpdatedAtUtc);

        clock.Advance(TimeSpan.FromMinutes(5));
        await using (var context = new AppDbContext(options))
        {
            var repo = CreateUserRepo(context, loggerFactory, clock);
            var result = await repo.UpdateProfileAsync(userId, "updated-user", "updated@example.com", 2);

            Assert.True(result.IsSuccess);
        }

        var afterNoOp = await ReadUserAsync(options);
        Assert.Equal(updatedAt.UpdatedAtUtc, afterNoOp.UpdatedAtUtc);
        Assert.Equal(2, afterNoOp.Version);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await using (var context = new AppDbContext(options))
        {
            var repo = CreateUserRepo(context, loggerFactory, clock);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                repo.UpdateProfileAsync(userId, "cancelled-user", "cancelled@example.com", 2, cancellation.Token));
        }

        var afterCancellation = await ReadUserAsync(options);
        Assert.Equal(afterNoOp.UpdatedAtUtc, afterCancellation.UpdatedAtUtc);
        Assert.Equal("updated-user", afterCancellation.Username);
    }

    [Fact]
    public async Task TvSeriesCreateAndSeasonUpdate_UseServerOwnedTimestamps()
    {
        var initial = new DateTimeOffset(2026, 8, 21, 11, 0, 0, TimeSpan.Zero);
        var clock = new MutableTimeProvider(initial);
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        var ownerId = Guid.NewGuid();
        var seriesId = Guid.NewGuid();
        var seasonId = Guid.NewGuid();

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
            setupContext.Users.Add(new User
            {
                Id = ownerId,
                Username = "media-owner",
                Email = "media-owner@example.com",
                PasswordHash = "hash"
            });
            await setupContext.SaveChangesAsync();
        }

        await using (var context = new AppDbContext(options))
        {
            var repo = CreateMediaRepo(context, loggerFactory, clock);
            var result = await repo.CreateAsync(new TvSeriesEntry
            {
                Id = seriesId,
                OwnerId = ownerId,
                Version = 1,
                Title = "Timestamp Series",
                Status = media_vault_app.Domain.Enums.Status.Ongoing,
                Rating = 4m,
                CreatedAtUtc = DateTime.MinValue,
                UpdatedAtUtc = DateTime.MinValue,
                Seasons =
                [
                    new Season
                    {
                        Id = seasonId,
                        SeasonNumber = 1,
                        Name = "Season 1",
                        Status = media_vault_app.Domain.Enums.Status.Ongoing,
                        Rating = 4m,
                        CreatedAtUtc = DateTime.MinValue,
                        UpdatedAtUtc = DateTime.MinValue
                    }
                ]
            });

            Assert.True(result.IsSuccess);
        }

        var created = await ReadSeriesAsync(options, seriesId);
        Assert.Equal(initial.UtcDateTime, created.CreatedAtUtc);
        Assert.Equal(initial.UtcDateTime, created.UpdatedAtUtc);
        Assert.Equal(initial.UtcDateTime, Assert.Single(created.Seasons).CreatedAtUtc);
        Assert.Equal(initial.UtcDateTime, Assert.Single(created.Seasons).UpdatedAtUtc);

        clock.Advance(TimeSpan.FromMinutes(5));
        await using (var context = new AppDbContext(options))
        {
            var repo = CreateMediaRepo(context, loggerFactory, clock);
            var result = await repo.UpdateTvSeriesAsync(ownerId, new TvSeriesEntry
            {
                Id = seriesId,
                OwnerId = ownerId,
                Title = "Timestamp Series",
                Status = media_vault_app.Domain.Enums.Status.Ongoing,
                Rating = 4m,
                Seasons =
                [
                    new Season
                    {
                        Id = seasonId,
                        SeasonNumber = 1,
                        Name = "Updated Season 1",
                        Status = media_vault_app.Domain.Enums.Status.Completed,
                        Rating = 4.5m
                    }
                ]
            });

            Assert.True(result.IsSuccess);
        }

        var updated = await ReadSeriesAsync(options, seriesId);
        var updatedSeason = Assert.Single(updated.Seasons);
        Assert.Equal(2, updated.Version);
        Assert.Equal(created.CreatedAtUtc, updated.CreatedAtUtc);
        Assert.Equal(initial.AddMinutes(5).UtcDateTime, updated.UpdatedAtUtc);
        Assert.Equal(initial.UtcDateTime, updatedSeason.CreatedAtUtc);
        Assert.Equal(initial.AddMinutes(5).UtcDateTime, updatedSeason.UpdatedAtUtc);
    }

    [Fact]
    public async Task FailedUserUpdate_DoesNotPersistTheCandidateTimestamp()
    {
        var initial = new DateTimeOffset(2026, 8, 21, 12, 0, 0, TimeSpan.Zero);
        var clock = new MutableTimeProvider(initial);
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        var firstUserId = Guid.NewGuid();

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
            setupContext.Users.AddRange(
                new User
                {
                    Id = firstUserId,
                    Username = "first-user",
                    Email = "first@example.com",
                    PasswordHash = "hash",
                    CreatedAtUtc = initial.UtcDateTime,
                    UpdatedAtUtc = initial.UtcDateTime
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "second-user",
                    Email = "second@example.com",
                    PasswordHash = "hash",
                    CreatedAtUtc = initial.UtcDateTime,
                    UpdatedAtUtc = initial.UtcDateTime
                });
            await setupContext.SaveChangesAsync();
        }

        clock.Advance(TimeSpan.FromMinutes(5));
        await using (var context = new AppDbContext(options))
        {
            var repo = CreateUserRepo(context, loggerFactory, clock);
            var result = await repo.UpdateProfileAsync(firstUserId, "first-user", "second@example.com", 1);

            Assert.True(result.IsFailure);
        }

        await using var verificationContext = new AppDbContext(options);
        var persisted = await verificationContext.Users.AsNoTracking().SingleAsync(user => user.Id == firstUserId);
        Assert.Equal("first@example.com", persisted.Email);
        Assert.Equal(initial.UtcDateTime, persisted.UpdatedAtUtc);
    }

    private static UserRepo CreateUserRepo(AppDbContext context, ILoggerFactory loggerFactory, TimeProvider clock) =>
        new(
            context,
            new ErrorEventLogger<UserRepo>(
                loggerFactory.CreateLogger<UserRepo>(),
                new ErrorEventPolicy(),
                new ErrorDiagnosticsOptions(false)),
            new ServerTimestampPolicy(clock));

    private static MediaEntryRepo CreateMediaRepo(AppDbContext context, ILoggerFactory loggerFactory, TimeProvider clock) =>
        new(
            context,
            new ErrorEventLogger<MediaEntryRepo>(
                loggerFactory.CreateLogger<MediaEntryRepo>(),
                new ErrorEventPolicy(),
                new ErrorDiagnosticsOptions(false)),
            new ServerTimestampPolicy(clock));

    private static async Task<Guid> ReadUserIdAsync(DbContextOptions<AppDbContext> options)
    {
        await using var context = new AppDbContext(options);
        return await context.Users.Select(user => user.Id).SingleAsync();
    }

    private static async Task<User> ReadUserAsync(DbContextOptions<AppDbContext> options)
    {
        await using var context = new AppDbContext(options);
        return await context.Users.AsNoTracking().SingleAsync();
    }

    private static async Task<TvSeriesEntry> ReadSeriesAsync(
        DbContextOptions<AppDbContext> options,
        Guid seriesId)
    {
        await using var context = new AppDbContext(options);
        return await context.TvSeriesEntries
            .AsNoTracking()
            .Include(series => series.Seasons)
            .SingleAsync(series => series.Id == seriesId);
    }

    private sealed class MutableTimeProvider(DateTimeOffset initial) : TimeProvider
    {
        private DateTimeOffset _utcNow = initial;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan amount) => _utcNow = _utcNow.Add(amount);
    }
}
