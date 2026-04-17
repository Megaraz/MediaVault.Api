using System.Net;
using System.Text.Json;

namespace Rasmus.SharedKernel.ResultPattern
{
    public static class HttpResponseToResultExtensions
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

        public static async Task<Result<TValue>> MapAsync<TValue>(this HttpResponseMessage? response, ErrorContext errorContext, CancellationToken ct = default)
        {
            var localErrorContext = CloneErrorContext(errorContext);

            if (response is null)
                return CreateFailureResult<TValue>(localErrorContext, CreateTransportFailureMessage());

            if (!response.IsSuccessStatusCode)
            {
                var failureMessage = await GetFailureMessageAsync(response, localErrorContext, ct);
                return CreateHttpFailureResult<TValue>(response.StatusCode, localErrorContext, failureMessage);
            }

            var responseBody = await ReadResponseBodyAsync(response, ct);
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return CreateFailureResult<TValue>(
                    localErrorContext,
                    $"The external service returned {(int)response.StatusCode} ({response.StatusCode}) without the expected response body.");
            }

            if (!HasJsonContentType(response))
            {
                return CreateFailureResult<TValue>(
                    localErrorContext,
                    $"The external service returned {(int)response.StatusCode} ({response.StatusCode}) with unsupported content type '{response.Content.Headers.ContentType?.MediaType ?? "unknown"}'.");
            }

