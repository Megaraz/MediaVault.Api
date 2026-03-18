using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.ResultPattern
{
    public enum ValidationErrorType
    {
        Custom = 0,
        Required = 1,
        InvalidFormat = 2,
        OutOfRange = 3,
        TooShort = 4,
        TooLong = 5,
    }

    public record ValidationError : Error
    {
        public ValidationErrorType ValidationErrorType { get; }

        // Private constructor to enforce the use of static factory methods for creating ValidationError instances
        // Sets the ErrorType of base-class to Validation for all instances of ValidationError
        private ValidationError(string Code, string Description, ValidationErrorType validationErrorType)
            : base(Code, Description, ErrorType.Validation)
        {
            ValidationErrorType = validationErrorType;
        }


        public static ValidationError InvalidFormat<T>(string currentOperation, string errorDescriptionPrefix, string fieldName, string expectedFormat) =>
            new ValidationError(
                ErrorCode.InvalidFormat<T>(currentOperation).Code,
                $"{errorDescriptionPrefix}: The field '{fieldName}' has an invalid format. Expected format: {expectedFormat}.",
                ValidationErrorType.InvalidFormat);

        //public static ValidationError Required<T>(string currentOperation, string errorDescriptionPrefix, out string errorMessageReason)
        public static ValidationError Required<T>(string currentOperation, string errorDescriptionPrefix, string fieldName) =>
            new ValidationError(
                ErrorCode.Required<T>(currentOperation).Code,
                $"{errorDescriptionPrefix}: The field '{fieldName}' is required and cannot be null or empty.",
                ValidationErrorType.Required);


        public static ValidationError OutOfRange<T>(string currentOperation, string errorDescriptionPrefix, string fieldName, string range) =>
            new ValidationError(
                ErrorCode.OutOfRange<T>(currentOperation).Code,
                $"{errorDescriptionPrefix}: The field '{fieldName}' is out of range. Expected range: {range}.",
                ValidationErrorType.OutOfRange);

        public static ValidationError Custom<T>(string currentOperation, string errorDescriptionPrefix, string customErrorType, string message) =>
            new ValidationError(
                ErrorCode.Custom<T>(currentOperation, customErrorType).Code,
                $"{errorDescriptionPrefix}: {message}",
                ValidationErrorType.Custom);
    }
}