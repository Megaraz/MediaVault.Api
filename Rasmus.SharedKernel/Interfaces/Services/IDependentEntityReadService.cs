using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface IDependentEntityReadService<TKeyOwner, TKeyDependent, TDetailedDto, TMinimalDto>
        where TKeyOwner : notnull, IEquatable<TKeyOwner>
        where TKeyDependent : notnull, IEquatable<TKeyDependent>
        where TDetailedDto : IDtoIdentifiable<TKeyDependent>
        where TMinimalDto : IDtoIdentifiable<TKeyDependent>
    {
        Task<Result<TDetailedDto>> GetByIdAsync(TKeyOwner ownerId, TKeyDependent id, CancellationToken ct = default);
        Task<Result<IEnumerable<TDetailedDto>>> GetDetailedCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<Result<IEnumerable<TMinimalDto>>> GetMinimalCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    }
}
