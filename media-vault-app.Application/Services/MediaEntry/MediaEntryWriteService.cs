using System;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Services.Base_Classes;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Validators;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Services.MediaEntry
{
    public class MediaEntryWriteService
        : DependentEntityWriteServiceBase<UserEntity, MediaEntryEntity, Guid, Guid, MediaEntryCreateDto, MediaEntryUpdateDto, MediaEntryDetailedDto>,
        IMediaEntryWriteService
    {
        public MediaEntryWriteService(
            IMediaEntryRepo ownedEntityRepo,
            IUserRepo ownerRepo,
            IMapEntityToDto<MediaEntryEntity, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto> entityMapper,
            IMapDtoToEntity<MediaEntryEntity, MediaEntryDetailedDto, MediaEntryCreateDto, MediaEntryUpdateDto, Guid> dtoMapper,
            IDtoValidator<Guid, MediaEntryCreateDto, MediaEntryUpdateDto> validator
            ) : base(ownedEntityRepo, ownerRepo, entityMapper, dtoMapper, validator)
        {
        }
    }
}
