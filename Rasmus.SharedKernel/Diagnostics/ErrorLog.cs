namespace Rasmus.SharedKernel.Diagnostics;

/// <summary>
/// A persisted MediaVault error-log entry.
/// </summary>
public sealed record ErrorLog(
    DateTimeOffset WriteDate,
    string Code,
    string Description,
    string ErrorType,
    string Layer,
    string Service,
    string Method,
    string? ExceptionMessage,
    string? StackTrace);
