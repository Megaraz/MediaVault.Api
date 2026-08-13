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
    public void RepositoryFailure_EmitsOneStructuredEventWithoutChangingTheResult()
    {
        using var provider = new RecordingLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Trace)
            .AddProvider(provider));
        using var dbContext = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source=:memory:").Options);
        var errorContext = new ErrorContext(OperationType.Get, "User");
        var repository = new TestRepository(
            dbContext,
            new ErrorEventLogger<RepoBase<User, Guid>>(
                factory.CreateLogger<RepoBase<User, Guid>>(),
                new ErrorEventPolicy(),
                new ErrorDiagnosticsOptions(false)));
        var error = DatabaseFailurePolicy.QueryFailure(
            errorContext,
            new InvalidOperationException("private diagnostic"));

        var result = repository.ReturnFailureAfterLogging(error, errorContext);

        Assert.True(result.IsFailure);
        Assert.Same(error, result.PrimaryError);
        var entry = Assert.Single(provider.Entries);
        Assert.Equal(2001, entry.EventId.Id);
        Assert.Equal("DatabaseOperationFailed", entry.EventId.Name);
        Assert.Equal("Infrastructure", entry.Properties["Layer"]);
        Assert.Equal(nameof(TestRepository), entry.Properties["Service"]);
        Assert.Equal(nameof(TestRepository.ReturnFailureAfterLogging), entry.Properties["Method"]);
        Assert.Null(entry.Exception);
        Assert.DoesNotContain("private diagnostic", entry.Message, StringComparison.Ordinal);
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
        var repository = new RepoBase<User, Guid>(
            queryContext,
            new ErrorEventLogger<RepoBase<User, Guid>>(
                factory.CreateLogger<RepoBase<User, Guid>>(),
                new ErrorEventPolicy(),
                new ErrorDiagnosticsOptions(false)));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.ExistsAsync(Guid.NewGuid()));

        Assert.Equal("controlled programming failure", exception.Message);
        Assert.DoesNotContain(provider.Entries, entry => entry.EventId.Id == 2001);
    }

    private sealed class TestRepository(
        AppDbContext dbContext,
        ErrorEventLogger<RepoBase<User, Guid>> logger)
        : RepoBase<User, Guid>(dbContext, logger)
    {
        public Result ReturnFailureAfterLogging(Error error, ErrorContext errorContext) =>
            LogAndFail(error, errorContext);
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
