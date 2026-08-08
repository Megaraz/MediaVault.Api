using System.Text.Json;
using media_vault_app.Infrastructure;
using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Infrastructure.Repos;
using media_vault_app.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Megaraz.ResultPattern;
using Megaraz.ResultPattern.AspNetCore;
using Megaraz.ResultPattern.Infrastructure;
using Rasmus.SharedKernel.Diagnostics;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;
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
    public void ErrorLogPolicy_LogsDatabaseErrorsAndSkipsExpectedClientAndCancellationFailures()
    {
        var policy = new ErrorLogPolicy();
        var context = new ErrorContext(OperationType.Get, "MediaEntry");

        Assert.True(policy.ShouldLog(DatabaseError.QueryFailure(context, new Exception("private"))));
        Assert.False(policy.ShouldLog(HttpError.BadRequest(context)));
        Assert.False(policy.ShouldLog(Error.Cancelled(context)));
    }

    [Fact]
    public async Task ErrorLogger_PreservesNdjsonSchemaAndDropsCorruptAndExpiredEntries()
    {
        var directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        try
        {
            var configuration = new ErrorLoggerConfiguration { BasePath = directory, RetentionPeriod = TimeSpan.FromDays(7) };
            var logger = new ErrorLogger(configuration);
            var context = new ErrorContext(OperationType.Get, "MediaEntry");
            var error = DatabaseFailurePolicy.QueryFailure(context, new InvalidOperationException("private diagnostic"));

            await logger.LogErrorToFileAsync(error, new ErrorLogContext("Infrastructure", "DatabaseAndLoggingTests", "ErrorLogger_PreservesNdjsonSchemaAndDropsCorruptAndExpiredEntries"));
            await File.AppendAllLinesAsync(configuration.FullPath, ["{ not json }", "{\"writeDate\":\"2000-01-01T00:00:00+00:00\",\"code\":\"old\",\"description\":\"old\",\"errorType\":\"External\",\"layer\":\"Infrastructure\",\"service\":\"Test\",\"method\":\"Test\",\"exceptionMessage\":null,\"stackTrace\":null}"]);

            await logger.CleanOldLogsAsync();

            var record = Assert.Single(await logger.GetErrorLogsAsync());
            Assert.Equal(error.Code, record.Code);
            using var document = JsonDocument.Parse(Assert.Single(await File.ReadAllLinesAsync(configuration.FullPath)));
            Assert.Equal(["writeDate", "code", "description", "errorType", "layer", "service", "method", "exceptionMessage", "stackTrace"], document.RootElement.EnumerateObject().Select(property => property.Name));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
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

    private sealed class TestRepository(
        AppDbContext dbContext,
        ErrorEventLogger<RepoBase<User, Guid>> logger)
        : RepoBase<User, Guid>(dbContext, logger)
    {
        public Result ReturnFailureAfterLogging(Error error, ErrorContext errorContext) =>
            LogAndFail(error, errorContext);
    }
}
