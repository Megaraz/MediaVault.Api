using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using media_vault_app.Application.DTOs.User.Response;
using UserEntity = media_vault_app.Domain.Entities.User;
using media_vault_app.Application.DTOs.User.Request;


namespace media_vault_app.Application.Interfaces.Mappers
{
    public interface IUserDtoMapper : IMapDtoToEntity<UserEntity, UserDetailedDto, UserRegisterDto, UserUpdateDto, Guid>
    {
    }
}
