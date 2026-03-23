using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces
{
    public interface IGenericRepo<TEntity, TKey> where TEntity : class, IEntityId<TKey>
    {
        Task<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken ct = default);
        // TODO: Consider adding pagination parameters to this method in the future.
        // TODO: Maybe split to minimal/detailed DTOs in the future if needed.
        Task<Result<IReadOnlyList<TEntity>>> GetCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<Result<TEntity>> GetByIdAsync(TKey id, CancellationToken ct = default);
        Task<Result> UpdateAsync(TEntity updatedEntity, CancellationToken ct = default);
        Task<Result> DeleteAsync(TKey id, CancellationToken ct = default);
    }
}
