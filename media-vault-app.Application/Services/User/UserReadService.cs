using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Mappers.User;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Services.User
{
    public class UserReadService : ReadServiceBase<UserEntity, Guid, UserDetailedDto, UserMinimalDto>, IUserReadService
    {
        public UserReadService(
            IGenericRepo<UserEntity, Guid> repo
            ) : base(repo, new UserEntityMapper())
        {
        }
    }
}
