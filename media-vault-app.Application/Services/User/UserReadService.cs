using System;
using System.Text;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Services.Base_Classes;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Services.User
{
    public class UserReadService : ReadServiceBase<UserEntity, Guid, UserDetailedDto, UserMinimalDto>, IUserReadService
    {
        public UserReadService(
            IUserRepo repo,
            IMapEntityToDto<UserEntity, Guid, UserDetailedDto, UserMinimalDto> entityMapper
            ) : base(repo, entityMapper)
        {
        }
    }
}
