using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
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
        protected readonly ErrorEventLogger<RepoBase<TEntity, TKey>> _errorEventLogger;

        public RepoBase(
            AppDbContext appDbContext,
            ErrorEventLogger<RepoBase<TEntity, TKey>> errorEventLogger)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntity>();
            _errorEventLogger = errorEventLogger;
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
                return LogAndFail<TEntity>(DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext, ex), baseErrorContext);
            }
            catch (DbUpdateException ex)
            {
                return LogAndFail<TEntity>(DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, ex), baseErrorContext);
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
            catch (System.Data.Common.DbException ex)
            {
                return LogAndFail<TEntity>(DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex), baseErrorContext);
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
            catch (System.Data.Common.DbException ex)
            {
                return LogAndFail<IReadOnlyList<TEntity>>(DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex), baseErrorContext);
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
            catch (DbUpdateConcurrencyException ex)
            {
                return LogAndFail(DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext, ex), baseErrorContext);
            }
            catch (DbUpdateException ex)
            {
                return LogAndFail(DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, ex), baseErrorContext);
            }
            catch (System.Data.Common.DbException ex)
            {
                return LogAndFail(DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex), baseErrorContext);
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
            catch (DbUpdateConcurrencyException ex)
            {
                return LogAndFail(DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext, ex), baseErrorContext);
            }
            catch (DbUpdateException ex)
            {
                return LogAndFail(DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, ex), baseErrorContext);
            }
            catch (System.Data.Common.DbException ex)
            {
                return LogAndFail(DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex), baseErrorContext);
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
            catch (System.Data.Common.DbException ex)
            {
                return LogAndFail<bool>(DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex), baseErrorContext);
            }
        }

        protected Result LogAndFail(
            Error error,
            ErrorContext errorContext,
            [CallerMemberName] string methodName = "")
        {
            var context = new ErrorEventContext(
                "Infrastructure", GetType().Name, methodName, errorContext);
            _errorEventLogger.Log(error, context);

            return Result.Failure(error);
        }

        protected Result<T> LogAndFail<T>(
            Error error,
            ErrorContext errorContext,
            [CallerMemberName] string methodName = "")
            where T : notnull
        {
            var context = new ErrorEventContext(
                "Infrastructure", GetType().Name, methodName, errorContext);
            _errorEventLogger.Log(error, context);

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
