using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.RateLimiting;
using media_vault_app.API.Controllers;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace media_vault_app.API.RateLimiting;

public static class MediaVaultRateLimitPolicies
{
    public const string LoginByIp = nameof(LoginByIp);
    public const string RegistrationByIp = nameof(RegistrationByIp);
    public const string RawgMetadataByUser = nameof(RawgMetadataByUser);
    public const string TmdbMetadataByUser = nameof(TmdbMetadataByUser);
    public const string GoogleBooksMetadataByUser = nameof(GoogleBooksMetadataByUser);
}

public static class MediaVaultRateLimitingConfiguration
{
    private const string MissingRemoteIpPartition = "missing-remote-ip";
    private const string UnauthenticatedUserPartition = "unauthenticated-user";

    public static IServiceCollection AddMediaVaultRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RateLimitingOptions>()
            .BindConfiguration(RateLimitingOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = configuration.GetSection(RateLimitingOptions.SectionName)
            .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.AddRateLimiter(rateLimiter =>
        {
            rateLimiter.OnRejected = MediaVaultRateLimitRejection.WriteAsync;
            rateLimiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            rateLimiter.AddPolicy(MediaVaultRateLimitPolicies.LoginByIp, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    RemoteIpPartition(context),
                    _ => ToFixedWindowOptions(options.LoginByIp)));
            rateLimiter.AddPolicy(MediaVaultRateLimitPolicies.RegistrationByIp, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    RemoteIpPartition(context),
                    _ => ToFixedWindowOptions(options.RegistrationByIp)));
            rateLimiter.AddPolicy(MediaVaultRateLimitPolicies.RawgMetadataByUser, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    UserPartition(context),
                    _ => ToFixedWindowOptions(options.RawgMetadataByUser)));
            rateLimiter.AddPolicy(MediaVaultRateLimitPolicies.TmdbMetadataByUser, context =>
                RateLimitPartition.GetTokenBucketLimiter(
                    UserPartition(context),
                    _ => ToTokenBucketOptions(options.TmdbMetadataByUser)));
            rateLimiter.AddPolicy(MediaVaultRateLimitPolicies.GoogleBooksMetadataByUser, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    UserPartition(context),
                    _ => ToFixedWindowOptions(options.GoogleBooksMetadataByUser)));
        });

        return services;
    }

    private static string RemoteIpPartition(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? MissingRemoteIpPartition;

    private static string UserPartition(HttpContext context) =>
        context.User.TryGetUserId(out var userId)
            ? userId.ToString("D")
            : UnauthenticatedUserPartition;

    private static FixedWindowRateLimiterOptions ToFixedWindowOptions(FixedWindowRateLimitOptions options) => new()
    {
        PermitLimit = options.PermitLimit,
        Window = TimeSpan.FromSeconds(options.WindowSeconds),
        QueueLimit = options.QueueLimit,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        AutoReplenishment = true
    };

    private static TokenBucketRateLimiterOptions ToTokenBucketOptions(TokenBucketRateLimitOptions options) => new()
    {
        TokenLimit = options.TokenLimit,
        TokensPerPeriod = options.TokensPerPeriod,
        ReplenishmentPeriod = TimeSpan.FromSeconds(options.ReplenishmentPeriodSeconds),
        QueueLimit = options.QueueLimit,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        AutoReplenishment = true
    };
}

internal static class MediaVaultRateLimitRejection
{
    private const string Message = "Too many requests. Please try again later.";
    private const string Code = "Request.RateLimited";
    private static readonly Meter Meter = new("MediaVault.Api.RateLimiting");
    private static readonly Counter<long> RejectionCounter = Meter.CreateCounter<long>("mediavault.inbound_rate_limit_rejections");

    public static ValueTask WriteAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        var policy = httpContext.GetEndpoint()?
            .Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName ?? "unknown";
        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfterValue)
            ? retryAfterValue
            : (TimeSpan?)null;
        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        var logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("media_vault_app.API.RateLimiting");

        RateLimitLogEvents.InboundRateLimitRejected(logger, policy, traceId, retryAfter.HasValue);
        RejectionCounter.Add(1,
            new KeyValuePair<string, object?>("policy", policy),
            new KeyValuePair<string, object?>("retry_after_present", retryAfter.HasValue));

        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        httpContext.Response.ContentType = "application/json; charset=utf-8";
        if (retryAfter is { } delay)
            httpContext.Response.Headers.RetryAfter = Math.Max(1, (int)Math.Ceiling(delay.TotalSeconds)).ToString();

        return new ValueTask(httpContext.Response.WriteAsJsonAsync(
            new { message = Message, code = Code }, cancellationToken));
    }
}

internal static partial class RateLimitLogEvents
{
    [LoggerMessage(EventId = 3002, EventName = "InboundRateLimitRejected", Level = LogLevel.Warning,
        Message = "Inbound rate limit rejected for policy {Policy}, trace {TraceId}, retry-after present {RetryAfterPresent}")]
    internal static partial void InboundRateLimitRejected(ILogger logger, string policy, string traceId, bool retryAfterPresent);
}
