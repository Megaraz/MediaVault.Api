using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using media_vault_app.API.Controllers;
using media_vault_app.Infrastructure.API.Clients;
using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rasmus.SharedKernel.ExternalServices;

namespace media_vault_app.Tests.Infrastructure.API.Clients;

public sealed class ProviderHttpResilienceTests
{
    private const string Provider = ProviderResilienceNames.Rawg;

    [Theory]
    [MemberData(nameof(InvalidOptions))]
    public void OptionsValidator_RejectsUnsafeOrUnboundedConfiguration(
        ProviderResilienceOptions options,
        string expectedFailure)
    {
        var result = new ProviderResilienceOptionsValidator(20_000)
            .Validate(Provider, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures ?? [], failure =>
            failure.Contains(expectedFailure, StringComparison.Ordinal));
    }

    [Fact]
    public void OptionsValidator_AcceptsApprovedDefaults()
    {
        var result = new ProviderResilienceOptionsValidator(20_000)
            .Validate(Provider, CreateOptions());

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void AllProviderNames_BindTheApprovedValidatedDefaults()
    {
        var values = new Dictionary<string, string?>();
        foreach (var provider in ProviderResilienceNames.All)
        {
            var prefix = ProviderResilienceNames.GetSectionName(provider);
            values[$"{prefix}:AttemptTimeoutMilliseconds"] = "5000";
            values[$"{prefix}:TotalTimeoutMilliseconds"] = "12000";
            values[$"{prefix}:MaximumRetryAttempts"] = "1";
            values[$"{prefix}:BaseDelayMilliseconds"] = "500";
            values[$"{prefix}:MaximumDelayMilliseconds"] = "1000";
            values[$"{prefix}:MaximumRetryAfterMilliseconds"] = "2000";
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        var services = new ServiceCollection();
        services.AddProviderResilienceOptions(configuration, 20_000);
        using var serviceProvider = services.BuildServiceProvider();
        var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<ProviderResilienceOptions>>();
        var logging = serviceProvider.GetRequiredService<IOptions<LoggerFilterOptions>>().Value;

        Assert.Contains(logging.Rules, rule =>
            rule.CategoryName == "Polly" && rule.LogLevel == LogLevel.None);

        foreach (var provider in ProviderResilienceNames.All)
        {
            var options = monitor.Get(provider);
            Assert.Equal(5_000, options.AttemptTimeoutMilliseconds);
            Assert.Equal(12_000, options.TotalTimeoutMilliseconds);
            Assert.Equal(1, options.MaximumRetryAttempts);
            Assert.Equal(500, options.BaseDelayMilliseconds);
            Assert.Equal(1_000, options.MaximumDelayMilliseconds);
            Assert.Equal(2_000, options.MaximumRetryAfterMilliseconds);
        }
    }

    [Fact]
    public async Task FirstAttemptSuccess_DoesNotRetry()
    {
        using var handler = new RecordingHandler((_, _, _) =>
            Task.FromResult(JsonResponse(HttpStatusCode.OK, "{}")));
        using var scope = CreateClient(handler);

        using var response = await scope.Client.GetAsync("items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, handler.Attempts);
    }

    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public async Task ApprovedTransientGet_RetriesOnceThenSucceeds(HttpStatusCode statusCode)
    {
        using var handler = new RecordingHandler((attempt, _, _) => Task.FromResult(
            attempt == 1
                ? JsonResponse(statusCode, "{}")
                : JsonResponse(HttpStatusCode.OK, "{}")));
        using var scope = CreateClient(handler);

        using var response = await scope.Client.GetAsync("items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.Attempts);
    }

    [Fact]
    public async Task TransportFailure_RetriesOnceThenSucceeds()
    {
        using var handler = new RecordingHandler((attempt, _, _) =>
            attempt == 1
                ? Task.FromException<HttpResponseMessage>(new HttpRequestException("private transport detail"))
                : Task.FromResult(JsonResponse(HttpStatusCode.OK, "{}")));
        using var scope = CreateClient(handler);

        using var response = await scope.Client.GetAsync("items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.Attempts);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.UnprocessableContent)]
    public async Task NonRetryableResponse_IsReturnedImmediately(HttpStatusCode statusCode)
    {
        using var handler = new RecordingHandler((_, _, _) =>
            Task.FromResult(JsonResponse(statusCode, "{}")));
        using var scope = CreateClient(handler);

        using var response = await scope.Client.GetAsync("items");

        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task UnsafeMethod_IsNeverRetried()
    {
        using var handler = new RecordingHandler((_, _, _) =>
            Task.FromResult(JsonResponse(HttpStatusCode.ServiceUnavailable, "{}")));
        using var scope = CreateClient(handler);

        using var response = await scope.Client.PostAsync(
            "items",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task AcceptedRetryAfter_IsHonoredBeforeRetry()
    {
        var stopwatch = Stopwatch.StartNew();
        var secondAttemptAt = TimeSpan.Zero;
        using var handler = new RecordingHandler((attempt, _, _) =>
        {
            if (attempt == 1)
            {
                var response = JsonResponse(HttpStatusCode.TooManyRequests, "{}");
                response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMilliseconds(40));
                return Task.FromResult(response);
            }

            secondAttemptAt = stopwatch.Elapsed;
            return Task.FromResult(JsonResponse(HttpStatusCode.OK, "{}"));
        });
        using var scope = CreateClient(handler, CreateOptions(maximumRetryAfterMilliseconds: 50));

        using var response = await scope.Client.GetAsync("items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.Attempts);
        Assert.True(secondAttemptAt >= TimeSpan.FromMilliseconds(25), secondAttemptAt.ToString());
    }

    [Fact]
    public async Task RetryAfterAboveCap_ReturnsFinalResponseWithoutRetryOrLongWait()
    {
        using var handler = new RecordingHandler((_, _, _) =>
        {
            var response = JsonResponse(HttpStatusCode.TooManyRequests, "{}");
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(30));
            return Task.FromResult(response);
        });
        using var scope = CreateClient(handler);

        using var response = await scope.Client.GetAsync("items");

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task AttemptTimeout_RetriesOnceThenSucceedsAndRecordsBoundedTelemetry()
    {
        var measurements = new List<RetryMeasurement>();
        using var meterListener = ListenForRetries(measurements);
        using var pipelineLogs = new RecordingLoggerProvider();
        using var handler = new RecordingHandler(async (attempt, _, cancellationToken) =>
        {
            if (attempt == 1)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                throw new InvalidOperationException("Unreachable after cancellation.");
            }

            return JsonResponse(HttpStatusCode.OK, "{}");
        });
        using var scope = CreateClient(
            handler,
            CreateOptions(
                attemptTimeoutMilliseconds: 25,
                totalTimeoutMilliseconds: 100),
            pipelineLogs);

        using var response = await scope.Client.GetAsync("items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.Attempts);
        var retry = Assert.Single(measurements, measurement =>
            measurement.InstrumentName == "mediavault.external_provider.retries");
        Assert.Equal(1, retry.Value);
        Assert.Equal(Provider, retry.Tags["provider"]);
        Assert.Equal(1, retry.Tags["attempt"]);
        Assert.Equal("attempt_timeout", retry.Tags["failure.kind"]);
        Assert.Equal("retry", retry.Tags["outcome"]);
        Assert.DoesNotContain(retry.Tags.Values, value =>
            value?.ToString()?.Contains("items", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(pipelineLogs.Entries, entry =>
            entry.Level >= LogLevel.Warning);
    }

    [Fact]
    public async Task CallerCancellation_StopsImmediatelyWithoutRetryTelemetry()
    {
        var measurements = new List<RetryMeasurement>();
        using var meterListener = ListenForRetries(measurements);
        using var handler = new RecordingHandler(async (_, _, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return JsonResponse(HttpStatusCode.OK, "{}");
        });
        using var scope = CreateClient(handler);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            scope.Client.GetAsync("items", cancellation.Token));

        Assert.InRange(handler.Attempts, 0, 1);
        Assert.DoesNotContain(measurements, measurement =>
            measurement.InstrumentName == "mediavault.external_provider.retries");
    }

    [Fact]
    public async Task RetryExhaustion_MapsSafeResultAndEmitsExactlyOneFinalFailureEvent()
    {
        const string privateBody = "private provider detail";
        using var logs = new RecordingLoggerProvider();
        using var handler = new RecordingHandler((_, _, _) =>
            Task.FromResult(JsonResponse(
                HttpStatusCode.ServiceUnavailable,
                $$"""{"message":"{{privateBody}}"}""")));
        using var scope = CreateClient(handler, loggerProvider: logs);
        var providerClient = new RawgApiClient(
            scope.Client,
            Options.Create(new RawgApiOptions
            {
                BaseUrl = "https://provider.test/",
                ApiKey = "private-key"
            }),
            CreateErrorEventLogger<RawgApiClient>(logs));

        var result = await providerClient.SearchGamesAsync(["search=game"]);

        Assert.True(result.IsFailure);
        Assert.Equal(2, handler.Attempts);
        Assert.Equal(ExternalServiceResponsePolicy.GetSafeUserMessage(HttpStatusCode.ServiceUnavailable), result.Message);
        Assert.DoesNotContain(privateBody, result.Message, StringComparison.Ordinal);
        var action = ResultResponseMapper.ToActionResult(new TestController(), result);
        var objectResult = Assert.IsType<ObjectResult>(action.Result);
        Assert.Equal(StatusCodes.Status502BadGateway, objectResult.StatusCode);
        var body = Assert.IsType<ErrorResponseBody>(objectResult.Value);
        Assert.Equal(result.Message, body.Message);
        Assert.DoesNotContain(privateBody, body.Message, StringComparison.Ordinal);
        var entry = Assert.Single(logs.Entries, entry => entry.Level >= LogLevel.Warning);
        Assert.Equal(2102, entry.EventId.Id);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.DoesNotContain(privateBody, entry.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("private-key", entry.Properties.Values.OfType<string>());
    }

    [Fact]
    public async Task AttemptTimeoutExhaustion_MapsSafeResultAndLogsOnce()
    {
        using var logs = new RecordingLoggerProvider();
        using var handler = new RecordingHandler(async (_, _, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return JsonResponse(HttpStatusCode.OK, "{}");
        });
        using var scope = CreateClient(
            handler,
            CreateOptions(
                attemptTimeoutMilliseconds: 25,
                totalTimeoutMilliseconds: 100),
            logs);
        var providerClient = new RawgApiClient(
            scope.Client,
            Options.Create(new RawgApiOptions
            {
                BaseUrl = "https://provider.test/",
                ApiKey = "private-key"
            }),
            CreateErrorEventLogger<RawgApiClient>(logs));

        var result = await providerClient.SearchGamesAsync(["search=game"]);

        Assert.True(result.IsFailure);
        Assert.Equal(2, handler.Attempts);
        Assert.Equal(ExternalServiceResponsePolicy.TransportFailureMessage, result.Message);
        var entry = Assert.Single(logs.Entries, entry => entry.Level >= LogLevel.Warning);
        Assert.Equal(2100, entry.EventId.Id);
        Assert.Equal(LogLevel.Warning, entry.Level);
    }

    public static IEnumerable<object[]> InvalidOptions()
    {
        yield return [CreateOptions(attemptTimeoutMilliseconds: 0), "positive"];
        yield return [CreateOptions(maximumRetryAttempts: 2), "single retry"];
        yield return [CreateOptions(
            attemptTimeoutMilliseconds: 13_000,
            totalTimeoutMilliseconds: 12_000), "Attempt timeout"];
        yield return [CreateOptions(
            baseDelayMilliseconds: 1_001,
            maximumDelayMilliseconds: 1_000), "Base delay"];
        yield return [CreateOptions(
            attemptTimeoutMilliseconds: 5_000,
            totalTimeoutMilliseconds: 11_999,
            maximumRetryAfterMilliseconds: 2_000), "All attempts"];
        yield return [CreateOptions(totalTimeoutMilliseconds: 20_000), "enclosing"];
    }

    private static ProviderResilienceOptions CreateOptions(
        int attemptTimeoutMilliseconds = 1_000,
        int totalTimeoutMilliseconds = 2_100,
        int maximumRetryAttempts = 1,
        int baseDelayMilliseconds = 1,
        int maximumDelayMilliseconds = 2,
        int maximumRetryAfterMilliseconds = 2) =>
        new()
        {
            AttemptTimeoutMilliseconds = attemptTimeoutMilliseconds,
            TotalTimeoutMilliseconds = totalTimeoutMilliseconds,
            MaximumRetryAttempts = maximumRetryAttempts,
            BaseDelayMilliseconds = baseDelayMilliseconds,
            MaximumDelayMilliseconds = maximumDelayMilliseconds,
            MaximumRetryAfterMilliseconds = maximumRetryAfterMilliseconds
        };

    private static ClientScope CreateClient(
        RecordingHandler handler,
        ProviderResilienceOptions? options = null,
        RecordingLoggerProvider? loggerProvider = null)
    {
        options ??= CreateOptions();
        var services = new ServiceCollection();
        services.AddLogging(logging =>
        {
            logging.AddFilter("Polly", LogLevel.None);
            if (loggerProvider is not null)
                logging.AddProvider(loggerProvider);
        });
        services.AddSingleton(TimeProvider.System);
        services
            .AddOptions<ProviderResilienceOptions>(Provider)
            .Configure(configured =>
            {
                configured.AttemptTimeoutMilliseconds = options.AttemptTimeoutMilliseconds;
                configured.TotalTimeoutMilliseconds = options.TotalTimeoutMilliseconds;
                configured.MaximumRetryAttempts = options.MaximumRetryAttempts;
                configured.BaseDelayMilliseconds = options.BaseDelayMilliseconds;
                configured.MaximumDelayMilliseconds = options.MaximumDelayMilliseconds;
                configured.MaximumRetryAfterMilliseconds = options.MaximumRetryAfterMilliseconds;
            });
        services
            .AddHttpClient("provider-test", client =>
            {
                client.BaseAddress = new Uri("https://provider.test/");
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddMediaVaultProviderResilience(Provider);

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient("provider-test");
        return new ClientScope(client, serviceProvider);
    }

    private static MeterListener ListenForRetries(List<RetryMeasurement> measurements)
    {
        var listener = new MeterListener
        {
            InstrumentPublished = (instrument, currentListener) =>
            {
                if (instrument.Meter.Name == ProviderResilienceTelemetry.MeterName)
                    currentListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
        {
            var capturedTags = tags.ToArray().ToDictionary(tag => tag.Key, tag => tag.Value);
            measurements.Add(new RetryMeasurement(instrument.Name, value, capturedTags));
        });
        listener.Start();
        return listener;
    }

    private static ErrorEventLogger<TCategory> CreateErrorEventLogger<TCategory>(
        RecordingLoggerProvider provider)
        where TCategory : class =>
        new(
            provider.CreateLogger<TCategory>(),
            new ErrorEventPolicy(),
            new ErrorDiagnosticsOptions(false));

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string body) =>
        new(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private sealed record RetryMeasurement(
        string InstrumentName,
        long Value,
        IReadOnlyDictionary<string, object?> Tags);

    private sealed class TestController : ControllerBase;

    private sealed class ClientScope(HttpClient client, ServiceProvider serviceProvider) : IDisposable
    {
        public HttpClient Client { get; } = client;

        public void Dispose()
        {
            Client.Dispose();
            serviceProvider.Dispose();
        }
    }

    private sealed class RecordingHandler(
        Func<int, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        : HttpMessageHandler
    {
        private int _attempts;

        public int Attempts => Volatile.Read(ref _attempts);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var attempt = Interlocked.Increment(ref _attempts);
            return sendAsync(attempt, request, cancellationToken);
        }
    }
}
