using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Interfaces;

namespace media_vault_app.Application.Interfaces.Repos
{
    public interface IGenericRepoEFCore<TEntity, TKey> where TEntity : class, IEntityId<TKey> 
    {
        Task<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken ct = default);
        Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(CancellationToken ct = default);
        Task<Result<TEntity>> GetByIdAsync(TKey id, CancellationToken ct = default);
        Task<Result> UpdateAsync(TEntity updatedEntity, Func<TEntity, TEntity, bool> shouldUpdate, CancellationToken ct = default);
        Task<Result> DeleteAsync(TKey id, CancellationToken ct = default);
    }
}
