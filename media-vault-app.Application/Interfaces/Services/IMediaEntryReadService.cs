using System;
using System.Collections.Generic;
using media_vault_app.Application.DTOs.ExternalAPIs;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IMediaEntryReadService
    {

        Task<Result<IEnumerable<MediaEntryMinimalDto>>> SearchMediaEntriesAsync(
            Guid userId,
            SearchRequestDto request,
            int pageNumber = 1,
            int pageSize = 10, CancellationToken ct = default);
        Task<Result<MediaEntryDetailedDto>> GetByIdAsync(Guid userId, Guid mediaEntryId, CancellationToken ct = default);
        Task<Result<IEnumerable<MediaEntryDetailedDto>>> GetDetailedCollectionAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<Result<IEnumerable<MediaEntryMinimalDto>>> GetMinimalCollectionAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    }
}
