using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.ResultPatternCompatibility
{
    /// <summary>
    /// Pure mapper that converts domain <see cref="Result"/> instances
    /// into framework-agnostic <see cref="MappedHttpResponse"/> descriptors.
    /// No ASP.NET dependencies — only HTTP semantics.
    /// </summary>
    public static class HttpResultMapper
    {
        /// <summary>
        /// Maps a successful <see cref="Result{TValue}"/> to 200 OK with the value as the body,
        /// or maps a failure to the appropriate HTTP error response.
        /// </summary>
        public static MappedHttpResponse ToHttpResponse<TValue>(Result<TValue> result)
            where TValue : notnull
        {
            return result.IsSuccess
                ? new MappedHttpResponse(200, result.Value)
                : MapFailure(result);
        }

        /// <summary>
        /// Maps a successful <see cref="Result"/> to 200 OK (no body),
        /// or maps a failure to the appropriate HTTP error response.
        /// </summary>
        public static MappedHttpResponse ToHttpResponse(Result result)
        {
            return result.IsSuccess
                ? new MappedHttpResponse(200)
                : MapFailure(result);
        }

        /// <summary>
        /// Maps a successful <see cref="Result"/> to 204 No Content,
        /// or maps a failure to the appropriate HTTP error response.
        /// </summary>
        public static MappedHttpResponse ToNoContentResponse(Result result)
        {
            return result.IsSuccess
                ? new MappedHttpResponse(204)
                : MapFailure(result);
        }

        /// <summary>
        /// Maps a successful <see cref="Result{TValue}"/> to 201 Created with the value as the body
        /// and an optional location, or maps a failure to the appropriate HTTP error response.
        /// </summary>
        public static MappedHttpResponse ToCreatedResponse<TValue>(Result<TValue> result, string? location = null)
            where TValue : notnull
        {
            return result.IsSuccess
                ? new MappedHttpResponse(201, result.Value, location)
                : MapFailure(result);
        }

        private static MappedHttpResponse MapFailure(Result result)
        {
            var primaryError = result.PrimaryError;
            var message = result.Message;
            var validationErrorItems = result.ValidationErrors
                .Select(x => new ValidationErrorItem(x.FieldName, x.UserMessage));

            var (statusCode, body) = BuildFailureResponse(
                message,
                primaryError,
                validationErrorItems);

            return new MappedHttpResponse(statusCode, body);
        }

        private static (int StatusCode, object Body) BuildFailureResponse(
            string message,
            Error primaryError,
            IEnumerable<ValidationErrorItem>? validationErrorItems)
        {
            if (primaryError is HttpError)
                return MapHttpErrorFailure(message, primaryError);

            if (primaryError is DatabaseError)
                return (500, new ErrorResponseBody(message, primaryError.Code));

            return primaryError.Type switch
            {
                ErrorType.Validation => (422, new ValidationErrorResponseBody(message, validationErrorItems)),
                ErrorType.NotFound => (404, new ErrorResponseBody(message, primaryError.Code)),
                ErrorType.Conflict => (409, new ErrorResponseBody(message, primaryError.Code)),
                ErrorType.Unauthorized => (401, new ErrorResponseBody(message, primaryError.Code)),
                ErrorType.Forbidden => (403, new ErrorResponseBody(message, primaryError.Code)),
                ErrorType.Failure => (500, new ErrorResponseBody(message, primaryError.Code)),
                ErrorType.Cancelled => (503, new ErrorResponseBody(message, primaryError.Code)),
                ErrorType.External => (500, new ErrorResponseBody(message, primaryError.Code)),
                _ => (400, new ErrorResponseBody(message, primaryError.Code))
            };
        }
        private static (int StatusCode, object Body) MapHttpErrorFailure(
            string message,
            Error error)
        {
            if (error is not HttpError httpError)
                return (502, new ErrorResponseBody(message, error.Code));

            var statusCode = httpError.HttpErrorType switch
            {
                HttpErrorType.BadRequest => 400,
                HttpErrorType.Unauthorized => 401,
                HttpErrorType.Forbidden => 403,
                HttpErrorType.NotFound => 404,
                HttpErrorType.Conflict => 409,
                HttpErrorType.UnprocessableContent => 422,
                HttpErrorType.TooManyRequests => 429,

                HttpErrorType.InternalServerError => 502,
                HttpErrorType.TransportFailure => 503,
                HttpErrorType.MalformedResponse => 502,
                HttpErrorType.UnexpectedStatusCode => 502,

                _ => 502
            };

            return (statusCode, new ErrorResponseBody(message, error.Code));
        }
    }
}
