using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Http.Timeouts;

namespace media_vault_app.API.Diagnostics;

public static class MediaVaultRequestTimeoutPolicies
{
    public const string Authentication = nameof(Authentication);
    public const string ExternalMetadata = nameof(ExternalMetadata);
}

internal static class MediaVaultRequestTimeoutResponse
{
    private const string Message = "The request timed out. Please try again.";
    private const string Code = "Request.Timeout";
    private static readonly Meter Meter = new("MediaVault.Api.RequestTimeouts");
    private static readonly Counter<long> TimeoutCounter =
        Meter.CreateCounter<long>("mediavault.request_timeouts");

    public static Task WriteAsync(HttpContext context)
    {
        // RequestTimeoutMiddleware invokes this callback only for its own expired
        // policy. It handles caller cancellation separately before this boundary.
        if (context.Response.HasStarted)
            return Task.CompletedTask;

        var policy = context.GetEndpoint()?
            .Metadata.GetMetadata<RequestTimeoutAttribute>()?.PolicyName ?? "unknown";
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("media_vault_app.API.RequestTimeouts");

        RequestTimeoutLogEvents.ServerRequestTimeout(logger, policy, traceId);
        TimeoutCounter.Add(1, new KeyValuePair<string, object?>("policy", policy));

        context.Response.ContentType = "application/json; charset=utf-8";
        return context.Response.WriteAsJsonAsync(new { message = Message, code = Code });
    }
}

internal static partial class RequestTimeoutLogEvents
{
    [LoggerMessage(
        EventId = 3001,
        EventName = "ServerRequestTimeout",
        Level = LogLevel.Warning,
        Message = "Server request timeout for policy {Policy}, trace {TraceId}")]
    internal static partial void ServerRequestTimeout(ILogger logger, string policy, string traceId);
}
