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
                UserId = movie.UserId,
                Status = movie.Status,
                Title = movie.Title,
                Rating = movie.Rating,
                Review = movie.Review,
                Genres = movie.Genres,
                ReleaseYear = movie.ReleaseYear,
                ImageUrl = movie.ImageUrl,
                CreatedAtUtc = movie.CreatedAtUtc,
                RuntimeMinutes = movie.RuntimeMinutes
            },
            TvSeriesEntity tvSeries => new TvSeriesEntryDetailedDto
            {
                Id = tvSeries.Id,
                IdExternal = tvSeries.IdExternal,
                UserId = tvSeries.UserId,
                Status = tvSeries.Status,
                Title = tvSeries.Title,
                Rating = tvSeries.Rating,
                Review = tvSeries.Review,
                Genres = tvSeries.Genres,
                ReleaseYear = tvSeries.ReleaseYear,
                ImageUrl = tvSeries.ImageUrl,
                CreatedAtUtc = tvSeries.CreatedAtUtc,
                TotalEpisodes = tvSeries.TotalEpisodes,
                TotalWatchedEpisodes = tvSeries.TotalWatchedEpisodes
            },
            GameEntity game => new GameEntryDetailedDto
            {
                Id = game.Id,
                IdExternal = game.IdExternal,
                UserId = game.UserId,
                Status = game.Status,
                Title = game.Title,
                Rating = game.Rating,
                Review = game.Review,
                Genres = game.Genres,
                ReleaseYear = game.ReleaseYear,
                ImageUrl = game.ImageUrl,
                CreatedAtUtc = game.CreatedAtUtc,
                DevStudioName = game.DevStudioName,
                HoursPlayed = game.HoursPlayed
            },
            BookEntity book => new BookEntryDetailedDto
            {
                Id = book.Id,
                IdExternal = book.IdExternal,
                UserId = book.UserId,
                Status = book.Status,
                Title = book.Title,
                Rating = book.Rating,
                Review = book.Review,
                Genres = book.Genres,
                ReleaseYear = book.ReleaseYear,
                ImageUrl = book.ImageUrl,
                CreatedAtUtc = book.CreatedAtUtc,
                AuthorId = book.AuthorId,
                AuthorName = book.Author != null 
                    ? $"{book.Author.FirstName} {book.Author.LastName}".Trim() 
                    : null
            },
            MangaEntity manga => new MangaEntryDetailedDto
            {
                Id = manga.Id,
                IdExternal = manga.IdExternal,
                UserId = manga.UserId,
                Status = manga.Status,
                Title = manga.Title,
                Rating = manga.Rating,
                Review = manga.Review,
                Genres = manga.Genres,
                ReleaseYear = manga.ReleaseYear,
                ImageUrl = manga.ImageUrl,
                CreatedAtUtc = manga.CreatedAtUtc,
                AuthorId = manga.AuthorId,
                AuthorName = manga.Author != null 
                    ? $"{manga.Author.FirstName} {manga.Author.LastName}".Trim() 
                    : null
            },
            _ => throw new NotSupportedException($"Unknown entity type: {entity.GetType().Name}")
        };

        public IEnumerable<MediaEntryDetailedDto> ToDetailedDtoCollection(IEnumerable<MediaEntryEntity> entities) =>
            entities.Select(ToDetailedDTO);

        public MediaEntryMinimalDto ToMinimalDTO(MediaEntryEntity entity) => entity switch
        {
            MovieEntity movie => new MovieEntryMinimalDto
            {
                Id = movie.Id,
                Title = movie.Title,
                ImageUrl = movie.ImageUrl
            },
            TvSeriesEntity tvSeries => new TvSeriesEntryMinimalDto
            {
                Id = tvSeries.Id,
                Title = tvSeries.Title,
                ImageUrl = tvSeries.ImageUrl
            },
            GameEntity game => new GameEntryMinimalDto
            {
                Id = game.Id,
                Title = game.Title,
                ImageUrl = game.ImageUrl
            },
            BookEntity book => new BookEntryMinimalDto
            {
                Id = book.Id,
                Title = book.Title,
                ImageUrl = book.ImageUrl
            },
            MangaEntity manga => new MangaEntryMinimalDto
            {
                Id = manga.Id,
                Title = manga.Title,
                ImageUrl = manga.ImageUrl
            },
            _ => throw new NotSupportedException($"Unknown entity type: {entity.GetType().Name}")
        };

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
                ReleaseYear = movie.ReleaseYear,
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
                ReleaseYear = tvSeries.ReleaseYear,
                ImageUrl = tvSeries.ImageUrl,
                TotalEpisodes = tvSeries.TotalEpisodes,
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
                ReleaseYear = game.ReleaseYear,
                ImageUrl = game.ImageUrl,
                DevStudioName = game.DevStudioName,
                HoursPlayed = game.HoursPlayed
            },
            BookEntity book => new BookEntryUpdateDto
            {
                IdExternal = book.IdExternal,
                Status = book.Status,
                Title = book.Title,
                Rating = book.Rating,
                Review = book.Review,
                Genres = book.Genres,
                ReleaseYear = book.ReleaseYear,
                ImageUrl = book.ImageUrl,
                AuthorId = book.AuthorId
            },
            MangaEntity manga => new MangaEntryUpdateDto
            {
                IdExternal = manga.IdExternal,
                Status = manga.Status,
                Title = manga.Title,
                Rating = manga.Rating,
                Review = manga.Review,
                Genres = manga.Genres,
                ReleaseYear = manga.ReleaseYear,
                ImageUrl = manga.ImageUrl,
                AuthorId = manga.AuthorId
            },
            _ => throw new NotSupportedException($"Unknown entity type: {entity.GetType().Name}")
        };
    }
}
