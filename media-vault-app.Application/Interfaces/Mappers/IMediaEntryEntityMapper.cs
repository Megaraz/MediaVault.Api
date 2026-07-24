using media_vault_app.Application.DTOs.MediaEntry.Response;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;

namespace media_vault_app.Application.Interfaces.Mappers
{
    public interface IMediaEntryEntityMapper : IMapEntityToDto<MediaEntryEntity, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto>
    {
    }
}
