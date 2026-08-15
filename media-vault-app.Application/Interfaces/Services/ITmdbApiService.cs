using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.DTOs.Tmdb;
using media_vault_app.Domain.Enums;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface ITmdbApiService
    {
        Task<Result<TmdbTvSeriesDetailedDto>> GetTvSeriesByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<TmdbMovieDetailedDto>> GetMovieByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<MediaEntryExternalSearchResultDto>>> SearchAsync(string search, MediaType mediaType, int page = 1, CancellationToken cancellationToken = default);
    }
}
