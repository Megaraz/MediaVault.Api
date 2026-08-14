using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace media_vault_app.Infrastructure.API.Clients;

public static class ProviderResilienceTelemetry
{
    public const string MeterName = "MediaVault.Infrastructure.ProviderResilience";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> RetryCounter = Meter.CreateCounter<long>(
        "mediavault.external_provider.retries",
        description: "Number of bounded outbound provider retry attempts.");
    private static readonly Histogram<double> RetryDelay = Meter.CreateHistogram<double>(
        "mediavault.external_provider.retry_delay",
        unit: "ms",
        description: "Selected delay before a bounded outbound provider retry.");

    public static void RecordRetry(
        string provider,
        int attemptNumber,
        string failureKind,
        TimeSpan delay)
    {
        TagList tags = new()
        {
            { "provider", provider },
            { "attempt", attemptNumber },
            { "failure.kind", failureKind },
            { "outcome", "retry" }
        };

        RetryCounter.Add(1, tags);
        RetryDelay.Record(delay.TotalMilliseconds, tags);
    }
}
