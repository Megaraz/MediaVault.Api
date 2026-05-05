using System;
using System.Collections.Generic;
using System.Linq;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.DTOs.Season;
using media_vault_app.Domain.Entities;
using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;

namespace media_vault_app.Application.Mappers.MediaEntry
{
    public class MediaEntryDtoMapper :
        IMapDtoToEntity<MediaEntryEntity, MediaEntryDetailedDto, MediaEntryCreateDto, MediaEntryUpdateDto, Guid>,
        IMapUpdateDtoToEntity<MediaEntryEntity, Guid, MediaEntryUpdateDto>
    {
        public MediaEntryEntity ToEntity(MediaEntryCreateDto createDto)
        {
            var entity = CreateEntityFromMediaType(createDto.MediaType);
            MapCommonPropertiesFromCreate(entity, createDto);
            MapTypeSpecificPropertiesFromCreate(entity, createDto);
            return entity;
        }

        public MediaEntryEntity ToEntity(MediaEntryDetailedDto detailedDto)
        {
            var entity = CreateEntityFromMediaType(detailedDto.MediaType);
            MapCommonPropertiesFromDetailed(entity, detailedDto);
            MapTypeSpecificPropertiesFromDetailed(entity, detailedDto);
            return entity;
        }

        public IEnumerable<MediaEntryEntity> ToEntities(IEnumerable<MediaEntryDetailedDto> detailedDtos) =>
            detailedDtos.Select(ToEntity);

        public MediaEntryEntity ToEntity(Guid id, MediaEntryUpdateDto updateDto)
        {
            var entity = CreateEntityFromMediaType(updateDto.MediaType);
            entity.Id = id;
            MapCommonPropertiesFromUpdate(entity, updateDto);
            MapTypeSpecificPropertiesFromUpdate(entity, updateDto);
            return entity;
        }

        // Factory method
        private static MediaEntryEntity CreateEntityFromMediaType(MediaType mediaType) => mediaType switch
        {
            MediaType.Movie => new MovieEntry(),
            MediaType.TvSeries => new TvSeriesEntry(),
            MediaType.Game => new GameEntry(),
            MediaType.Book => new BookEntry(),
            MediaType.Manga => new MangaEntry(),
            _ => throw new NotSupportedException($"Unknown media type: {mediaType}")
        };

        // Common property mapping helpers
        private static void MapCommonPropertiesFromCreate(MediaEntryEntity entity, MediaEntryCreateDto dto)
        {
            entity.Id = Guid.NewGuid();
            entity.IdExternal = dto.IdExternal;
            entity.Status = dto.Status;
            entity.Title = dto.Title;
            entity.Rating = dto.Rating;
            entity.Review = dto.Review;
            entity.Genres = dto.Genres;
            entity.ReleaseDate = dto.ReleaseDate ?? DateTime.MinValue;
            entity.ImageUrl = dto.ImageUrl;
            entity.Overview = dto.Overview;
            entity.CreatedAtUtc = DateTime.UtcNow;
        }

        private static void MapCommonPropertiesFromDetailed(MediaEntryEntity entity, MediaEntryDetailedDto dto)
        {
            entity.Id = dto.Id;
            entity.IdExternal = dto.IdExternal;
            entity.OwnerId = dto.UserId;
            entity.Status = dto.Status;
            entity.Title = dto.Title;
            entity.Rating = dto.Rating;
            entity.Overview = dto.Overview;
            entity.Review = dto.Review;
            entity.Genres = dto.Genres;
            entity.ReleaseDate = dto.ReleaseDate;
            entity.ImageUrl = dto.ImageUrl;
            entity.CreatedAtUtc = dto.CreatedAtUtc;
        }

        private static void MapCommonPropertiesFromUpdate(MediaEntryEntity entity, MediaEntryUpdateDto dto)
        {
            entity.IdExternal = dto.IdExternal;
            entity.Status = dto.Status;
            entity.Title = dto.Title;
            entity.Rating = dto.Rating;
            entity.Overview = dto.Overview;
            entity.Review = dto.Review;
            entity.Genres = dto.Genres;
            entity.ReleaseDate = dto.ReleaseDate ?? DateTime.MinValue;
            entity.ImageUrl = dto.ImageUrl;
        }

        // Type-specific property mapping
        private static void MapTypeSpecificPropertiesFromCreate(MediaEntryEntity entity, MediaEntryCreateDto dto)
        {
            switch (entity)
            {
                case MovieEntry movie when dto is MovieEntryCreateDto movieDto:
                    movie.RuntimeMinutes = movieDto.RuntimeMinutes;
                break;

                case TvSeriesEntry tvSeries when dto is TvSeriesEntryCreateDto tvDto:
                    tvSeries.BackdropImageUrl = tvDto.BackdropImageUrl;
                    tvSeries.LastAirDate = tvDto.LastAirDate;
                    tvSeries.NumberOfSeasons = tvDto.NumberOfSeasons;
                    tvSeries.NumberOfEpisodes = tvDto.NumberOfEpisodes;
                    tvSeries.AiringStatus = tvDto.AiringStatus;
                    tvSeries.TotalWatchedEpisodes = tvDto.TotalWatchedEpisodes;
                    tvSeries.Seasons = tvDto.Seasons?.Select(s => new Season
                    {
                        Id = Guid.NewGuid(),
                        OwnerId = tvSeries.Id,
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
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    }).ToList() ?? new List<Season>();
                break;

                case GameEntry game when dto is GameEntryCreateDto gameDto:
                    //game.DevStudioName = gameDto.DevStudioName;
                    game.HoursPlayed = gameDto.HoursPlayed;
                    game.MetacriticRating = gameDto.MetacriticRating;
                    game.Platforms = gameDto.Platforms;
                    game.Website = gameDto.Website;
                    game.PcRequirements = gameDto.PcRequirements != null ? new GamePcRequirements
                    {
                        Minimum = gameDto.PcRequirements.Minimum,
                        Recommended = gameDto.PcRequirements.Recommended,
                        High = gameDto.PcRequirements.High,
                        VeryHigh = gameDto.PcRequirements.VeryHigh,
                        Ultra = gameDto.PcRequirements.Ultra
                    } : null;
                break;

                case BookEntry book when dto is BookEntryCreateDto bookDto:
                    book.Author = bookDto.Author;
                break;

                case MangaEntry manga when dto is MangaEntryCreateDto mangaDto:
                    manga.Author = mangaDto.Author;
                break;
            }
        }

        private static void MapTypeSpecificPropertiesFromDetailed(MediaEntryEntity entity, MediaEntryDetailedDto dto)
        {
            switch (entity)
            {
                case MovieEntry movie when dto is MovieEntryDetailedDto movieDto:
                    movie.RuntimeMinutes = movieDto.RuntimeMinutes;
                break;

                case TvSeriesEntry tvSeries when dto is TvSeriesEntryDetailedDto tvDto:
                    tvSeries.BackdropImageUrl = tvDto.BackdropImageUrl;
                    tvSeries.LastAirDate = tvDto.LastAirDate;
                    tvSeries.NumberOfSeasons = tvDto.NumberOfSeasons;
                    tvSeries.NumberOfEpisodes = tvDto.NumberOfEpisodes;
                    tvSeries.AiringStatus = tvDto.AiringStatus;
                    tvSeries.TotalWatchedEpisodes = tvDto.TotalWatchedEpisodes;
                break;

                case GameEntry game when dto is GameEntryDetailedDto gameDto:
                    //game.DevStudioName = gameDto.DevStudioName;
                    game.HoursPlayed = gameDto.HoursPlayed;
                    game.MetacriticRating = gameDto.MetacriticRating;
                    game.Platforms = gameDto.Platforms;
                    game.Website = gameDto.Website;
                    game.PcRequirements = gameDto.PcRequirements != null ? new GamePcRequirements
                    {
                        Minimum = gameDto.PcRequirements.Minimum,
                        Recommended = gameDto.PcRequirements.Recommended,
                        High = gameDto.PcRequirements.High,
                        VeryHigh = gameDto.PcRequirements.VeryHigh,
                        Ultra = gameDto.PcRequirements.Ultra
                    } : null;
                break;

                case BookEntry book when dto is BookEntryDetailedDto bookDto:
                    book.Author = bookDto.Author;
                break;

                case MangaEntry manga when dto is MangaEntryDetailedDto mangaDto:
                    manga.Author = mangaDto.Author;
                break;
            }
        }

        private static void MapTypeSpecificPropertiesFromUpdate(MediaEntryEntity entity, MediaEntryUpdateDto dto)
        {
            switch (entity)
            {
                case MovieEntry movie when dto is MovieEntryUpdateDto movieDto:
                    movie.RuntimeMinutes = movieDto.RuntimeMinutes;
                break;

                case TvSeriesEntry tvSeries when dto is TvSeriesEntryUpdateDto tvDto:
                    tvSeries.BackdropImageUrl = tvDto.BackdropImageUrl;
                    tvSeries.LastAirDate = tvDto.LastAirDate;
                    tvSeries.NumberOfSeasons = tvDto.NumberOfSeasons;
                    tvSeries.NumberOfEpisodes = tvDto.NumberOfEpisodes;
                    tvSeries.AiringStatus = tvDto.AiringStatus;
                    tvSeries.TotalWatchedEpisodes = tvDto.TotalWatchedEpisodes;
                    tvSeries.Seasons = tvDto.Seasons?.Select(s => new Season
                    {
                        Id = Guid.NewGuid(),
                        OwnerId = tvSeries.Id,
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
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    }).ToList() ?? new List<Season>();
                break;

                case GameEntry game when dto is GameEntryUpdateDto gameDto:
                    //game.DevStudioName = gameDto.DevStudioName;
                    game.HoursPlayed = gameDto.HoursPlayed;
                    game.MetacriticRating = gameDto.MetacriticRating;
                    game.Platforms = gameDto.Platforms;
                    game.Website = gameDto.Website;
                    game.PcRequirements = gameDto.PcRequirements != null ? new GamePcRequirements
                    {
                        Minimum = gameDto.PcRequirements.Minimum,
                        Recommended = gameDto.PcRequirements.Recommended,
                        High = gameDto.PcRequirements.High,
                        VeryHigh = gameDto.PcRequirements.VeryHigh,
                        Ultra = gameDto.PcRequirements.Ultra
                    } : null;
                break;

                case BookEntry book when dto is BookEntryUpdateDto bookDto:
                    book.Author = bookDto.Author;
                break;

                case MangaEntry manga when dto is MangaEntryUpdateDto mangaDto:
                    manga.Author = mangaDto.Author;
                break;
            }
        }
    }
}
