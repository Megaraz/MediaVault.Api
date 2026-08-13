using System.ComponentModel.DataAnnotations;

namespace media_vault_app.API.Observability;

public sealed class TelemetryOptions
{
    public const string SectionName = "OpenTelemetry";

    public bool Enabled { get; init; } = true;

    public bool OtlpExporterEnabled { get; init; }

    [Required]
    public string ServiceName { get; init; } = "MediaVault.API";

    public string? ServiceVersion { get; init; }

    public string? Environment { get; init; }

    [Range(0d, 1d)]
    public double TraceSamplingRatio { get; init; } = 0.1d;
}
