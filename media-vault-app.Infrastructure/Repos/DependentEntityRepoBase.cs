using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

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

        protected DependentEntityRepoBase(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntityDependent>();
        }

        public virtual async Task<Result<TEntityDependent>> CreateAsync(TEntityDependent entity, CancellationToken ct = default)
        {

            try
            {
                entity.CreatedAtUtc = DateTime.UtcNow;
                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return Result<TEntityDependent>.Success(entity);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);
                return Result<TEntityDependent>.Failure(DatabaseError.ConcurrencyFailure(baseErrorContext, ex));
            }
            catch (DbUpdateException ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);
                return Result<TEntityDependent>.Failure(DatabaseError.CreateFailure(baseErrorContext, ex));
            }
            catch (OperationCanceledException)
            {
                var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);
                return Result<TEntityDependent>.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);
                return Result<TEntityDependent>.Failure(DatabaseError.CreateFailure(baseErrorContext, ex));
            }

        }

        public virtual async Task<Result<IReadOnlyList<TEntityDependent>>> GetCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber, int pageSize, CancellationToken ct = default)
        {

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
                var baseErrorContext = DefineErrorContext(nameof(GetCollectionByOwnerIdAsync), OperationType.GetCollection);
                return Result<IReadOnlyList<TEntityDependent>>.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(GetCollectionByOwnerIdAsync), OperationType.GetCollection);
                return Result<IReadOnlyList<TEntityDependent>>.Failure(DatabaseError.GetCollectionFailure(baseErrorContext, ex));
            }
        }

        public virtual async Task<Result<TEntityDependent>> GetByIdAsync(TKeyOwner ownerId, TKeyDependent entityId, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            try
            {
                var dependentEntity = await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(currentDependentEntity =>
                        currentDependentEntity.Id.Equals(entityId) &&
                        currentDependentEntity.OwnerId.Equals(ownerId), ct)
                    .ConfigureAwait(false);

                if (dependentEntity is null)
                {
                    return Result<TEntityDependent>.Failure(
                        Error.NotFound(baseErrorContext));
                }

                return Result<TEntityDependent>.Success(dependentEntity);
            }
            catch (OperationCanceledException)
            {
                return Result<TEntityDependent>.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return Result<TEntityDependent>.Failure(DatabaseError.GetFailure(baseErrorContext, ex));
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
                        Error.NotFound(baseErrorContext));
                }

                var createdAt = existingDependentEntity.CreatedAtUtc;
                _appDbContext.Entry(existingDependentEntity).CurrentValues.SetValues(updatedDependentEntity);
                existingDependentEntity.CreatedAtUtc = createdAt;
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
                        Error.NotFound(baseErrorContext));
                }

                _dbSet.Remove(dependentEntity);
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


        protected virtual ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null)
        {
            return new ErrorContext(
                Layer: "Infrastructure",
                ServiceName: this.GetType().Name,
                MethodName: methodName,
                Operation: operation,
                EntityName: typeof(TEntityDependent).Name,
                FieldName: fieldName);
        }

    }
}
