using media_vault_app.Application.DTOs.External_API_Contracts.Rawg;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Clients
{
    public interface IRawgApiClient
    {
        Task<Result<RawgGameDetailedResponse>> GetGameByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<RawgSearchResponse>> SearchGamesAsync(
            List<string> queryParameters,
            CancellationToken cancellationToken = default);
    }
}
