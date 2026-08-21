using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Infrastructure.Timestamps;

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
        protected readonly ErrorEventLogger<DependentEntityRepoBase<TEntityDependent, TKeyOwner, TKeyDependent>> _errorEventLogger;
        protected readonly ServerTimestampPolicy _timestampPolicy;

        protected DependentEntityRepoBase(
            AppDbContext appDbContext,
            ErrorEventLogger<DependentEntityRepoBase<TEntityDependent, TKeyOwner, TKeyDependent>> errorEventLogger,
            ServerTimestampPolicy? timestampPolicy = null)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntityDependent>();
            _errorEventLogger = errorEventLogger;
            _timestampPolicy = timestampPolicy ?? new ServerTimestampPolicy(TimeProvider.System);
        }

        public virtual async Task<Result<TEntityDependent>> CreateAsync(TEntityDependent entity, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);

            try
            {
                _timestampPolicy.Initialize(entity);

                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);

                return Result<TEntityDependent>.Success(entity);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var concurrencyError = DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext, ex);

                return LogAndFail<TEntityDependent>(concurrencyError, baseErrorContext);
            }
            catch (DbUpdateException ex)
            {
                var createError = DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, ex);

                return LogAndFail<TEntityDependent>(createError, baseErrorContext);

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
                    .OrderByDescending(dependentEntity => dependentEntity.CreatedAtUtc)
                    .ThenBy(dependentEntity => dependentEntity.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct).ConfigureAwait(false);

                return Result<IReadOnlyList<TEntityDependent>>.Success(dependentEntities);
            }
            catch (System.Data.Common.DbException ex)
            {
                var error = DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex);

                return LogAndFail<IReadOnlyList<TEntityDependent>>(error, baseErrorContext);
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
            catch (System.Data.Common.DbException ex)
            {
                var error = DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex);

                return LogAndFail<TEntityDependent>(error, baseErrorContext);
            }
        }


        public virtual async Task<Result> UpdateAsync(
            TKeyOwner ownerId,
            TEntityDependent updatedDependentEntity,
            CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);
            var expectedVersion = updatedDependentEntity.Version;

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

                if (existingDependentEntity.Version != expectedVersion)
                {
                    return LogAndFail(
                        DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext),
                        baseErrorContext);
                }
                var originalId = existingDependentEntity.Id;
                var originalOwnerId = existingDependentEntity.OwnerId;
                var createdAt = existingDependentEntity.CreatedAtUtc;
                var updatedAt = existingDependentEntity.UpdatedAtUtc;

                _appDbContext.Entry(existingDependentEntity)
                    .CurrentValues
                    .SetValues(updatedDependentEntity);

                existingDependentEntity.Id = originalId;
                existingDependentEntity.OwnerId = originalOwnerId;
                var hasMeaningfulChanges = ApplyUpdateTimestamp(
                    existingDependentEntity,
                    createdAt,
                    updatedAt);
                ApplyConcurrencyVersion(
                    existingDependentEntity,
                    expectedVersion,
                    hasMeaningfulChanges);

                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);

                return Result.Success();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var error = DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext, ex);
                return LogAndFail(error, baseErrorContext);
            }
            catch (DbUpdateException ex)
            {
                var error = DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, ex);
                return LogAndFail(error, baseErrorContext);
            }
            catch (System.Data.Common.DbException ex)
            {
                var error = DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex);
                return LogAndFail(error, baseErrorContext);
            }
        }

        public virtual async Task<Result> DeleteAsync(
            TKeyOwner ownerId,
            TKeyDependent dependentEntityId,
            int expectedVersion,
            CancellationToken ct = default)
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

                if (dependentEntity.Version != expectedVersion)
                {
                    return LogAndFail(
                        DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext),
                        baseErrorContext);
                }

                _appDbContext.Entry(dependentEntity)
                    .Property(nameof(IConcurrencyVersion.Version))
                    .OriginalValue = expectedVersion;

                _dbSet.Remove(dependentEntity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);

                return Result.Success();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var error = DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext, ex);
                return LogAndFail(error, baseErrorContext);
            }
            catch (DbUpdateException ex)
            {
                var error = DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, ex);
                return LogAndFail(error, baseErrorContext);
            }
            catch (System.Data.Common.DbException ex)
            {
                var error = DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex);
                return LogAndFail(error, baseErrorContext);
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
                entityName: typeof(TEntityDependent).Name,
                fieldName: fieldName);
        }

        protected bool ApplyUpdateTimestamp(
            IEntity<TKeyDependent> entity,
            DateTime originalCreatedAtUtc,
            DateTime originalUpdatedAtUtc,
            bool relatedEntityChanged = false)
        {
            _appDbContext.ChangeTracker.DetectChanges();
            var entry = _appDbContext.Entry(entity);
            var hasMeaningfulChanges = relatedEntityChanged || entry.Properties.Any(
                property => property.IsModified &&
                    property.Metadata.Name is not nameof(ICreatedAtUtc.CreatedAtUtc) and
                    not nameof(IUpdatedAtUtc.UpdatedAtUtc));

            _timestampPolicy.ApplyUpdate(
                entity,
                originalCreatedAtUtc,
                originalUpdatedAtUtc,
                hasMeaningfulChanges);

            entry.Property(nameof(ICreatedAtUtc.CreatedAtUtc)).IsModified = false;
            entry.Property(nameof(IUpdatedAtUtc.UpdatedAtUtc)).IsModified = hasMeaningfulChanges;

            return hasMeaningfulChanges;
        }

        protected void ApplyConcurrencyVersion(
            TEntityDependent entity,
            int expectedVersion,
            bool hasMeaningfulChanges)
        {
            var versionProperty = _appDbContext.Entry(entity)
                .Property(nameof(IConcurrencyVersion.Version));
            versionProperty.OriginalValue = expectedVersion;
            versionProperty.CurrentValue = hasMeaningfulChanges
                ? checked(expectedVersion + 1)
                : expectedVersion;
            versionProperty.IsModified = hasMeaningfulChanges;
        }

    }
}
