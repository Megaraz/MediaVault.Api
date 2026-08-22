using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Domain.Entities;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Repos
{
    public interface IMediaEntryRepo
    {
        Task<Result<MediaEntry>> GetDetailedByIdAsync(
            Guid ownerId,
            Guid id,
            CancellationToken ct = default);

        Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> GetMinimalCollectionByOwnerIdAsync(
            Guid ownerId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);

        Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> SearchMediaEntriesAsync(
            Guid ownerId,
            string query,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);

        Task<Result<MediaEntry>> CreateAsync(
            MediaEntry entity,
            CancellationToken ct = default);

        Task<Result> UpdateMovieAsync(
            Guid ownerId,
            MovieEntry entity,
            CancellationToken ct = default);

        Task<Result> UpdateTvSeriesAsync(
            Guid ownerId,
            TvSeriesEntry entity,
            CancellationToken ct = default);

        Task<Result> UpdateGameAsync(
            Guid ownerId,
            GameEntry entity,
            CancellationToken ct = default);

        Task<Result> UpdateBookAsync(
            Guid ownerId,
            BookEntry entity,
            CancellationToken ct = default);

        Task<Result> UpdateMangaAsync(
            Guid ownerId,
            MangaEntry entity,
            CancellationToken ct = default);

        Task<Result> DeleteAsync(
            Guid ownerId,
            Guid id,
            int expectedVersion,
            CancellationToken ct = default);
    }
}
