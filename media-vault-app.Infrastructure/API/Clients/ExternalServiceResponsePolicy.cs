using System.Net;

namespace media_vault_app.Infrastructure.API.Clients;

/// <summary>
/// MediaVault-owned limits and safe client messages for responses from external services.
/// </summary>
public static class ExternalServiceResponsePolicy
{
    public const int MaxInspectedBodyBytes = 2 * 1024 * 1024;

    public const string TransportFailureMessage = "The external service is currently unavailable.";

    public static string GetSafeUserMessage(HttpStatusCode statusCode) =>
        statusCode switch
        {
            HttpStatusCode.BadRequest => "The external service rejected the request.",
            HttpStatusCode.Unauthorized => "The external service could not authenticate the request.",
            HttpStatusCode.Forbidden => "The external service refused the request.",
            HttpStatusCode.NotFound => "The requested resource was not found in the external service.",
            HttpStatusCode.Conflict => "The external service reported a conflict.",
            HttpStatusCode.UnprocessableContent => "The external service could not process the request.",
            HttpStatusCode.TooManyRequests => "The external service is temporarily rate-limiting requests.",
            HttpStatusCode.InternalServerError => "The external service encountered an internal error.",
            _ => "The external service returned an unexpected response."
        };
}
