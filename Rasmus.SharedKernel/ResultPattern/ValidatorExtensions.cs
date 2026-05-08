using System;
using System.Collections.Generic;

namespace Rasmus.SharedKernel.ResultPattern
{
    /// <summary>
    /// Validation extension methods that produce <see cref="ValidationError"/> instances.
    /// </summary>
    /// <remarks>
    /// Convention: all methods return <see langword="true"/> when the validation check <b>fails</b>
    /// (i.e., the invalid condition is detected). This matches the BCL pattern used by
    /// <see cref="string.IsNullOrWhiteSpace"/> and makes validation code read naturally without double-negatives:
    /// <code>if (value.IsNullOrWhiteSpace(ctx, out var e)) errors.Add(e);</code>
    /// </remarks>
    public static class ValidatorExtensions
    {

        /// <summary>
        /// Returns <see langword="true"/> if the id is <b>not valid</b> and populates <paramref name="idValidationError"/>.
        /// Returns <see langword="false"/> if the id is valid.
        /// </summary>
        public static bool IsNotValidId<TKey>(this TKey id, ErrorContext errorContext, out ValidationError idValidationError)
        {
            idValidationError = default!;

            if (!Validator.IsValidId(id))
            {
                string fieldName = string.IsNullOrWhiteSpace(errorContext.FieldName) ? nameof(id) : errorContext.FieldName;
                string descriptionSuffix = $"A valid {fieldName} is required for the entity '{errorContext.EntityName}' and cannot be null or empty.";

                idValidationError = ValidationError.Required(errorContext with { FieldName = fieldName, DescriptionSuffix = descriptionSuffix });
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="value"/> is <see langword="null"/>
        /// and populates <paramref name="nullValueError"/>. Returns <see langword="false"/> otherwise.
        /// </summary>
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

            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> if any field in <paramref name="requiredValues"/> is null or whitespace
        /// and populates <paramref name="validationErrors"/> with one error per failing field.
        /// Returns <see langword="false"/> if all fields have content.
        /// </summary>
        public static bool RequiredFieldsAreNullOrWhiteSpace(
            this IEnumerable<(string FieldName, string Value)> requiredValues,
            ErrorContext errorContext,
            out IReadOnlyList<ValidationError> validationErrors)
        {
            var errors = new List<ValidationError>();

            foreach (var (fieldName, value) in requiredValues)
            {
                if (value.IsNullOrWhiteSpace(fieldName, errorContext, out var error))
                    errors.Add(error);
            }

            validationErrors = errors;
            return errors.Count > 0;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="value"/> is null or whitespace
        /// and populates <paramref name="nullOrEmptyError"/>. Returns <see langword="false"/> otherwise.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string value, string fieldName, ErrorContext errorContext, out ValidationError nullOrEmptyError)
        {
            nullOrEmptyError = default!;

            if (string.IsNullOrWhiteSpace(value))
            {
                string resolvedFieldName = string.IsNullOrWhiteSpace(fieldName) ? errorContext.FieldName ?? nameof(value) : fieldName;

                nullOrEmptyError = ValidationError.Required(errorContext with
                {
                    FieldName = resolvedFieldName,
                    DescriptionSuffix = $"The field '{resolvedFieldName}' is required for the entity '{errorContext.EntityName}' and cannot be null or empty."
                });
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="value"/> is null or whitespace
        /// and populates <paramref name="nullOrEmptyError"/>. Returns <see langword="false"/> otherwise.
        /// Uses <see cref="ErrorContext.FieldName"/> as the field label in the error description.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string value, ErrorContext errorContext, out ValidationError nullOrEmptyError)
        {
            nullOrEmptyError = default!;

            if (string.IsNullOrWhiteSpace(value))
            {
                nullOrEmptyError = ValidationError.Required(errorContext with
                {
                    DescriptionSuffix = $"The field '{errorContext.FieldName}' is required for the entity '{errorContext.EntityName}' and cannot be null or empty."
                });
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="value"/> is below <paramref name="minValue"/>
        /// and populates <paramref name="tooLowError"/>. Returns <see langword="false"/> otherwise.
        /// </summary>
        public static bool IsTooLow(this int value, int minValue, ErrorContext errorContext, out ValidationError tooLowError)
        {
            tooLowError = default!;

            if (value < minValue)
            {
                tooLowError = ValidationError.OutOfRange(errorContext with
                {
                    DescriptionSuffix = $"The field '{errorContext.FieldName}' must be greater than or equal to {minValue} for the entity '{errorContext.EntityName}'."
                }, $">= {minValue}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="value1"/> and <paramref name="value2"/> do <b>not</b> match
        /// and populates <paramref name="notMatchingError"/>. Returns <see langword="false"/> if they match.
        /// Comparison is ordinal (case-sensitive).
        /// </summary>
        public static bool DoesNotMatch(this string value1, string value2, ErrorContext errorContext, out ValidationError notMatchingError)
        {
            notMatchingError = default!;

            if (!string.Equals(value1, value2, StringComparison.Ordinal))
            {
                notMatchingError = ValidationError.NonMatchingValues(errorContext with
                {
                    DescriptionSuffix = $"The fields '{errorContext.FieldName}' and '{errorContext.ConfirmFieldName}' must match for the entity '{errorContext.EntityName}'."
                });
                return true;
            }

            return false;
        }

    }
}
