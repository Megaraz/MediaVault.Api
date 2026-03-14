using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.ResultPattern;
using Microsoft.AspNetCore.Mvc;
using media_vault_app.Application.DTOs.User.Response;

namespace media_vault_app.Server.Controllers
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
            var result = await _userRepo.CreateAsync(user, ct);
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new UserDetailedDto(user.Id, user.Username, user.Email, user.CreatedAtUtc));
            }
            else if (result is ErrorResult<User> errorResult)
            {
                return BadRequest(errorResult.Errors);
            }
            else
            {
                return BadRequest("An unexpected error occurred.");
            }
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<UserDetailedDto>> GetUserById(Guid id, CancellationToken ct)
        {

            var result = await _userRepo.GetByIdAsync(id, ct);

            return result switch
            {
                SuccessResult<User> successResult =>
                    Ok(new UserDetailedDto(successResult.Data.Id, successResult.Data.Username, successResult.Data.Email, successResult.Data.CreatedAtUtc)),
                ErrorResult<User> errorResult => BadRequest(errorResult.Errors),
                _ => BadRequest("An unexpected error occurred.")
            };


        }
    }
}
