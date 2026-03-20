using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{
    /// <summary>
    /// Generic repository for performing CRUD operations on entities of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <remarks> This class implements the generic repository interface <see cref="IGenericRepo{TEntity, TKey}"/></remarks>
    public class GenericRepoEFCore<TEntity, TKey> :
        IGenericRepo<TEntity, TKey> where TEntity : class, IEntityId<TKey>
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
            ErrorContext errorContext = new(
                layer: "Infrastructure",
                serviceName: this.GetType().Name,
                methodName: nameof(CreateAsync),
                operation: OperationType.Create,
                entityName: typeof(TEntity).Name
            );


            if (entity is null || entity.Equals(default(TEntity)))
            {
                errorContext.DescriptionSuffix = $"A value for the entity '{errorContext.EntityName}' is required and cannot be null or empty.";

                ValidationError nullValueError = ValidationError.Required<TEntity>(errorContext);

                return Result<TEntity>.ValidationFailure([nullValueError], errorContext.DescriptionSuffix);
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
                    $"{errorContext.DescriptionPrefix}: {errorContext.DescriptionSuffix}",
                    ex);

                return Result<TEntity>.Failure(dbCreateFailure, "An error occurred while creating the entity.");
            }

        }


        public virtual async Task<Result<TEntity>> GetByIdAsync(TKey id, CancellationToken ct = default)
        {
            // Define error handling context
            ErrorContext errorContext = new(
                layer: "Infrastructure",
                serviceName: this.GetType().Name,
                methodName: nameof(GetByIdAsync),
                operation: OperationType.Get,
                entityName: typeof(TEntity).Name
            );

            if (!Validator.IsValidId(id))
            {
                errorContext.DescriptionSuffix = $"A valid Id is required and cannot be null or empty.";
                errorContext.EntityName = nameof(id);

                var nullValueError = ValidationError.Required<TKey>(errorContext);

                return Result<TEntity>.ValidationFailure([nullValueError], errorContext.DescriptionSuffix);
            }

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id! }, ct);
                if (entity is null)
                {
                    Error notFoundError = Error.NotFound<TEntity>(errorContext.DescriptionPrefix);

                    return Result<TEntity>.Failure(notFoundError, $"{typeof(TEntity).Name} not found");
                }
                return Result<TEntity>.Success(entity);
            }
            catch (Exception ex)
            {
                return Result<TEntity>.Failure(
                    Error.DbGetFailure<TEntity>(errorContext.DescriptionPrefix, ex),
                    $"An error occurred while retrieving the {typeof(TEntity).Name}.");
            }

        }

        public virtual async Task<Result<IReadOnlyList<TEntity>>> GetCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            // Define error handling context
            ErrorContext errorContext = new(
                layer: "Infrastructure",
                serviceName: this.GetType().Name,
                methodName: nameof(GetCollectionAsync),
                operation: OperationType.GetCollection,
                entityName: typeof(TEntity).Name
            );

            try
            {
                var entities = await _dbSet
                    .AsNoTracking()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                return Result<IReadOnlyList<TEntity>>.Success(entities);
            }
            catch (Exception ex)
            {
                return Result<IReadOnlyList<TEntity>>.Failure(
                    Error.DbGetCollectionFailure<TEntity>(errorContext.DescriptionPrefix, ex),
                    $"An error occurred while retrieving the {typeof(TEntity).Name} collection.");
            }
        }

        public virtual async Task<Result> DeleteAsync(TKey id, CancellationToken ct = default)
        {
            // Define error handling context
            var errorContext = new ErrorContext(
                layer: "Infrastructure",
                serviceName: this.GetType().Name,
                methodName: nameof(DeleteAsync),
                operation: OperationType.Delete,
                entityName: typeof(TEntity).Name
            );

            if (!Validator.IsValidId(id))
            {
                errorContext.DescriptionSuffix = $"A valid Id is required and cannot be null or empty.";
                errorContext.EntityName = nameof(id);

                var nullValueError = ValidationError.Required<TKey>(errorContext);

                return Result.ValidationFailure([nullValueError], errorContext.DescriptionSuffix);
            }

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id! }, ct);

                if (entity is null)
                {
                    return Result.Failure(
                        Error.NotFound<TEntity>(errorContext.DescriptionPrefix),
                        $"{typeof(TEntity).Name} not found");
                }

                _dbSet.Remove(entity);
                await _appDbContext.SaveChangesAsync(ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(
                    Error.DbDeleteFailure<TEntity>(errorContext.DescriptionPrefix, ex),
                    $"An error occurred while deleting the {typeof(TEntity).Name}.");
            }
        }

        public virtual async Task<Result> UpdateAsync(
            TEntity updatedEntity,
            Func<TEntity, TEntity, bool> shouldUpdate,
            CancellationToken ct = default)
        {

            // Define error handling context
            var errorContext = new ErrorContext(
                layer: "Infrastructure",
                serviceName: this.GetType().Name,
                methodName: nameof(UpdateAsync),
                operation: OperationType.Update,
                entityName: typeof(TEntity).Name
            );

            if (updatedEntity is null || updatedEntity.Equals(default(TEntity)))
            {
                errorContext.DescriptionSuffix = $"A value for the entity '{errorContext.EntityName}' is required and cannot be null or empty.";

                var requiredValueError = ValidationError.Required<TEntity>(errorContext);

                return Result.ValidationFailure([requiredValueError], errorContext.DescriptionSuffix);
            }


            if (!Validator.IsValidId(updatedEntity.Id))
            {
                errorContext.DescriptionSuffix = $"A valid Id is required and cannot be null or empty.";
                errorContext.EntityName = nameof(updatedEntity.Id);

                var nullValueError = ValidationError.Required<TKey>(errorContext);
                return Result.ValidationFailure([nullValueError], errorContext.DescriptionSuffix);
            }
            try
            {

                var oldEntity = await _dbSet.FindAsync(new object[] { updatedEntity.Id! }, ct);

                if (oldEntity is null)
                {
                    return Result.Failure(
                        Error.NotFound<TEntity>(errorContext.DescriptionPrefix),
                        $"{errorContext.EntityName} not found");
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
                    Error.DbUpdateFailure<TEntity>(errorContext.DescriptionPrefix, ex),
                    $"An error occurred while updating {errorContext.EntityName}.");
            }

        }
    }
}
