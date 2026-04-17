using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface IOwnedEntityWriteService<TKeyOwner, TKeyOwned, TCreateDto, TUpdateDto, TDetailedDto>
        where TKeyOwner : notnull, IEquatable<TKeyOwner>
        where TKeyOwned : notnull, IEquatable<TKeyOwned>
        where TDetailedDto : IDtoID<TKeyOwned>
    {
        Task<Result<TDetailedDto>> CreateAsync(TKeyOwner ownerId, TCreateDto createDto, CancellationToken ct);
        Task<Result> DeleteAsync(TKeyOwner ownerId, TKeyOwned ownedId, CancellationToken ct = default);
        Task<Result> UpdateAsync(TKeyOwner ownerId, TKeyOwned id, TUpdateDto updateDto, CancellationToken ct = default);
    }
}
