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
    public class GenericRepoEFCore<TEntity, TKey> : IGenericRepoEFCore<TEntity, TKey> where TEntity : class, IEntityId<TKey>
    {
        protected readonly AppDbContext _appDbContext;
        protected readonly DbSet<TEntity> _dbSet;
        //private readonly string _entityName;

        public GenericRepoEFCore(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntity>();
            //_entityName = typeof(TEntity).Name;
        }
        public virtual async Task<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken ct = default)
        {
            // Define error handling context
            string methodName = nameof(CreateAsync);
            string currentOperation = ErrorCodes.Operation.Create;
            string errorDescriptionPrefix = $"An error occurred when trying to create the entity in Infrastructure Layer: {this.GetType().Name}.{methodName}()";

            if (entity is null || entity.Equals(default(TEntity)))
            {
                ValidationError nullValueError = ValidationError.NullValue<TEntity>(
                    currentOperation,
                    errorDescriptionPrefix,
                    out string errorMessageReason);

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
            string currentOperation = ErrorCodes.Operation.Get;
            string errorDescriptionPrefix = $"An error occurred when trying to get the entity by Id in Infrastructure layer: {this.GetType().Name}.{methodName}()";

            if (!Validator.IsValidId(id))
            {
                var nullValueError = ValidationError.NullValue<TKey>(
                    currentOperation,
                    errorDescriptionPrefix,
                    out string errorMessageReason);

                return Result<TEntity>.ValidationFailure([nullValueError], errorMessageReason);
            }

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id }, ct);
                if (entity is null)
                {
                    Error notFoundError = Error.NotFound<TEntity>(errorDescriptionPrefix);

                    return Result<TEntity>.Failure(notFoundError, "Entity not found");
                }
                return Result<TEntity>.Success(entity);
            }
            catch (Exception ex)
            {
                return Result<TEntity>.Failure(
                    Error.DbGetFailure<TEntity>(errorDescriptionPrefix, ex),
                    "An error occurred while retrieving the entity.");
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
                    "An error occurred while retrieving the entities.");
            }
        }

        public virtual async Task<Result> DeleteAsync(TKey id, CancellationToken ct = default)
        {
            // Define error handling context
            string methodName = nameof(DeleteAsync);
            string currentOperation = ErrorCodes.Operation.Delete;
            string errorDescriptionPrefix = $"An error occurred when trying to delete the entity in Infrastructure layer: {this.GetType().Name}.{methodName}()";

            if (!Validator.IsValidId(id))
            {
                var nullValueError = ValidationError.NullValue<TKey>(
                    currentOperation,
                    errorDescriptionPrefix,
                    out string errorMessageReason);

                return Result.ValidationFailure([nullValueError], errorMessageReason);
            }

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id! }, ct);

                if (entity is null)
                {
                    return Result.Failure(
                        Error.NotFound<TEntity>(errorDescriptionPrefix),
                        "Entity not found");
                }

                _dbSet.Remove(entity);
                await _appDbContext.SaveChangesAsync(ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(
                    Error.DbDeleteFailure<TEntity>(errorDescriptionPrefix, ex),
                    "An error occurred while deleting the entity.");
            }
        }

        public virtual async Task<Result> UpdateAsync(
            TEntity updatedEntity,
            Func<TEntity, TEntity, bool> shouldUpdate,
            CancellationToken ct = default)
        {

            // Define error handling context
            string methodName = nameof(UpdateAsync);
            string currentOperation = ErrorCodes.Operation.Update;
            string errorDescriptionPrefix = $"An error occurred when trying to update the entity in Infrastructure layer: {this.GetType().Name}.{methodName}()";

            if (updatedEntity is null || updatedEntity.Equals(default(TEntity)))
            {
                var nullValueError = ValidationError.NullValue<TEntity>(
                    currentOperation,
                    errorDescriptionPrefix,
                    out string errorMessageReason);
                return Result.ValidationFailure([nullValueError], errorMessageReason);
            }


            if (!Validator.IsValidId(updatedEntity.Id))
            {
                var nullValueError = ValidationError.NullValue<TKey>(
                    currentOperation,
                    errorDescriptionPrefix,
                    out string errorMessageReason);

                return Result.ValidationFailure([nullValueError], errorMessageReason);
            }
            try
            {

                var oldEntity = await _dbSet.FindAsync(new object[] { updatedEntity.Id! }, ct);

                if (oldEntity is null)
                {
                    return Result.Failure(
                        Error.NotFound<TEntity>(errorDescriptionPrefix),
                        "Entity not found");
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
                    "An error occurred while updating the entity.");
            }

        }
    }
}
