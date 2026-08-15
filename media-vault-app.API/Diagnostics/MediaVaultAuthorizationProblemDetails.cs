using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Diagnostics;

/// <summary>
/// The safe ProblemDetails contract returned by the authentication boundary.
/// </summary>
public sealed class MediaVaultAuthorizationProblemDetails : ProblemDetails
{
    public string TraceId { get; init; } = string.Empty;
}
