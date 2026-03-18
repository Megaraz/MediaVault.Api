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
    }
    public enum ErrorReasonCode
    {
        Custom = 0,

        ValidationRequired = 100,
        ValidationInvalidFormat = 101,
        ValidationOutOfRange = 102,
        ValidationTooShort = 103,
        ValidationTooLong = 104,

        DatabaseFailure = 200,

        GeneralFailure = 300,
        GeneralNotFound = 301,
        GeneralConflict = 302,
        GeneralUnauthorized = 303,
        GeneralForbidden = 304
    }

    // Represents a structured error code that encapsulates the operation, entity type, and error type.
    // The property Code is a concatenation of the three components, making it easy to identify and categorize errors in a consistent manner.
    // Code will look like: "Create.User..Required" or "GetCollection.Order.Timeout"
    public sealed record ErrorCode
    {
        public OperationType Operation { get; }
        public string NameOfEntity { get; }
        //public string ErrorCodeType { get; }
        public ErrorReasonCode Reason { get; }

        public string Code => $"{Operation}.{NameOfEntity}.{Reason.ToCodePart()}";

        private ErrorCode(OperationType operation, string nameOfEntity, ErrorReasonCode reason)
        {
            Operation = operation;
            NameOfEntity = nameOfEntity;
            Reason = reason;
        }

        public static ErrorCode For<T>(OperationType operation, ErrorReasonCode reason) =>
            new(operation, typeof(T).Name, reason);

    }
}
