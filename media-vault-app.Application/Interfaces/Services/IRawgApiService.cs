using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.DTOs.Rawg;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IRawgApiService
    {
        Task<Result<RawgGameDetailedDto>> GetGameByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<MediaEntrySearchResultDto>>> SearchGamesAsync(
            string search, 
            int page = 1, 
            int pageSize = 8, 
            bool? searchPrecise = null, 
            bool? searchExact = null, 
            string? ordering = null, 
            CancellationToken cancellationToken = default);
    }
}