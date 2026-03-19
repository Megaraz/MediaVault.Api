using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Services;
using UserEntitiy = media_vault_app.Domain.Entities.User;
using Rasmus.SharedKernel.ResultPattern;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;

namespace media_vault_app.Application.Services.User
{
    public class UserWriteService : WriteServiceBase<UserEntitiy, Guid, UserCreateDto, UserUpdateDto, UserDetailedDto>, IUserWriteService
    {
        public UserWriteService(
                IGenericRepo<UserEntitiy, Guid> repo,
                IMapEntityToDetailedDto<UserEntitiy, UserDetailedDto> entityToDtoMapper,
                IMapDtoToEntity<UserEntitiy, UserDetailedDto, UserCreateDto, Guid, UserUpdateDto> dtoToEntityMapper
            ) : base(repo, entityToDtoMapper, dtoToEntityMapper)
        {
        }

        public override async Task<Result<UserDetailedDto>> CreateAsync(UserCreateDto createDto, CancellationToken ct)
        {

            // TODO : Add hashing of password and validation of email and username uniqueness here before calling base.CreateAsync

            return await base.CreateAsync(createDto, ct);
        }


    }
}
