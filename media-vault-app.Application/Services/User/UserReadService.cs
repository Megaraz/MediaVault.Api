using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Mappers;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Services.Base_Classes;
using Microsoft.Extensions.Logging;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Services.User
{
    public class UserReadService : ReadServiceBase<UserEntity, Guid, UserDetailedDto, UserMinimalDto>, IUserReadService
    {
        public UserReadService(
            IUserRepo repo,
            IUserEntityMapper entityMapper,
            ILogger<UserReadService> logger
            ) : base(repo, entityMapper, logger)
        {
        }
    }
}
