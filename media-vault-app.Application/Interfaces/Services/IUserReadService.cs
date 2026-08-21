using media_vault_app.Application.DTOs.User.Response;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IUserReadService
    {
        Task<Result<UserDetailedDto>> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
    }
}
