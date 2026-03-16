using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.ResultPattern
{
    public enum ValidationErrorType
    {
        Required,
        InvalidFormat,
        NullValue,
        OutOfRange,
        Custom
    }

    public record ValidationError : Error
    {
        public ValidationErrorType ValidationErrorType { get; }
        private ValidationError(string Code, string Description, ValidationErrorType validationErrorType) : base(Code, Description, ErrorType.Validation)
        {
            ValidationErrorType = validationErrorType;
        }

        public static ValidationError Required<T>(string currentOperation, string fieldName) =>
            new ValidationError(
                ErrorCode.NullValue<T>(currentOperation).Code,
                $"The field '{fieldName}' is required and cannot be null or empty.",
                ValidationErrorType.Required);

        public static ValidationError InvalidFormat<T>(string currentOperation, string fieldName, string expectedFormat) =>
            new ValidationError(
                ErrorCode.InvalidFormat<T>(currentOperation).Code,
                $"The field '{fieldName}' has an invalid format. Expected format: {expectedFormat}.",
                ValidationErrorType.InvalidFormat);

        public new static ValidationError NullValue<T>(string currentOperation, string errorDescriptionPrefix, out string errorMessageReason)
        {
            var errorCode = ErrorCode.NullValue<T>(currentOperation);

            errorMessageReason = $"{errorCode.NameOfEntity} cannot be null or default";

            // Create and return full ValidationError of ValidationErrorType.NullValue, with ErrorCode from above
            return new ValidationError(
                errorCode.Code,
                $"{errorDescriptionPrefix}: {errorMessageReason}",
                ValidationErrorType.NullValue);
        }

        public static ValidationError OutOfRange<T>(string currentOperation, string fieldName, string range) =>
            new ValidationError(
                ErrorCode.OutOfRange<T>(currentOperation).Code,
                $"The field '{fieldName}' is out of range. Expected range: {range}.",
                ValidationErrorType.OutOfRange);

        public static ValidationError Custom<T>(string currentOperation, string customErrorType, string message) =>
            new ValidationError(
                ErrorCode.Custom<T>(currentOperation, customErrorType).Code,
                message,
                ValidationErrorType.Custom);
    }
}