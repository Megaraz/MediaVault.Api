using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace media_vault_app.Tests.API;

[Collection(OpenTelemetryIntegrationCollection.Name)]
public sealed class OpenTelemetryTests
{
    private const string DashboardExportEnvironmentVariable =
        "MEDIAVAULT_TEST_OTLP_EXPORT";
    private const string SecretQueryValue = "private-provider-key";

    [Fact]
    public async Task Request_ExportsCorrelatedRedactedTelemetryAndBoundedMetrics()
    {
        await using var upstream = new LoopbackHttpServer();
        await using var factory = new TelemetryFactory(upstream.BaseAddress, captureInMemory: true);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/_test/telemetry/success");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tracerProvider = factory.Services.GetRequiredService<TracerProvider>();
        var meterProvider = factory.Services.GetRequiredService<MeterProvider>();
        Assert.True(tracerProvider.ForceFlush(5_000));
        Assert.True(meterProvider.ForceFlush(5_000));

        var incoming = Assert.Single(
            factory.Activities,
            activity => activity.DisplayName == "GET _test/telemetry/success");
        var outgoing = Assert.Single(
            factory.Activities,
            activity => activity.DisplayName == "GET");
        var exportedLog = Assert.Single(
            factory.Logs,
            record => record.EventId.Id == 3999);

        Assert.Equal(incoming.TraceId, outgoing.TraceId);
        Assert.Equal(ActivityKind.Client, outgoing.Kind);
        Assert.Equal(incoming.TraceId, exportedLog.TraceId);
        Assert.Equal(incoming.SpanId, outgoing.ParentSpanId);
        Assert.Equal("media_vault_app.API.Program", exportedLog.CategoryName);
        Assert.Equal(LogLevel.Warning, exportedLog.LogLevel);

        var outboundUrl = outgoing.GetTagItem("url.full")?.ToString();
        Assert.EndsWith("?*", outboundUrl, StringComparison.Ordinal);
        Assert.DoesNotContain(SecretQueryValue, outboundUrl, StringComparison.Ordinal);
        Assert.DoesNotContain(
            SecretQueryValue,
            exportedLog.Attributes?.Select(attribute => attribute.Value?.ToString()) ?? []);

        Assert.Contains(factory.Metrics, metric =>
            metric.Name == "http.server.request.duration");
        Assert.Contains(factory.Metrics, metric =>
            metric.Name == "http.client.request.duration");
        Assert.Contains(factory.Metrics, metric =>
            metric.MeterName == "System.Runtime");

        var requestMetric = Assert.Single(factory.Metrics, metric =>
            metric.Name == "http.server.request.duration");
        var requestTagKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (ref readonly var point in requestMetric.GetMetricPoints())
        {
            foreach (var tag in point.Tags)
                requestTagKeys.Add(tag.Key);
        }
        Assert.DoesNotContain("user.id", requestTagKeys);
        Assert.DoesNotContain("media.id", requestTagKeys);

        var resource = tracerProvider.GetResource();
        Assert.Contains(resource.Attributes, attribute =>
            attribute.Key == "service.name" &&
            Equals(attribute.Value, "MediaVault.TelemetryTests"));
        Assert.Contains(resource.Attributes, attribute =>
            attribute.Key == "service.version" &&
            Equals(attribute.Value, "9.8.7"));
        Assert.Contains(resource.Attributes, attribute =>
            attribute.Key == "deployment.environment.name" &&
            Equals(attribute.Value, "TelemetryTests"));
    }

    [Fact]
    public async Task UnreachableOtlpReceiver_DoesNotChangeRequestOutcome()
    {
        await using var upstream = new LoopbackHttpServer();
        await using var factory = new TelemetryFactory(
            upstream.BaseAddress,
            captureInMemory: false,
            enableOtlp: true);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/_test/telemetry/success");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        factory.Services.GetRequiredService<TracerProvider>().ForceFlush(5_000);
        factory.Services.GetRequiredService<MeterProvider>().ForceFlush(5_000);
    }

