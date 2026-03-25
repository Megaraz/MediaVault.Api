using System;
using System.Runtime.CompilerServices;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IUserWriteService : IWriteService<User, Guid, UserRegisterDto, UserDetailedDto>
    {
        Task<Result> UpdateUserInfoAsync(Guid id, UserUpdateDto updateDto, CancellationToken ct = default);
    }
}
