using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Mappers;
using media_vault_app.Domain.Entities;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;

namespace media_vault_app.Application.Mappers.MediaEntry
{
    public class MediaEntryDtoMapper : IMediaEntryDtoMapper
    {
        public MediaEntryEntity ToEntity(MediaEntryCreateDto createDto) =>
            createDto switch
            {
                MovieEntryCreateDto dto => MapMovieFromCreate(dto),
                TvSeriesEntryCreateDto dto => MapTvSeriesFromCreate(dto),
                GameEntryCreateDto dto => MapGameFromCreate(dto),
                BookEntryCreateDto dto => MapBookFromCreate(dto),
                MangaEntryCreateDto dto => MapMangaFromCreate(dto),
                _ => throw new NotSupportedException($"Unknown create DTO type: {createDto.GetType()}")
            };

        public MediaEntryEntity ToEntity(MediaEntryDetailedDto detailedDto) =>
            detailedDto switch
            {
                MovieEntryDetailedDto dto => MapMovieFromDetailed(dto),
                TvSeriesEntryDetailedDto dto => MapTvSeriesFromDetailed(dto),
                GameEntryDetailedDto dto => MapGameFromDetailed(dto),
                BookEntryDetailedDto dto => MapBookFromDetailed(dto),
                MangaEntryDetailedDto dto => MapMangaFromDetailed(dto),
                _ => throw new NotSupportedException($"Unknown detailed DTO type: {detailedDto.GetType()}")
            };

        public IEnumerable<MediaEntryEntity> ToEntities(IEnumerable<MediaEntryDetailedDto> detailedDtos) =>
            detailedDtos.Select(ToEntity);

        public MediaEntryEntity ToEntity(Guid id, MediaEntryUpdateDto updateDto) =>
            updateDto switch
            {
                MovieEntryUpdateDto dto => MapMovieFromUpdate(id, dto),
                TvSeriesEntryUpdateDto dto => MapTvSeriesFromUpdate(id, dto),
                GameEntryUpdateDto dto => MapGameFromUpdate(id, dto),
                BookEntryUpdateDto dto => MapBookFromUpdate(id, dto),
                MangaEntryUpdateDto dto => MapMangaFromUpdate(id, dto),
                _ => throw new NotSupportedException($"Unknown update DTO type: {updateDto.GetType()}")
            };


        #region Movie Internal Mapping Methods
        private static MovieEntry MapMovieFromCreate(MovieEntryCreateDto dto) => new()
        {
            Id = Guid.NewGuid(),
            IdExternal = dto.IdExternal,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            RuntimeMinutes = dto.RuntimeMinutes
        };

        private static MovieEntry MapMovieFromDetailed(MovieEntryDetailedDto dto) => new()
        {
            Id = dto.Id,
            IdExternal = dto.IdExternal,
            OwnerId = dto.UserId,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            RuntimeMinutes = dto.RuntimeMinutes
        };

        private static MovieEntry MapMovieFromUpdate(Guid id, MovieEntryUpdateDto dto) => new()
        {
            Id = id,
            Version = dto.ExpectedVersion,
            IdExternal = dto.IdExternal,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            RuntimeMinutes = dto.RuntimeMinutes
        };
        #endregion

        #region TvSeries Internal Mapping Methods
        private TvSeriesEntry MapTvSeriesFromCreate(TvSeriesEntryCreateDto dto) => new()
        {
            Id = Guid.NewGuid(),
            IdExternal = dto.IdExternal,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            BackdropImageUrl = dto.BackdropImageUrl,
            LastAirDate = dto.LastAirDate,
            NumberOfSeasons = dto.NumberOfSeasons,
            NumberOfEpisodes = dto.NumberOfEpisodes,
            AiringStatus = dto.AiringStatus,
            TotalWatchedEpisodes = dto.TotalWatchedEpisodes,
            Seasons = dto.Seasons.Select(s => new Season
            {
                Id = Guid.NewGuid(),
                TvSeriesEntryId = s.TvSeriesId,
                IdExternal = s.IdExternal,
                Name = s.Name,
                Overview = s.Overview,
                ImageUrl = s.ImageUrl,
                SeasonNumber = s.SeasonNumber,
                AirDate = s.AirDate,
                WatchedEpisodes = s.WatchedEpisodes,
                Episodes = s.Episodes,
                Status = s.Status,
                Rating = s.Rating
            }).ToList()
        };

        private TvSeriesEntry MapTvSeriesFromDetailed(TvSeriesEntryDetailedDto dto) => new()
        {
            Id = dto.Id,
            IdExternal = dto.IdExternal,
            OwnerId = dto.UserId,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            BackdropImageUrl = dto.BackdropImageUrl,
            LastAirDate = dto.LastAirDate,
            NumberOfSeasons = dto.NumberOfSeasons,
            NumberOfEpisodes = dto.NumberOfEpisodes,
            AiringStatus = dto.AiringStatus,
            TotalWatchedEpisodes = dto.TotalWatchedEpisodes,
            Seasons = dto.Seasons.Select(s => new Season
            {
                Id = Guid.NewGuid(),
                TvSeriesEntryId = s.TvSeriesId,
                IdExternal = s.IdExternal,
                Name = s.Name,
                Overview = s.Overview,
                ImageUrl = s.ImageUrl,
                SeasonNumber = s.SeasonNumber,
                AirDate = s.AirDate,
                WatchedEpisodes = s.WatchedEpisodes,
                Episodes = s.Episodes,
                Status = s.Status,
                Rating = s.Rating
            }).ToList()
        };

