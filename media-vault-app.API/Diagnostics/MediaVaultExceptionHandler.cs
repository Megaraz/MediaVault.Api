using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Diagnostics;

/// <summary>
/// Owns the safe HTTP and diagnostic boundary for genuinely unexpected request failures.
/// </summary>
public sealed class MediaVaultExceptionHandler(
    ILogger<MediaVaultExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted || IsCallerCancellation(httpContext, exception))
            return false;

        var traceId = Activity.Current?.TraceId.ToString();
        if (string.IsNullOrWhiteSpace(traceId))
            traceId = httpContext.TraceIdentifier;

        ExceptionLogEvents.UnhandledRequestException(
            logger,
            layer: "API",
            service: nameof(MediaVaultExceptionHandler),
            method: nameof(TryHandleAsync),
            exceptionType: exception.GetType().FullName ?? exception.GetType().Name,
            traceId,
            environment.IsDevelopment() ? exception : null);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-500-internal-server-error",
            Title = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "The server could not complete the request."
        };
        problemDetails.Extensions["traceId"] = traceId;

        var written = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        return written;
    }

    private static bool IsCallerCancellation(HttpContext context, Exception exception) =>
        exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested;
}

internal static partial class ExceptionLogEvents
{
    [LoggerMessage(
        EventId = 3000,
        EventName = "UnhandledRequestException",
        Level = LogLevel.Error,
        Message = "Unhandled request exception in {Layer}.{Service}.{Method}: exception type {ExceptionType}, trace {TraceId}")]
    internal static partial void UnhandledRequestException(
        ILogger logger,
        string layer,
        string service,
        string method,
        string exceptionType,
        string traceId,
        Exception? exception);
}
