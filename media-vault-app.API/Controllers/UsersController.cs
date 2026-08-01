using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : MediaVaultControllerBase
    {

        private readonly IUserReadService _userReadService;
        private readonly IUserWriteService _userWriteService;

        public UsersController(IUserReadService userReadService, IUserWriteService userWriteService)
        {
            _userReadService = userReadService;
            _userWriteService = userWriteService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<UserDetailedDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<UserDetailedDto>>> GetUsers(CancellationToken ct)
        {
            var result = await _userReadService.GetDetailedCollectionAsync(ct: ct);

            return this.ToActionResult(result);
        }

        [HttpGet("{id:Guid}")]
        [ProducesResponseType(typeof(UserDetailedDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserDetailedDto>> GetUserById(Guid id, CancellationToken ct)
        {

            var result = await _userReadService.GetByIdAsync(id, ct);

            return this.ToActionResult(result);
        }

        [HttpDelete("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
        {
            var result = await _userWriteService.DeleteAsync(id, ct);
            return this.ToNoContentResult(result);
        }

    }
}
