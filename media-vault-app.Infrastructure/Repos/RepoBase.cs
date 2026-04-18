using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{
    /// <summary>
    /// Generic repository for performing CRUD operations on entities of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <remarks> This class implements the generic repository interface <see cref="IRepo{TEntity, TKey}"/></remarks>
    public class RepoBase<TEntity, TKey> :
        IRepo<TEntity, TKey> where TEntity : class, IWriteableEntity<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        protected readonly AppDbContext _appDbContext;
        protected readonly DbSet<TEntity> _dbSet;

        public RepoBase(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntity>();
        }
        public virtual async Task<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);
            try
            {
                entity.CreatedAtUtc = DateTime.UtcNow;
                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return Result<TEntity>.Success(entity);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Result<TEntity>.Failure(DatabaseError.ConcurrencyFailure(baseErrorContext, ex));
            }
            catch (DbUpdateException ex)
            {
                return Result<TEntity>.Failure(DatabaseError.CreateFailure(baseErrorContext, ex));
            }
            catch (OperationCanceledException)
            {
                return Result<TEntity>.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return Result<TEntity>.Failure(DatabaseError.CreateFailure(baseErrorContext, ex));
            }
        }


        public virtual async Task<Result<TEntity>> GetByIdAsync(TKey id, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id! }, ct).ConfigureAwait(false);
                if (entity is null)
                {
                    return Result<TEntity>.Failure(
                        Error.NotFound(baseErrorContext));
                }
                return Result<TEntity>.Success(entity);
            }
            catch (OperationCanceledException)
            {
                return Result<TEntity>.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return Result<TEntity>.Failure(DatabaseError.GetFailure(baseErrorContext, ex));
            }

        }

        public virtual async Task<Result<IReadOnlyList<TEntity>>> GetCollectionAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {

            try
            {
                var entities = await _dbSet
                    .AsNoTracking()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct).ConfigureAwait(false);

                return Result<IReadOnlyList<TEntity>>.Success(entities);
            }
            catch (OperationCanceledException)
            {
                var baseErrorContext = DefineErrorContext(nameof(GetCollectionAsync), OperationType.GetCollection);
                return Result<IReadOnlyList<TEntity>>.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(GetCollectionAsync), OperationType.GetCollection);
                return Result<IReadOnlyList<TEntity>>.Failure(DatabaseError.GetCollectionFailure(baseErrorContext, ex));
            }
        }

        public virtual async Task<Result> DeleteAsync(TKey id, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id! }, ct).ConfigureAwait(false);

                if (entity is null)
                {
                    return Result.Failure(
                        Error.NotFound(baseErrorContext));
                }

                _dbSet.Remove(entity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Result.Failure(DatabaseError.ConcurrencyFailure(baseErrorContext, ex));
            }
            catch (DbUpdateException ex)
            {
                return Result.Failure(DatabaseError.DeleteFailure(baseErrorContext, ex));
            }
            catch (OperationCanceledException)
            {
                return Result.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return Result.Failure(DatabaseError.DeleteFailure(baseErrorContext, ex));
            }
        }

        public virtual async Task<Result> UpdateAsync(
            TEntity updatedEntity,
            CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            try
            {
                var oldEntity = await _dbSet.FindAsync(new object[] { updatedEntity.Id! }, ct).ConfigureAwait(false);

                if (oldEntity is null)
                {
                    return Result.Failure(
                        Error.NotFound(baseErrorContext));
                }

                var createdAt = oldEntity.CreatedAtUtc;
                _appDbContext.Entry(oldEntity).CurrentValues.SetValues(updatedEntity);
                oldEntity.CreatedAtUtc = createdAt;
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);

                return Result.Success();

            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Result.Failure(DatabaseError.ConcurrencyFailure(baseErrorContext, ex));
            }
            catch (DbUpdateException ex)
            {
                return Result.Failure(DatabaseError.UpdateFailure(baseErrorContext, ex));
            }
            catch (OperationCanceledException)
            {
                return Result.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return Result.Failure(DatabaseError.UpdateFailure(baseErrorContext, ex));
            }

        }

        public virtual async Task<Result<bool>> ExistsAsync(TKey id, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(ExistsAsync), OperationType.Get);

            try
            {
                var exists = await _dbSet.AnyAsync(entity => entity.Id.Equals(id), ct).ConfigureAwait(false);

                if (!exists)
                {
                    return Result<bool>.Failure(
                        Error.NotFound(baseErrorContext));
                }

                return Result<bool>.Success(true);
            }
            catch (OperationCanceledException)
            {
                return Result<bool>.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(DatabaseError.GetFailure(baseErrorContext, ex));
            }
        }

        protected virtual ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null, string? confirmFieldName = null)
        {
            return new ErrorContext(
                Layer: "Infrastructure",
                ServiceName: this.GetType().Name,
                MethodName: methodName,
                Operation: operation,
                EntityName: typeof(TEntity).Name,
                FieldName: fieldName,
                ConfirmFieldName: confirmFieldName);
        }
    }
}