        private TvSeriesEntry MapTvSeriesFromUpdate(Guid id, TvSeriesEntryUpdateDto dto) => new()
        {
            Id = id,
            Version = dto.ExpectedVersion,
            IdExternal = dto.IdExternal,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            BackdropImageUrl = dto.BackdropImageUrl,
            LastAirDate = dto.LastAirDate,
            NumberOfSeasons = dto.NumberOfSeasons,
            NumberOfEpisodes = dto.NumberOfEpisodes,
            AiringStatus = dto.AiringStatus,
            TotalWatchedEpisodes = dto.TotalWatchedEpisodes,
            Seasons = dto.Seasons.Select(s => new Season
            {
                Id = s.Id,
                TvSeriesEntryId = s.TvSeriesId,
                IdExternal = s.IdExternal,
                Name = s.Name,
                Overview = s.Overview,
                ImageUrl = s.ImageUrl,
                SeasonNumber = s.SeasonNumber,
                AirDate = s.AirDate,
                WatchedEpisodes = s.WatchedEpisodes,
                Episodes = s.Episodes,
                Status = s.Status,
                Rating = s.Rating
            }).ToList()

        };
        #endregion

        #region Game Internal Mapping Methods
        private static GameEntry MapGameFromCreate(GameEntryCreateDto dto) => new()
        {
            Id = Guid.NewGuid(),
            IdExternal = dto.IdExternal,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            HoursPlayed = dto.HoursPlayed,
            MetacriticRating = dto.MetacriticRating,
            Platforms = dto.Platforms,
            Website = dto.Website,
            PcRequirements = dto.PcRequirements != null ? new GamePcRequirements
            (
                Minimum: dto.PcRequirements.Minimum,
                Recommended: dto.PcRequirements.Recommended,
                High: dto.PcRequirements.High,
                VeryHigh: dto.PcRequirements.VeryHigh,
                Ultra: dto.PcRequirements.Ultra
            ) : null
        };

        private static GameEntry MapGameFromDetailed(GameEntryDetailedDto dto) => new()
        {
            Id = dto.Id,
            IdExternal = dto.IdExternal,
            OwnerId = dto.UserId,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            HoursPlayed = dto.HoursPlayed,
            MetacriticRating = dto.MetacriticRating,
            Platforms = dto.Platforms,
            Website = dto.Website,
            PcRequirements = dto.PcRequirements != null ? new GamePcRequirements
            (
                Minimum: dto.PcRequirements.Minimum,
                Recommended: dto.PcRequirements.Recommended,
                High: dto.PcRequirements.High,
                VeryHigh: dto.PcRequirements.VeryHigh,
                Ultra: dto.PcRequirements.Ultra
            ) : null
        };

        private static GameEntry MapGameFromUpdate(Guid id, GameEntryUpdateDto dto) => new()
        {
            Id = id,
            Version = dto.ExpectedVersion,
            IdExternal = dto.IdExternal,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            HoursPlayed = dto.HoursPlayed,
            MetacriticRating = dto.MetacriticRating,
            Platforms = dto.Platforms,
            Website = dto.Website,
            PcRequirements = dto.PcRequirements != null ? new GamePcRequirements
            (
                Minimum: dto.PcRequirements.Minimum,
                Recommended: dto.PcRequirements.Recommended,
                High: dto.PcRequirements.High,
                VeryHigh: dto.PcRequirements.VeryHigh,
                Ultra: dto.PcRequirements.Ultra
            ) : null
        };
        #endregion

        #region Book Internal Mapping Methods
        private static BookEntry MapBookFromCreate(BookEntryCreateDto dto) => new()
        {
            Id = Guid.NewGuid(),
            IdExternal = dto.IdExternal,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            Author = dto.Author
        };

        private static BookEntry MapBookFromDetailed(BookEntryDetailedDto dto) => new()
        {
            Id = dto.Id,
            IdExternal = dto.IdExternal,
            OwnerId = dto.UserId,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            Author = dto.Author
        };

        private static BookEntry MapBookFromUpdate(Guid id, BookEntryUpdateDto dto) => new()
        {
            Id = id,
            Version = dto.ExpectedVersion,
            IdExternal = dto.IdExternal,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            Author = dto.Author
        };
        #endregion

        #region Manga Internal Mapping Methods
        private static MangaEntry MapMangaFromCreate(MangaEntryCreateDto dto) => new()
        {
            Id = Guid.NewGuid(),
            IdExternal = dto.IdExternal,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            Author = dto.Author
        };

        private static MangaEntry MapMangaFromDetailed(MangaEntryDetailedDto dto) => new()
        {
            Id = dto.Id,
            IdExternal = dto.IdExternal,
            OwnerId = dto.UserId,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            Author = dto.Author
        };

        private static MangaEntry MapMangaFromUpdate(Guid id, MangaEntryUpdateDto dto) => new()
        {
            Id = id,
            Version = dto.ExpectedVersion,
            IdExternal = dto.IdExternal,
            Status = dto.Status,
            Title = dto.Title,
            Rating = dto.Rating,
            Review = dto.Review,
            Genres = dto.Genres,
            Overview = dto.Overview,
            ReleaseDate = dto.ReleaseDate,
            ImageUrl = dto.ImageUrl,
            Author = dto.Author
        };
        #endregion
    }
}
