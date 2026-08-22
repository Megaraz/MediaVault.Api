using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IMediaEntryWriteService
    {
        Task<Result<MovieEntryDetailedDto>> CreateMovieAsync(
            Guid ownerId,
            MovieEntryCreateDto createDto,
            CancellationToken ct = default);

        Task<Result<TvSeriesEntryDetailedDto>> CreateTvSeriesAsync(
            Guid ownerId,
            TvSeriesEntryCreateDto createDto,
            CancellationToken ct = default);

        Task<Result<GameEntryDetailedDto>> CreateGameAsync(
            Guid ownerId,
            GameEntryCreateDto createDto,
            CancellationToken ct = default);

        Task<Result<BookEntryDetailedDto>> CreateBookAsync(
            Guid ownerId,
            BookEntryCreateDto createDto,
            CancellationToken ct = default);

        Task<Result<MangaEntryDetailedDto>> CreateMangaAsync(
            Guid ownerId,
            MangaEntryCreateDto createDto,
            CancellationToken ct = default);

        Task<Result> UpdateMovieAsync(
            Guid ownerId,
            Guid id,
            MovieEntryUpdateDto updateDto,
            CancellationToken ct = default);

        Task<Result> UpdateTvSeriesAsync(
            Guid ownerId,
            Guid id,
            TvSeriesEntryUpdateDto updateDto,
            CancellationToken ct = default);

        Task<Result> UpdateGameAsync(
            Guid ownerId,
            Guid id,
            GameEntryUpdateDto updateDto,
            CancellationToken ct = default);

        Task<Result> UpdateBookAsync(
            Guid ownerId,
            Guid id,
            BookEntryUpdateDto updateDto,
            CancellationToken ct = default);

        Task<Result> UpdateMangaAsync(
            Guid ownerId,
            Guid id,
            MangaEntryUpdateDto updateDto,
            CancellationToken ct = default);

        Task<Result> DeleteAsync(
            Guid ownerId,
            Guid id,
            int expectedVersion,
            CancellationToken ct = default);
    }
}
