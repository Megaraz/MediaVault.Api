using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{
    internal static class RepoHelpers
    {

        public static Result GenerateNullValueResult<TKey>(string currentOperation, string errorDescriptionPrefix)
        {
            var nullValueError = ValidationError.Required<TKey>(
                currentOperation,
                errorDescriptionPrefix,
                out string errorMessageReason);

            return Result.ValidationFailure([nullValueError], errorMessageReason);
        }
        public static Result<TValue> GenerateNullValueResult<TValue, TKey>(string currentOperation, string errorDescriptionPrefix)
        {
            var nullValueError = ValidationError.Required<TKey>(
                currentOperation,
                errorDescriptionPrefix,
                out string errorMessageReason);

            return Result<TValue>.ValidationFailure([nullValueError], errorMessageReason);
        }
    }
}