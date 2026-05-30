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

        public static HttpError TooManyRequests(ErrorContext errorContext, string? callerMessage = null)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpTooManyRequests);

            string userMessage = string.IsNullOrWhiteSpace(callerMessage) ? "Too Many Requests" : callerMessage;
            string formattedErrorDescription = FormatDescription(errorContext, userMessage);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.TooManyRequests, userMessage);
        }

        public static HttpError MalformedResponse(ErrorContext errorContext, Exception? exception = null, string? detail = null)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpMalformedResponse);

            string userMessage = "The external service returned a malformed or unexpected response.";
            string descriptionSuffix = string.IsNullOrWhiteSpace(detail) ? userMessage : detail;
            string formattedErrorDescription = FormatDescription(errorContext, descriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.MalformedResponse, userMessage, exception);
        }

        public static HttpError UnexpectedStatusCode(ErrorContext errorContext, HttpStatusCode statusCode)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpUnexpectedStatusCode);

            string defaultDescriptionSuffix = $"The external service returned an unexpected HTTP status code {(int)statusCode} ({statusCode}).";
            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.UnexpectedStatusCode, defaultDescriptionSuffix);
        }

        public static HttpError UnprocessableContent(ErrorContext errorContext, string? callerMessage = null)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpUnprocessableContent);

            string userMessage = string.IsNullOrWhiteSpace(callerMessage) ? "Unprocessable Content" : callerMessage;
            string formattedErrorDescription = FormatDescription(errorContext, userMessage);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.UnprocessableContent, userMessage);
        }

        public static HttpError BadRequest(ErrorContext errorContext, string? callerMessage = null)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpBadRequest);

            string userMessage = string.IsNullOrWhiteSpace(callerMessage) ? "Bad Request" : callerMessage;
            string formattedErrorDescription = FormatDescription(errorContext, userMessage);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.BadRequest, userMessage);
        }

        public static HttpError UnauthorizedAccess(ErrorContext errorContext, string? callerMessage = null)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpUnauthorized);

            string userMessage = string.IsNullOrWhiteSpace(callerMessage) ? "Unauthorized" : callerMessage;
            string formattedErrorDescription = FormatDescription(errorContext, userMessage);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.Unauthorized, userMessage);
        }


        public static HttpError Forbidden(ErrorContext errorContext, string? callerMessage = null)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpForbidden);

            string userMessage = string.IsNullOrWhiteSpace(callerMessage) ? "Forbidden" : callerMessage;
            string formattedErrorDescription = FormatDescription(errorContext, userMessage);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.Forbidden, userMessage);
        }


        public static new HttpError NotFound(ErrorContext errorContext, string? callerMessage = null)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpNotFound);

            string userMessage = string.IsNullOrWhiteSpace(callerMessage) ? "Not Found" : callerMessage;
            string formattedErrorDescription = FormatDescription(errorContext, userMessage);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.NotFound, userMessage);
        }

        public static new HttpError Conflict(ErrorContext errorContext, string? callerMessage = null)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpConflict);

            string userMessage = string.IsNullOrWhiteSpace(callerMessage) ? "Conflict" : callerMessage;
            string formattedErrorDescription = FormatDescription(errorContext, userMessage);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.Conflict, userMessage);
        }

        public static HttpError InternalServerError(ErrorContext errorContext, string? callerMessage = null)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpInternalServerError);

            string userMessage = string.IsNullOrWhiteSpace(callerMessage) ? "Internal Server Error" : callerMessage;
            string formattedErrorDescription = FormatDescription(errorContext, userMessage);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.InternalServerError, userMessage);
        }

    }
}


