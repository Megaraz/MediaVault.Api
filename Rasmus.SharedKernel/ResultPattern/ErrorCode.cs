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
    // Code will look like: "Create.User.ValidationError.Required" or "GetCollection.Order.DatabaseError.Timeout"
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

        // Factory methods for common error scenarios

        //// Generate an error code for a Create-Operation on a specific entity type
        //public static ErrorCode Create<T>(ErrorReasonCode reason) =>
        //    new(OperationType.Create, typeof(T).Name, reason);

        //// Generate an ErrorCode for a Get-Operation on a specific entity type
        //public static ErrorCode Get<T>(ErrorReasonCode reason) =>
        //    new(OperationType.Get, typeof(T).Name, reason);

        //// Generate an ErrorCode for a GetCollection-Operation on a specific entity type
        //public static ErrorCode GetCollection<T>(ErrorReasonCode reason) =>
        //    new(OperationType.GetCollection, typeof(T).Name, reason);
        //// Generate an ErrorCode for an Update-Operation on a specific entity type
        //public static ErrorCode Update<T>(ErrorReasonCode reason) =>
        //    new(OperationType.Update, typeof(T).Name, reason);
        //// Generate an ErrorCode for a Delete-Operation on a specific entity type
        //public static ErrorCode Delete<T>(ErrorReasonCode reason) =>
        //    new(OperationType.Delete, typeof(T).Name, reason);

        //// Generate an ErrorCode for a Required error on a specific entity type and operation
        //public static ErrorCode Required<T>(OperationType currentOperation) =>
        //    new(currentOperation, typeof(T).Name, ErrorReasonCode.ValidationRequired);

        //public static ErrorCode InvalidFormat<T>(OperationType currentOperation) =>
        //    new(currentOperation, typeof(T).Name, ErrorReasonCode.ValidationInvalidFormat);

        //public static ErrorCode OutOfRange<T>(OperationType currentOperation) =>
        //    new(currentOperation, typeof(T).Name, ErrorReasonCode.ValidationOutOfRange);
        //public static ErrorCode TooShort<T>(OperationType currentOperation) =>
        //    new(currentOperation, typeof(T).Name, ErrorReasonCode.ValidationTooShort);

        //public static ErrorCode TooLong<T>(OperationType currentOperation) =>
        //    new(currentOperation, typeof(T).Name, ErrorReasonCode.ValidationTooLong);

        //public static ErrorCode Custom<T>(OperationType currentOperation) =>
        //    new(currentOperation, typeof(T).Name, ErrorReasonCode.Custom);

    }
}
