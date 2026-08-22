using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Mappers.MediaEntry;
using media_vault_app.Application.Services.MediaEntry;
using media_vault_app.Domain.Enums;
using media_vault_app.Infrastructure;
using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Infrastructure.Repos;
using media_vault_app.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using Megaraz.ResultPattern;
using media_vault_app.Domain.Entities;

namespace media_vault_app.Tests.Infrastructure;

public sealed class DatabaseAndLoggingTests
{
    [Theory]
    [InlineData("DatabaseSaveChangesFailure", "A database failure occurred while saving changes for MediaEntry.")]
    [InlineData("DatabaseQueryFailure", "A database failure occurred while querying MediaEntry.")]
    [InlineData("DatabaseConcurrencyFailure", "A concurrency conflict occurred while processing MediaEntry. The entity was modified or deleted by another process.")]
    [InlineData("DatabaseUnexpectedFailure", "An unexpected infrastructure failure occurred while performing Update for entity MediaEntry.")]
    public void DatabaseFailurePolicy_UsesPackageCodesAndApprovedSafeMessages(string codeSuffix, string userMessage)
    {
        var context = new ErrorContext(OperationType.Update, "MediaEntry");
        var exception = new InvalidOperationException("private database diagnostic");
        var error = codeSuffix switch
        {
            "DatabaseSaveChangesFailure" => DatabaseFailurePolicy.SaveChangesFailure(context, exception),
            "DatabaseQueryFailure" => DatabaseFailurePolicy.QueryFailure(context, exception),
            "DatabaseConcurrencyFailure" => DatabaseFailurePolicy.ConcurrencyFailure(context, exception),
            _ => DatabaseFailurePolicy.UnexpectedFailure(context, exception)
        };

        Assert.Equal($"Update.MediaEntry.{codeSuffix}", error.Code);
        Assert.Equal(ErrorType.External, error.Type);
        Assert.Equal(userMessage, error.UserMessage);
        Assert.Same(exception, error.Exception);
    }

    [Fact]
    public async Task DetailedReads_LoadAllMediaSubtypesFromSqlite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var ownerId = Guid.NewGuid();
        var movieId = Guid.NewGuid();
        var tvSeriesId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var mangaId = Guid.NewGuid();
        var seasonId = Guid.NewGuid();

        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
            setupContext.Users.Add(new User
            {
                Id = ownerId,
                Username = "detail-read-owner",
                Email = "detail-read-owner@example.test",
                PasswordHash = "hash"
            });
            var tvSeriesEntry = new TvSeriesEntry
            {
                Id = tvSeriesId,
                OwnerId = ownerId,
                Title = "TV series",
                Status = Status.Ongoing,
                Rating = 4.5m
            };
            setupContext.MediaEntries.AddRange(
                new MovieEntry
                {
                    Id = movieId,
                    OwnerId = ownerId,
                    Title = "Movie",
                    Status = Status.Completed,
                    Rating = 4m
                },
                tvSeriesEntry,
                new GameEntry
                {
                    Id = gameId,
                    OwnerId = ownerId,
                    Title = "Game",
                    Status = Status.Backlog,
                    Rating = 3.5m,
                    Platforms = ["PC"],
                    PcRequirements = new GamePcRequirements(
                        Minimum: "minimum",
                        Recommended: "recommended",
                        High: "high",
                        VeryHigh: "very high",
                        Ultra: "ultra")
                },
                new BookEntry
                {
                    Id = bookId,
                    OwnerId = ownerId,
                    Title = "Book",
                    Status = Status.Completed,
                    Rating = 4m,
                    Author = "Author"
                },
                new MangaEntry
                {
                    Id = mangaId,
                    OwnerId = ownerId,
                    Title = "Manga",
                    Status = Status.Ongoing,
                    Rating = 4m,
                    Author = "Author"
                });
            setupContext.Seasons.Add(new Season
            {
                Id = seasonId,
                TvSeriesEntryId = tvSeriesId,
                Name = "Season 1",
                SeasonNumber = 1,
                Status = Status.Completed,
                Rating = 4m
            });

