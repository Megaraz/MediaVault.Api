using media_vault_app.Application.DTOs.Rawg;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IRawgApiService
    {
        Task<Result<GameSearchResultDto>> GetGameByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<GameSearchResultDto>>> SearchGamesAsync(string search, int page = 1, int pageSize = 10, bool? searchPrecise = null, bool? searchExact = null, string? ordering = null, CancellationToken cancellationToken = default);
    }
}