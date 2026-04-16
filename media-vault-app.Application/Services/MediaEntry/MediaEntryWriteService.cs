using System;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Mappers.MediaEntry;
using media_vault_app.Application.Services.Base_Classes;
using media_vault_app.Application.Validators.MediaEntry;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Validators;
using Rasmus.SharedKernel.ResultPattern;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Services.MediaEntry
{
    public class MediaEntryWriteService
        : OwnedEntityWriteServiceBase<UserEntity, Guid, MediaEntryEntity, Guid, MediaEntryCreateDto, MediaEntryUpdateDto, MediaEntryDetailedDto>,
        IMediaEntryWriteService
    {
        public MediaEntryWriteService(
            IOwnedEntityGenericRepo<UserEntity, Guid, MediaEntryEntity, Guid> ownedEntityRepo, 
            IGenericRepo<UserEntity, Guid> ownerRepo, 
            IMapEntityToDetailedDto<MediaEntryEntity, MediaEntryDetailedDto> entityToDtoMapper, 
            IMapDtoToEntity<MediaEntryEntity, MediaEntryDetailedDto, MediaEntryCreateDto, MediaEntryUpdateDto, Guid> dtoToEntityMapper, 
            IDtoValidator<Guid, MediaEntryCreateDto, MediaEntryUpdateDto> dtoValidator
            ) : base(ownedEntityRepo, ownerRepo, entityToDtoMapper, dtoToEntityMapper, dtoValidator)
        {
        }
    }
}
