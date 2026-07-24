using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services.Repositories
{
    public interface IRepo<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        Task<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken ct = default);
        Task<Result<IReadOnlyList<TEntity>>> GetCollectionAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<Result<TEntity>> GetByIdAsync(TKey id, CancellationToken ct = default);
        Task<Result> UpdateAsync(TEntity updatedEntity, CancellationToken ct = default);
        Task<Result> DeleteAsync(TKey id, CancellationToken ct = default);
        Task<Result<bool>> ExistsAsync(TKey id, CancellationToken ct = default);
    }
}
