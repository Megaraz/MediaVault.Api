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
        NonMatchingValues = 4,
        TooShort = 5,
        TooLong = 6,
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


        public static ValidationError InvalidFormat<T>(ErrorContext errorContext, string expectedFormat) =>
            new ValidationError(
                ErrorCode.For<T>(errorContext.Operation, ErrorReasonCode.ValidationInvalidFormat).Code,
                $"{errorContext.DescriptionPrefix}: The field '{errorContext.FieldName}' has an invalid format. Expected format: {expectedFormat}.",
                ValidationErrorType.InvalidFormat);

        public static ValidationError Required<T>(ErrorContext errorContext)
        {
            if (string.IsNullOrWhiteSpace(errorContext.DescriptionSuffix))
                errorContext.DescriptionSuffix = $"A value for the field or entity '{errorContext.EntityName}' is required and cannot be null or empty.";

            string description = $"{errorContext.DescriptionPrefix}: {errorContext.DescriptionSuffix}";

            return new ValidationError(
                ErrorCode.For<T>(errorContext.Operation, ErrorReasonCode.ValidationRequired).Code,
                description,
                ValidationErrorType.Required);
        }

        public static ValidationError TooLong<T>(ErrorContext errorContext, string range) =>
            new ValidationError(
                ErrorCode.For<T>(errorContext.Operation, ErrorReasonCode.ValidationTooLong).Code,
                $"{errorContext.DescriptionPrefix}: The field '{errorContext.FieldName}' is too long. Expected maximum length: {range}.",
                ValidationErrorType.TooLong);
        public static ValidationError OutOfRange<T>(ErrorContext errorContext, string range) =>
            new ValidationError(
                ErrorCode.For<T>(errorContext.Operation, ErrorReasonCode.ValidationOutOfRange).Code,
                $"{errorContext.DescriptionPrefix}: The field '{errorContext.FieldName}' is out of range. Expected range: {range}.",
                ValidationErrorType.OutOfRange);

        public static ValidationError TooShort<T>(ErrorContext errorContext, string range) =>
            new ValidationError(
                ErrorCode.For<T>(errorContext.Operation, ErrorReasonCode.ValidationTooShort).Code,
                $"{errorContext.DescriptionPrefix}: The field '{errorContext.FieldName}' is too short. Expected minimum length: {range}.",
                ValidationErrorType.TooShort);

        public static ValidationError NonMatchingValues<T>(ErrorContext errorContext) =>
            new ValidationError(
                ErrorCode.For<T>(errorContext.Operation, ErrorReasonCode.ValidationNonMatchingValues).Code,
                $"{errorContext.DescriptionPrefix}: {errorContext.DescriptionSuffix}",
                ValidationErrorType.NonMatchingValues);
        public static ValidationError Custom<T>(ErrorContext errorContext) =>
            new ValidationError(
                ErrorCode.For<T>(errorContext.Operation, ErrorReasonCode.Custom).Code,
                $"{errorContext.DescriptionPrefix}: {errorContext.DescriptionSuffix}",
                ValidationErrorType.Custom);
    }
}