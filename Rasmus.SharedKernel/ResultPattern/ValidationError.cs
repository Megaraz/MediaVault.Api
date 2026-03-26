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
        AlreadyExists = 5,
        TooShort = 6,
        TooLong = 7,
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

        public static ValidationError AlreadyExists(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.GeneralConflict);

            string defaultDescriptionSuffix = $"A {errorContext.EntityName} with that {errorContext.FieldName} already exists, please choose a different {errorContext.FieldName}.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new ValidationError(errorCode.Code, formattedErrorDescription, ValidationErrorType.AlreadyExists);
        }

        public static ValidationError InvalidFormat(ErrorContext errorContext, string expectedFormat)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.ValidationInvalidFormat);

            string defaultDescriptionSuffix = $"The field '{errorContext.FieldName}' has an invalid format. Expected format: {expectedFormat}.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new ValidationError(errorCode.Code, formattedErrorDescription, ValidationErrorType.InvalidFormat);
        }

        public static ValidationError Required(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.ValidationRequired);

            string defaultDescriptionSuffix = string.IsNullOrWhiteSpace(errorContext.FieldName)
                ? $"A value for the entity '{errorContext.EntityName}' is required and cannot be null or empty."
                : $"A value for the field '{errorContext.FieldName}' is required and cannot be null or empty.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new ValidationError(errorCode.Code, formattedErrorDescription, ValidationErrorType.Required);
        }

        public static ValidationError TooLong(ErrorContext errorContext, string range)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.ValidationTooLong);

            string defaultDescriptionSuffix = $"The field '{errorContext.FieldName}' is too long. Expected maximum length: {range}.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new ValidationError(errorCode.Code, formattedErrorDescription, ValidationErrorType.TooLong);
        }

        public static ValidationError OutOfRange(ErrorContext errorContext, string range)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.ValidationOutOfRange);

            string defaultDescriptionSuffix = $"The field '{errorContext.FieldName}' is out of range. Expected range: {range}.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new ValidationError(errorCode.Code, formattedErrorDescription, ValidationErrorType.OutOfRange);
        }

        public static ValidationError TooShort(ErrorContext errorContext, string range)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.ValidationTooShort);

            string defaultDescriptionSuffix = $"The field '{errorContext.FieldName}' is too short. Expected minimum length: {range}.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new ValidationError(errorCode.Code, formattedErrorDescription, ValidationErrorType.TooShort);
        }

        public static ValidationError NonMatchingValues(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.ValidationNonMatchingValues);

            string defaultDescriptionSuffix = !string.IsNullOrWhiteSpace(errorContext.FieldName) && !string.IsNullOrWhiteSpace(errorContext.ConfirmFieldName)
                ? $"The values for '{errorContext.FieldName}' and '{errorContext.ConfirmFieldName}' do not match."
                : "The provided values do not match.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new ValidationError(errorCode.Code, formattedErrorDescription, ValidationErrorType.NonMatchingValues);
        }

        public static ValidationError Custom(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.Custom);

            string defaultDescriptionSuffix = "A custom validation error occurred.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new ValidationError(errorCode.Code, formattedErrorDescription, ValidationErrorType.Custom);
        }
    }
}