using System;
using System.Collections.Generic;
using System.Linq;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;

namespace media_vault_app.Application.Mappers.MediaEntry
{
    public class MediaEntryDtoMapper :
        IMapDtoToEntity<MediaEntryEntity, MediaEntryDetailedDto, MediaEntryCreateDto, Guid>,
        IMapUpdateDtoToEntity<MediaEntryEntity, Guid, MediaEntryUpdateDto>
    {
        public MediaEntryEntity ToEntity(MediaEntryCreateDto createDto)
        {
            var entity = CreateMediaEntryInstance();

            entity.Id = Guid.NewGuid();
            entity.IdExternal = createDto.IdExternal;
            entity.Status = createDto.Status;
            entity.Title = createDto.Title;
            entity.Rating = createDto.Rating;
            entity.Review = createDto.Review;
            entity.Genre = createDto.Genre;
            entity.ReleaseYear = createDto.ReleaseYear ?? 0;
            entity.ImageUrl = createDto.ImageUrl;
            entity.MediaType = createDto.MediaType;
            entity.CreatedAtUtc = DateTime.UtcNow;

            return entity;
        }

        public MediaEntryEntity ToEntity(MediaEntryDetailedDto detailedDto)
        {
            var entity = CreateMediaEntryInstance();

            entity.Id = detailedDto.Id;
            entity.IdExternal = detailedDto.IdExternal;
            entity.OwnerId = detailedDto.UserId;
            entity.Status = detailedDto.Status;
            entity.Title = detailedDto.Title;
            entity.Rating = detailedDto.Rating;
            entity.Review = detailedDto.Review;
            entity.Genre = detailedDto.Genre;
            entity.ReleaseYear = detailedDto.ReleaseYear;
            entity.ImageUrl = detailedDto.ImageUrl;
            entity.MediaType = detailedDto.MediaType;
            entity.CreatedAtUtc = detailedDto.CreatedAtUtc;

            return entity;
        }

        public IEnumerable<MediaEntryEntity> ToEntities(IEnumerable<MediaEntryDetailedDto> detailedDtos) =>
            detailedDtos.Select(ToEntity);

        public MediaEntryEntity MapToEntity(Guid id, MediaEntryUpdateDto updateDto)
        {
            var entity = CreateMediaEntryInstance();

            entity.Id = id;
            entity.IdExternal = updateDto.IdExternal;
            entity.Status = updateDto.Status;
            entity.Title = updateDto.Title;
            entity.Rating = updateDto.Rating;
            entity.Review = updateDto.Review;
            entity.Genre = updateDto.Genre;
            entity.ReleaseYear = updateDto.ReleaseYear ?? 0;
            entity.ImageUrl = updateDto.ImageUrl;
            entity.MediaType = updateDto.MediaType;

            return entity;
        }

        private static MediaEntryEntity CreateMediaEntryInstance() => new MovieEntry();
    }
}
