using media_vault_app.Application.DTOs.User.Request;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IUserWriteService
    {
        Task<Result> UpdateProfileAsync(Guid userId, UserUpdateDto updateDto, CancellationToken ct = default);
        Task<Result> DeleteOwnAccountAsync(Guid userId, CancellationToken ct = default);
    }
}
