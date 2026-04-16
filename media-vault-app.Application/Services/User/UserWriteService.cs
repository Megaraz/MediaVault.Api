using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using UserEntitiy = media_vault_app.Domain.Entities.User;
using Rasmus.SharedKernel.ResultPattern;
using media_vault_app.Application.Mappers.User;
using media_vault_app.Application.Validators.User;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using Rasmus.SharedKernel.Interfaces.Validators;

namespace media_vault_app.Application.Services.User
{
    public class UserWriteService
        : WriteServiceBase<UserEntitiy, Guid, UserRegisterDto, UserUpdateDto, UserDetailedDto>, IUserWriteService
    {
        public UserWriteService(
            IGenericRepo<UserEntitiy, Guid> repo, 
            IMapEntityToDetailedDto<UserEntitiy, UserDetailedDto> entityToDtoMapper, 
            IMapDtoToEntity<UserEntitiy, UserDetailedDto, UserRegisterDto, UserUpdateDto, Guid> dtoToEntityMapper, 
            IDtoValidator<Guid, UserRegisterDto, UserUpdateDto> dtoValidator
            ) : base(repo, entityToDtoMapper, dtoToEntityMapper, dtoValidator)
        {
        }
    }
}
