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

            var dto = result.IsSuccess
                ? new UserDetailedDto(result.Value.Id, result.Value.Username, result.Value.Email, result.Value.CreatedAtUtc)
                : null;


            return this.ToCreated(result, nameof(GetUserById), value => new { id = dto!.Id });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailedDto>>> GetAllUsers(CancellationToken ct)
        {
            var result = await _userReadService.GetDetailedCollectionAsync(ct: ct);

            if (result.IsSuccess)
            {
                var users = result.Value;
                return Ok(users.Select(user => new UserDetailedDto(user.Id, user.Username, user.Email, user.CreatedAtUtc)));
            }

            return result.PrimaryError.Type switch
            {
                ErrorType.NotFound => NotFound(result.PrimaryError.Description),
                ErrorType.Validation => BadRequest(string.Join(", \n", result.ValidationErrors.Select(ve => ve.Description))),
                ErrorType.Unauthorized => Unauthorized(result.PrimaryError.Description),
                ErrorType.Conflict => BadRequest(string.Join(", \n", result.ValidationErrors.Select(ve => ve.Description))),
                ErrorType.Forbidden => Forbid(result.PrimaryError.Description),
                ErrorType.Failure => StatusCode(500, result.PrimaryError.Description),
                _ => StatusCode(500, "An unexpected error occurred.")
            };
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<UserDetailedDto>> GetUserById(Guid id, CancellationToken ct)
        {

            var result = await _userReadService.GetByIdAsync(id, ct);

            if (result.IsSuccess)
            {
                var user = result.Value;
                return Ok(new UserDetailedDto(user.Id, user.Username, user.Email, user.CreatedAtUtc));
            }

            Debug.WriteLine($"PrimaryError Description: {result.PrimaryError.Description}");

            return result.PrimaryError.Type switch
            {
                //ErrorType.NotFound => NotFound(result.PrimaryError.Description),
                ErrorType.NotFound => NotFound("TEST TEST TEST"),
                ErrorType.Validation => BadRequest(string.Join(", \n", result.ValidationErrors.Select(ve => ve.Description))),
                ErrorType.Unauthorized => Unauthorized(result.PrimaryError.Description),
                ErrorType.Conflict => BadRequest(string.Join(", \n", result.ValidationErrors.Select(ve => ve.Description))),
                ErrorType.Forbidden => Forbid(result.PrimaryError.Description),
                ErrorType.Failure => StatusCode(500, result.PrimaryError.Description),
                _ => StatusCode(500, "An unexpected error occurred.")
            };
        }
    }
}
