using System.Text.Json;
using media_vault_app.API.Controllers;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace media_vault_app.API.Diagnostics;

/// <summary>
/// Preserves MediaVault's exact correlated ProblemDetails contracts instead of
/// allowing the framework writer to substitute a different request identifier.
/// </summary>
public sealed class MediaVaultProblemDetailsWriter(
    IOptions<JsonOptions> jsonOptions) : IProblemDetailsWriter
{
    public bool CanWrite(ProblemDetailsContext context) =>
        context.ProblemDetails is MediaVaultAuthorizationProblemDetails ||
        context.ProblemDetails.Status == StatusCodes.Status500InternalServerError &&
        context.ProblemDetails.Title == "An unexpected error occurred." &&
        context.ProblemDetails.Extensions.ContainsKey("traceId");

    public async ValueTask WriteAsync(ProblemDetailsContext context)
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(
            context.HttpContext.Response.Body,
            context.ProblemDetails,
            context.ProblemDetails.GetType(),
            jsonOptions.Value.SerializerOptions,
            context.HttpContext.RequestAborted);
    }
}
