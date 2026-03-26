using media_vault_app.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.API.Controllers
{

    public static class ResultResponseMapper
    {
        public static ActionResult<TValue> ToOk<TValue>(this ControllerBase c, Result<TValue> result)
        {
            return result.IsSuccess
                ? c.Ok(result.Value)
                : c.ToFailureResponse(result);
        }

        public static IActionResult ToOk(this ControllerBase c, Result result)
        {
            return result.IsSuccess
                ? c.Ok()
                : c.ToFailureResponse(result);
        }

        public static IActionResult ToNoContent(this ControllerBase c, Result result)
        {
            return result.IsSuccess
                ? c.NoContent()
                : c.ToFailureResponse(result);
        }

        public static ActionResult<TValue> ToCreated<TValue>(
            this ControllerBase c,
            Result<TValue> result,
            string actionName,
            Func<TValue, object> routeValuesFactory)
        {
            return result.IsSuccess
                ? c.CreatedAtAction(actionName, routeValuesFactory(result.Value), result.Value)
                : c.ToFailureResponse(result);
        }

        private static ActionResult<TValue> ToFailureResponse<TValue>(this ControllerBase c, Result<TValue> result)
            => new(c.BuildFailureResponse(
                result.Message,
                result.PrimaryError.Type,
                result.PrimaryError.Code,
                result.ValidationErrors?.Select(x => x.Code)));

        private static IActionResult ToFailureResponse(this ControllerBase c, Result result)
            => c.BuildFailureResponse(
                result.Message,
                result.PrimaryError.Type,
                result.PrimaryError.Code,
                result.ValidationErrors?.Select(x => x.Code));

        private static ActionResult BuildFailureResponse(
            this ControllerBase c,
            string message,
            ErrorType errorType,
            string errorCode,
            IEnumerable<string>? validationErrors)
        {

            var responseDto = new ResponseDto(message, errorCode);

            return errorType switch
            {
                ErrorType.Validation => c.UnprocessableEntity(new ValidationResponseDto(message, validationErrors)),
                ErrorType.NotFound => c.NotFound(responseDto),
                ErrorType.Conflict => c.Conflict(responseDto),
                ErrorType.Unauthorized => c.Unauthorized(responseDto),
                ErrorType.Forbidden => c.StatusCode(403, responseDto),
                ErrorType.Failure => c.StatusCode(500, responseDto),
                ErrorType.Database => c.StatusCode(500, responseDto),
                _ => c.BadRequest(responseDto)
            };
        }
    }
}
