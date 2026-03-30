namespace Rasmus.SharedKernel.ResultPattern
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
        {
            return result.IsSuccess
                ? new MappedHttpResponse(201, result.Value, location)
                : MapFailure(result);
        }

        private static MappedHttpResponse MapFailure(Result result)
        {
            var errorType = result.PrimaryError.Type;
            var errorCode = result.PrimaryError.Code;
            var message = result.Message;
            var validationErrors = result.ValidationErrors?.Select(x => x.Code);

            var (statusCode, body) = BuildFailureResponse(message, errorType, errorCode, validationErrors);

            return new MappedHttpResponse(statusCode, body);
        }

        private static (int StatusCode, object Body) BuildFailureResponse(
            string message,
            ErrorType errorType,
            string errorCode,
            IEnumerable<string>? validationErrors)
        {
            return errorType switch
            {
                ErrorType.Validation => (422, new ValidationErrorResponseBody(message, validationErrors)),
                ErrorType.NotFound => (404, new ErrorResponseBody(message, errorCode)),
                ErrorType.Conflict => (409, new ErrorResponseBody(message, errorCode)),
                ErrorType.Unauthorized => (401, new ErrorResponseBody(message, errorCode)),
                ErrorType.Forbidden => (403, new ErrorResponseBody(message, errorCode)),
                ErrorType.Failure => (500, new ErrorResponseBody(message, errorCode)),
                ErrorType.Database => (500, new ErrorResponseBody(message, errorCode)),
                _ => (400, new ErrorResponseBody(message, errorCode))
            };
        }
    }
}
