using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface IOwnedEntityReadService<TKeyOwner, TKeyOwned, TDetailedDto, TMinimalDto>
        where TKeyOwner : notnull, IEquatable<TKeyOwner>
        where TKeyOwned : notnull, IEquatable<TKeyOwned>
        where TDetailedDto : IDtoIdentifiable<TKeyOwned>
        where TMinimalDto : IDtoIdentifiable<TKeyOwned>
    {
        Task<Result<TDetailedDto>> GetByIdAsync(TKeyOwner ownerId, TKeyOwned id, CancellationToken ct = default);
        Task<Result<IEnumerable<TDetailedDto>>> GetDetailedCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<Result<IEnumerable<TMinimalDto>>> GetMinimalCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    }
}
