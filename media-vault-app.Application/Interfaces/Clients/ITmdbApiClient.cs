using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.Tmdb.Movie;
using media_vault_app.Application.DTOs.Tmdb.TVSeries;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Clients
{
    public interface ITmdbApiClient
    {
        Task<Result<TmdbMovieResult>> GetMovieAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<TmdbTvResult>> GetTvSeriesAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<TmdbMovieSearchResponse>> SearchMoviesAsync(List<string> queryParameters, CancellationToken cancellationToken = default);
        Task<Result<TmdbTvSearchResponse>> SearchTvSeriesAsync(List<string> queryParameters, CancellationToken cancellationToken = default);
    }
}
