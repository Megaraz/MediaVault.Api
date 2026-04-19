using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{

    public abstract class OwnedEntityRepoBase<TEntityOwned, TKeyOwner, TKeyOwned>
        : IOwnedEntityRepo<TEntityOwned, TKeyOwner, TKeyOwned>
            where TEntityOwned : class, IOwnableEntity<TKeyOwner, TKeyOwned>
            where TKeyOwner : notnull, IEquatable<TKeyOwner>
            where TKeyOwned : notnull, IEquatable<TKeyOwned>
    {

        protected readonly AppDbContext _appDbContext;
        protected readonly DbSet<TEntityOwned> _dbSet;

        protected OwnedEntityRepoBase(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntityOwned>();
        }

        public virtual async Task<Result<TEntityOwned>> CreateAsync(TEntityOwned entity, CancellationToken ct = default)
        {

            try
            {
                entity.CreatedAtUtc = DateTime.UtcNow;
                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return Result<TEntityOwned>.Success(entity);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);
                return Result<TEntityOwned>.Failure(DatabaseError.ConcurrencyFailure(baseErrorContext, ex));
            }
            catch (DbUpdateException ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);
                return Result<TEntityOwned>.Failure(DatabaseError.CreateFailure(baseErrorContext, ex));
            }
            catch (OperationCanceledException)
            {
                var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);
                return Result<TEntityOwned>.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);
                return Result<TEntityOwned>.Failure(DatabaseError.CreateFailure(baseErrorContext, ex));
            }

        }

        public virtual async Task<Result<IReadOnlyList<TEntityOwned>>> GetCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber, int pageSize, CancellationToken ct = default)
        {

            try
            {
                var ownedEntities = await _dbSet
                    .AsNoTracking()
                    .Where(ownedEntity => ownedEntity.OwnerId.Equals(ownerId))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct).ConfigureAwait(false);

                return Result<IReadOnlyList<TEntityOwned>>.Success(ownedEntities);
            }
            catch (OperationCanceledException)
            {
                var baseErrorContext = DefineErrorContext(nameof(GetCollectionByOwnerIdAsync), OperationType.GetCollection);
                return Result<IReadOnlyList<TEntityOwned>>.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(GetCollectionByOwnerIdAsync), OperationType.GetCollection);
                return Result<IReadOnlyList<TEntityOwned>>.Failure(DatabaseError.GetCollectionFailure(baseErrorContext, ex));
            }
        }

        public virtual async Task<Result<TEntityOwned>> GetByIdAsync(TKeyOwner ownerId, TKeyOwned entityId, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            try
            {
                var ownedEntity = await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(currentOwnedEntity =>
                        currentOwnedEntity.Id.Equals(entityId) &&
                        currentOwnedEntity.OwnerId.Equals(ownerId), ct)
                    .ConfigureAwait(false);

                if (ownedEntity is null)
                {
                    return Result<TEntityOwned>.Failure(
                        Error.NotFound(baseErrorContext));
                }

                return Result<TEntityOwned>.Success(ownedEntity);
            }
            catch (OperationCanceledException)
            {
                return Result<TEntityOwned>.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return Result<TEntityOwned>.Failure(DatabaseError.GetFailure(baseErrorContext, ex));
            }
        }

        public virtual async Task<Result> UpdateAsync(TKeyOwner ownerId, TEntityOwned updatedOwnedEntity, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            try
            {
                var existingOwnedEntity = await _dbSet
                    .FirstOrDefaultAsync(currentOwnedEntity =>
                        currentOwnedEntity.Id.Equals(updatedOwnedEntity.Id) &&
                        currentOwnedEntity.OwnerId.Equals(ownerId), ct)
                    .ConfigureAwait(false);

                if (existingOwnedEntity is null)
                {
                    return Result.Failure(
                        Error.NotFound(baseErrorContext));
                }

                var createdAt = existingOwnedEntity.CreatedAtUtc;
                _appDbContext.Entry(existingOwnedEntity).CurrentValues.SetValues(updatedOwnedEntity);
                existingOwnedEntity.CreatedAtUtc = createdAt;
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

        public virtual async Task<Result> DeleteAsync(TKeyOwner ownerId, TKeyOwned ownedEntityId, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            try
            {
                var ownedEntity = await _dbSet
                    .FirstOrDefaultAsync(currentOwnedEntity =>
                        currentOwnedEntity.Id.Equals(ownedEntityId) &&
                        currentOwnedEntity.OwnerId.Equals(ownerId), ct)
                    .ConfigureAwait(false);

                if (ownedEntity is null)
                {
                    return Result.Failure(
                        Error.NotFound(baseErrorContext));
                }

                _dbSet.Remove(ownedEntity);
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
                EntityName: typeof(TEntityOwned).Name,
                FieldName: fieldName);
        }

    }
}
