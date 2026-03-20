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
using Rasmus.SharedKernel.Interfaces.Validators;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;
namespace media_vault_app.Application.Services.MediaEntry
{
    // TODO : Make either an entire IEntryService for User>Entry. Or Separated methods which has User > MediaEntry functionality
    public class MediaEntryWriteService : WriteServiceBase<MediaEntryEntity, Guid, MediaEntryCreateDto, MediaEntryUpdateDto, MediaEntryDetailedDto>, IMediaEntryWriteService
    {
        public MediaEntryWriteService(
            IGenericRepo<MediaEntryEntity, Guid> repo, 
            IMapEntityToDetailedDto<MediaEntryEntity, MediaEntryDetailedDto> entityToDtoMapper, 
            IMapDtoToEntity<MediaEntryEntity, MediaEntryDetailedDto, MediaEntryCreateDto, Guid, MediaEntryUpdateDto> dtoToEntityMapper,
            IDtoValidator<Guid, MediaEntryCreateDto, MediaEntryUpdateDto> dtoValidator
            ) : base(repo, entityToDtoMapper, dtoToEntityMapper, dtoValidator)
        {
        }
    }
}
