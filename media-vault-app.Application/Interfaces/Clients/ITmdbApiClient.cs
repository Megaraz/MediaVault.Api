using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.Tmdb.Movie;
using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Clients
{
    public interface ITmdbApiClient
    {
        Task<Result<TmdbResult>> GetByIdAsync(int id, MediaEntryType mediaType, CancellationToken cancellationToken = default);
        Task<Result<TmdbSearchResponse>> SearchAsync(
            List<string> queryParameters,
            MediaEntryType mediaType,
            CancellationToken cancellationToken = default);
    }
}
