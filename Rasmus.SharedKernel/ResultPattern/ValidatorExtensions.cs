using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.ResultPattern
{
    public static class ValidatorExtensions
    {

        /// <summary>
        /// Validates if the provided ID is valid.
        /// And creates a ValidationError if the ID is invalid, with a description that includes the field name (if provided) and the entity name from the error context.
        /// </summary>
        /// <typeparam name="TKey">The type of the ID.</typeparam>
        /// <param name="id">The ID to validate.</param>
        /// <param name="errorContext">The error context for validation.</param>
        /// <param name="idValidationError">The validation error if the ID is invalid.</param>
        /// <returns>True if the ID is valid; otherwise, false.</returns>
        /// <out name="idValidationError">The validation error if the ID is invalid.</out>
        public static bool IsValidId<TKey>(this TKey id, ErrorContext errorContext, out ValidationError idValidationError)
        {
            idValidationError = default!;

            if (!Validator.IsValidId(id))
            {
                string fieldName = string.IsNullOrWhiteSpace(errorContext.FieldName) ? nameof(id) : errorContext.FieldName;
                string descriptionSuffix = $"A valid {fieldName} is required for the entity '{errorContext.EntityName}' and cannot be null or empty.";

                idValidationError = ValidationError.Required(errorContext with { FieldName = fieldName, DescriptionSuffix = descriptionSuffix });
                return false;
            }
            return true;
        }


        public static bool IsNull<TValue>(this TValue value, ErrorContext errorContext, out ValidationError nullValueError)
        {
            nullValueError = default!;

            if (value is null)
            {
                nullValueError = ValidationError.Required(errorContext with
                {
                    DescriptionSuffix = $"A value for the entity '{errorContext.EntityName}' is required and cannot be null or empty."
                });

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
            return validationErrors.Any();
        }

        public static bool IsNullOrWhiteSpace(this string value, string fieldName, ErrorContext errorContext, out ValidationError nullOrEmptyError)
        {
            nullOrEmptyError = default!;

            if (string.IsNullOrWhiteSpace(value))
            {
                string localFieldName = string.IsNullOrWhiteSpace(fieldName) ? nameof(value) : fieldName;
                var localErrorContext = errorContext with
                {
                    FieldName = localFieldName,
                    DescriptionSuffix = $"The field '{localFieldName}' is required for the entity '{errorContext.EntityName}' and cannot be null or empty."
                };

                nullOrEmptyError = ValidationError.Required(localErrorContext);
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
                var localErrorContext = errorContext with
                {
                    DescriptionSuffix = $"The field '{errorContext.FieldName}' is required for the entity '{errorContext.EntityName}' and cannot be null or empty."
                };

                nullOrEmptyError = ValidationError.Required(localErrorContext);
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
                var localErrorContext = errorContext with
                {
                    DescriptionSuffix = $"The field '{errorContext.FieldName}' must be greater than or equal to {minValue} for the entity '{errorContext.EntityName}'."
                };

                toLowError = ValidationError.OutOfRange(localErrorContext, $"Greater than or equal to {minValue}");
                return false;
            }
            else
                return true;
        }

        //public static bool Matches<TOriginal, TConfirm>(
        //    ErrorContext errorContext,
        //    out ValidationError notMatchingError)
        //{
        //    notMatchingError = default!;

        //    if (typeof(TOriginal) != typeof(TConfirm))
        //    {
        //        var localErrorContext = errorContext with
        //        {
        //            DescriptionSuffix = $"The types '{typeof(TOriginal).Name}' and '{typeof(TConfirm).Name}' must match be matching"
        //        };

        //        notMatchingError = ValidationError.NonMatchingValues(localErrorContext);
        //        return false;
        //    }
        //    else
        //        return true;
        //}

        public static bool Matches(this string value1, string value2, ErrorContext errorContext, out ValidationError notMatchingError)
        {
            notMatchingError = default!;

            if (!string.Equals(value1, value2, StringComparison.Ordinal))
            {
                var localErrorContext = errorContext with
                {
                    DescriptionSuffix = $"The fields '{errorContext.FieldName}' and '{errorContext.ConfirmFieldName}' must match for the entity '{errorContext.EntityName}'."
                };

                notMatchingError = ValidationError.NonMatchingValues(localErrorContext);
                return false;
            }
            else
                return true;
        }

    }
}
