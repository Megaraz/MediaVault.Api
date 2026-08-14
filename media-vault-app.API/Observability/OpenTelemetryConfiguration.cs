using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace media_vault_app.API.Observability;

public static class OpenTelemetryConfiguration
{
    private const string DeploymentEnvironmentAttribute = "deployment.environment.name";

    public static WebApplicationBuilder AddMediaVaultOpenTelemetry(
        this WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection(TelemetryOptions.SectionName);

        builder.Services
            .AddOptions<TelemetryOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .Validate(
                options => !options.OtlpExporterEnabled || options.Enabled,
                "OTLP export requires OpenTelemetry to be enabled.")
            .ValidateOnStart();

        var options = section.Get<TelemetryOptions>() ?? new TelemetryOptions();
        if (!options.Enabled)
            return builder;

        var serviceVersion = string.IsNullOrWhiteSpace(options.ServiceVersion)
            ? typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "unknown"
            : options.ServiceVersion;
        var environment = string.IsNullOrWhiteSpace(options.Environment)
            ? builder.Environment.EnvironmentName
            : options.Environment;

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(options.ServiceName, serviceVersion: serviceVersion)
                .AddAttributes([
                    new KeyValuePair<string, object>(DeploymentEnvironmentAttribute, environment)
                ]))
            .WithTracing(tracing =>
            {
                tracing
                    .SetSampler(new ParentBasedSampler(
                        new TraceIdRatioBasedSampler(options.TraceSamplingRatio)))
                    .AddAspNetCoreInstrumentation(instrumentation =>
                        instrumentation.RecordException = false)
                    .AddHttpClientInstrumentation(instrumentation =>
                        instrumentation.RecordException = false);

                if (options.OtlpExporterEnabled)
                    tracing.AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("MediaVault.Api.RequestTimeouts");

                if (options.OtlpExporterEnabled)
                    metrics.AddOtlpExporter();
            });

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeScopes = true;
            logging.IncludeFormattedMessage = false;
            logging.ParseStateValues = true;

            if (options.OtlpExporterEnabled)
                logging.AddOtlpExporter();
        });
        builder.Logging.AddFilter<OpenTelemetryLoggerProvider>(IsApprovedExportLog);

        return builder;
    }

    internal static bool IsApprovedExportLog(string? category, LogLevel level) =>
        level >= LogLevel.Warning &&
        category?.StartsWith("media_vault_app.", StringComparison.Ordinal) == true;
}
