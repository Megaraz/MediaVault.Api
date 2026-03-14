using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{
    internal static class GenericRepoHelpers
    {
        internal static Result CheckNullOrDefault<T>(
            T valueToCheck, 
            string entityName, 
            string currentOperation,
            string errorDescriptionPrefix)
        {
            if (valueToCheck is null || valueToCheck.Equals(default(T)))
            {
                var errorCode = new ErrorCode(
                    currentOperation,
                    entityName,
                    ErrorCodes.ValidationError.Required);

                var errorMessageReason = "Value cannot be null or default";

                return Result.Failure(
                    new Error(errorCode,
                        $"{errorDescriptionPrefix}: {errorMessageReason}",
                        ErrorType.Validation),
                    errorMessageReason);
            }

            return Result.Success();
        }
    }
}