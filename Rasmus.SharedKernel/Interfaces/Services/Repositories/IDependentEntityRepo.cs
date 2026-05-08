using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services.Repositories
{
    public interface IDependentEntityRepo<TEntityDependent, TKeyOwner, TKeyDependent> 
        where TEntityDependent : class, IDependentEntity<TKeyOwner, TKeyDependent>
        where TKeyOwner : notnull, IEquatable<TKeyOwner>
        where TKeyDependent : notnull, IEquatable<TKeyDependent>
    {

        Task<Result<TEntityDependent>> CreateAsync(TEntityDependent entity, CancellationToken ct = default);
        Task<Result> DeleteAsync(TKeyOwner ownerId, TKeyDependent dependentEntityId, CancellationToken ct = default);
        Task<Result<TEntityDependent>> GetByIdAsync(TKeyOwner ownerId, TKeyDependent dependentEntityId, CancellationToken ct = default);
        Task<Result<IReadOnlyList<TEntityDependent>>> GetCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<Result> UpdateAsync(TKeyOwner ownerId, TEntityDependent updatedDependentEntity, CancellationToken ct = default);
    }
}
