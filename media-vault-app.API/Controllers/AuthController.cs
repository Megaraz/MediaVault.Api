using media_vault_app.API.Security;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using media_vault_app.API.RateLimiting;
using media_vault_app.API.RequestLimits;

namespace media_vault_app.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : MediaVaultControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserReadService _userReadService;
        private readonly IUserWriteService _userWriteService;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(
            IAuthService authService,
            IUserReadService userReadService,
            IUserWriteService userWriteService,
            IJwtTokenService jwtTokenService)
        {
            _authService = authService;
            _userReadService = userReadService;
            _userWriteService = userWriteService;
            _jwtTokenService = jwtTokenService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.RegistrationByIp)]
        [RequestTimeout(MediaVaultRequestTimeoutPolicies.Authentication)]
        [RequestSizeLimit(MediaVaultWriteLimits.MaxRequestBodyBytes)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status504GatewayTimeout)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RegisterUser(
            [FromBody] UserRegisterDto createDto,
            CancellationToken ct) =>
                this.ToNoContentResult(await _authService.RegisterUserAsync(createDto, ct));

        [AllowAnonymous]
        [HttpPost("login")]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.LoginByIp)]
        [RequestTimeout(MediaVaultRequestTimeoutPolicies.Authentication)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status504GatewayTimeout)]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> LoginUser(
            [FromBody] UserLoginDto loginDto,
            CancellationToken ct)
        {
            var result = await _authService.LoginAsync(loginDto, ct);

            if (result.IsFailure)
                return this.ToActionResult(result).Result!;

            var userDto = result.Value;

            var token = _jwtTokenService.GenerateToken(userDto.Id, userDto.Username, userDto.Email);

            return Ok(new LoginResponseDto(userDto, token));
        }

        [Authorize]
        [HttpPut]
        [EnableRateLimiting(MediaVaultRateLimitPolicies.AuthenticatedWriteByUser)]
        [RequestSizeLimit(MediaVaultWriteLimits.MaxRequestBodyBytes)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponseBody), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateUser(
            [FromBody] UserUpdateDto updateDto,
            CancellationToken ct = default) =>
                !User.TryGetUserId(out var userId)
                    ? Unauthorized()
                    : this.ToNoContentResult(await _userWriteService.UpdateProfileAsync(userId, updateDto, ct));

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserDetailedDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserDetailedDto>> GetCurrentUser(CancellationToken ct = default)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _userReadService.GetByIdAsync(userId, ct);
            return this.ToActionResult(result);
        }

    }
}
