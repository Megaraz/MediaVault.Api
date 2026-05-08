using System;
using System.Collections.Generic;
using System.Linq;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Mappers;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Mappers.User
{
    public class UserEntityMapper : IUserEntityMapper 
    {
        public UserDetailedDto ToDetailedDto(UserEntity entity) =>
            new(entity.Id, entity.Username, entity.Email, entity.CreatedAtUtc);

        public IReadOnlyList<UserDetailedDto> ToDetailedDtoCollection(IEnumerable<UserEntity> entities) =>
            entities.Select(ToDetailedDto).ToList();

        public UserMinimalDto ToMinimalDto(UserEntity entity) =>
            new(entity.Id, entity.Username, entity.Email);

        public IReadOnlyList<UserMinimalDto> ToMinimalDtoCollection(IEnumerable<UserEntity> entities) =>
            entities.Select(ToMinimalDto).ToList();

    }
}
