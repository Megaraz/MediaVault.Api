using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.Errors;

/// <summary>
/// Creates core ResultPattern errors with MediaVault's stable, presentation-safe messages.
/// </summary>
public static class MediaVaultErrors
{
    public static Error NotFound(ErrorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var message = $"{context.EntityName} not found";
        return Error.NotFound(context, userMessage: message);
    }

    public static Error Conflict(ErrorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var message = $"Unique {context.EntityName} constraint violated.";
        return Error.Conflict(context, userMessage: message);
    }

    public static Error Unauthorized(ErrorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var message = "Unauthorized access" +
            (string.IsNullOrWhiteSpace(context.FieldName) ? string.Empty : $" to {context.FieldName}");
        return Error.Unauthorized(context, userMessage: message);
    }

    public static Error Failure(
        ErrorContext context,
        string? description = null,
        Exception? exception = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        var message = string.IsNullOrWhiteSpace(description)
            ? $"An unexpected failure occurred while processing {context.EntityName}."
            : description;

        return Error.Failure(context, description, exception, userMessage: message);
    }

    public static Error Cancelled(ErrorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var message = $"The operation on {context.EntityName} was cancelled.";
        return Error.Cancelled(context, userMessage: message);
    }
}
