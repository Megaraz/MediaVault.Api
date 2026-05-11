using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;
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
        IRepo<TEntity, TKey> 
        where TEntity : class, IEntity<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        protected readonly AppDbContext _appDbContext;
        protected readonly DbSet<TEntity> _dbSet;
        protected readonly IErrorLogger _errorLogger;

        public RepoBase(AppDbContext appDbContext, IErrorLogger errorLogger)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntity>();
            _errorLogger = errorLogger;
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
            catch (OperationCanceledException)
            {
                return Result<TEntity>.Failure(Error.Cancelled(baseErrorContext));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return await LogAndFailAsync<TEntity>(DatabaseError.ConcurrencyFailure(baseErrorContext, ex), CancellationToken.None);
            }
            catch (DbUpdateException ex)
            {
                return await LogAndFailAsync<TEntity>(DatabaseError.SaveChangesFailure(baseErrorContext, ex), CancellationToken.None);
            }
            catch (Exception ex)
            {
                return await LogAndFailAsync<TEntity>(DatabaseError.UnexpectedFailure(baseErrorContext, ex), CancellationToken.None);
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
                return Result<TEntity>.Failure(Error.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return await LogAndFailAsync<TEntity>(DatabaseError.QueryFailure(baseErrorContext, ex), CancellationToken.None);
            }
        }

        public virtual async Task<Result<IReadOnlyList<TEntity>>> GetCollectionAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetCollectionAsync), OperationType.GetCollection);

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
                return Result<IReadOnlyList<TEntity>>.Failure(Error.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return await LogAndFailAsync<IReadOnlyList<TEntity>>(DatabaseError.QueryFailure(baseErrorContext, ex), CancellationToken.None);
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
            catch (OperationCanceledException)
            {
                return Result.Failure(Error.Cancelled(baseErrorContext));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return await LogAndFailAsync(DatabaseError.ConcurrencyFailure(baseErrorContext, ex), CancellationToken.None);
            }
            catch (DbUpdateException ex)
            {
                return await LogAndFailAsync(DatabaseError.SaveChangesFailure(baseErrorContext, ex), CancellationToken.None);
            }
            catch (Exception ex)
            {
                return await LogAndFailAsync(DatabaseError.UnexpectedFailure(baseErrorContext, ex), CancellationToken.None);
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
            catch (OperationCanceledException)
            {
                return Result.Failure(Error.Cancelled(baseErrorContext));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return await LogAndFailAsync(DatabaseError.ConcurrencyFailure(baseErrorContext, ex), CancellationToken.None);
            }
            catch (DbUpdateException ex)
            {
                return await LogAndFailAsync(DatabaseError.SaveChangesFailure(baseErrorContext, ex), CancellationToken.None);
            }
            catch (Exception ex)
            {
                return await LogAndFailAsync(DatabaseError.UnexpectedFailure(baseErrorContext, ex), CancellationToken.None);
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
                return Result<bool>.Failure(Error.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return await LogAndFailAsync<bool>(DatabaseError.QueryFailure(baseErrorContext, ex), CancellationToken.None);
            }
        }

        protected async Task<Result> LogAndFailAsync(Error error, CancellationToken ct = default)
        {
            try
            {
                await _errorLogger.LogErrorToFileAsync(error, ct);
            }
            catch
            {
            }

            return Result.Failure(error);
        }

        protected async Task<Result<T>> LogAndFailAsync<T>(Error error, CancellationToken ct = default)
        {
            try
            {
                await _errorLogger.LogErrorToFileAsync(error, ct);
            }
            catch
            {
            }

            return Result<T>.Failure(error);
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
