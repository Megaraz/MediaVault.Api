using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;

namespace media_vault_app.Application.Services.User
{
    public class MediaEntryReadService : ReadServiceBase<MediaEntry, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto>, IMediaEntryReadService
    {
        public MediaEntryReadService(IGenericRepo<MediaEntry, Guid> repo, IMapEntityToDto<MediaEntry, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto> entityToDtoMapper) : base(repo, entityToDtoMapper)
        {
        }
    }
}
