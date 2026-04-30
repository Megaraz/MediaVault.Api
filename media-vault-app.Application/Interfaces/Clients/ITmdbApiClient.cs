using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Shared;
using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Clients
{
    public interface ITmdbApiClient
    {
        Task<Result<TmdbSearchResult>> GetByIdAsync(int id, MediaType mediaType, CancellationToken cancellationToken = default);
        Task<Result<TmdbSearchResponse>> SearchAsync(
            List<string> queryParameters,
            MediaType mediaType,
            CancellationToken cancellationToken = default);
    }
}
