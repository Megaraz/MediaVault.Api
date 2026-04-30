using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.External_API_Contracts.Rawg;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Clients
{
    public interface IRawgApiClient
    {
        Task<Result<RawgGameResponse>> GetGameAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<RawgSearchResponse>> SearchGamesAsync(
            List<string> queryParameters,
            CancellationToken cancellationToken = default);
    }
}
