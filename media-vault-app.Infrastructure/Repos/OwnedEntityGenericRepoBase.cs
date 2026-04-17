using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{

    public abstract class OwnedEntityGenericRepoBase<TEntityOwner, TKeyOwner, TEntityOwned, TKeyOwned>
        : IOwnedEntityGenericRepo<TEntityOwner, TKeyOwner, TEntityOwned, TKeyOwned>
            where TEntityOwner : class, IOwnerEntity<TEntityOwner, TKeyOwner>
            where TKeyOwner : notnull, IEquatable<TKeyOwner>
            where TEntityOwned : class, IOwnedEntity<TEntityOwner, TKeyOwner, TEntityOwned, TKeyOwned>
            where TKeyOwned : notnull, IEquatable<TKeyOwned>
    {

        protected readonly AppDbContext _appDbContext;
        protected readonly DbSet<TEntityOwned> _dbSet;

        protected OwnedEntityGenericRepoBase(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntityOwned>();
        }

        public virtual async Task<Result<TEntityOwned>> CreateAsync(TEntityOwned entity, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);

            if (!entity.IsNull(baseErrorContext, out var nullValueError))
                throw new ArgumentNullException(nameof(entity), nullValueError.ToString());


            if (!entity.OwnerId.IsValidId(baseErrorContext with
            {
                FieldName = nameof(entity.OwnerId)

            }, out var invalidOwnerIdError))
            {
                throw new ArgumentException(invalidOwnerIdError.ToString(), nameof(entity));
            }


            try
            {
                entity.CreatedAtUtc = DateTime.UtcNow;
                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return Result<TEntityOwned>.Success(entity);
            }
            catch (Exception ex)
            {
                var dbExceptionErrorContext = baseErrorContext
                    with
                { DescriptionSuffix = $"An error occurred while creating the {baseErrorContext.EntityName}." };

                Error dbCreateFailure = Error.DbCreateFailure(dbExceptionErrorContext, ex);

                return Result<TEntityOwned>.Failure(dbCreateFailure, dbExceptionErrorContext.DescriptionSuffix);
            }

        }

        public virtual async Task<Result<IReadOnlyList<TEntityOwned>>> GetCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetCollectionByOwnerIdAsync), OperationType.GetCollection);

            if (!ownerId.IsValidId(baseErrorContext with
            {
                FieldName = nameof(ownerId)
            }, out var invalidOwnerIdError))
            {
                throw new ArgumentException(invalidOwnerIdError.ToString(), nameof(ownerId));
            }

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
            catch (Exception ex)
            {
                var dbExceptionErrorContext = baseErrorContext with
                { DescriptionSuffix = "An error occurred while retrieving the owned entity collection." };

                return Result<IReadOnlyList<TEntityOwned>>.Failure(
                    Error.DbGetCollectionFailure(dbExceptionErrorContext, ex),
                    dbExceptionErrorContext.DescriptionSuffix);
            }
        }

        public virtual async Task<Result<TEntityOwned>> GetByIdAsync(TKeyOwner ownerId, TKeyOwned ownedEntityId, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            if (!Validator.IsValidId(ownerId))
                throw new ArgumentException("Owner ID is not valid.", nameof(ownerId));

            if (!Validator.IsValidId(ownedEntityId))
                throw new ArgumentException("Owned entity ID is not valid.", nameof(ownedEntityId));


            try
            {
                var ownedEntity = await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(currentOwnedEntity =>
                        currentOwnedEntity.Id.Equals(ownedEntityId) &&
                        currentOwnedEntity.OwnerId.Equals(ownerId), ct)
                    .ConfigureAwait(false);

                if (ownedEntity is null)
                {

                    var notFoundErrorContext = baseErrorContext with { DescriptionSuffix = "Owned entity not found." };

                    return Result<TEntityOwned>.Failure(
                        Error.NotFound(notFoundErrorContext),
                        notFoundErrorContext.DescriptionSuffix);
                }

                return Result<TEntityOwned>.Success(ownedEntity);
            }
            catch (Exception ex)
            {
                var dbExceptionErrorContext = baseErrorContext with
                {
                    DescriptionSuffix = $"An error occurred while retrieving the owned entity with ID '{ownedEntityId}'."
                };
                return Result<TEntityOwned>.Failure(
                    Error.DbGetFailure(dbExceptionErrorContext, ex),
                    dbExceptionErrorContext.DescriptionSuffix);
            }
        }

        public virtual async Task<Result> UpdateAsync(TKeyOwner ownerId, TEntityOwned updatedOwnedEntity, CancellationToken ct = default)
        {
            if (!Validator.IsValidId(ownerId))
                throw new ArgumentException("Owner ID is not valid.", nameof(ownerId));

            ArgumentNullException.ThrowIfNull(updatedOwnedEntity);

            if (!Validator.IsValidId(updatedOwnedEntity.Id))
                throw new ArgumentException("Entity ID is not valid.", nameof(updatedOwnedEntity));

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
                        Error.NotFound(baseErrorContext with { DescriptionSuffix = "Owned entity not found." }),
                        "Owned entity not found.");
                }

                _appDbContext.Entry(existingOwnedEntity).CurrentValues.SetValues(updatedOwnedEntity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);

                return Result.Success();
            }
            catch (Exception ex)
            {
                var dbExceptionErrorContext = baseErrorContext with
                {
                    DescriptionSuffix = "An error occurred while updating the owned entity."
                };

                return Result.Failure(
                    Error.DbUpdateFailure(dbExceptionErrorContext, ex),
                    dbExceptionErrorContext.DescriptionSuffix);
            }
        }

        public virtual async Task<Result> DeleteAsync(TKeyOwner ownerId, TKeyOwned ownedEntityId, CancellationToken ct = default)
        {
            if (!Validator.IsValidId(ownerId))
                throw new ArgumentException("Owner ID is not valid.", nameof(ownerId));

            if (!Validator.IsValidId(ownedEntityId))
                throw new ArgumentException("Owned entity ID is not valid.", nameof(ownedEntityId));

            var baseErrorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            try
            {
                var ownedEntity = await _dbSet
                    .FirstOrDefaultAsync(currentOwnedEntity => currentOwnedEntity.Id.Equals(ownedEntityId) && currentOwnedEntity.OwnerId.Equals(ownerId), ct).ConfigureAwait(false);

                if (ownedEntity is null)
                {
                    return Result.Failure(
                        Error.NotFound(baseErrorContext),
                        "Owned entity not found.");
                }

                _dbSet.Remove(ownedEntity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);

                return Result.Success();
            }
            catch (Exception ex)
            {
                var dbExceptionErrorContext = baseErrorContext with
                {
                    DescriptionSuffix = "An error occurred while deleting the owned entity."
                };

                return Result.Failure(
                    Error.DbDeleteFailure(dbExceptionErrorContext, ex),
                    dbExceptionErrorContext.DescriptionSuffix);
            }
        }


        protected virtual ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null)
        {
            return new ErrorContext(
                layer: "Infrastructure",
                serviceName: this.GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: typeof(TEntityOwned).Name,
                fieldName: fieldName);
        }

    }
}
