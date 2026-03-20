using System;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Repos
{
    public interface IUserRepo : IGenericRepo<User, Guid>
    {
        Task<Result<User>> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default);
    }
}
