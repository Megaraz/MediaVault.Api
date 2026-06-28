using System.Security.Claims;
using media_vault_app.API.Security;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
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

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(
            [FromBody] UserRegisterDto createDto,
            CancellationToken ct = default) =>
                this.ToActionResult(await _authService.RegisterUserAsync(createDto, ct));

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(
            [FromBody] UserLoginDto loginDto,
            CancellationToken ct = default)
        {
            var result = await _authService.LoginAsync(loginDto, ct);

            if (result.IsFailure)
                return this.ToActionResult(result).Result!;

            var token = _jwtTokenService.GenerateToken(result.Value);
            return Ok(new LoginResponseDto(result.Value, token));
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateUser(
            [FromBody] UserUpdateDto updateDto,
            CancellationToken ct = default) =>
                !TryGetCurrentUserId(out var userId)
                    ? Unauthorized()
                    : this.ToNoContentResult(await _userWriteService.UpdateAsync(userId, updateDto, ct));

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDetailedDto>> GetCurrentUser(CancellationToken ct = default)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await _userReadService.GetByIdAsync(userId, ct);
            return this.ToActionResult(result);
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out userId))
                return false;
            return true;
        }
    }
}
