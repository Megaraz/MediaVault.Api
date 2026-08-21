using media_vault_app.Domain.Entities;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Repos
{
    public interface IUserRepo : IEntityExistsRepo<Guid>
    {
        Task<Result<User>> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Result> RegisterUserAsync(User entity, CancellationToken ct = default);
        Task<Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>> CheckRegistrationAvailabilityAsync(string username, string email, CancellationToken ct = default);
        Task<Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>> CheckProfileUpdateAvailabilityAsync(Guid userId, string username, string email, CancellationToken ct = default);
        Task<Result> UpdateProfileAsync(
            Guid userId,
            string username,
            string email,
            int expectedVersion,
            CancellationToken ct = default);
        Task<Result> DeleteAccountAsync(Guid userId, CancellationToken ct = default);
        Task<Result<User>> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default);
    }
}
