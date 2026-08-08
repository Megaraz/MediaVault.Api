using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Megaraz.ResultPattern.AspNetCore;
using Microsoft.Extensions.Logging;

namespace media_vault_app.Tests.Infrastructure;

public sealed class ErrorEventLoggerTests
{
    private static readonly ErrorContext OperationContext =
        new(OperationType.Get, "MediaEntry");

    private static readonly ErrorEventContext EventContext =
        new("Infrastructure", nameof(RepresentativeRepositoryBoundary), "QueryAsync", OperationContext);

    [Fact]
    public void RepresentativeBoundary_EmitsSanitizedStructuredEventWithoutChangingResult()
    {
        using var provider = new RecordingLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Trace)
            .AddProvider(provider));
        var exception = new InvalidOperationException("secret database detail");
        var error = DatabaseFailurePolicy.QueryFailure(OperationContext, exception);
        var boundary = CreateBoundary(factory, includeExceptionDetails: false);

        var result = boundary.ReturnFailure(error);

        Assert.True(result.IsFailure);
        Assert.Same(error, result.PrimaryError);
        var entry = Assert.Single(provider.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal(2001, entry.EventId.Id);
        Assert.Equal("DatabaseOperationFailed", entry.EventId.Name);
        Assert.Equal(
            typeof(RepresentativeRepositoryBoundary).FullName!.Replace('+', '.'),
            entry.Category);
        Assert.Null(entry.Exception);
        Assert.Equal("Infrastructure", entry.Properties["Layer"]);
        Assert.Equal(nameof(RepresentativeRepositoryBoundary), entry.Properties["Service"]);
        Assert.Equal("QueryAsync", entry.Properties["Method"]);
        Assert.Equal(OperationType.Get.ToString(), entry.Properties["Operation"]);
        Assert.Equal("MediaEntry", entry.Properties["EntityName"]);
        Assert.Equal(error.Code, entry.Properties["ErrorCode"]);
        Assert.Equal(error.Type.ToString(), entry.Properties["ErrorType"]);
        Assert.Equal(exception.GetType().FullName, entry.Properties["ExceptionType"]);
        Assert.DoesNotContain("Description", entry.Properties.Keys);
        Assert.DoesNotContain(exception.Message, entry.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(error.Description, entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DevelopmentPolicy_AttachesExceptionWithoutCopyingItsMessageIntoStructuredState()
    {
        using var provider = new RecordingLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var exception = new InvalidOperationException("development-only exception detail");
        var error = DatabaseFailurePolicy.QueryFailure(OperationContext, exception);
        var boundary = CreateBoundary(factory, includeExceptionDetails: true);

        boundary.ReturnFailure(error);

        var entry = Assert.Single(provider.Entries);
        Assert.Same(exception, entry.Exception);
        Assert.DoesNotContain(exception.Message, entry.Properties.Values.OfType<string>());
    }

    [Fact]
    public void RoutineExpectedFailuresAndCallerCancellation_EmitNoEvent()
    {
        using var provider = new RecordingLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var boundary = CreateBoundary(factory, includeExceptionDetails: false);

        boundary.Emit(HttpError.NotFound(OperationContext));
        boundary.Emit(Error.Cancelled(OperationContext));
        boundary.Emit(ValidationError.Custom(
            OperationContext,
            description: "unsafe submitted value"));

        Assert.Empty(provider.Entries);
    }

    [Fact]
    public void ConcurrencyFailure_UsesApprovedWarningIdentity()
    {
        using var provider = new RecordingLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var error = DatabaseFailurePolicy.ConcurrencyFailure(
            OperationContext,
            new InvalidOperationException("private concurrency detail"));
        var boundary = CreateBoundary(factory, includeExceptionDetails: false);

        boundary.ReturnFailure(error);

        var entry = Assert.Single(provider.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal(2000, entry.EventId.Id);
        Assert.Equal("DatabaseConcurrencyConflict", entry.EventId.Name);
    }

    [Theory]
    [InlineData(HttpErrorType.TransportFailure, 2100, "ExternalDependencyTransientFailure", LogLevel.Warning)]
    [InlineData(HttpErrorType.Unauthorized, 2101, "ExternalDependencyAuthenticationFailed", LogLevel.Error)]
    [InlineData(HttpErrorType.MalformedResponse, 2102, "ExternalDependencyInvalidResponse", LogLevel.Error)]
    public void ExternalFailurePolicy_UsesApprovedEventIdentity(
        HttpErrorType errorType,
        int eventId,
        string eventName,
        LogLevel level)
    {
        using var provider = new RecordingLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var logger = new ErrorEventLogger<RepresentativeRepositoryBoundary>(
            factory.CreateLogger<RepresentativeRepositoryBoundary>(),
            new ErrorEventPolicy(),
            new ErrorDiagnosticsOptions(false));
        var error = CreateHttpError(errorType);
        var context = new ErrorEventContext(
            "Infrastructure",
            nameof(RepresentativeRepositoryBoundary),
            "FetchAsync",
            OperationContext,
            provider: "TMDB",
            failureKind: errorType.ToString(),
            statusCode: errorType == HttpErrorType.Unauthorized ? 401 : null);

        logger.Log(error, context);

        var entry = Assert.Single(provider.Entries);
        Assert.Equal(level, entry.Level);
        Assert.Equal(eventId, entry.EventId.Id);
        Assert.Equal(eventName, entry.EventId.Name);
        Assert.Equal("TMDB", entry.Properties["Provider"]);
    }

    private static RepresentativeRepositoryBoundary CreateBoundary(
        ILoggerFactory factory,
        bool includeExceptionDetails) =>
        new(new ErrorEventLogger<RepresentativeRepositoryBoundary>(
            factory.CreateLogger<RepresentativeRepositoryBoundary>(),
            new ErrorEventPolicy(),
            new ErrorDiagnosticsOptions(includeExceptionDetails)));

    private static HttpError CreateHttpError(HttpErrorType errorType) => errorType switch
    {
        HttpErrorType.TransportFailure => HttpError.TransportFailure(
            OperationContext,
            new HttpRequestException("private transport detail")),
        HttpErrorType.Unauthorized => HttpError.UnauthorizedAccess(OperationContext),
        HttpErrorType.MalformedResponse => HttpError.MalformedResponse(
            OperationContext,
            new InvalidOperationException("private malformed response detail")),
        _ => throw new ArgumentOutOfRangeException(nameof(errorType))
    };

    private sealed class RepresentativeRepositoryBoundary(
        ErrorEventLogger<RepresentativeRepositoryBoundary> logger)
    {
        public Result ReturnFailure(Error error)
        {
            Emit(error);
            return Result.Failure(error);
        }

        public void Emit(Error error) => logger.Log(error, EventContext);
    }
}