            await setupContext.SaveChangesAsync();
        }

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        await using var queryContext = new AppDbContext(options);
        var mediaEntryRepo = new MediaEntryRepo(
            queryContext,
            new ErrorEventLogger<MediaEntryRepo>(
                loggerFactory.CreateLogger<MediaEntryRepo>(),
                new ErrorEventPolicy(),
                new ErrorDiagnosticsOptions(false)));
        var userRepo = new UserRepo(
            queryContext,
            new ErrorEventLogger<UserRepo>(
                loggerFactory.CreateLogger<UserRepo>(),
                new ErrorEventPolicy(),
                new ErrorDiagnosticsOptions(false)));
        var readService = new MediaEntryReadService(
            mediaEntryRepo,
            userRepo,
            ServiceTestLogger.Create<MediaEntryReadService>());

        var entryIdsAndTypes = new (Guid Id, Type Type)[]
        {
            (movieId, typeof(MovieEntryDetailedDto)),
            (tvSeriesId, typeof(TvSeriesEntryDetailedDto)),
            (gameId, typeof(GameEntryDetailedDto)),
            (bookId, typeof(BookEntryDetailedDto)),
            (mangaId, typeof(MangaEntryDetailedDto))
        };

        foreach (var (id, expectedType) in entryIdsAndTypes)
        {
            var result = await readService.GetDetailedByIdAsync(ownerId, id);

            Assert.True(result.IsSuccess);
            Assert.IsType(expectedType, result.Value);
        }

        var gameResult = await readService.GetGameByIdAsync(ownerId, gameId);
        var game = Assert.IsType<GameEntryDetailedDto>(gameResult.Value);
        Assert.Equal("minimum", game.PcRequirements?.Minimum);
        Assert.Equal("ultra", game.PcRequirements?.Ultra);

        var tvSeriesResult = await readService.GetTvSeriesByIdAsync(ownerId, tvSeriesId);
        var tvSeries = Assert.IsType<TvSeriesEntryDetailedDto>(tvSeriesResult.Value);
        Assert.Equal("Season 1", Assert.Single(tvSeries.Seasons).Name);
    }

    [Fact]
    public async Task UnknownRepositoryException_IsNotHiddenAsADatabaseResult()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var setupOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        await using (var setupContext = new AppDbContext(setupOptions))
            await setupContext.Database.EnsureCreatedAsync();

        var queryOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new ProgrammingFailureInterceptor())
            .Options;
        await using var queryContext = new AppDbContext(queryOptions);
        using var provider = new RecordingLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var repository = new UserRepo(
            queryContext,
            new ErrorEventLogger<UserRepo>(
                factory.CreateLogger<UserRepo>(),
                new ErrorEventPolicy(),
                new ErrorDiagnosticsOptions(false)));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.ExistsAsync(Guid.NewGuid()));

        Assert.Equal("controlled programming failure", exception.Message);
        Assert.DoesNotContain(provider.Entries, entry => entry.EventId.Id == 2001);
    }

    [Fact]
    public async Task RepositoryCancellation_PropagatesCallerCancellationWithoutResultOrLogging()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        using var provider = new RecordingLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Trace)
            .AddProvider(provider));
        var userRepository = new UserRepo(
            dbContext,
            new ErrorEventLogger<UserRepo>(
                factory.CreateLogger<UserRepo>(),
                new ErrorEventPolicy(),
                new ErrorDiagnosticsOptions(false)));
        var mediaEntryRepository = new MediaEntryRepo(
            dbContext,
            new ErrorEventLogger<MediaEntryRepo>(
                factory.CreateLogger<MediaEntryRepo>(),
                new ErrorEventPolicy(),
                new ErrorDiagnosticsOptions(false)));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var operations = new Func<Task>[]
        {
            async () => await mediaEntryRepository.GetMinimalCollectionByOwnerIdAsync(Guid.NewGuid(), 1, 10, cancellation.Token),
            async () => await mediaEntryRepository.SearchMediaEntriesAsync(Guid.NewGuid(), "query", 1, 10, cancellation.Token),
            async () => await userRepository.GetByUsernameOrEmailAsync("user", cancellation.Token)
        };

        foreach (var operation in operations)
        {
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(operation);
            Assert.Equal(cancellation.Token, exception.CancellationToken);
        }

        Assert.Empty(provider.Entries);
    }

    private sealed class ProgrammingFailureInterceptor : DbCommandInterceptor
    {
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromException<InterceptionResult<DbDataReader>>(
                new InvalidOperationException("controlled programming failure"));
    }
}
