using System;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Repos
{
    public interface IUserRepo : IGenericRepo<User, Guid>
    {
        Task<Result> RegisterUserAsync(User entity, CancellationToken ct = default);
        Task<Result<bool>> IsUserNameAvailable(string username, CancellationToken ct = default);
        Task<Result<bool>> IsEmailAvailable(string email, CancellationToken ct = default);
        Task<Result<User>> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default);
    }
}
