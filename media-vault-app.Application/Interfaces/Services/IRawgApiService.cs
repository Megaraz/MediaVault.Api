using media_vault_app.Application.DTOs;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IRawgApiService
    {
        Task<Result<SearchResultDto>> GetGameByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<SearchResultDto>>> SearchGamesAsync(string search, int page = 1, int pageSize = 10, bool? searchPrecise = null, bool? searchExact = null, string? ordering = null, CancellationToken cancellationToken = default);
    }
}