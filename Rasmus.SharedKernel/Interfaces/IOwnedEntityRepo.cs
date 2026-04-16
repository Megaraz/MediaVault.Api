using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces
{
    public interface IOwnedEntityGenericRepo<TEntityOwner, TKeyOwner, TEntityOwned, TKeyOwned> : IGenericRepo<TEntityOwned, TKeyOwned>
        where TEntityOwner : class, IOwnerEntity<TEntityOwner, TKeyOwner>
        where TKeyOwner : notnull, IEquatable<TKeyOwner>
        where TEntityOwned : class, IOwnedEntity<TEntityOwner, TKeyOwner, TEntityOwned, TKeyOwned>
        where TKeyOwned : notnull, IEquatable<TKeyOwned>
    {
        Task<Result> DeleteAsync(TKeyOwner ownerId, TKeyOwned ownedEntityId, CancellationToken ct = default);
        Task<Result<TEntityOwned>> GetByIdAsync(TKeyOwner ownerId, TKeyOwned ownedEntityId, CancellationToken ct = default);
        Task<Result<IReadOnlyList<TEntityOwned>>> GetCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<Result> UpdateAsync(TKeyOwner ownerId, TEntityOwned updatedOwnedEntity, CancellationToken ct = default);
    }
}
