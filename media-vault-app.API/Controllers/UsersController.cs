using media_vault_app.Application.DTOs.User.Request;
using Rasmus.SharedKernel.ResultPattern;
using Microsoft.AspNetCore.Mvc;
using media_vault_app.Application.DTOs.User.Response;
using System.Diagnostics;
using media_vault_app.Application.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace media_vault_app.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly IUserReadService _userReadService;
        private readonly IUserWriteService _userWriteService;

        public UsersController(IUserReadService userReadService, IUserWriteService userWriteService)
        {
            _userReadService = userReadService;
            _userWriteService = userWriteService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDetailedDto>> RegisterUser([FromBody] UserCreateDto createDto, CancellationToken ct)
        {

            var result = await _userWriteService.CreateAsync(createDto, ct);

            return this.ToCreated(result, nameof(GetUserById), value => new { id = result.Value.Id });
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDetailedDto>> LoginUser([FromBody] UserLoginDto loginDto, CancellationToken ct)
        {
            var result = await _userWriteService.LoginAsync(loginDto, ct);

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailedDto>>> GetUsers(CancellationToken ct)
        {
            var result = await _userReadService.GetDetailedCollectionAsync(ct: ct);

            return this.ToOk(result);
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<UserDetailedDto>> GetUserById(Guid id, CancellationToken ct)
        {

            var result = await _userReadService.GetByIdAsync(id, ct);

            return this.ToOk(result);
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto updateDto, CancellationToken ct)
        {
            var result = await _userWriteService.UpdateUserInfoAsync(id, updateDto, ct);
            return this.ToNoContent(result);
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
        {
            var result = await _userWriteService.DeleteAsync(id, ct);
            return this.ToNoContent(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            await HttpContext.SignOutAsync();
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDetailedDto>> GetCurrentUser(CancellationToken ct)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var result = await _userReadService.GetByIdAsync(userId, ct);
            return this.ToOk(result);
        }
    }
}
