using media_vault_app.Application.DTOs.Tmdb.TVSeries;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface ITmdbTvSeriesApiService
    {
        Task<Result<TvSearchResultDto>> GetTvSeriesByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<TvSearchResultDto>>> SearchTvSeriesAsync(string search, int page = 1, int pageSize = 10, string? ordering = null, CancellationToken cancellationToken = default);
    }
}
