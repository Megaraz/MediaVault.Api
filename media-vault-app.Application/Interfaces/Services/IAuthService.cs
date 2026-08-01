using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<Result> RegisterUserAsync(UserRegisterDto createDto, CancellationToken ct = default);
        Task<Result<UserDetailedDto>> LoginAsync(UserLoginDto loginDto, CancellationToken ct = default);

    }
}
