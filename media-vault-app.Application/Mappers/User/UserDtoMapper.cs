using System;
using System.Collections.Generic;
using System.Linq;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Mappers.User
{
    public class UserDtoMapper :
        IMapDtoToEntity<UserEntity, UserDetailedDto, UserCreateDto, Guid>,
        IMapUpdateDtoToEntity<UserEntity, Guid, UserUpdateDto>
    {
        public UserEntity ToEntity(UserCreateDto createDto) =>
            new()
            {
                Id = Guid.NewGuid(),
                Username = createDto.Username,
                Email = createDto.Email,
                PasswordHash = createDto.Password,
                CreatedAtUtc = DateTime.UtcNow
            };

        public UserEntity ToEntity(UserDetailedDto detailedDto) =>
            new()
            {
                Id = detailedDto.Id,
                Username = detailedDto.Username,
                Email = detailedDto.Email,
                CreatedAtUtc = detailedDto.CreatedAtUtc
            };

        public IEnumerable<UserEntity> ToEntities(IEnumerable<UserDetailedDto> detailedDtos) =>
            detailedDtos.Select(ToEntity);

        public UserEntity MapToEntity(Guid id, UserUpdateDto updateDto) =>
            new()
            {
                Id = id,
                Username = updateDto.UserName,
                Email = updateDto.Email
            };
    }
}
