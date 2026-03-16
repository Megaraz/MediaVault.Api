using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.ResultPattern
{
    // Represents a structured error code that encapsulates the operation, entity type, and error type.
    // The property Code is a concatenation of the three components, making it easy to identify and categorize errors in a consistent manner.
    // Code will look like: "Create.User.ValidationError.Required" or "GetCollection.Order.DatabaseError.Timeout"
    public sealed record ErrorCode
    {
        public string Operation { get; }
        public string NameOfEntity { get; }
        public string ErrorType { get; }

        public string Code { get; }

        private ErrorCode(string operation, string nameOfEntity, string errorType)
        {
            Operation = operation;
            NameOfEntity = nameOfEntity;
            ErrorType = errorType;
            Code = $"{Operation}.{NameOfEntity}.{ErrorType}";
        }

        // Factory methods for common error scenarios

        // Generate an error code for a Create-Operation on a specific entity type
        public static ErrorCode Create<T>(string errorType) =>
            new(ErrorCodes.Operation.Create, typeof(T).Name, errorType);

        // Generate an ErrorCode for a Get-Operation on a specific entity type
        public static ErrorCode Get<T>(string errorType) =>
            new(ErrorCodes.Operation.Get, typeof(T).Name, errorType);

        // Generate an ErrorCode for a GetCollection-Operation on a specific entity type
        public static ErrorCode GetCollection<T>(string errorType) =>
            new(ErrorCodes.Operation.GetCollection, typeof(T).Name, errorType);

        // Generate an ErrorCode for an Update-Operation on a specific entity type
        public static ErrorCode Update<T>(string errorType) =>
            new(ErrorCodes.Operation.Update, typeof(T).Name, errorType);

        // Generate an ErrorCode for a Delete-Operation on a specific entity type
        public static ErrorCode Delete<T>(string errorType) =>
            new(ErrorCodes.Operation.Delete, typeof(T).Name, errorType);

        // Generate an ErrorCode for a NullValue error on a specific entity type and operation
        public static ErrorCode NullValue<T>(string currentOperation) =>
            new(currentOperation, typeof(T).Name, ErrorCodes.ValidationError.Required);

        public static ErrorCode InvalidFormat<T>(string currentOperation) =>
            new(currentOperation, typeof(T).Name, ErrorCodes.ValidationError.InvalidFormat);

        public static ErrorCode OutOfRange<T>(string currentOperation) =>
            new(currentOperation, typeof(T).Name, ErrorCodes.ValidationError.OutOfRange);

        public static ErrorCode Custom<T>(string currentOperation, string customErrorType) =>
            new(currentOperation, typeof(T).Name, customErrorType);

        public override string ToString()
        {
            return $"{Operation}.{NameOfEntity}.{ErrorType}";

        }



    }
}
