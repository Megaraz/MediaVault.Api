using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace media_vault_app.Infrastructure.API.Clients;

public static class ProviderHttpResilience
{
    private static readonly HashSet<HttpStatusCode> RetryableStatusCodes =
    [
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.TooManyRequests,
        HttpStatusCode.InternalServerError,
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.GatewayTimeout
    ];

    public static IServiceCollection AddProviderResilienceOptions(
        this IServiceCollection services,
        IConfiguration configuration,
        int enclosingRequestBudgetMilliseconds)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddLogging(logging =>
            logging.AddFilter("Polly", LogLevel.None));
        services.AddSingleton<IValidateOptions<ProviderResilienceOptions>>(
            new ProviderResilienceOptionsValidator(enclosingRequestBudgetMilliseconds));

        foreach (var provider in ProviderResilienceNames.All)
        {
            services
                .AddOptions<ProviderResilienceOptions>(provider)
                .Bind(configuration.GetSection(ProviderResilienceNames.GetSectionName(provider)))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }

        return services;
    }

    public static IHttpClientBuilder AddMediaVaultProviderResilience(
        this IHttpClientBuilder httpClientBuilder,
        string provider)
    {
        if (!ProviderResilienceNames.All.Contains(provider, StringComparer.Ordinal))
            throw new ArgumentException("Unknown provider resilience policy.", nameof(provider));

        httpClientBuilder.AddResilienceHandler(
            $"{provider}ProviderResilience",
            (pipeline, context) =>
            {
                var options = context.ServiceProvider
                    .GetRequiredService<IOptionsMonitor<ProviderResilienceOptions>>()
                    .Get(provider);
                var timeProvider = context.ServiceProvider.GetRequiredService<TimeProvider>();

                pipeline.TimeProvider = timeProvider;
                pipeline
                    .AddTimeout(TimeSpan.FromMilliseconds(options.TotalTimeoutMilliseconds))
                    .AddRetry(CreateRetryOptions(provider, options, timeProvider))
                    .AddTimeout(TimeSpan.FromMilliseconds(options.AttemptTimeoutMilliseconds));
            });

        return httpClientBuilder;
    }

    private static HttpRetryStrategyOptions CreateRetryOptions(
        string provider,
        ProviderResilienceOptions options,
        TimeProvider timeProvider)
    {
        var retryOptions = new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = options.MaximumRetryAttempts,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Delay = TimeSpan.FromMilliseconds(options.BaseDelayMilliseconds),
            MaxDelay = TimeSpan.FromMilliseconds(options.MaximumDelayMilliseconds),
            ShouldRetryAfterHeader = false,
            ShouldHandle = arguments => new ValueTask<bool>(
                ShouldRetry(arguments, options, timeProvider)),
            DelayGenerator = arguments => new ValueTask<TimeSpan?>(
                GetRetryAfterDelay(arguments.Outcome.Result, timeProvider)),
            OnRetry = arguments =>
            {
                ProviderResilienceTelemetry.RecordRetry(
                    provider,
                    arguments.AttemptNumber + 1,
                    GetFailureKind(arguments.Outcome),
                    arguments.RetryDelay);
                return default;
            }
        };

        retryOptions.DisableForUnsafeHttpMethods();
        return retryOptions;
    }

    private static bool ShouldRetry(
        RetryPredicateArguments<HttpResponseMessage> arguments,
        ProviderResilienceOptions options,
        TimeProvider timeProvider)
    {
        var request = arguments.Context.GetRequestMessage();
        if (request?.Method != HttpMethod.Get)
            return false;

        if (arguments.Outcome.Exception is HttpRequestException or TimeoutRejectedException)
            return true;

        var response = arguments.Outcome.Result;
        if (response is null || !RetryableStatusCodes.Contains(response.StatusCode))
            return false;

        var retryAfter = GetRetryAfterDelay(response, timeProvider);
        return retryAfter is null ||
            retryAfter.Value <= TimeSpan.FromMilliseconds(options.MaximumRetryAfterMilliseconds);
    }

    private static TimeSpan? GetRetryAfterDelay(
        HttpResponseMessage? response,
        TimeProvider timeProvider)
    {
        var retryAfter = response?.Headers.RetryAfter;
        if (retryAfter?.Delta is { } delta)
            return delta < TimeSpan.Zero ? TimeSpan.Zero : delta;

        if (retryAfter?.Date is not { } date)
            return null;

        var dateDelay = date - timeProvider.GetUtcNow();
        return dateDelay < TimeSpan.Zero ? TimeSpan.Zero : dateDelay;
    }

    private static string GetFailureKind(Outcome<HttpResponseMessage> outcome)
    {
        if (outcome.Exception is TimeoutRejectedException)
            return "attempt_timeout";
        if (outcome.Exception is HttpRequestException)
            return "transport";
        if (outcome.Result is { } response)
            return $"http_{(int)response.StatusCode}";

        return "unknown";
    }
}
