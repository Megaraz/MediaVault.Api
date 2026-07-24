using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Mappers;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Interfaces.Validators;
using media_vault_app.Application.Services.Base_Classes;
using Microsoft.Extensions.Logging;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Services.User
{
    public class UserWriteService
        : WriteServiceBase<UserEntity, Guid, UserRegisterDto, UserUpdateDto, UserDetailedDto>, IUserWriteService
    {
        public UserWriteService(
            IUserRepo repo,
            IUserEntityMapper entityMapper,
            IUserDtoMapper dtoMapper,
            IUserDtoValidator validator,
            ILogger<UserWriteService> logger
            ) : base(repo, entityMapper, dtoMapper, validator, logger)
        {
        }
    }
}