            try
            {
                var value = JsonSerializer.Deserialize<TValue>(responseBody, JsonSerializerOptions);

                if (value is null)
                {
                    return CreateFailureResult<TValue>(
                        localErrorContext,
                        $"The external service returned {(int)response.StatusCode} ({response.StatusCode}) with an empty or invalid JSON body.");
                }

                return Result<TValue>.Success(value);
            }
            catch (JsonException exception)
            {
                return CreateFailureResult<TValue>(localErrorContext, "The external service returned malformed JSON.", exception);
            }
            catch (NotSupportedException exception)
            {
                return CreateFailureResult<TValue>(localErrorContext, "The external service returned an unsupported JSON payload.", exception);
            }
        }

        public static async Task<Result> Map(this HttpResponseMessage? response, ErrorContext errorContext, CancellationToken ct = default)
        {
            var localErrorContext = CloneErrorContext(errorContext);

            if (response is null)
                return Result.Failure(Error.Failure(localErrorContext, CreateTransportFailureMessage()));

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var failureMessage = await GetFailureMessageAsync(response, localErrorContext, ct);

            return CreateHttpFailureResult(response.StatusCode, localErrorContext, failureMessage);
        }

        private static ErrorContext CloneErrorContext(ErrorContext errorContext)
        {
            return new ErrorContext(
                Layer: errorContext.Layer,
                ServiceName: errorContext.ServiceName,
                MethodName: errorContext.MethodName,
                Operation: errorContext.Operation,
                EntityName: errorContext.EntityName,
                FieldName: errorContext.FieldName,
                ConfirmFieldName: errorContext.ConfirmFieldName)
            {
                DescriptionSuffix = errorContext.DescriptionSuffix,
            };
        }

        private static Result<TValue> CreateFailureResult<TValue>(ErrorContext errorContext, string message, Exception? exception = null)
        {
            return Result<TValue>.Failure(Error.Failure(errorContext, message, exception));
        }

        private static Result<TValue> CreateHttpFailureResult<TValue>(HttpStatusCode statusCode, ErrorContext errorContext, string message)
        {
            var localErrorContext = errorContext with { DescriptionSuffix = message };
            return Result<TValue>.Failure(MapHttpError(statusCode, localErrorContext));
        }

        private static Result CreateHttpFailureResult(HttpStatusCode statusCode, ErrorContext errorContext, string message)
        {
            var localErrorContext = errorContext with { DescriptionSuffix = message };
            return Result.Failure(MapHttpError(statusCode, localErrorContext));
        }

        private static HttpError MapHttpError(HttpStatusCode statusCode, ErrorContext errorContext)
        {
            return statusCode switch
            {
                HttpStatusCode.NotFound => HttpError.NotFound(errorContext),
                HttpStatusCode.BadRequest => HttpError.BadRequest(errorContext),
                HttpStatusCode.UnprocessableContent => HttpError.UnprocessableContent(errorContext),
                HttpStatusCode.Conflict => HttpError.Conflict(errorContext),
                HttpStatusCode.Unauthorized => HttpError.Unauthorized(errorContext),
                HttpStatusCode.Forbidden => HttpError.Forbidden(errorContext),
                HttpStatusCode.InternalServerError => HttpError.InternalServerError(errorContext),
                _ => HttpError.Custom(errorContext),
            };
        }

        private static async Task<string> GetFailureMessageAsync(HttpResponseMessage response, ErrorContext errorContext, CancellationToken ct)
        {
            var responseMessage = await TryGetResponseMessageAsync(response, ct);
            return BuildFailureMessage(responseMessage, errorContext.DescriptionSuffix, GetDefaultFailureMessage(response.StatusCode), response.ReasonPhrase);
        }

        private static string BuildFailureMessage(string? responseMessage, string? descriptionSuffix, string defaultMessage, string? reasonPhrase)
        {
            return FirstNonEmpty(responseMessage, descriptionSuffix, defaultMessage, reasonPhrase, "An error occurred while calling the external service.");
        }

        private static string GetDefaultFailureMessage(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.BadRequest => "The external service rejected the request.",
                HttpStatusCode.Unauthorized => "The external service requires authentication.",
                HttpStatusCode.Forbidden => "The external service refused the request.",
                HttpStatusCode.NotFound => "The requested resource was not found in the external service.",
                HttpStatusCode.Conflict => "The external service reported a conflict.",
                HttpStatusCode.UnprocessableContent => "The external service could not process the request.",
                HttpStatusCode.InternalServerError => "The external service encountered an internal server error.",
                _ => $"The external service returned an unexpected HTTP status code {(int)statusCode} ({statusCode}).",
            };
        }

        private static string CreateTransportFailureMessage()
        {
            return "No HTTP response was received from the external service.";
        }

        private static bool HasJsonContentType(HttpResponseMessage response)
        {
            var mediaType = response.Content?.Headers.ContentType?.MediaType;

            return string.IsNullOrWhiteSpace(mediaType)
                || mediaType.Contains("json", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<string?> ReadResponseBodyAsync(HttpResponseMessage response, CancellationToken ct)
        {
            if (response.Content is null)
                return null;

            return await response.Content.ReadAsStringAsync(ct);
        }

        private static async Task<string?> TryGetResponseMessageAsync(HttpResponseMessage response, CancellationToken ct)
        {
            var responseBody = await ReadResponseBodyAsync(response, ct);
            if (string.IsNullOrWhiteSpace(responseBody))
                return null;

            try
            {
                using var document = JsonDocument.Parse(responseBody);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                    return responseBody.Trim();

                foreach (var propertyName in new[] { "message", "detail", "title", "error", "error_description" })
                {
                    if (document.RootElement.TryGetProperty(propertyName, out var property)
                        && property.ValueKind == JsonValueKind.String
                        && !string.IsNullOrWhiteSpace(property.GetString()))
                    {
                        return property.GetString();
                    }
                }

                if (document.RootElement.TryGetProperty("errors", out var errorsProperty))
                {
                    var errors = ExtractErrors(errorsProperty);
                    if (!string.IsNullOrWhiteSpace(errors))
                        return errors;
                }
            }
            catch (JsonException)
            {
            }

            return responseBody.Trim();
        }

        private static string? ExtractErrors(JsonElement errorsElement)
        {
            if (errorsElement.ValueKind == JsonValueKind.Array)
            {
                var errors = errorsElement
                    .EnumerateArray()
                    .Where(static value => value.ValueKind == JsonValueKind.String)
                    .Select(static value => value.GetString())
                    .Where(static value => !string.IsNullOrWhiteSpace(value));

                return string.Join(" ", errors!);
            }

            if (errorsElement.ValueKind == JsonValueKind.Object)
            {
                var errors = errorsElement
                    .EnumerateObject()
                    .SelectMany(static property => property.Value.ValueKind == JsonValueKind.Array
                        ? property.Value.EnumerateArray().Where(static value => value.ValueKind == JsonValueKind.String).Select(static value => value.GetString())
                        : Array.Empty<string?>())
                    .Where(static value => !string.IsNullOrWhiteSpace(value));

                return string.Join(" ", errors!);
            }

            return null;
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            return values.First(value => !string.IsNullOrWhiteSpace(value))!;
        }

    }
}
