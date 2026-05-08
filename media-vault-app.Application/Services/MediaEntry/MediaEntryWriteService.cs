using System;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Mappers;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Interfaces.Validators;
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
            IMediaEntryRepo dependentEntityRepo,
            IUserRepo ownerRepo,
            IMediaEntryEntityMapper entityMapper,
            IMediaEntryDtoMapper dtoMapper,
            IMediaEntryDtoValidator dtoValidator
            ) : base(dependentEntityRepo, ownerRepo, entityMapper, dtoMapper, dtoValidator)
        {
        }
    }
}
