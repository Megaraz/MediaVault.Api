using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Rasmus.SharedKernel.Diagnostics;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using Rasmus.SharedKernel.ResultPatternCompatibility;

namespace media_vault_app.Infrastructure.Repos
{

    public abstract class DependentEntityRepoBase<TEntityDependent, TKeyOwner, TKeyDependent>
        : IDependentEntityRepo<TEntityDependent, TKeyOwner, TKeyDependent>
            where TEntityDependent : class, IDependentEntity<TKeyOwner, TKeyDependent>
            where TKeyOwner : notnull, IEquatable<TKeyOwner>
            where TKeyDependent : notnull, IEquatable<TKeyDependent>
    {

        protected readonly AppDbContext _appDbContext;
        protected readonly DbSet<TEntityDependent> _dbSet;
        protected readonly IErrorLogger _errorLogger;

        protected DependentEntityRepoBase(AppDbContext appDbContext, IErrorLogger errorLogger)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntityDependent>();
            _errorLogger = errorLogger;
        }

        public virtual async Task<Result<TEntityDependent>> CreateAsync(TEntityDependent entity, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);

            try
            {
                entity.CreatedAtUtc = DateTime.UtcNow;

                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);

                return Result<TEntityDependent>.Success(entity);
            }
            catch (OperationCanceledException)
            {
                return Result<TEntityDependent>.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var concurrencyError = DatabaseError.ConcurrencyFailure(baseErrorContext, ex);

                return await LogAndFailAsync<TEntityDependent>(concurrencyError, CancellationToken.None);
            }
            catch (DbUpdateException ex)
            {
                var createError = DatabaseError.SaveChangesFailure(baseErrorContext, ex);

                return await LogAndFailAsync<TEntityDependent>(createError, CancellationToken.None);

            }
            catch (Exception ex)
            {
                var error = DatabaseError.UnexpectedFailure(baseErrorContext, ex);

                return await LogAndFailAsync<TEntityDependent>(error, CancellationToken.None);
            }

        }

        public virtual async Task<Result<IReadOnlyList<TEntityDependent>>> GetCollectionByOwnerIdAsync(
            TKeyOwner ownerId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {

            var baseErrorContext = DefineErrorContext(nameof(GetCollectionByOwnerIdAsync), OperationType.GetCollection);

            try
            {
                var dependentEntities = await _dbSet
                    .AsNoTracking()
                    .Where(dependentEntity => dependentEntity.OwnerId.Equals(ownerId))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct).ConfigureAwait(false);

                return Result<IReadOnlyList<TEntityDependent>>.Success(dependentEntities);
            }
            catch (OperationCanceledException)
            {
                return Result<IReadOnlyList<TEntityDependent>>.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                var error = DatabaseError.QueryFailure(baseErrorContext, ex);

                return await LogAndFailAsync<IReadOnlyList<TEntityDependent>>(error, CancellationToken.None);
            }
        }

        public virtual async Task<Result<TEntityDependent>> GetByIdAsync(
            TKeyOwner ownerId,
            TKeyDependent entityId,
            Func<IQueryable<TEntityDependent>, IQueryable<TEntityDependent>>? include = null,
            CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            try
            {
                IQueryable<TEntityDependent> query = _dbSet.AsNoTracking();

                if (include is not null)
                {
                    query = include(query);
                }

                var dependentEntity = await query
                    .FirstOrDefaultAsync(
                        currentDependentEntity =>
                            currentDependentEntity.Id.Equals(entityId) &&
                            currentDependentEntity.OwnerId.Equals(ownerId),
                        ct)
                    .ConfigureAwait(false);

                if (dependentEntity is null)
                {
                    return Result<TEntityDependent>.Failure(MediaVaultErrors.NotFound(baseErrorContext));
                }

                return Result<TEntityDependent>.Success(dependentEntity);
            }
            catch (OperationCanceledException)
            {
                return Result<TEntityDependent>.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                var error = DatabaseError.UnexpectedFailure(baseErrorContext, ex);

                return await LogAndFailAsync<TEntityDependent>(error, CancellationToken.None);
            }
        }


        public virtual async Task<Result> UpdateAsync(TKeyOwner ownerId, TEntityDependent updatedDependentEntity, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            try
            {
                var existingDependentEntity = await _dbSet
                    .FirstOrDefaultAsync(currentDependentEntity =>
                        currentDependentEntity.Id.Equals(updatedDependentEntity.Id) &&
                        currentDependentEntity.OwnerId.Equals(ownerId), ct)
                    .ConfigureAwait(false);

                if (existingDependentEntity is null)
                {
                    return Result.Failure(
                        MediaVaultErrors.NotFound(baseErrorContext));
                }
                var originalId = existingDependentEntity.Id;
                var originalOwnerId = existingDependentEntity.OwnerId;
                var createdAt = existingDependentEntity.CreatedAtUtc;

                _appDbContext.Entry(existingDependentEntity)
                    .CurrentValues
                    .SetValues(updatedDependentEntity);

                existingDependentEntity.Id = originalId;
                existingDependentEntity.OwnerId = originalOwnerId;
                existingDependentEntity.CreatedAtUtc = createdAt;
                existingDependentEntity.UpdatedAtUtc = DateTime.UtcNow;

                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);

                return Result.Success();
            }
            catch (OperationCanceledException)
            {
                return Result.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var error = DatabaseError.ConcurrencyFailure(baseErrorContext, ex);
                return await LogAndFailAsync(error, CancellationToken.None);
            }
            catch (DbUpdateException ex)
            {
                var error = DatabaseError.SaveChangesFailure(baseErrorContext, ex);
                return await LogAndFailAsync(error, CancellationToken.None);
            }
            catch (Exception ex)
            {
                var error = DatabaseError.UnexpectedFailure(baseErrorContext, ex);
                return await LogAndFailAsync(error, CancellationToken.None);
            }
        }

        public virtual async Task<Result> DeleteAsync(TKeyOwner ownerId, TKeyDependent dependentEntityId, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            try
            {
                var dependentEntity = await _dbSet
                    .FirstOrDefaultAsync(currentDependentEntity =>
                        currentDependentEntity.Id.Equals(dependentEntityId) &&
                        currentDependentEntity.OwnerId.Equals(ownerId), ct)
                    .ConfigureAwait(false);

                if (dependentEntity is null)
                {
                    return Result.Failure(
                        MediaVaultErrors.NotFound(baseErrorContext));
                }

                _dbSet.Remove(dependentEntity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);

                return Result.Success();
            }
            catch (OperationCanceledException)
            {
                return Result.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var error = DatabaseError.ConcurrencyFailure(baseErrorContext, ex);
                return await LogAndFailAsync(error, CancellationToken.None);
            }
            catch (DbUpdateException ex)
            {
                var error = DatabaseError.SaveChangesFailure(baseErrorContext, ex);
                return await LogAndFailAsync(error, CancellationToken.None);
            }
            catch (Exception ex)
            {
                var error = DatabaseError.UnexpectedFailure(baseErrorContext, ex);
                return await LogAndFailAsync(error, CancellationToken.None);
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
                // Swallow logging failure, or fallback somewhere else later.
            }

            return Result<T>.Failure(error);
        }

        protected virtual ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null)
        {
            return new ErrorContext(
                operation: operation,
                entityName: typeof(TEntityDependent).Name,
                fieldName: fieldName);
        }

    }
}
