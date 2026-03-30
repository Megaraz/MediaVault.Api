using System;
using System.Collections.Generic;
using System.Text;

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
    }

    public record HttpError : Error
    {
        public HttpErrorType HttpErrorType { get; }
        public HttpError(string Code, string Description, ErrorType Type, Exception? exception = null) : base(Code, Description, Type, exception)
        {
        }


        // Private constructor to enforce the use of static factory methods for creating HttpError instances
        // Sets the ErrorType of base-class to HttpError for all instances of HttpError
        private HttpError(string code, string description, HttpErrorType type)
            : base(code, description, ErrorType.HttpError)
        {
            HttpErrorType = type;
        }


        public static HttpError Custom(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.Custom);

            string defaultDescriptionSuffix = $"Custom Error";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.Custom);
        }

        public static HttpError UnprocessableContent(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpUnprocessableContent);

            string defaultDescriptionSuffix = $"Unprocessable Content";
            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.UnprocessableContent);
        }

        public static HttpError BadRequest(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpBadRequest);

            string defaultDescriptionSuffix = $"Bad Request";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.BadRequest);
        }

        public new static HttpError Unauthorized(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpUnauthorized);

            string defaultDescriptionSuffix = $"Unauthorized";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.Unauthorized);
        }


        public static HttpError Forbidden(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpForbidden);

            string defaultDescriptionSuffix = $"Forbidden";
            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.Forbidden);
        }


        public static new HttpError NotFound(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpNotFound);

            string defaultDescriptionSuffix = $"Not Found";
            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.NotFound);
        }

        public static new HttpError Conflict(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpConflict);

            string defaultDescriptionSuffix = $"Bad Request";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.Conflict);
        }

        public static HttpError InternalServerError(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.HttpInternalServerError);

            string defaultDescriptionSuffix = $"Internal Server Error";
            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new HttpError(errorCode.Code, formattedErrorDescription, HttpErrorType.InternalServerError);
        }

    }
}
