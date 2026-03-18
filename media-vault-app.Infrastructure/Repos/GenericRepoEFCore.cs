using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{
    /// <summary>
    /// Generic repository for performing CRUD operations on entities of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <remarks> This class implements the generic repository interface <see cref="IGenericRepo{TEntity, TKey}"/></remarks>
    public class GenericRepoEFCore<TEntity, TKey> : IGenericRepoEFCore<TEntity, TKey> where TEntity : class, IEntityId<TKey>, new()
    {
        protected readonly AppDbContext _appDbContext;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepoEFCore(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntity>();
        }
        public virtual async Task<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken ct = default)
        {
            // Define error handling context
            string methodName = nameof(CreateAsync);
            string errorDescriptionPrefix = $"An error occurred when trying to create the entity in Infrastructure Layer: {this.GetType().Name}.{methodName}()";
            string entityName = typeof(TEntity).Name;

            if (entity is null || entity.Equals(default(TEntity)))
            {
                string errorMessageReason = $"A value for the entity '{entityName}' is required and cannot be null or empty.";

                ValidationError nullValueError = ValidationError.Required<TEntity>(
                    OperationType.Create,
                    errorDescriptionPrefix,
                    entityName,
                    errorMessageReason);

                return Result<TEntity>.ValidationFailure([nullValueError], errorMessageReason);
            }

            try
            {
                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct);
                return Result<TEntity>.Success(entity);
            }
            catch (Exception ex)
            {
                Error dbCreateFailure = Error.DbCreateFailure<TEntity>(
                    errorDescriptionPrefix,
                    ex);

                return Result<TEntity>.Failure(dbCreateFailure, "An error occurred while creating the entity.");
            }

        }


        public virtual async Task<Result<TEntity>> GetByIdAsync(TKey id, CancellationToken ct = default)
        {
            // Define error handling context
            string methodName = nameof(GetByIdAsync);
            string errorDescriptionPrefix = $"An error occurred when trying to get the entity by Id in Infrastructure layer: {this.GetType().Name}.{methodName}()";

            if (!Validator.IsValidId(id))
            {
                string errorMessageReason = $"A valid Id is required and cannot be null or empty.";

                var nullValueError = ValidationError.Required<TKey>(
                    OperationType.Get,
                    errorDescriptionPrefix,
                    nameof(id),
                    errorMessageReason);

                return Result<TEntity>.ValidationFailure([nullValueError], errorMessageReason);
            }

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id! }, ct);
                if (entity is null)
                {
                    Error notFoundError = Error.NotFound<TEntity>(errorDescriptionPrefix);

                    return Result<TEntity>.Failure(notFoundError, $"{typeof(TEntity).Name} not found");
                }
                return Result<TEntity>.Success(entity);
            }
            catch (Exception ex)
            {
                return Result<TEntity>.Failure(
                    Error.DbGetFailure<TEntity>(errorDescriptionPrefix, ex),
                    $"An error occurred while retrieving the {typeof(TEntity).Name}.");
            }

        }

        public virtual async Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(CancellationToken ct = default)
        {
            // Define error handling context
            string methodName = nameof(GetAllAsync);
            string errorDescriptionPrefix = $"An error occurred when trying to get all entities in Infrastructure layer: {this.GetType().Name}.{methodName}()";

            try
            {
                var entities = await _dbSet
                    .AsNoTracking()
                    .ToListAsync(ct);

                return Result<IReadOnlyList<TEntity>>.Success(entities);
            }
            catch (Exception ex)
            {
                return Result<IReadOnlyList<TEntity>>.Failure(
                    Error.DbGetCollectionFailure<TEntity>(errorDescriptionPrefix, ex),
                    $"An error occurred while retrieving the {typeof(TEntity).Name} collection.");
            }
        }

        public virtual async Task<Result> DeleteAsync(TKey id, CancellationToken ct = default)
        {
            // Define error handling context
            string methodName = nameof(DeleteAsync);
            string errorDescriptionPrefix = $"An error occurred when trying to delete the {typeof(TEntity).Name} in Infrastructure layer: {this.GetType().Name}.{methodName}()";

            if (!Validator.IsValidId(id))
            {
                string errorMessageReason = $"A valid Id is required and cannot be null or empty.";

                var nullValueError = ValidationError.Required<TKey>(
                    OperationType.Delete,
                    errorDescriptionPrefix,
                    nameof(id),
                    errorMessageReason);

                return Result.ValidationFailure([nullValueError], errorMessageReason);
            }

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id! }, ct);

                if (entity is null)
                {
                    return Result.Failure(
                        Error.NotFound<TEntity>(errorDescriptionPrefix),
                        $"{typeof(TEntity).Name} not found");
                }

                _dbSet.Remove(entity);
                await _appDbContext.SaveChangesAsync(ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(
                    Error.DbDeleteFailure<TEntity>(errorDescriptionPrefix, ex),
                    $"An error occurred while deleting the {typeof(TEntity).Name}.");
            }
        }

        public virtual async Task<Result> UpdateAsync(
            TEntity updatedEntity,
            Func<TEntity, TEntity, bool> shouldUpdate,
            CancellationToken ct = default)
        {

            // Define error handling context
            string methodName = nameof(UpdateAsync);
            string entityName = typeof(TEntity).Name;
            string errorDescriptionPrefix = $"An error occurred when trying to update the {entityName} in Infrastructure layer: {this.GetType().Name}.{methodName}()";
            string? errorMessageReason;

            if (updatedEntity is null || updatedEntity.Equals(default(TEntity)))
            {
                errorMessageReason = $"A value for the entity '{entityName}' is required and cannot be null or empty.";

                var requiredValueError = ValidationError.Required<TEntity>(
                    OperationType.Update,
                    errorDescriptionPrefix,
                    entityName,
                    errorMessageReason);

                return Result.ValidationFailure([requiredValueError], errorMessageReason);
            }


            if (!Validator.IsValidId(updatedEntity.Id))
            {
                errorMessageReason = $"A valid Id is required and cannot be null or empty.";

                var nullValueError = ValidationError.Required<TKey>(
                    OperationType.Update,
                    errorDescriptionPrefix,
                    nameof(updatedEntity.Id),
                    errorMessageReason);

                return Result.ValidationFailure([nullValueError], errorMessageReason);
            }
            try
            {

                var oldEntity = await _dbSet.FindAsync(new object[] { updatedEntity.Id! }, ct);

                if (oldEntity is null)
                {
                    return Result.Failure(
                        Error.NotFound<TEntity>(errorDescriptionPrefix),
                        $"{entityName} not found");
                }

                if (!shouldUpdate(oldEntity, updatedEntity))
                {
                    return Result.Success();
                }

                _appDbContext.Entry(oldEntity).CurrentValues.SetValues(updatedEntity);
                await _appDbContext.SaveChangesAsync(ct);

                return Result.Success();

            }
            catch (Exception ex)
            {
                return Result.Failure(
                    Error.DbUpdateFailure<TEntity>(errorDescriptionPrefix, ex),
                    $"An error occurred while updating {entityName}.");
            }

        }
    }
}
