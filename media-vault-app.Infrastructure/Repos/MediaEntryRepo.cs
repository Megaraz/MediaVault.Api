using System;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{
    public class MediaEntryRepo : GenericRepoEFCore<MediaEntry, Guid>, IMediaEntryRepo
    {
        public MediaEntryRepo(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<Result<IReadOnlyList<MediaEntry>>> GetCollectionByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var errorContext = DefineErrorContext(nameof(GetCollectionByUserIdAsync), OperationType.GetCollection);

            if (!userId.IsValidId(errorContext, out var idError))
            {
                return Result<IReadOnlyList<MediaEntry>>.ValidationFailure([idError], errorContext.DescriptionSuffix!);
            }

            try
            {
                var mediaEntries = await _dbSet
                    .AsNoTracking()
                    .Where(mediaEntry => mediaEntry.UserId == userId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                return Result<IReadOnlyList<MediaEntry>>.Success(mediaEntries);
            }
            catch (Exception ex)
            {
                errorContext.DescriptionSuffix = "An error occurred while retrieving the MediaEntry collection.";

                return Result<IReadOnlyList<MediaEntry>>.Failure(
                    Error.DbGetCollectionFailure(errorContext, ex),
                    errorContext.DescriptionSuffix);
            }
        }

        public async Task<Result<MediaEntry>> GetByIdAsync(Guid userId, Guid mediaEntryId, CancellationToken ct = default)
        {
            var errorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            if (!userId.IsValidId(errorContext, out var userIdError))
            {
                return Result<MediaEntry>.ValidationFailure([userIdError], errorContext.DescriptionSuffix!);
            }

            if (!mediaEntryId.IsValidId(errorContext, out var mediaEntryIdError))
            {
                return Result<MediaEntry>.ValidationFailure([mediaEntryIdError], errorContext.DescriptionSuffix!);
            }

            try
            {
                var mediaEntry = await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(currentMediaEntry => currentMediaEntry.Id == mediaEntryId && currentMediaEntry.UserId == userId, ct);

                if (mediaEntry is null)
                {
                    return Result<MediaEntry>.Failure(
                        Error.NotFound<MediaEntry>(errorContext.DescriptionPrefix),
                        "MediaEntry not found.");
                }

                return Result<MediaEntry>.Success(mediaEntry);
            }
            catch (Exception ex)
            {
                errorContext.DescriptionSuffix = "An error occurred while retrieving the MediaEntry.";

                return Result<MediaEntry>.Failure(
                    Error.DbGetFailure(errorContext, ex),
                    errorContext.DescriptionSuffix);
            }
        }

        public async Task<Result> UpdateAsync(Guid userId, MediaEntry updatedEntity, Func<MediaEntry, MediaEntry, bool> shouldUpdate, CancellationToken ct = default)
        {
            var errorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            if (!userId.IsValidId(errorContext, out var userIdError))
                return Result.ValidationFailure([userIdError], errorContext.DescriptionSuffix!);

            if (updatedEntity.IsNull(errorContext, out var requiredValueError))
                return Result.ValidationFailure([requiredValueError], errorContext.DescriptionSuffix!);

            if (!updatedEntity.Id.IsValidId(errorContext, out var mediaEntryIdError))
                return Result.ValidationFailure([mediaEntryIdError], errorContext.DescriptionSuffix!);

            try
            {
                var existingEntity = await _dbSet
                    .FirstOrDefaultAsync(currentMediaEntry => currentMediaEntry.Id == updatedEntity.Id && currentMediaEntry.UserId == userId, ct);

                if (existingEntity is null)
                {
                    return Result.Failure(
                        Error.NotFound<MediaEntry>(errorContext.DescriptionPrefix),
                        "MediaEntry not found.");
                }

                if (!shouldUpdate(existingEntity, updatedEntity))
                {
                    return Result.Success();
                }

                _appDbContext.Entry(existingEntity).CurrentValues.SetValues(updatedEntity);
                await _appDbContext.SaveChangesAsync(ct);

                return Result.Success();
            }
            catch (Exception ex)
            {
                errorContext.DescriptionSuffix = "An error occurred while updating the MediaEntry.";

                return Result.Failure(
                    Error.DbUpdateFailure(errorContext, ex),
                    errorContext.DescriptionSuffix);
            }
        }

        public async Task<Result> DeleteAsync(Guid userId, Guid mediaEntryId, CancellationToken ct = default)
        {
            var errorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            if (!userId.IsValidId(errorContext, out var userIdError))
                return Result.ValidationFailure([userIdError], errorContext.DescriptionSuffix!);

            if (!mediaEntryId.IsValidId(errorContext, out var mediaEntryIdError))
                return Result.ValidationFailure([mediaEntryIdError], errorContext.DescriptionSuffix!);

            try
            {
                var mediaEntry = await _dbSet
                    .FirstOrDefaultAsync(currentMediaEntry => currentMediaEntry.Id == mediaEntryId && currentMediaEntry.UserId == userId, ct);

                if (mediaEntry is null)
                {
                    return Result.Failure(
                        Error.NotFound<MediaEntry>(errorContext.DescriptionPrefix),
                        "MediaEntry not found.");
                }

                _dbSet.Remove(mediaEntry);
                await _appDbContext.SaveChangesAsync(ct);

                return Result.Success();
            }
            catch (Exception ex)
            {
                errorContext.DescriptionSuffix = "An error occurred while deleting the MediaEntry.";

                return Result.Failure(
                    Error.DbDeleteFailure(errorContext, ex),
                    errorContext.DescriptionSuffix);
            }
        }

        private ErrorContext DefineErrorContext(string methodName, OperationType operation)
        {
            return new ErrorContext(
                layer: "Infrastructure",
                serviceName: this.GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: typeof(MediaEntry).Name);
        }
    }
}
