using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Repos
{
    public interface IMediaEntryRepo : IDependentEntityRepo<MediaEntry, Guid, Guid>
    {
        Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> GetMinimalCollectionByOwnerIdAsync(
            Guid ownerId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);

        Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> SearchMediaEntriesAsync(
            Guid userId,
            string query,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);
    }
}
