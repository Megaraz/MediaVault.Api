using Rasmus.SharedKernel.Interfaces.Identifiers;
using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface IDependentEntityWriteService<TKeyOwner, TKeyDependent, TCreateDto, TUpdateDto, TDetailedDto>
        where TKeyOwner : notnull, IEquatable<TKeyOwner>
        where TKeyDependent : notnull, IEquatable<TKeyDependent>
        where TDetailedDto : IDtoIdentifiable<TKeyDependent>
    {
        Task<Result<TDetailedDto>> CreateAsync(TKeyOwner ownerId, TCreateDto createDto, CancellationToken ct = default);
        Task<Result> DeleteAsync(
            TKeyOwner ownerId,
            TKeyDependent dependentId,
            int expectedVersion,
            CancellationToken ct = default);
        Task<Result> UpdateAsync(TKeyOwner ownerId, TKeyDependent id, TUpdateDto updateDto, CancellationToken ct = default);
    }
}
