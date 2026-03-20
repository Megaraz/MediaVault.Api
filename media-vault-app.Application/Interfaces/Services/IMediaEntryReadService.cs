using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces.Services;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IMediaEntryReadService : IReadService<MediaEntry, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto>
    {
    }
}
