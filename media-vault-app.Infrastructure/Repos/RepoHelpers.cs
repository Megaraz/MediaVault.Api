using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{
    internal static class RepoHelpers
    {

        public static Result ValidateId<TKey>(TKey id, string currentOperation, string errorDescriptionPrefix)
        {
            if (id is null || id.Equals(default(TKey)))
            {
                var nullValueError = ValidationError.NullValue<TKey>(
                    currentOperation,
                    errorDescriptionPrefix,
                    out string errorMessageReason);

                return Result.ValidationFailure([nullValueError], errorMessageReason);
            }
            return Result.Success();
        }
    }
}