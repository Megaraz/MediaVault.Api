using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{

    public class OwnedEntityRepoBase<TEntityOwner, TKeyOwner, TEntityOwned, TKeyOwned>
        : GenericRepoBase<TEntityOwned, TKeyOwned>,
        IOwnedEntityGenericRepo<TEntityOwner, TKeyOwner, TEntityOwned, TKeyOwned>
            where TEntityOwner : class, IOwnerEntity<TEntityOwner, TKeyOwner>
            where TKeyOwner : notnull, IEquatable<TKeyOwner>
            where TEntityOwned : class, IOwnedEntity<TEntityOwner, TKeyOwner, TEntityOwned, TKeyOwned>
            where TKeyOwned : notnull, IEquatable<TKeyOwned>
    {
        public OwnedEntityRepoBase(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<Result<IReadOnlyList<TEntityOwned>>> GetCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetCollectionByOwnerIdAsync), OperationType.GetCollection);

            if (!ownerId.IsValidId(baseErrorContext with { FieldName = nameof(ownerId) }, out var idError))
            {
                return Result<IReadOnlyList<TEntityOwned>>.ValidationFailure([idError], baseErrorContext.DescriptionSuffix!);
            }

            try
            {
                var ownedEntities = await _dbSet
                    .AsNoTracking()
                    .Where(ownedEntity => ownedEntity.OwnerId.Equals(ownerId))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

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

        public async Task<Result<TEntityOwned>> GetByIdAsync(TKeyOwner ownerId, TKeyOwned ownedEntityId, CancellationToken ct = default)
        {

            var baseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            List<ValidationError> validationErrors = new();

            if (!ownerId.IsValidId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdError))
                validationErrors.Add(ownerIdError);

            if (!ownedEntityId.IsValidId(baseErrorContext with { FieldName = nameof(ownedEntityId) }, out var ownedEntityIdError))
                validationErrors.Add(ownedEntityIdError);

            if (validationErrors.Count > 0)
                return Result<TEntityOwned>.ValidationFailure(validationErrors, "Validation Errors occurred, see validationErrors for details.");

            try
            {
                var ownedEntity = await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(currentOwnedEntity => currentOwnedEntity.Id.Equals(ownedEntityId) && currentOwnedEntity.OwnerId.Equals(ownerId), ct);

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

        public async Task<Result> UpdateAsync(TKeyOwner ownerId, TEntityOwned updatedOwnedEntity, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            List<ValidationError> validationErrors = new();

            if (!ownerId.IsValidId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdError))
                validationErrors.Add(ownerIdError);

            if (updatedOwnedEntity.IsNull(baseErrorContext with { FieldName = nameof(updatedOwnedEntity) }, out var requiredValueError))
            {
                validationErrors.Add(requiredValueError);

                return Result.ValidationFailure(validationErrors, "Validation Errors occurred, see validationErrors for details.");
            }

            if (!updatedOwnedEntity.Id.IsValidId(baseErrorContext with { FieldName = nameof(updatedOwnedEntity.Id) }, out var updatedOwnedEntityIdError))
                validationErrors.Add(updatedOwnedEntityIdError);

            if (validationErrors.Count > 0)
                return Result.ValidationFailure(validationErrors, "Validation Errors occurred, see validationErrors for details.");

            try
            {
                var existingOwnedEntity = await _dbSet
                    .FirstOrDefaultAsync(currentOwnedEntity => currentOwnedEntity.Id.Equals(updatedOwnedEntity.Id) && currentOwnedEntity.OwnerId.Equals(ownerId), ct);

                if (existingOwnedEntity is null)
                {
                    return Result.Failure(
                        Error.NotFound(baseErrorContext with { DescriptionSuffix = "Owned entity not found." }),
                        "Owned entity not found.");
                }

                _appDbContext.Entry(existingOwnedEntity).CurrentValues.SetValues(updatedOwnedEntity);
                await _appDbContext.SaveChangesAsync(ct);

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

        public async Task<Result> DeleteAsync(TKeyOwner ownerId, TKeyOwned ownedEntityId, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            List<ValidationError> validationErrors = new();

            if (!ownerId.IsValidId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdError))
                validationErrors.Add(ownerIdError);

            if (!ownedEntityId.IsValidId(baseErrorContext with { FieldName = nameof(ownedEntityId) }, out var ownedEntityIdError))
                validationErrors.Add(ownedEntityIdError);

            if (validationErrors.Count > 0)
                return Result.ValidationFailure(validationErrors, "Validation Errors occurred, see validationErrors for details.");

            try
            {
                var ownedEntity = await _dbSet
                    .FirstOrDefaultAsync(currentOwnedEntity => currentOwnedEntity.Id.Equals(ownedEntityId) && currentOwnedEntity.OwnerId.Equals(ownerId), ct);

                if (ownedEntity is null)
                {
                    return Result.Failure(
                        Error.NotFound(baseErrorContext),
                        "Owned entity not found.");
                }

                _dbSet.Remove(ownedEntity);
                await _appDbContext.SaveChangesAsync(ct);

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


    }
}
