using System;
using System.Collections.Generic;
using media_vault_app.Application.DTOs;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IMediaEntryReadService : IDependentEntityReadService<Guid, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto>
    {
        Task<Result<IEnumerable<MediaEntryMinimalDto>>> SearchMediaEntriesAsync(
            Guid ownerId,
            SearchRequestDto request,
            int pageNumber = 1,
            int pageSize = 10, CancellationToken ct = default);
    }
}
