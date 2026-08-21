using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.DTOs.Season;
using media_vault_app.Application.Interfaces.Mappers;
using BookEntity = media_vault_app.Domain.Entities.BookEntry;
using GameEntity = media_vault_app.Domain.Entities.GameEntry;
using MangaEntity = media_vault_app.Domain.Entities.MangaEntry;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;
using MovieEntity = media_vault_app.Domain.Entities.MovieEntry;
using TvSeriesEntity = media_vault_app.Domain.Entities.TvSeriesEntry;

namespace media_vault_app.Application.Mappers.MediaEntry
{


    public class MediaEntryEntityMapper : IMediaEntryEntityMapper
    {

        public MediaEntryDetailedDto ToDetailedDto(MediaEntryEntity entity) =>
            entity switch
            {
                MovieEntity movie => MapMovie(movie),
                TvSeriesEntity tvSeries => MapTvSeries(tvSeries),
                GameEntity game => MapGame(game),
                BookEntity book => MapBook(book),
                MangaEntity manga => MapManga(manga),
                _ => throw new NotSupportedException($"Unknown entity type: {entity.GetType().Name}")
            };


        public IReadOnlyList<MediaEntryDetailedDto> ToDetailedDtoCollection(IEnumerable<MediaEntryEntity> entities) =>
            entities.Select(ToDetailedDto).ToList();
        public MediaEntryMinimalDto ToMinimalDto(MediaEntryEntity entity)
        {

            return new MediaEntryMinimalDto
            {
                Id = entity.Id,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                Rating = entity.Rating,
                ReleaseDate = entity.ReleaseDate ?? default,
                Genres = entity.Genres,
                MediaType = entity.MediaType,
                Status = entity.Status,
                CreatedAtUtc = entity.CreatedAtUtc,
                UpdatedAtUtc = entity.UpdatedAtUtc
            };
        }

        public IReadOnlyList<MediaEntryMinimalDto> ToMinimalDtoCollection(IEnumerable<MediaEntryEntity> entities) =>
            entities.Select(ToMinimalDto).ToList();

        // Movie
        private static MovieEntryDetailedDto MapMovie(MovieEntity movie) => new()
        {
            Id = movie.Id,
            IdExternal = movie.IdExternal,
            UserId = movie.OwnerId,
            Status = movie.Status,
            Title = movie.Title,
            Rating = movie.Rating,
            Review = movie.Review,
            Genres = movie.Genres,
            Overview = movie.Overview,
            ReleaseDate = movie.ReleaseDate ?? default,
            ImageUrl = movie.ImageUrl,
            CreatedAtUtc = movie.CreatedAtUtc,
            UpdatedAtUtc = movie.UpdatedAtUtc,
            RuntimeMinutes = movie.RuntimeMinutes
        };

        // TvSeries
        private static TvSeriesEntryDetailedDto MapTvSeries(TvSeriesEntity tvSeries) => new()
        {
            Id = tvSeries.Id,
            IdExternal = tvSeries.IdExternal,
            UserId = tvSeries.OwnerId,
            Status = tvSeries.Status,
            Title = tvSeries.Title,
            Rating = tvSeries.Rating,
            Review = tvSeries.Review,
            Genres = tvSeries.Genres,
            Overview = tvSeries.Overview,
            ReleaseDate = tvSeries.ReleaseDate ?? default,
            ImageUrl = tvSeries.ImageUrl,
            CreatedAtUtc = tvSeries.CreatedAtUtc,
            UpdatedAtUtc = tvSeries.UpdatedAtUtc,
            BackdropImageUrl = tvSeries.BackdropImageUrl,
            LastAirDate = tvSeries.LastAirDate,
            NumberOfSeasons = tvSeries.NumberOfSeasons,
            NumberOfEpisodes = tvSeries.NumberOfEpisodes,
            AiringStatus = tvSeries.AiringStatus,
            TotalWatchedEpisodes = tvSeries.TotalWatchedEpisodes,
            Seasons = tvSeries.Seasons.Select(MapSeason).ToList()
        };

        private static SeasonMinimalDto MapSeason(Domain.Entities.Season s) => new()
        {
            Id = s.Id,
            TvSeriesId = s.TvSeriesEntryId,
            IdExternal = s.IdExternal,
            Name = s.Name,
            Overview = s.Overview,
            ImageUrl = s.ImageUrl,
            SeasonNumber = s.SeasonNumber,
            AirDate = s.AirDate,
            WatchedEpisodes = s.WatchedEpisodes,
            Episodes = s.Episodes,
            Status = s.Status,
            Rating = s.Rating,
            CreatedAtUtc = s.CreatedAtUtc,
            UpdatedAtUtc = s.UpdatedAtUtc
        };

        // Game
        private static GameEntryDetailedDto MapGame(GameEntity game) => new()
        {
            Id = game.Id,
            IdExternal = game.IdExternal,
            UserId = game.OwnerId,
            Status = game.Status,
            Title = game.Title,
            Rating = game.Rating,
            Review = game.Review,
            Genres = game.Genres,
            Overview = game.Overview,
            ReleaseDate = game.ReleaseDate ?? default,
            ImageUrl = game.ImageUrl,
            CreatedAtUtc = game.CreatedAtUtc,
            UpdatedAtUtc = game.UpdatedAtUtc,
            HoursPlayed = game.HoursPlayed,
            MetacriticRating = game.MetacriticRating,
            Platforms = game.Platforms,
            Website = game.Website,
            PcRequirements = game.PcRequirements.HasValue ? new GamePcRequirementsDto
            (
                Minimum: game.PcRequirements.Value.Minimum,
                Recommended: game.PcRequirements.Value.Recommended,
                High: game.PcRequirements.Value.High,
                VeryHigh: game.PcRequirements.Value.VeryHigh,
                Ultra: game.PcRequirements.Value.Ultra
            ) : null
        };

        // Book
        private static BookEntryDetailedDto MapBook(BookEntity book) => new()
        {
            Id = book.Id,
            IdExternal = book.IdExternal,
            UserId = book.OwnerId,
            Status = book.Status,
            Title = book.Title,
            Rating = book.Rating,
            Review = book.Review,
            Genres = book.Genres,
            Overview = book.Overview,
            ReleaseDate = book.ReleaseDate ?? default,
            ImageUrl = book.ImageUrl,
            CreatedAtUtc = book.CreatedAtUtc,
            UpdatedAtUtc = book.UpdatedAtUtc,
            Author = book.Author
        };

        // Manga
        private static MangaEntryDetailedDto MapManga(MangaEntity manga) => new()
        {
            Id = manga.Id,
            IdExternal = manga.IdExternal,
            UserId = manga.OwnerId,
            Status = manga.Status,
            Title = manga.Title,
            Rating = manga.Rating,
            Review = manga.Review,
            Genres = manga.Genres,
            Overview = manga.Overview,
            ReleaseDate = manga.ReleaseDate ?? default,
            ImageUrl = manga.ImageUrl,
            CreatedAtUtc = manga.CreatedAtUtc,
            UpdatedAtUtc = manga.UpdatedAtUtc,
            Author = manga.Author
        };

    }
}
