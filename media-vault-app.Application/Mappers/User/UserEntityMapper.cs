using System;
using System.Collections.Generic;
using System.Linq;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Mappers.User
{
    public class UserEntityMapper : IMapEntityToDto<UserEntity, Guid, UserDetailedDto, UserMinimalDto>
    {
        public UserDetailedDto ToDetailedDTO(UserEntity entity) =>
            new(entity.Id, entity.Username, entity.Email, entity.CreatedAtUtc);

        public IEnumerable<UserDetailedDto> ToDetailedDtoCollection(IEnumerable<UserEntity> entities) =>
            entities.Select(ToDetailedDTO);

        public UserMinimalDto ToMinimalDTO(UserEntity entity) =>
            new(entity.Id, entity.Username, entity.Email);

        public IEnumerable<UserMinimalDto> ToMinimalDtoCollection(IEnumerable<UserEntity> entities) =>
            entities.Select(ToMinimalDTO);

    }
}
