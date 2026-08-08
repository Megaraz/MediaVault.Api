using Megaraz.ResultPattern;

namespace media_vault_app.Infrastructure.Diagnostics;

/// <summary>
/// Supplies safe, structured ownership data for a MediaVault diagnostic event.
/// </summary>
public sealed record ErrorEventContext
{
    public ErrorEventContext(
        string layer,
        string service,
        string method,
        ErrorContext errorContext,
        string? provider = null,
        string? failureKind = null,
        int? statusCode = null)
    {
        Layer = RequireValue(layer, nameof(layer));
        Service = RequireValue(service, nameof(service));
        Method = RequireValue(method, nameof(method));
        ErrorContext = errorContext ?? throw new ArgumentNullException(nameof(errorContext));
        Provider = provider;
        FailureKind = failureKind;
        StatusCode = statusCode;
    }

    public string Layer { get; }

    public string Service { get; }

    public string Method { get; }

    public ErrorContext ErrorContext { get; }

    public string? Provider { get; }

    public string? FailureKind { get; }

    public int? StatusCode { get; }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Event context values cannot be null, empty, or whitespace.", parameterName);

        return value;
    }
}
