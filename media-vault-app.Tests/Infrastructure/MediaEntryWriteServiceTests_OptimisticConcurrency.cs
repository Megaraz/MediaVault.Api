using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.Mappers.MediaEntry;
using media_vault_app.Application.Services.MediaEntry;
using media_vault_app.Application.Validators.MediaEntry;
using media_vault_app.Domain.Entities;
using media_vault_app.Domain.Enums;
using media_vault_app.Infrastructure;
using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Infrastructure.Repos;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace media_vault_app.Tests.Infrastructure;

public sealed class MediaEntryWriteServiceTests_OptimisticConcurrency
{
    [Fact]
    public async Task CompetingWriters_RejectStaleUpdateAndDeleteWithoutOverwritingOrRemovingEntry()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var ownerId = Guid.NewGuid();
        var entryId = Guid.NewGuid();

        await SeedMovieAsync(options, ownerId, entryId);

        var firstReadVersion = await ReadVersionAsync(options, ownerId, entryId);
        var secondReadVersion = await ReadVersionAsync(options, ownerId, entryId);
        Assert.Equal(1, firstReadVersion);
        Assert.Equal(firstReadVersion, secondReadVersion);

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using (var firstWriteContext = new AppDbContext(options))
        {
            var result = await CreateService(firstWriteContext, loggerFactory).UpdateAsync(
                ownerId,
                entryId,
                CreateUpdateDto("First writer", firstReadVersion));
            Assert.True(result.IsSuccess);
        }

        await using (var staleWriteContext = new AppDbContext(options))
        {
            var result = await CreateService(staleWriteContext, loggerFactory).UpdateAsync(
                ownerId,
                entryId,
                CreateUpdateDto("Stale writer", secondReadVersion));
            AssertConcurrencyFailure(result);
        }

        await using (var staleDeleteContext = new AppDbContext(options))
        {
            var result = await CreateService(staleDeleteContext, loggerFactory).DeleteAsync(
                ownerId,
                entryId,
                secondReadVersion);
            AssertConcurrencyFailure(result);
        }

        await using (var verificationContext = new AppDbContext(options))
        {
            var persisted = await verificationContext.MediaEntries
                .AsNoTracking()
                .SingleAsync(entry => entry.Id == entryId && entry.OwnerId == ownerId);
            Assert.Equal("First writer", persisted.Title);
            Assert.Equal(2, persisted.Version);
        }

        await using (var currentDeleteContext = new AppDbContext(options))
        {
            var result = await CreateService(currentDeleteContext, loggerFactory).DeleteAsync(
                ownerId,
                entryId,
                expectedVersion: 2);
            Assert.True(result.IsSuccess);
        }

        await using var deletedVerificationContext = new AppDbContext(options);
        Assert.False(await deletedVerificationContext.MediaEntries.AnyAsync(entry => entry.Id == entryId));
    }

    [Fact]
    public async Task CancelledUpdate_PropagatesCancellationAndPreservesVersionAndValues()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var ownerId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        await SeedMovieAsync(options, ownerId, entryId);

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using var updateContext = new AppDbContext(options);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateService(updateContext, loggerFactory).UpdateAsync(
                ownerId,
                entryId,
                CreateUpdateDto("Cancelled writer", 1),
                cancellation.Token));
        Assert.Equal(cancellation.Token, exception.CancellationToken);

        await using var verificationContext = new AppDbContext(options);
        var persisted = await verificationContext.MediaEntries.AsNoTracking().SingleAsync(entry => entry.Id == entryId);
        Assert.Equal("Original", persisted.Title);
        Assert.Equal(1, persisted.Version);
    }

    [Fact]
    public async Task CrossUserUpdate_ReturnsNotFoundAndPreservesOwnedEntry()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        await SeedMovieAsync(options, ownerId, entryId);

        await using (var setupContext = new AppDbContext(options))
        {
            setupContext.Users.Add(new User
            {
                Id = otherUserId,
                Username = "other-user",
                Email = "other@example.com",
                PasswordHash = "hash"
            });
            await setupContext.SaveChangesAsync();
        }

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using (var updateContext = new AppDbContext(options))
        {
            var result = await CreateService(updateContext, loggerFactory).UpdateAsync(
                otherUserId,
                entryId,
                CreateUpdateDto("Cross-user writer", 1));

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.PrimaryError.Type);
        }

        await using var verificationContext = new AppDbContext(options);
        var persisted = await verificationContext.MediaEntries.AsNoTracking().SingleAsync(entry => entry.Id == entryId);
        Assert.Equal(ownerId, persisted.OwnerId);
        Assert.Equal("Original", persisted.Title);
        Assert.Equal(1, persisted.Version);
    }

    private static async Task SeedMovieAsync(
        DbContextOptions<AppDbContext> options,
        Guid ownerId,
        Guid entryId)
    {
        await using var setupContext = new AppDbContext(options);
        await setupContext.Database.EnsureCreatedAsync();
        setupContext.Users.Add(new User
        {
            Id = ownerId,
            Username = "owner",
            Email = "owner@example.com",
            PasswordHash = "hash"
        });
        setupContext.MediaEntries.Add(new MovieEntry
        {
            Id = entryId,
            OwnerId = ownerId,
            Title = "Original",
            Status = Status.Completed,
            Rating = 4m,
            RuntimeMinutes = 120
        });
        await setupContext.SaveChangesAsync();
    }

    private static async Task<int> ReadVersionAsync(
        DbContextOptions<AppDbContext> options,
        Guid ownerId,
        Guid entryId)
    {
        await using var context = new AppDbContext(options);
        return await context.MediaEntries
            .AsNoTracking()
            .Where(entry => entry.Id == entryId && entry.OwnerId == ownerId)
            .Select(entry => entry.Version)
            .SingleAsync();
    }

    private static MediaEntryWriteService CreateService(
        AppDbContext context,
        ILoggerFactory loggerFactory)
    {
        var mediaRepo = new MediaEntryRepo(
            context,
            new ErrorEventLogger<DependentEntityRepoBase<MediaEntry, Guid, Guid>>(
                loggerFactory.CreateLogger<DependentEntityRepoBase<MediaEntry, Guid, Guid>>(),
                new ErrorEventPolicy(),
                new ErrorDiagnosticsOptions(false)));
        var userRepo = new UserRepo(
            context,
            new ErrorEventLogger<UserRepo>(
                loggerFactory.CreateLogger<UserRepo>(),
                new ErrorEventPolicy(),
                new ErrorDiagnosticsOptions(false)));
        return new MediaEntryWriteService(
            mediaRepo,
            userRepo,
            new MediaEntryEntityMapper(),
            new MediaEntryDtoMapper(),
            new MediaEntryDtoValidator(),
            ServiceTestLogger.Create<MediaEntryWriteService>());
    }

    private static MovieEntryUpdateDto CreateUpdateDto(string title, int expectedVersion) =>
        new()
        {
            ExpectedVersion = expectedVersion,
            Title = title,
            Status = Status.Completed,
            Rating = 4.5m,
            RuntimeMinutes = 121
        };

    private static void AssertConcurrencyFailure(Result result)
    {
        Assert.True(result.IsFailure);
        Assert.EndsWith(".DatabaseConcurrencyFailure", result.PrimaryError.Code, StringComparison.Ordinal);
        Assert.DoesNotContain("SQLite", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
