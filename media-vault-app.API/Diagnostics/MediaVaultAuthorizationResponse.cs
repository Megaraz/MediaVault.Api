using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace media_vault_app.API.Diagnostics;

internal static class MediaVaultAuthorizationResponse
{
    private const string UnauthorizedType =
        "https://www.rfc-editor.org/rfc/rfc9110.html#name-401-unauthorized";
    private const string ForbiddenType =
        "https://www.rfc-editor.org/rfc/rfc9110.html#name-403-forbidden";
    private const string UnauthorizedTitle = "Authentication required.";
    private const string UnauthorizedDetail =
        "A valid bearer token is required to access this resource.";
    private const string ForbiddenTitle = "Forbidden.";
    private const string ForbiddenDetail =
        "You do not have permission to access this resource.";

    public static async Task WriteChallengeAsync(JwtBearerChallengeContext context)
    {
        context.HandleResponse();
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers.WWWAuthenticate = JwtBearerDefaults.AuthenticationScheme;

        await WriteAsync(
            context.HttpContext,
            StatusCodes.Status401Unauthorized,
            UnauthorizedType,
            UnauthorizedTitle,
            UnauthorizedDetail);
    }

    public static Task WriteForbiddenAsync(ForbiddenContext context) =>
        WriteAsync(
            context.HttpContext,
            StatusCodes.Status403Forbidden,
            ForbiddenType,
            ForbiddenTitle,
            ForbiddenDetail);

    private static async Task WriteAsync(
        HttpContext context,
        int statusCode,
        string type,
        string title,
        string detail)
    {
        context.Response.StatusCode = statusCode;

        var problemDetails = new MediaVaultAuthorizationProblemDetails
        {
            Type = type,
            Title = title,
            Status = statusCode,
            Detail = detail,
            TraceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier
        };

        var problemDetailsService =
            context.RequestServices.GetRequiredService<IProblemDetailsService>();
        var written = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problemDetails
        });

        if (!written)
            throw new InvalidOperationException(
                "The MediaVault authorization ProblemDetails response could not be written.");
    }
}
