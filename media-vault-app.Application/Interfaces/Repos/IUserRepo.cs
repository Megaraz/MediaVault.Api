using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Repos
{
    public interface IUserRepo : IRepo<User, Guid>
    {
        Task<Result> RegisterUserAsync(User entity, CancellationToken ct = default);
        Task<Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>> CheckRegistrationAvailabilityAsync(string username, string email, CancellationToken ct = default);
        Task<Result<User>> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default);
    }
}
