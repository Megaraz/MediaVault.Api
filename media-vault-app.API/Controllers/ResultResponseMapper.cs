using Microsoft.AspNetCore.Mvc;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.API.Controllers
{
    /// <summary>
    /// Thin ASP.NET adapter that converts domain <see cref="Result"/> instances into ASP.NET
    /// <see cref="ActionResult"/> types, using <see cref="HttpResultMapper"/> for framework-agnostic HTTP mapping.
    /// </summary>
    public static class ResultResponseMapper
    {
        /// <summary>
        /// Maps a <see cref="Result{TValue}"/> to a 200 OK <see cref="ActionResult{TValue}"/> on success,
        /// or the appropriate error response on failure.
        /// </summary>
        public static ActionResult<TValue> ToActionResult<TValue>(this ControllerBase c, Result<TValue> result)
        {
            var response = HttpResultMapper.ToHttpResponse(result);
            return c.ToActionResult<TValue>(response);
        }

        /// <summary>
        /// Maps a <see cref="Result"/> to a 200 OK <see cref="IActionResult"/> on success,
        /// or the appropriate error response on failure.
        /// </summary>
        public static IActionResult ToActionResult(this ControllerBase c, Result result)
        {
            var response = HttpResultMapper.ToHttpResponse(result);
            return c.ToActionResult(response);
        }

        /// <summary>
        /// Maps a <see cref="Result"/> to a 204 No Content <see cref="IActionResult"/> on success,
        /// or the appropriate error response on failure.
        /// </summary>
        public static IActionResult ToNoContentResult(this ControllerBase c, Result result)
        {
            var response = HttpResultMapper.ToNoContentResponse(result);
            return c.ToActionResult(response);
        }

        /// <summary>
        /// Maps a <see cref="Result{TValue}"/> to a 201 Created <see cref="ActionResult{TValue}"/> on success
        /// using ASP.NET's <see cref="ControllerBase.CreatedAtAction"/>,
        /// or the appropriate error response on failure.
        /// </summary>
        public static ActionResult<TValue> ToCreatedResult<TValue>(
            this ControllerBase c,
            Result<TValue> result,
            string actionName,
            Func<TValue, object> routeValuesFactory)
        {
            if (result.IsFailure)
            {
                var failureResponse = HttpResultMapper.ToHttpResponse(result);
                return c.ToActionResult<TValue>(failureResponse);
            }

            return c.CreatedAtAction(actionName, routeValuesFactory(result.Value), result.Value);
        }

        private static ActionResult<TValue> ToActionResult<TValue>(this ControllerBase c, MappedHttpResponse response)
        {
            return response.StatusCode switch
            {
                200 => c.Ok(response.Body),
                201 when response.Location is not null => new ActionResult<TValue>(c.Created(response.Location, response.Body)),
                _ => new ActionResult<TValue>(c.StatusCode(response.StatusCode, response.Body))
            };
        }

        private static IActionResult ToActionResult(this ControllerBase c, MappedHttpResponse response)
        {
            return response.StatusCode switch
            {
                200 when response.Body is not null => c.Ok(response.Body),
                200 => c.Ok(),
                201 when response.Location is not null => c.Created(response.Location, response.Body),
                204 => c.NoContent(),
                _ => c.StatusCode(response.StatusCode, response.Body)
            };
        }
    }
}
