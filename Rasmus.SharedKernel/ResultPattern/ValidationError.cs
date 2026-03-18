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
        private ValidationError(string code, string description, ValidationErrorType type)
            : base(code, description, ErrorType.Validation)
        {
            ValidationErrorType = type;
        }


        public static ValidationError InvalidFormat<T>(OperationType currentOperation, string errorDescriptionPrefix, string fieldName, string expectedFormat) =>
            new ValidationError(
                ErrorCode.For<T>(currentOperation, ErrorReasonCode.ValidationInvalidFormat).Code,
                $"{errorDescriptionPrefix}: The field '{fieldName}' has an invalid format. Expected format: {expectedFormat}.",
                ValidationErrorType.InvalidFormat);

        public static ValidationError Required<T>(OperationType currentOperation, string errorDescriptionPrefix, string fieldName, string? errorMessageReason)
        {

            if (string.IsNullOrWhiteSpace(errorMessageReason))
                errorMessageReason = $"A value for the field or entity '{fieldName}' is required and cannot be null or empty.";

            string description = $"{errorDescriptionPrefix}: {errorMessageReason}";

            return new ValidationError(
                ErrorCode.For<T>(currentOperation, ErrorReasonCode.ValidationRequired).Code,
                description,
                ValidationErrorType.Required);
        }

        public static ValidationError TooLong<T>(OperationType currentOperation, string errorDescriptionPrefix, string fieldName, string range) =>
            new ValidationError(
                ErrorCode.For<T>(currentOperation, ErrorReasonCode.ValidationTooLong).Code,
                $"{errorDescriptionPrefix}: The field '{fieldName}' is too long. Expected maximum length: {range}.",
                ValidationErrorType.TooLong);
        public static ValidationError OutOfRange<T>(OperationType currentOperation, string errorDescriptionPrefix, string fieldName, string range) =>
            new ValidationError(
                ErrorCode.For<T>(currentOperation, ErrorReasonCode.ValidationOutOfRange).Code,
                $"{errorDescriptionPrefix}: The field '{fieldName}' is out of range. Expected range: {range}.",
                ValidationErrorType.OutOfRange);

        public static ValidationError TooShort<T>(OperationType currentOperation, string errorDescriptionPrefix, string fieldName, string range) =>
            new ValidationError(
                ErrorCode.For<T>(currentOperation, ErrorReasonCode.ValidationTooShort).Code,
                $"{errorDescriptionPrefix}: The field '{fieldName}' is too short. Expected minimum length: {range}.",
                ValidationErrorType.TooShort);
        public static ValidationError Custom<T>(OperationType currentOperation, string errorDescriptionPrefix, string message) =>
            new ValidationError(
                ErrorCode.For<T>(currentOperation, ErrorReasonCode.Custom).Code,
                $"{errorDescriptionPrefix}: {message}",
                ValidationErrorType.Custom);
    }
}