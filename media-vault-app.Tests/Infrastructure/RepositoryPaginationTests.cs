using media_vault_app.Domain.Entities;
using media_vault_app.Domain.Enums;
using media_vault_app.Infrastructure;
using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Infrastructure.Repos;
using media_vault_app.Tests.TestHelpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace media_vault_app.Tests.Infrastructure;

public sealed class RepositoryPaginationTests
{
    [Fact]
    public async Task RepoBase_GetCollectionAsync_OrdersByCreationDateThenId_AndKeepsPagesDisjoint()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var ids = CreateOrderedIds();
        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
            setupContext.Users.AddRange(
                CreateUser(ids[0], new DateTime(2026, 1, 3), "newest"),
                CreateUser(ids[1], new DateTime(2026, 1, 2), "tie-low"),
                CreateUser(ids[2], new DateTime(2026, 1, 2), "tie-high"),
                CreateUser(ids[3], new DateTime(2026, 1, 1), "oldest-low"),
                CreateUser(ids[4], new DateTime(2026, 1, 1), "oldest-high"));
            await setupContext.SaveChangesAsync();
        }

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using var queryContext = new AppDbContext(options);
        var repository = new RepoBase<User, Guid>(
            queryContext,
            CreateErrorLogger<RepoBase<User, Guid>>(loggerFactory));

        var firstPage = await repository.GetCollectionAsync(1, 2);
        var repeatedFirstPage = await repository.GetCollectionAsync(1, 2);
        var secondPage = await repository.GetCollectionAsync(2, 2);

        Assert.True(firstPage.IsSuccess);
        Assert.True(repeatedFirstPage.IsSuccess);
        Assert.True(secondPage.IsSuccess);
        Assert.Equal(new[] { ids[0], ids[1] }, firstPage.Value.Select(user => user.Id));
        Assert.Equal(firstPage.Value.Select(user => user.Id), repeatedFirstPage.Value.Select(user => user.Id));
        Assert.Equal(new[] { ids[2], ids[3] }, secondPage.Value.Select(user => user.Id));
        Assert.Empty(firstPage.Value.Select(user => user.Id).Intersect(secondPage.Value.Select(user => user.Id)));
    }

    [Fact]
    public async Task DependentEntityRepoBase_GetCollectionByOwnerIdAsync_OrdersByCreationDateThenId_AndKeepsPagesDisjoint()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var ownerId = Guid.NewGuid();
        var ids = CreateOrderedIds();

        await SeedMediaEntriesAsync(options, ownerId, ids, includeExcludedEntry: false);

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using var queryContext = new AppDbContext(options);
        var repository = new MediaEntryRepo(
            queryContext,
            CreateErrorLogger<DependentEntityRepoBase<MediaEntry, Guid, Guid>>(loggerFactory));

        var firstPage = await repository.GetCollectionByOwnerIdAsync(ownerId, 1, 2);
        var repeatedFirstPage = await repository.GetCollectionByOwnerIdAsync(ownerId, 1, 2);
        var secondPage = await repository.GetCollectionByOwnerIdAsync(ownerId, 2, 2);

        Assert.True(firstPage.IsSuccess);
        Assert.True(repeatedFirstPage.IsSuccess);
        Assert.True(secondPage.IsSuccess);
        Assert.Equal(new[] { ids[0], ids[1] }, firstPage.Value.Select(entry => entry.Id));
        Assert.Equal(firstPage.Value.Select(entry => entry.Id), repeatedFirstPage.Value.Select(entry => entry.Id));
        Assert.Equal(new[] { ids[2], ids[3] }, secondPage.Value.Select(entry => entry.Id));
        Assert.Empty(firstPage.Value.Select(entry => entry.Id).Intersect(secondPage.Value.Select(entry => entry.Id)));
    }

    [Fact]
    public async Task MediaEntryRepo_SearchMediaEntriesAsync_OrdersFilteredResultsByCreationDateThenId_AndKeepsPagesDisjoint()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var ownerId = Guid.NewGuid();
        var ids = CreateOrderedIds();

        await SeedMediaEntriesAsync(options, ownerId, ids, includeExcludedEntry: true);

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using var queryContext = new AppDbContext(options);
        var repository = new MediaEntryRepo(
            queryContext,
            CreateErrorLogger<DependentEntityRepoBase<MediaEntry, Guid, Guid>>(loggerFactory));

        var firstPage = await repository.SearchMediaEntriesAsync(ownerId, "Match", 1, 2);
        var repeatedFirstPage = await repository.SearchMediaEntriesAsync(ownerId, "Match", 1, 2);
        var secondPage = await repository.SearchMediaEntriesAsync(ownerId, "Match", 2, 2);

        Assert.True(firstPage.IsSuccess);
        Assert.True(repeatedFirstPage.IsSuccess);
        Assert.True(secondPage.IsSuccess);
        Assert.Equal(new[] { ids[0], ids[1] }, firstPage.Value.Select(entry => entry.Id));
        Assert.Equal(firstPage.Value.Select(entry => entry.Id), repeatedFirstPage.Value.Select(entry => entry.Id));
        Assert.Equal(new[] { ids[2], ids[3] }, secondPage.Value.Select(entry => entry.Id));
        Assert.Empty(firstPage.Value.Select(entry => entry.Id).Intersect(secondPage.Value.Select(entry => entry.Id)));
    }

    private static Guid[] CreateOrderedIds() =>
    [
        Guid.Parse("00000000-0000-0000-0000-000000000005"),
        Guid.Parse("00000000-0000-0000-0000-000000000001"),
        Guid.Parse("00000000-0000-0000-0000-000000000002"),
        Guid.Parse("00000000-0000-0000-0000-000000000003"),
        Guid.Parse("00000000-0000-0000-0000-000000000004")
    ];

    private static User CreateUser(Guid id, DateTime createdAtUtc, string suffix) =>
        new()
        {
            Id = id,
            Username = $"pagination-{suffix}",
            Email = $"pagination-{suffix}@example.test",
            PasswordHash = "hash",
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };

    private static async Task SeedMediaEntriesAsync(
        DbContextOptions<AppDbContext> options,
        Guid ownerId,
        IReadOnlyList<Guid> ids,
        bool includeExcludedEntry)
    {
        await using var setupContext = new AppDbContext(options);
        await setupContext.Database.EnsureCreatedAsync();
        setupContext.Users.Add(CreateUser(ownerId, new DateTime(2025, 1, 1), "media-owner"));
        var mediaEntries = new List<MediaEntry>
        {
            CreateMovie(ids[0], ownerId, new DateTime(2026, 1, 3), "Match newest"),
            CreateMovie(ids[1], ownerId, new DateTime(2026, 1, 2), "Match tie-low"),
            CreateMovie(ids[2], ownerId, new DateTime(2026, 1, 2), "Match tie-high"),
            CreateMovie(ids[3], ownerId, new DateTime(2026, 1, 1), "Match oldest-low"),
            CreateMovie(ids[4], ownerId, new DateTime(2026, 1, 1), "Match oldest-high")
        };
        if (includeExcludedEntry)
        {
            mediaEntries.Add(CreateMovie(
                Guid.Parse("00000000-0000-0000-0000-000000000006"),
                ownerId,
                new DateTime(2026, 1, 4),
                "Exclude"));
        }

        setupContext.MediaEntries.AddRange(mediaEntries);
        await setupContext.SaveChangesAsync();
    }

    private static MovieEntry CreateMovie(Guid id, Guid ownerId, DateTime createdAtUtc, string title) =>
        new()
        {
            Id = id,
            OwnerId = ownerId,
            Title = title,
            Status = Status.Completed,
            Rating = 4m,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };

    private static ErrorEventLogger<TCategory> CreateErrorLogger<TCategory>(ILoggerFactory loggerFactory) =>
        new(
            loggerFactory.CreateLogger<TCategory>(),
            new ErrorEventPolicy(),
            new ErrorDiagnosticsOptions(false));
}
