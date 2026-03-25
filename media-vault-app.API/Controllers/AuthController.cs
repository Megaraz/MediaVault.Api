using System.Security.Claims;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(
            [FromBody] UserRegisterDto createDto,
            CancellationToken ct = default) =>
                this.ToOk(await authService.RegisterUserAsync(createDto, ct));


        [HttpPost("login")]
        public async Task<ActionResult<UserDetailedDto>> LoginUser(
            [FromBody] UserLoginDto loginDto,
            CancellationToken ct = default)
        {
            var result = await authService.LoginAsync(loginDto, ct);

            if (result.IsFailure)
            {
                return this.ToOk(result);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.Value.Id.ToString()),
                new(ClaimTypes.Name, result.Value.Username),
                new(ClaimTypes.Email, result.Value.Email)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return this.ToOk(result);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateUser(
            IUserWriteService userWriteService,
            [FromBody] UserUpdateDto updateDto,
            CancellationToken ct = default) =>
                !TryGetCurrentUserId(out var userId)
                    ? Unauthorized()
                    : this.ToNoContent(await userWriteService.UpdateUserInfoAsync(userId, updateDto, ct));

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct = default)
        {
            await HttpContext.SignOutAsync();
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDetailedDto>> GetCurrentUser(
            IUserReadService userReadService,
            CancellationToken ct = default)
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            var result = await userReadService.GetByIdAsync(userId, ct);
            return this.ToOk(result);
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out userId))
            {
                return false;
            }
            return true;
        }
    }
}
