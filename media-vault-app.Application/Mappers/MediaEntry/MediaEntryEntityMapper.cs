using System;
using System.Collections.Generic;
using System.Linq;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;
using MovieEntity = media_vault_app.Domain.Entities.MovieEntry;
using TvSeriesEntity = media_vault_app.Domain.Entities.TvSeriesEntry;
using GameEntity = media_vault_app.Domain.Entities.GameEntry;
using BookEntity = media_vault_app.Domain.Entities.BookEntry;
using MangaEntity = media_vault_app.Domain.Entities.MangaEntry;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.Mappers.MediaEntry
{
    public class MediaEntryEntityMapper : IMapEntityToDto<MediaEntryEntity, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto>
    {
        public MediaEntryDetailedDto ToDetailedDTO(MediaEntryEntity entity) =>
            entity switch
            {
                MovieEntity movie => new MovieEntryDetailedDto
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
                    ReleaseDate = movie.ReleaseDate,
                    ImageUrl = movie.ImageUrl,
                    CreatedAtUtc = movie.CreatedAtUtc,
                    RuntimeMinutes = movie.RuntimeMinutes
                },
                TvSeriesEntity tvSeries => new TvSeriesEntryDetailedDto
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
                    ReleaseDate = tvSeries.ReleaseDate,
                    ImageUrl = tvSeries.ImageUrl,
                    CreatedAtUtc = tvSeries.CreatedAtUtc,
                    TotalEpisodes = tvSeries.NumberOfEpisodes,
                    TotalWatchedEpisodes = tvSeries.TotalWatchedEpisodes
                },
                GameEntity game => new GameEntryDetailedDto
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
                    ReleaseDate = game.ReleaseDate,
                    ImageUrl = game.ImageUrl,
                    CreatedAtUtc = game.CreatedAtUtc,
                    //DevStudioName = game.DevStudioName,
                    HoursPlayed = game.HoursPlayed,
                    MetacriticRating = game.MetacriticRating,
                    Platforms = game.Platforms,
                    Website = game.Website,
                    PcRequirements = game.PcRequirements is not null ? new GamePcRequirementsDto
                    (
                        Minimum: game.PcRequirements.Minimum,
                        Recommended: game.PcRequirements.Recommended,
                        High: game.PcRequirements.High,
                        VeryHigh: game.PcRequirements.VeryHigh,
                        Ultra: game.PcRequirements.Ultra
                    ) : null

                },
                BookEntity book => new BookEntryDetailedDto
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
                    ReleaseDate = book.ReleaseDate,
                    ImageUrl = book.ImageUrl,
                    CreatedAtUtc = book.CreatedAtUtc,
                    Author = book.Author,
                },
                MangaEntity manga => new MangaEntryDetailedDto
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
                    ReleaseDate = manga.ReleaseDate,
                    ImageUrl = manga.ImageUrl,
                    CreatedAtUtc = manga.CreatedAtUtc,
                    Author = manga.Author,
                },
                _ => throw new NotSupportedException($"Unknown entity type: {entity.GetType().Name}")
            };

        public IEnumerable<MediaEntryDetailedDto> ToDetailedDtoCollection(IEnumerable<MediaEntryEntity> entities) =>
            entities.Select(ToDetailedDTO);
        public MediaEntryMinimalDto ToMinimalDTO(MediaEntryEntity entity)
        {

            return new MediaEntryMinimalDto
            {
                Id = entity.Id,
                Title = entity.Title,
                ImageUrl = entity.ImageUrl,
                Rating = entity.Rating,
                ReleaseDate = entity.ReleaseDate,
                Genres = entity.Genres,
                MediaType = entity.MediaType,
                Status = entity.Status,
                CreatedAtUtc = entity.CreatedAtUtc
            };
        }

        public IEnumerable<MediaEntryMinimalDto> ToMinimalDtoCollection(IEnumerable<MediaEntryEntity> entities) =>
            entities.Select(ToMinimalDTO);

        public MediaEntryUpdateDto ToUpdateDTO(MediaEntryEntity entity) => entity switch
        {
            MovieEntity movie => new MovieEntryUpdateDto
            {
                IdExternal = movie.IdExternal,
                Status = movie.Status,
                Title = movie.Title,
                Rating = movie.Rating,
                Review = movie.Review,
                Genres = movie.Genres,
                Overview = movie.Overview,
                ReleaseDate = movie.ReleaseDate,
                ImageUrl = movie.ImageUrl,
                RuntimeMinutes = movie.RuntimeMinutes
            },
            TvSeriesEntity tvSeries => new TvSeriesEntryUpdateDto
            {
                IdExternal = tvSeries.IdExternal,
                Status = tvSeries.Status,
                Title = tvSeries.Title,
                Rating = tvSeries.Rating,
                Review = tvSeries.Review,
                Genres = tvSeries.Genres,
                Overview = tvSeries.Overview,
                ReleaseDate = tvSeries.ReleaseDate,
                ImageUrl = tvSeries.ImageUrl,
                TotalEpisodes = tvSeries.NumberOfEpisodes,
                TotalWatchedEpisodes = tvSeries.TotalWatchedEpisodes
            },
            GameEntity game => new GameEntryUpdateDto
            {
                IdExternal = game.IdExternal,
                Status = game.Status,
                Title = game.Title,
                Rating = game.Rating,
                Review = game.Review,
                Genres = game.Genres,
                Overview = game.Overview,
                ReleaseDate = game.ReleaseDate,
                ImageUrl = game.ImageUrl,
                //DevStudioName = game.DevStudioName,
                HoursPlayed = game.HoursPlayed,
                MetacriticRating = game.MetacriticRating,
                Platforms = game.Platforms,
                Website = game.Website,
                PcRequirements = game.PcRequirements is not null ? new GamePcRequirementsDto
                (
                    Minimum: game.PcRequirements.Minimum,
                    Recommended: game.PcRequirements.Recommended,
                    High: game.PcRequirements.High,
                    VeryHigh: game.PcRequirements.VeryHigh,
                    Ultra: game.PcRequirements.Ultra
                ) : null

            },
            BookEntity book => new BookEntryUpdateDto
            {
                IdExternal = book.IdExternal,
                Status = book.Status,
                Title = book.Title,
                Rating = book.Rating,
                Review = book.Review,
                Genres = book.Genres,
                Overview = book.Overview,
                ReleaseDate = book.ReleaseDate,
                ImageUrl = book.ImageUrl,
                Author = book.Author
            },
            MangaEntity manga => new MangaEntryUpdateDto
            {
                IdExternal = manga.IdExternal,
                Status = manga.Status,
                Title = manga.Title,
                Rating = manga.Rating,
                Review = manga.Review,
                Genres = manga.Genres,
                Overview = manga.Overview,
                ReleaseDate = manga.ReleaseDate,
                ImageUrl = manga.ImageUrl,
                Author = manga.Author
            },
            _ => throw new NotSupportedException($"Unknown entity type: {entity.GetType().Name}")
        };
    }
}
