using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;

namespace media_vault_app.Application.Services.MediaEntry
{
    public class MediaEntryReadService : ReadServiceBase<MediaEntryEntity, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto>, IMediaEntryReadService
    {
        private readonly IMediaEntryRepo _mediaEntryRepo;
        public MediaEntryReadService(IMediaEntryRepo repo, IMapEntityToDto<MediaEntryEntity, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto> entityToDtoMapper) : base(repo, entityToDtoMapper)
        {
            _mediaEntryRepo = repo;
        }



    }
}
