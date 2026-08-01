namespace Rasmus.SharedKernel.Diagnostics;

/// <summary>
/// Identifies the MediaVault component that owns an error log event.
/// </summary>
/// <remarks>
/// Operation, entity, and field diagnostics belong to the ResultPattern error description.
/// This context deliberately remains application-owned so logging can preserve layer, service,
/// and method metadata without adding those concerns to package error types.
/// </remarks>
public sealed record ErrorLogContext
{
    public ErrorLogContext(string layer, string service, string method)
    {
        Layer = RequireValue(layer, nameof(layer));
        Service = RequireValue(service, nameof(service));
        Method = RequireValue(method, nameof(method));
    }

    public string Layer { get; }

    public string Service { get; }

    public string Method { get; }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Log context values cannot be null, empty, or whitespace.", parameterName);

        return value;
    }
}
