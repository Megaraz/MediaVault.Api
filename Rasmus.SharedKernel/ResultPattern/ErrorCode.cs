using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.ResultPattern
{
    public enum OperationType
    {
        Custom = 0,

        Create = 1,
        Get = 2,
        GetCollection = 3,
        Update = 4,
        Delete = 5,

        Login = 100,
    }
    public enum ErrorReasonCode
    {
        Custom = 0,

        ValidationRequired = 100,
        ValidationInvalidFormat = 101,
        ValidationOutOfRange = 102,
        ValidationNonMatchingValues = 103,
        ValidationTooShort = 104,
        ValidationTooLong = 105,

        DatabaseFailure = 200,

        GeneralFailure = 300,
        GeneralNotFound = 301,
        GeneralConflict = 302,
        GeneralUnauthorized = 303,
        GeneralForbidden = 304,

        UserLoginFailure = 399,

        HttpBadRequest = 400,
        HttpUnauthorized = 401,
        HttpForbidden = 403,
        HttpNotFound = 404,
        HttpMethodNotAllowed = 405,
        HttpRequestTimeout = 408,
        HttpConflict = 409,
        HttpUnprocessableContent = 422,
        HttpInternalServerError = 500,
        HttpBadGateway = 502,
        HttpServiceUnavailable = 503, 
        HttpGatewayTimeout = 504

    }

    // Represents a structured error code that encapsulates the operation, entity type, and error type.
    // The property Code is a concatenation of the three components, making it easy to identify and categorize errors in a consistent manner.
    // Code will look like: "Create.User.Required" or "GetCollection.Order.Timeout"
    public sealed record ErrorCode
    {
        /// <summary>
        /// Gets the type of operation that caused the error (e.g., Create, Get, Update, Delete).
        /// </summary>
        public OperationType Operation { get; }

        /// <summary>
        /// Gets the name of the entity which the error is related to (e.g., User, Order). This is typically the name of the class or entity involved in the operation.
        /// </summary>
        public string NameOfEntity { get; }

        /// <summary>
        /// Gets the reason code that indicates the type of error encountered.
        /// </summary>
        public ErrorReasonCode Reason { get; }

        /// <summary>
        /// Gets the full error code as a string, which is a combination of the operation, entity name, and reason code. This provides a standardized way to represent errors across the application.
        /// </summary>
        public string Code => $"{Operation}.{NameOfEntity}.{Reason.ToCodePart()}";

        private ErrorCode(OperationType operation, string nameOfEntity, ErrorReasonCode reason)
        {
            Operation = operation;
            NameOfEntity = nameOfEntity;
            Reason = reason;
        }

        /// <summary>
        /// Creates a new <see cref="ErrorCode"/> instance for the specified operation type and error reason, associating it with the
        /// type parameter.
        /// </summary>
        /// <typeparam name="T">The type to associate with the error code. Typically represents the context or entity related to the error.</typeparam>
        /// <param name="operation">The operation for which the error code is being generated.</param>
        /// <param name="nameOfFieldOrEntity">The name of the field or entity to associate with the error code.</param>
        /// <param name="reason">The reason code that describes the specific error condition.</param>
        /// <returns>An ErrorCode instance representing the specified operation and reason, associated with the type parameter.</returns>
        //public static ErrorCode For(OperationType operation, string nameOfFieldOrEntity, ErrorReasonCode reason) =>
        //    new(operation, nameOfFieldOrEntity, reason);

        public static ErrorCode For(ErrorContext errorContext, ErrorReasonCode reason) =>
            new(errorContext.Operation, errorContext.EntityName, reason);
    }
}
