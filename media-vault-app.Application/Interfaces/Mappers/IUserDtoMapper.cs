using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using UserEntity = media_vault_app.Domain.Entities.User;


namespace media_vault_app.Application.Interfaces.Mappers
{
    public interface IUserDtoMapper : IMapDtoToEntity<UserEntity, UserDetailedDto, UserRegisterDto, UserUpdateDto, Guid>
    {
    }
}
