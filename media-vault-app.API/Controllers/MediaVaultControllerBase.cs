using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Controllers;

/// <summary>
/// Publishes the response metadata shared by controllers that use
/// <see cref="ResultResponseMapper"/>.
/// </summary>
[ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status400BadRequest)]
[ProducesResponseType(
    typeof(MediaVaultAuthorizationProblemDetails),
    StatusCodes.Status401Unauthorized,
    "application/problem+json")]
[ProducesResponseType(
    typeof(MediaVaultAuthorizationProblemDetails),
    StatusCodes.Status403Forbidden,
    "application/problem+json")]
[ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status409Conflict)]
[ProducesResponseType(typeof(ValidationErrorResponseBody), StatusCodes.Status422UnprocessableEntity)]
[ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
[ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status502BadGateway)]
[ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status503ServiceUnavailable)]
public abstract class MediaVaultControllerBase : ControllerBase;
