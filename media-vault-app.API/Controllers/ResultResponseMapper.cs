using Microsoft.AspNetCore.Mvc;
using Megaraz.ResultPattern;
using PackageMvcMapper = Megaraz.ResultPattern.AspNetCore.AspNetCoreResultExtensions;
using PackageHttpPolicy = Megaraz.ResultPattern.AspNetCore.HttpResultMappingPolicy;

namespace media_vault_app.API.Controllers
{
    /// <summary>
    /// Thin ASP.NET adapter that converts domain <see cref="Result"/> instances through the package mapper.
    /// </summary>
    public static class ResultResponseMapper
    {
        private static readonly PackageHttpPolicy MediaVaultHttpResultMappingPolicy =
            PackageHttpPolicy.Default with
            {
                ErrorTypeStatusCode = errorType =>
                    errorType switch
                    {
                        ErrorType.Validation => 422,
                        ErrorType.NotFound => 404,
                        ErrorType.Conflict => 409,
                        ErrorType.Unauthorized => 401,
                        ErrorType.Forbidden => 403,
                        ErrorType.Failure => 500,
                        ErrorType.Cancelled => 503,
                        ErrorType.External => 500,
                        _ => 400
                    },
                FailureBodyFactory = CreateFailureBody
            };

        /// <summary>
        /// Maps a <see cref="Result{TValue}"/> to a 200 OK <see cref="ActionResult{TValue}"/> on success,
        /// or the appropriate error response on failure.
        /// </summary>
        public static ActionResult<TValue> ToActionResult<TValue>(this ControllerBase c, Result<TValue> result)
            where TValue : notnull =>
            PackageMvcMapper.ToActionResult(c, result, MediaVaultHttpResultMappingPolicy);

        /// <summary>
        /// Maps a <see cref="Result"/> to a 200 OK <see cref="IActionResult"/> on success,
        /// or the appropriate error response on failure.
        /// </summary>
        public static IActionResult ToActionResult(this ControllerBase c, Result result) =>
            PackageMvcMapper.ToActionResult(c, result, MediaVaultHttpResultMappingPolicy);

        /// <summary>
        /// Maps a <see cref="Result"/> to a 204 No Content <see cref="IActionResult"/> on success,
        /// or the appropriate error response on failure.
        /// </summary>
        public static IActionResult ToNoContentResult(this ControllerBase c, Result result) =>
            PackageMvcMapper.ToNoContentResult(c, result, MediaVaultHttpResultMappingPolicy);

        /// <summary>
        /// Maps a <see cref="Result{TValue}"/> to a 201 Created response on success using ASP.NET's
        /// <see cref="ControllerBase.CreatedAtAction"/>, or the appropriate error response on failure.
        /// </summary>
        public static ActionResult<TValue> ToCreatedResult<TValue>(
            this ControllerBase c,
            Result<TValue> result,
            string actionName,
            Func<TValue, object> routeValuesFactory)
            where TValue : notnull
            => PackageMvcMapper.ToCreatedResult(
                c,
                result,
                actionName,
                routeValuesFactory,
                MediaVaultHttpResultMappingPolicy);

        private static object CreateFailureBody(Result result)
        {
            if (result.PrimaryError.Type == ErrorType.Validation)
            {
                var validationErrors = result.ValidationErrors
                    .Select(error => new ValidationErrorItem(error.FieldName, error.UserMessage))
                    .ToArray();
                return new ValidationErrorResponseBody(result.Message, validationErrors);
            }

            return new ErrorResponseBody(result.Message, result.PrimaryError.Code);
        }
    }
}
