using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using UserEntity = media_vault_app.Domain.Entities.User;
using Rasmus.SharedKernel.ResultPattern;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using Rasmus.SharedKernel.Interfaces.Validators;

namespace media_vault_app.Application.Services.User
{
    public class UserWriteService
        : WriteServiceBase<UserEntity, Guid, UserRegisterDto, UserUpdateDto, UserDetailedDto>, IUserWriteService
    {
        public UserWriteService(
            IUserRepo repo,
            IMapEntityToDto<UserEntity, Guid, UserDetailedDto, UserMinimalDto> entityMapper,
            IMapDtoToEntity<UserEntity, UserDetailedDto, UserRegisterDto, UserUpdateDto, Guid> dtoMapper,
            IDtoValidator<Guid, UserRegisterDto, UserUpdateDto> validator
            ) : base(repo, entityMapper, dtoMapper, validator)
        {
        }
    }
}
