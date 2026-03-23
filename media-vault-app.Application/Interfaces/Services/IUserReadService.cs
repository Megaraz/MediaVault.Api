using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces.Services;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IUserReadService : IReadService<User, Guid, UserDetailedDto, UserMinimalDto>
    {
    }
}
