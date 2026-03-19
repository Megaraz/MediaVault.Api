using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces.Services;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IUserWriteService : IWriteService<User, Guid, UserCreateDto, UserUpdateDto, UserDetailedDto>
    {
    }
}
