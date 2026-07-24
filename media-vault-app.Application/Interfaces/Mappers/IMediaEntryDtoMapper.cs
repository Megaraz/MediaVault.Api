using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;

namespace media_vault_app.Application.Interfaces.Mappers
{
    public interface IMediaEntryDtoMapper : IMapDtoToEntity<MediaEntryEntity, MediaEntryDetailedDto, MediaEntryCreateDto, MediaEntryUpdateDto, Guid>
    {
    }
}
