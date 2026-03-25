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

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
        {
            var result = await _userWriteService.DeleteAsync(id, ct);
            return this.ToNoContent(result);
        }

    }
}
