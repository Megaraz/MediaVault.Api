using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.ResultPattern
{
    public static class ValidatorExtensions
    {

        public static bool IsValidId<TKey>(this TKey id, ErrorContext errorContext, out ValidationError idError)
        {
            idError = default!;

            if (!Validator.IsValidId(id))
            {
                if (string.IsNullOrWhiteSpace(errorContext.FieldName))
                {
                    errorContext.FieldName = nameof(id);
                }
                errorContext.DescriptionSuffix = $"A valid Id is required for the entity '{errorContext.EntityName}' and cannot be null or empty.";

                idError = ValidationError.Required(errorContext);
                return false;
            }
            return true;
        }


        public static bool IsNull<TValue>(this TValue value, ErrorContext errorContext, out ValidationError nullValueError)
        {
            nullValueError = default!;

            if (value is null)
            {
                errorContext.DescriptionSuffix = $"A value for the entity '{errorContext.EntityName}' is required and cannot be null or empty.";

                nullValueError = ValidationError.Required(errorContext);
                return true;
            }
            else
                return false;
        }

        public static bool RequiredFieldsAreNullOrWhiteSpace(this IEnumerable<(string FieldName, string Value)> requiredValues, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            validationErrors = new List<ValidationError>();
            var internalErrors = new List<ValidationError>();

            if (requiredValues.IsNull(errorContext, out var nullValueError))
            {
                internalErrors.Add(nullValueError);
                validationErrors = internalErrors;
                return true;
            }

            foreach (var (FieldName, Value) in requiredValues)
            {
                if (Value.IsNullOrWhiteSpace(FieldName, errorContext, out var nullOrEmptyError))
                {
                    internalErrors.Add(nullOrEmptyError);
                }
            }
            validationErrors = internalErrors;
            return !validationErrors.Any();
        }

        public static bool IsNullOrWhiteSpace(this string value, string fieldName, ErrorContext errorContext, out ValidationError nullOrEmptyError)
        {
            nullOrEmptyError = default!;

            if (string.IsNullOrWhiteSpace(value))
            {
                errorContext.FieldName = string.IsNullOrWhiteSpace(fieldName) ? nameof(value) : fieldName;
                errorContext.DescriptionSuffix = $"The field '{errorContext.FieldName}' is required for the entity '{errorContext.EntityName}' and cannot be null or empty.";
                nullOrEmptyError = ValidationError.Required(errorContext);
                return true;
            }
            else
                return false;
        }
        public static bool IsNullOrWhiteSpace(this string value, ErrorContext errorContext, out ValidationError nullOrEmptyError)
        {
            nullOrEmptyError = default!;

            if (string.IsNullOrWhiteSpace(value))
            {
                errorContext.DescriptionSuffix = $"The field '{errorContext.FieldName}' is required for the entity '{errorContext.EntityName}' and cannot be null or empty.";
                nullOrEmptyError = ValidationError.Required(errorContext);
                return true;
            }
            else
                return false;
        }

        public static bool IsToLow(this int value, int minValue, ErrorContext errorContext, out ValidationError toLowError)
        {
            toLowError = default!;
            if (value < minValue)
            {
                errorContext.DescriptionSuffix = $"The field '{errorContext.FieldName}' must be greater than or equal to {minValue} for the entity '{errorContext.EntityName}'.";
                toLowError = ValidationError.TooShort(errorContext, $"Greater than or equal to {minValue}");
                return false;
            }
            else
                return true;
        }

        public static bool Matches(this string value1, string value2, ErrorContext errorContext, out ValidationError notMatchingError)
        {
            notMatchingError = default!;

            if (!string.Equals(value1, value2, StringComparison.Ordinal))
            {
                errorContext.DescriptionSuffix = $"The fields '{errorContext.FieldName}' and '{errorContext.ConfirmFieldName}' must match for the entity '{errorContext.EntityName}'.";
                notMatchingError = ValidationError.NonMatchingValues(errorContext);
                return false;
            }
            else
                return true;
        }

    }
}
