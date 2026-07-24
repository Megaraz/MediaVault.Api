using media_vault_app.Application.DTOs.User.Response;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Interfaces.Mappers
{
    public interface IUserEntityMapper : IMapEntityToDto<UserEntity, Guid, UserDetailedDto, UserMinimalDto>
    {
    }
}
