namespace media_vault_app.API.Controllers;

/// <summary>MediaVault's stable ordinary error response contract.</summary>
public sealed record ErrorResponseBody(string Message, string Code);

/// <summary>MediaVault's stable field-level validation error contract.</summary>
public sealed record ValidationErrorItem(string? Field, string Message);

/// <summary>MediaVault's stable validation error response contract.</summary>
public sealed record ValidationErrorResponseBody(
    string Message,
    IEnumerable<ValidationErrorItem>? ValidationErrors);
