using System;
using System.Collections.Generic;
using System.Linq;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;

namespace media_vault_app.Application.Mappers.MediaEntry
{
    public class MediaEntryEntityMapper : IMapEntityToDto<MediaEntryEntity, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto>
    {
        public MediaEntryDetailedDto ToDetailedDTO(MediaEntryEntity entity) =>
            new()
            {
                Id = entity.Id,
                IdExternal = entity.IdExternal,
                UserId = entity.OwnerId,
                Status = entity.Status,
                Title = entity.Title,
                Rating = entity.Rating,
                Review = entity.Review,
                Genres = entity.Genres,
                ReleaseYear = entity.ReleaseYear,
                ImageUrl = entity.ImageUrl,
                MediaType = entity.MediaType,
                CreatedAtUtc = entity.CreatedAtUtc
            };

        public IEnumerable<MediaEntryDetailedDto> ToDetailedDtoCollection(IEnumerable<MediaEntryEntity> entities) =>
            entities.Select(ToDetailedDTO);

        public MediaEntryMinimalDto ToMinimalDTO(MediaEntryEntity entity) =>
            new(entity.Id, entity.Title, entity.MediaType, entity.ImageUrl);

        public IEnumerable<MediaEntryMinimalDto> ToMinimalDtoCollection(IEnumerable<MediaEntryEntity> entities) =>
            entities.Select(ToMinimalDTO);

        public MediaEntryUpdateDto ToUpdateDTO(MediaEntryEntity entity) =>
            new(
                entity.IdExternal,
                entity.Status,
                entity.Title ?? string.Empty,
                entity.Rating,
                entity.Review,
                entity.Genres,
                entity.ReleaseYear,
                entity.ImageUrl,
                entity.MediaType);
    }
}
