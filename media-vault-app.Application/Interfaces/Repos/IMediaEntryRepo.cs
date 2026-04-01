using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Repos
{
    public interface IMediaEntryRepo : IGenericRepo<MediaEntry, Guid>
    {
        Task<Result<IReadOnlyList<MediaEntry>>> GetCollectionByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<Result<IReadOnlyList<MediaEntry>>> SearchMediaEntriesAsync(Guid userId, string query, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<Result<MediaEntry>> GetByIdAsync(Guid userId, Guid mediaEntryId, CancellationToken ct = default);
        Task<Result> UpdateAsync(Guid userId, MediaEntry updatedEntity, CancellationToken ct = default);
        Task<Result> DeleteAsync(Guid userId, Guid mediaEntryId, CancellationToken ct = default);
    }
}
