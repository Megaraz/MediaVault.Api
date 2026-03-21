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
                errorContext.FieldName = nameof(id);
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

        public static bool AnyIsNullOrWhiteSpace(this IEnumerable<string> values, ErrorContext errorContext, out ValidationError nullOrEmptyError)
        {
            nullOrEmptyError = default!;

            if (values.IsNull(errorContext, out var nullValueError))
            {
                nullOrEmptyError = nullValueError;
                return true;
            }

            if (values.Any(string.IsNullOrWhiteSpace))
            {
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

        public static bool Matches(this string value1, string value2, ErrorContext errorContext, out ValidationError notMatchingError)
        {
            notMatchingError = default!;

            if (!string.Equals(value1, value2, StringComparison.Ordinal))
            {
                errorContext.DescriptionSuffix = $"The fields '{errorContext.FieldName}' and '{errorContext.ConfirmFieldName}' must match for the entity '{errorContext.EntityName}'.";
                notMatchingError = ValidationError.NonMatchingValues(errorContext);
                return true;
            }
            else
                return false;
        }

    }
}
