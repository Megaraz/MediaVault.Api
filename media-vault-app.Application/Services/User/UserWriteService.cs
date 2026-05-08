using System;
using System.Text;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using UserEntity = media_vault_app.Domain.Entities.User;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using Rasmus.SharedKernel.Interfaces.Validators;
using media_vault_app.Application.Services.Base_Classes;
using media_vault_app.Application.Interfaces.Mappers;
using media_vault_app.Application.Interfaces.Validators;

namespace media_vault_app.Application.Services.User
{
    public class UserWriteService
        : WriteServiceBase<UserEntity, Guid, UserRegisterDto, UserUpdateDto, UserDetailedDto>, IUserWriteService
    {
        public UserWriteService(
            IUserRepo repo,
            IUserEntityMapper entityMapper,
            IUserDtoMapper dtoMapper, 
            IUserDtoValidator validator
            ) : base(repo, entityMapper, dtoMapper, validator)
        {
        }
    }
}
