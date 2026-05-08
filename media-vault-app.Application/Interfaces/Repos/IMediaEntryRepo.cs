using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Repos
{
    public interface IMediaEntryRepo : IDependentEntityRepo<MediaEntry, Guid, Guid>
    {
        Task<Result<IReadOnlyList<MediaEntry>>> SearchMediaEntriesAsync(
            Guid userId, 
            string query, 
            int pageNumber, 
            int pageSize, 
            CancellationToken ct = default);
    }
}
