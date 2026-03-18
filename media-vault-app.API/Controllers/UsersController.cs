using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.ResultPattern;
using Microsoft.AspNetCore.Mvc;
using media_vault_app.Application.DTOs.User.Response;
using System.Diagnostics;

namespace media_vault_app.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly IUserRepo _userRepo;

        public UsersController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpPost]
        public async Task<ActionResult<UserDetailedDto>> CreateUser([FromBody] UserCreateDto userCreateDto, CancellationToken ct)
        {
            var user = new User
            {
                Username = userCreateDto.Username,
                Email = userCreateDto.Email,
                PasswordHash = userCreateDto.Password, // In a real application, you should hash the password before storing it,
                CreatedAtUtc = DateTime.UtcNow

            };

            //return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new UserDetailedDto(user.Id, user.Username, user.Email, user.CreatedAtUtc));
            var result = await _userRepo.CreateAsync(user, ct);

            var dto = result.IsSuccess
                ? new UserDetailedDto(result.Value.Id, result.Value.Username, result.Value.Email, result.Value.CreatedAtUtc)
                : null;


            return this.ToCreated(result, nameof(GetUserById), value => new { id = dto!.Id });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailedDto>>> GetAllUsers(CancellationToken ct)
        {
            var result = await _userRepo.GetAllAsync(ct);

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

            var result = await _userRepo.GetByIdAsync(id, ct);

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
