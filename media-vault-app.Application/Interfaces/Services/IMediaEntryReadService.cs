using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IMediaEntryReadService
    {
        Task<Result<MediaEntryDetailedDto>> GetDetailedByIdAsync(
            Guid ownerId,
            Guid id,
            CancellationToken ct = default);

        Task<Result<MovieEntryDetailedDto>> GetMovieByIdAsync(
            Guid ownerId,
            Guid id,
            CancellationToken ct = default);

        Task<Result<TvSeriesEntryDetailedDto>> GetTvSeriesByIdAsync(
            Guid ownerId,
            Guid id,
            CancellationToken ct = default);

        Task<Result<GameEntryDetailedDto>> GetGameByIdAsync(
            Guid ownerId,
            Guid id,
            CancellationToken ct = default);

        Task<Result<BookEntryDetailedDto>> GetBookByIdAsync(
            Guid ownerId,
            Guid id,
            CancellationToken ct = default);

        Task<Result<MangaEntryDetailedDto>> GetMangaByIdAsync(
            Guid ownerId,
            Guid id,
            CancellationToken ct = default);

        Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> GetMinimalCollectionByOwnerIdAsync(
            Guid ownerId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default);

        Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> SearchMediaEntriesAsync(
            Guid ownerId,
            SearchRequestDto request,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default);
    }
}
