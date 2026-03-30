using System.Net;
using System.Net.Http.Json;
using System.Reflection.Metadata.Ecma335;

namespace Rasmus.SharedKernel.ResultPattern
{
    public class HttpResponseResultMapper
    {
        public async Task<Result<TValue>> FromResponseWithValue<TValue>(HttpResponseMessage response, ErrorContext errorContext, CancellationToken ct = default)
        {
            if (response is null)
                return Result<TValue>.Failure(Error.Unauthorized(errorContext), errorContext.DescriptionSuffix!);

            TValue? value = default;

            string defaultReasonPhrase = string.IsNullOrWhiteSpace(response.ReasonPhrase)
                ? (string.IsNullOrWhiteSpace(errorContext.DescriptionSuffix)
                    ? "An error occurred"
                    : errorContext.DescriptionSuffix)
                : response.ReasonPhrase;

            switch (response.StatusCode)
            {

                case HttpStatusCode.OK:
                value = await response.Content.ReadFromJsonAsync<TValue>(cancellationToken: ct);
                break;

                case HttpStatusCode.NotFound:
                return Result<TValue>.Failure(HttpError.NotFound(errorContext), defaultReasonPhrase);

                case HttpStatusCode.BadRequest:
                return Result<TValue>.Failure(HttpError.BadRequest(errorContext), defaultReasonPhrase);

                case (HttpStatusCode)422:
                return Result<TValue>.Failure(HttpError.BadRequest(errorContext), defaultReasonPhrase);

                case HttpStatusCode.Conflict:
                return Result<TValue>.Failure(HttpError.Conflict(errorContext), defaultReasonPhrase);

                case HttpStatusCode.Unauthorized:
                return Result<TValue>.Failure(HttpError.Unauthorized(errorContext), defaultReasonPhrase);

                case HttpStatusCode.Forbidden:
                return Result<TValue>.Failure(HttpError.Forbidden(errorContext), defaultReasonPhrase);

                case HttpStatusCode.InternalServerError:
                return Result<TValue>.Failure(HttpError.InternalServerError(errorContext), defaultReasonPhrase);

                default:
                return Result<TValue>.Failure(HttpError.Custom(errorContext), defaultReasonPhrase);
            }

            if (response.IsSuccessStatusCode)
            {

                if (value is not null)
                    return Result<TValue>.Success(value);
                else
                    return Result<TValue>.Failure(HttpError.NotFound(errorContext), defaultReasonPhrase);
            }

            return Result<TValue>.Failure(HttpError.Custom(errorContext), defaultReasonPhrase);

        }

        public Result FromResponseNoValue(HttpResponseMessage response, ErrorContext errorContext, CancellationToken ct = default)
        {
            if (response is null)
                return Result.Failure(Error.Unauthorized(errorContext), errorContext.DescriptionSuffix!);

            string defaultReasonPhrase = string.IsNullOrWhiteSpace(response.ReasonPhrase)
                ? (string.IsNullOrWhiteSpace(errorContext.DescriptionSuffix)
                    ? "An error occurred"
                    : errorContext.DescriptionSuffix)
                : response.ReasonPhrase;

            return response.StatusCode switch
            {
                HttpStatusCode.OK => Result.Success(),
                HttpStatusCode.NotFound => Result.Failure(HttpError.NotFound(errorContext), defaultReasonPhrase),
                HttpStatusCode.BadRequest => Result.Failure(HttpError.BadRequest(errorContext), defaultReasonPhrase),
                HttpStatusCode.UnprocessableContent => Result.Failure(HttpError.UnprocessableContent(errorContext), defaultReasonPhrase),
                HttpStatusCode.Conflict => Result.Failure(HttpError.Conflict(errorContext), defaultReasonPhrase),
                HttpStatusCode.Unauthorized => Result.Failure(HttpError.Unauthorized(errorContext), defaultReasonPhrase),
                HttpStatusCode.Forbidden => Result.Failure(HttpError.Forbidden(errorContext), defaultReasonPhrase),
                HttpStatusCode.InternalServerError => Result.Failure(HttpError.InternalServerError(errorContext), defaultReasonPhrase),
                _ => Result.Failure(HttpError.Custom(errorContext), defaultReasonPhrase),
            };
        }

    }
}
