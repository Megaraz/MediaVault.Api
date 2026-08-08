using Megaraz.ResultPattern;
using Microsoft.Extensions.Logging;

namespace media_vault_app.Infrastructure.Diagnostics;

public sealed record ErrorDiagnosticsOptions(bool IncludeExceptionDetails);

/// <summary>
/// Emits the approved structured event for a known Result failure through a typed logger.
/// </summary>
public sealed class ErrorEventLogger<TCategory>(
    ILogger<TCategory> logger,
    ErrorEventPolicy policy,
    ErrorDiagnosticsOptions options)
{
    public void Log(Error error, ErrorEventContext context)
    {
        var eventKind = policy.GetEventKind(error);
        if (eventKind is null)
            return;

        var exception = options.IncludeExceptionDetails ? error.Exception : null;
        var exceptionType = error.Exception?.GetType().FullName;
        var operation = context.ErrorContext.Operation.ToString();
        var entityName = context.ErrorContext.EntityName;
        var errorType = error.Type.ToString();

        switch (eventKind)
        {
            case ErrorEventKind.DatabaseConcurrencyConflict:
                ErrorLogEvents.DatabaseConcurrencyConflict(
                    logger, context.Layer, context.Service, context.Method, operation, entityName,
                    error.Code, errorType, exceptionType, exception);
                break;
            case ErrorEventKind.DatabaseOperationFailed:
                ErrorLogEvents.DatabaseOperationFailed(
                    logger, context.Layer, context.Service, context.Method, operation, entityName,
                    error.Code, errorType, exceptionType, exception);
                break;
            case ErrorEventKind.ExternalDependencyTransientFailure:
                ErrorLogEvents.ExternalDependencyTransientFailure(
                    logger, context.Layer, context.Service, context.Method, operation, entityName,
                    context.Provider, context.FailureKind, context.StatusCode, error.Code, errorType,
                    exceptionType, exception);
                break;
            case ErrorEventKind.ExternalDependencyAuthenticationFailed:
                ErrorLogEvents.ExternalDependencyAuthenticationFailed(
                    logger, context.Layer, context.Service, context.Method, operation, entityName,
                    context.Provider, context.StatusCode, error.Code, errorType, exceptionType, exception);
                break;
            case ErrorEventKind.ExternalDependencyInvalidResponse:
                ErrorLogEvents.ExternalDependencyInvalidResponse(
                    logger, context.Layer, context.Service, context.Method, operation, entityName,
                    context.Provider, context.FailureKind, context.StatusCode, error.Code, errorType,
                    exceptionType, exception);
                break;
        }
    }
}

internal static partial class ErrorLogEvents
{
    [LoggerMessage(
        EventId = 2000,
        EventName = "DatabaseConcurrencyConflict",
        Level = LogLevel.Warning,
        Message = "Database concurrency conflict in {Layer}.{Service}.{Method} for {Operation} {EntityName}: {ErrorCode} ({ErrorType}); exception type {ExceptionType}")]
    internal static partial void DatabaseConcurrencyConflict(
        ILogger logger,
        string layer,
        string service,
        string method,
        string operation,
        string entityName,
        string errorCode,
        string errorType,
        string? exceptionType,
        Exception? exception);

    [LoggerMessage(
        EventId = 2001,
        EventName = "DatabaseOperationFailed",
        Level = LogLevel.Error,
        Message = "Database operation failed in {Layer}.{Service}.{Method} for {Operation} {EntityName}: {ErrorCode} ({ErrorType}); exception type {ExceptionType}")]
    internal static partial void DatabaseOperationFailed(
        ILogger logger,
        string layer,
        string service,
        string method,
        string operation,
        string entityName,
        string errorCode,
        string errorType,
        string? exceptionType,
        Exception? exception);

    [LoggerMessage(
        EventId = 2100,
        EventName = "ExternalDependencyTransientFailure",
        Level = LogLevel.Warning,
        Message = "External dependency transient failure in {Layer}.{Service}.{Method} for {Operation} {EntityName}: provider {Provider}, failure {FailureKind}, status {StatusCode}, {ErrorCode} ({ErrorType}); exception type {ExceptionType}")]
    internal static partial void ExternalDependencyTransientFailure(
        ILogger logger,
        string layer,
        string service,
        string method,
        string operation,
        string entityName,
        string? provider,
        string? failureKind,
        int? statusCode,
        string errorCode,
        string errorType,
        string? exceptionType,
        Exception? exception);

    [LoggerMessage(
        EventId = 2101,
        EventName = "ExternalDependencyAuthenticationFailed",
        Level = LogLevel.Error,
        Message = "External dependency authentication failed in {Layer}.{Service}.{Method} for {Operation} {EntityName}: provider {Provider}, status {StatusCode}, {ErrorCode} ({ErrorType}); exception type {ExceptionType}")]
    internal static partial void ExternalDependencyAuthenticationFailed(
        ILogger logger,
        string layer,
        string service,
        string method,
        string operation,
        string entityName,
        string? provider,
        int? statusCode,
        string errorCode,
        string errorType,
        string? exceptionType,
        Exception? exception);

    [LoggerMessage(
        EventId = 2102,
        EventName = "ExternalDependencyInvalidResponse",
        Level = LogLevel.Error,
        Message = "External dependency response was invalid in {Layer}.{Service}.{Method} for {Operation} {EntityName}: provider {Provider}, failure {FailureKind}, status {StatusCode}, {ErrorCode} ({ErrorType}); exception type {ExceptionType}")]
    internal static partial void ExternalDependencyInvalidResponse(
        ILogger logger,
        string layer,
        string service,
        string method,
        string operation,
        string entityName,
        string? provider,
        string? failureKind,
        int? statusCode,
        string errorCode,
        string errorType,
        string? exceptionType,
        Exception? exception);
}
