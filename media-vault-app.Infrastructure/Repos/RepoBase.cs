using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Rasmus.SharedKernel.Diagnostics;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using media_vault_app.Infrastructure.Diagnostics;

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
                return Result<TEntity>.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return await LogAndFailAsync<TEntity>(DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext, ex), CancellationToken.None);
            }
            catch (DbUpdateException ex)
            {
                return await LogAndFailAsync<TEntity>(DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, ex), CancellationToken.None);
            }
            catch (Exception ex)
            {
                return await LogAndFailAsync<TEntity>(DatabaseFailurePolicy.UnexpectedFailure(baseErrorContext, ex), CancellationToken.None);
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
                        MediaVaultErrors.NotFound(baseErrorContext));
                }
                return Result<TEntity>.Success(entity);
            }
            catch (OperationCanceledException)
            {
                return Result<TEntity>.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return await LogAndFailAsync<TEntity>(DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex), CancellationToken.None);
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
                return Result<IReadOnlyList<TEntity>>.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return await LogAndFailAsync<IReadOnlyList<TEntity>>(DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex), CancellationToken.None);
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
                        MediaVaultErrors.NotFound(baseErrorContext));
                }

                _dbSet.Remove(entity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return Result.Success();
            }
            catch (OperationCanceledException)
            {
                return Result.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return await LogAndFailAsync(DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext, ex), CancellationToken.None);
            }
            catch (DbUpdateException ex)
            {
                return await LogAndFailAsync(DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, ex), CancellationToken.None);
            }
            catch (Exception ex)
            {
                return await LogAndFailAsync(DatabaseFailurePolicy.UnexpectedFailure(baseErrorContext, ex), CancellationToken.None);
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
                        MediaVaultErrors.NotFound(baseErrorContext));
                }

                var createdAt = oldEntity.CreatedAtUtc;
                _appDbContext.Entry(oldEntity).CurrentValues.SetValues(updatedEntity);
                oldEntity.CreatedAtUtc = createdAt;
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);

                return Result.Success();
            }
            catch (OperationCanceledException)
            {
                return Result.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return await LogAndFailAsync(DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext, ex), CancellationToken.None);
            }
            catch (DbUpdateException ex)
            {
                return await LogAndFailAsync(DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, ex), CancellationToken.None);
            }
            catch (Exception ex)
            {
                return await LogAndFailAsync(DatabaseFailurePolicy.UnexpectedFailure(baseErrorContext, ex), CancellationToken.None);
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
                        MediaVaultErrors.NotFound(baseErrorContext));
                }

                return Result<bool>.Success(true);
            }
            catch (OperationCanceledException)
            {
                return Result<bool>.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return await LogAndFailAsync<bool>(DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex), CancellationToken.None);
            }
        }

        protected async Task<Result> LogAndFailAsync(
            Error error,
            CancellationToken ct = default,
            [CallerMemberName] string methodName = "")
        {
            try
            {
                var context = new ErrorLogContext("Infrastructure", GetType().Name, methodName);
                await _errorLogger.LogErrorToFileAsync(error, context, ct);
            }
            catch
            {
            }

            return Result.Failure(error);
        }

        protected async Task<Result<T>> LogAndFailAsync<T>(
            Error error,
            CancellationToken ct = default,
            [CallerMemberName] string methodName = "")
            where T : notnull
        {
            try
            {
                var context = new ErrorLogContext("Infrastructure", GetType().Name, methodName);
                await _errorLogger.LogErrorToFileAsync(error, context, ct);
            }
            catch
            {
            }

            return Result<T>.Failure(error);
        }

        protected virtual ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null)
        {
            return new ErrorContext(
                operation: operation,
                entityName: typeof(TEntity).Name,
                fieldName: fieldName);
        }
    }
}
