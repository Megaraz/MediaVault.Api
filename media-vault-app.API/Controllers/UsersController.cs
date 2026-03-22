using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.ResultPattern;
using Microsoft.AspNetCore.Mvc;
using media_vault_app.Application.DTOs.User.Response;
using System.Diagnostics;
using media_vault_app.Application.Interfaces.Services;

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

        [HttpPost]
        public async Task<ActionResult<UserDetailedDto>> CreateUser([FromBody] UserCreateDto createDto, CancellationToken ct)
        {

            var result = await _userWriteService.CreateAsync(createDto, ct);

            return this.ToCreated(result, nameof(GetUserById), value => new { id = result.Value.Id });
        }

        [HttpPost]
        public async Task<ActionResult<UserDetailedDto>> LoginUser([FromBody] UserLoginDto loginDto, CancellationToken ct)
        {
            var result = await _userWriteService.LoginAsync(loginDto, ct);

            return this.ToOk(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailedDto>>> GetAllUsers(CancellationToken ct)
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
    }
}
