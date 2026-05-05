using System;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IMediaEntryWriteService : IDependentEntityWriteService<Guid, Guid, MediaEntryCreateDto, MediaEntryUpdateDto, MediaEntryDetailedDto>
    {
    }
}
