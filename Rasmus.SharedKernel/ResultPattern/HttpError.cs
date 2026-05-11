using System.Net;

namespace Rasmus.SharedKernel.ResultPattern
{

    public enum HttpErrorType
    {
        Custom = 0,
        BadRequest = 1,
        Unauthorized = 2,
        Forbidden = 3,
        NotFound = 4,
        Conflict = 5,
        InternalServerError = 6,
        UnprocessableContent = 7,

        // new additions
        TooManyRequests = 8,
        TransportFailure = 9,
        MalformedResponse = 10,
        UnexpectedStatusCode = 11
    }

    public record HttpError : Error
    {
        public HttpErrorType HttpErrorType { get; }

        // Private constructor to enforce the use of static factory methods for creating HttpError instances
        // Sets the ErrorType of base-class to HttpError for all instances of HttpError
        private HttpError(string code, string description, HttpErrorType type, string userMessage, Exception? exception = null)
            : base(code, description, ErrorType.HttpError, userMessage, exception)
        {
            HttpErrorType = type;
        }

        public static HttpError Custom(ErrorContext errorContext, string customDescriptionSuffix)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.Custom);

            string formattedErrorDescription = FormatDescription(errorContext, customDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.Custom, customDescriptionSuffix);
        }

        public static HttpError TransportFailure(ErrorContext errorContext, Exception? exception = null)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpTransportFailure);

            string defaultDescriptionSuffix = $"Transport Failure";
            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.TransportFailure, defaultDescriptionSuffix, exception);
        }

        public static HttpError TooManyRequests(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpTooManyRequests);

            string defaultDescriptionSuffix = $"Too Many Requests";
            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.TooManyRequests, defaultDescriptionSuffix);
        }

        public static HttpError MalformedResponse(ErrorContext errorContext, Exception? exception = null)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpMalformedResponse);

            string defaultDescriptionSuffix = $"The external service returned a malformed or unexpected response.";
            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.MalformedResponse, defaultDescriptionSuffix, exception);
        }

        public static HttpError UnexpectedStatusCode(ErrorContext errorContext, HttpStatusCode statusCode)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpUnexpectedStatusCode);

            string defaultDescriptionSuffix = $"The external service returned an unexpected HTTP status code {(int)statusCode} ({statusCode}).";
            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.UnexpectedStatusCode, defaultDescriptionSuffix);
        }

        public static HttpError UnprocessableContent(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpUnprocessableContent);

            string defaultDescriptionSuffix = $"Unprocessable Content";
            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.UnprocessableContent, defaultDescriptionSuffix);
        }

        public static HttpError BadRequest(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpBadRequest);

            string defaultDescriptionSuffix = $"Bad Request";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.BadRequest, defaultDescriptionSuffix);
        }

        public static HttpError UnauthorizedAccess(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpUnauthorized);

            string defaultDescriptionSuffix = $"Unauthorized";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.Unauthorized, defaultDescriptionSuffix);
        }


        public static HttpError Forbidden(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpForbidden);

            string defaultDescriptionSuffix = $"Forbidden";
            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.Forbidden, defaultDescriptionSuffix);
        }


        public static new HttpError NotFound(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpNotFound);

            string defaultDescriptionSuffix = $"Not Found";
            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.NotFound, defaultDescriptionSuffix);
        }

        public static new HttpError Conflict(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpConflict);

            string defaultDescriptionSuffix = $"Conflict";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.Conflict, defaultDescriptionSuffix);
        }

        public static HttpError InternalServerError(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpInternalServerError);

            string defaultDescriptionSuffix = $"Internal Server Error";
            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.InternalServerError, defaultDescriptionSuffix);
        }

    }
}


