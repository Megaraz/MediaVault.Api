using media_vault_app.Application.DTOs.Tmdb.Movie;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface ITmdbMovieApiService
    {
        Task<Result<MovieSearchResultDto>> GetMovieByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<MovieSearchResultDto>>> SearchMoviesAsync(string search, int page = 1, int pageSize = 10, string? ordering = null, CancellationToken cancellationToken = default);
    }
}