    [Fact]
    public async Task ExpectedFailure_ExportsRequestTraceWithoutUnhandledEvent()
    {
        await using var upstream = new LoopbackHttpServer();
        await using var factory = new TelemetryFactory(upstream.BaseAddress, captureInMemory: true);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/_test/telemetry/expected");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.True(factory.Services.GetRequiredService<TracerProvider>().ForceFlush(5_000));
        Assert.Contains(factory.Activities, activity =>
            activity.DisplayName == "GET _test/telemetry/expected");
        Assert.DoesNotContain(factory.Logs, record => record.EventId.Id == 3000);
    }

    [Fact]
    public async Task OutboundFailure_RemainsCorrelatedAndRecordsSafeHttpShape()
    {
        await using var upstream = new LoopbackHttpServer(HttpStatusCode.ServiceUnavailable);
        await using var factory = new TelemetryFactory(upstream.BaseAddress, captureInMemory: true);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/_test/telemetry/success");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(factory.Services.GetRequiredService<TracerProvider>().ForceFlush(5_000));

        var incoming = Assert.Single(factory.Activities, activity =>
            activity.DisplayName == "GET _test/telemetry/success");
        var outgoing = Assert.Single(factory.Activities, activity =>
            activity.DisplayName == "GET");

        Assert.Equal(incoming.TraceId, outgoing.TraceId);
        Assert.Equal(ActivityStatusCode.Error, outgoing.Status);
        Assert.Equal(
            (int)HttpStatusCode.ServiceUnavailable,
            Convert.ToInt32(outgoing.GetTagItem("http.response.status_code")));
        Assert.DoesNotContain(
            SecretQueryValue,
            outgoing.GetTagItem("url.full")?.ToString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnexpectedException_ExportsOwnedRedactedLogOnRequestTrace()
    {
        await using var upstream = new LoopbackHttpServer();
        await using var factory = new TelemetryFactory(upstream.BaseAddress, captureInMemory: true);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/_test/telemetry/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.True(factory.Services.GetRequiredService<TracerProvider>().ForceFlush(5_000));

        var incoming = Assert.Single(factory.Activities, activity =>
            activity.DisplayName == "GET _test/telemetry/throw");
        var exportedLog = Assert.Single(factory.Logs, record => record.EventId.Id == 3000);

        Assert.Equal(incoming.TraceId, exportedLog.TraceId);
        Assert.Null(exportedLog.Exception);
        Assert.DoesNotContain(
            "private SQL password=super-secret upstream-body",
            exportedLog.Body,
            StringComparison.Ordinal);
    }

    private sealed class TelemetryFactory(
        Uri upstreamBaseAddress,
        bool captureInMemory,
        bool enableOtlp = false)
        : WebApplicationFactory<media_vault_app.API.Program>
    {
        public List<Activity> Activities { get; } = [];
        public List<LogRecord> Logs { get; } = [];
        public List<Metric> Metrics { get; } = [];

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var exportToConfiguredOtlp =
                !enableOtlp &&
                string.Equals(
                    Environment.GetEnvironmentVariable(DashboardExportEnvironmentVariable),
                    "true",
                    StringComparison.OrdinalIgnoreCase);

            builder.UseEnvironment("TelemetryTests");
            builder.UseSetting("ConnectionStrings:Default", "Data Source=:memory:");
            builder.UseSetting("ExternalApis:Rawg:BaseUrl", "https://rawg.test/");
            builder.UseSetting("ExternalApis:Rawg:ApiKey", "test-key");
            builder.UseSetting("ExternalApis:Tmdb:BaseUrl", "https://tmdb.test/");
            builder.UseSetting("ExternalApis:Tmdb:ApiAccessToken", "test-token");
            builder.UseSetting("ExternalApis:GoogleBooks:BaseUrl", "https://books.test/");
            builder.UseSetting("ExternalApis:GoogleBooks:ApiKey", "test-key");
            builder.UseSetting("Jwt:SecretKey", "integration-test-signing-key-at-least-32-bytes");
            builder.UseSetting("Jwt:Issuer", "MediaVault.Tests");
            builder.UseSetting("Jwt:Audience", "MediaVault.Tests");
            builder.UseSetting("OpenTelemetry:Enabled", "true");
            builder.UseSetting(
                "OpenTelemetry:OtlpExporterEnabled",
                (enableOtlp || exportToConfiguredOtlp).ToString());
            builder.UseSetting("OpenTelemetry:ServiceName", "MediaVault.TelemetryTests");
            builder.UseSetting("OpenTelemetry:ServiceVersion", "9.8.7");
            builder.UseSetting("OpenTelemetry:Environment", "TelemetryTests");
            builder.UseSetting("OpenTelemetry:TraceSamplingRatio", "1");

            if (enableOtlp)
            {
                builder.UseSetting("OTEL_EXPORTER_OTLP_ENDPOINT", "http://127.0.0.1:1");
                builder.UseSetting("OTEL_EXPORTER_OTLP_PROTOCOL", "grpc");
                builder.UseSetting("OTEL_EXPORTER_OTLP_TIMEOUT", "100");
            }

            if (captureInMemory)
            {
                builder.ConfigureLogging(logging =>
                    logging.AddOpenTelemetry(options =>
                        options.AddInMemoryExporter(Logs)));
            }

            builder.ConfigureServices(services =>
            {
                services.AddControllers().AddApplicationPart(typeof(TelemetryTestController).Assembly);
                services
                    .AddHttpClient("telemetry-test")
                    .ConfigureHttpClient(client => client.BaseAddress = upstreamBaseAddress);

                if (captureInMemory)
                {
                    services.ConfigureOpenTelemetryTracerProvider(tracing =>
                        tracing.AddInMemoryExporter(Activities));
                    services.ConfigureOpenTelemetryMeterProvider(metrics =>
                        metrics.AddInMemoryExporter(Metrics));
                }
            });
        }
    }

    private sealed class LoopbackHttpServer : IAsyncDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly HttpStatusCode _statusCode;
        private readonly Task _responseTask;

        public LoopbackHttpServer(HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _statusCode = statusCode;
            _listener.Start();
            var endpoint = (IPEndPoint)_listener.LocalEndpoint;
            BaseAddress = new Uri($"http://127.0.0.1:{endpoint.Port}/");
            _responseTask = RespondOnceAsync();
        }

        public Uri BaseAddress { get; }

        public async ValueTask DisposeAsync()
        {
            _listener.Stop();
            try
            {
                await _responseTask;
            }
            catch (SocketException)
            {
                // Disposal may stop a listener before a request reaches it.
            }
            catch (ObjectDisposedException)
            {
                // Disposal may stop a listener before a request reaches it.
            }
        }

        private async Task RespondOnceAsync()
        {
            using var client = await _listener.AcceptTcpClientAsync();
            await using var stream = client.GetStream();
            var requestBuffer = new byte[4_096];
            await stream.ReadAtLeastAsync(requestBuffer, minimumBytes: 1);

            var response = Encoding.ASCII.GetBytes(
                $"HTTP/1.1 {(int)_statusCode} {_statusCode}\r\n" +
                "Content-Length: 0\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(response);
        }
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class OpenTelemetryIntegrationCollection
{
    public const string Name = "OpenTelemetry integration";
}

[ApiController]
[AllowAnonymous]
[Route("_test/telemetry")]
public sealed class TelemetryTestController(
    IHttpClientFactory clientFactory,
    ILogger<media_vault_app.API.Program> logger) : ControllerBase
{
    [HttpGet("success")]
    public async Task<IActionResult> Success(CancellationToken cancellationToken)
    {
        using var client = clientFactory.CreateClient("telemetry-test");
        using var response = await client.GetAsync(
            "items?api_key=private-provider-key",
            cancellationToken);

        logger.LogWarning(
            new EventId(3999, "TelemetryTestWarning"),
            "Telemetry test request completed for {Provider}",
            "TestProvider");

        return Ok(new { upstreamStatus = (int)response.StatusCode });
    }

    [HttpGet("expected")]
    public IActionResult Expected() =>
        NotFound(new { message = "Expected telemetry test failure." });

    [HttpGet("throw")]
    public IActionResult Throw() =>
        throw new InvalidOperationException(
            "private SQL password=super-secret upstream-body");
}
