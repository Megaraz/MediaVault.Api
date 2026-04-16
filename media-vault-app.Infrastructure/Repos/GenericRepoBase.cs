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
    public class GenericRepoBase<TEntity, TKey> :
        IGenericRepo<TEntity, TKey> where TEntity : class, IEntityId<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        protected readonly AppDbContext _appDbContext;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepoBase(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntity>();
        }
        public virtual async Task<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken ct = default)
        {
            // Define error handling context
            var errorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);


            if (entity.IsNull(errorContext, out var nullValueError))
                return Result<TEntity>.ValidationFailure([nullValueError], errorContext.DescriptionSuffix!);

            try
            {
                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct);
                entity.CreatedAtUtc = DateTime.UtcNow; // Set CreatedAtUtc after saving to ensure it has a value
                return Result<TEntity>.Success(entity);
            }
            catch (Exception ex)
            {
                errorContext.DescriptionSuffix = $"An error occurred while creating the {errorContext.EntityName}.";

                Error dbCreateFailure = Error.DbCreateFailure(errorContext, ex);

                return Result<TEntity>.Failure(dbCreateFailure, "An error occurred while creating the entity.");
            }

        }


        public virtual async Task<Result<TEntity>> GetByIdAsync(TKey id, CancellationToken ct = default)
        {
            // Define error handling context
            var errorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            if (!id.IsValidId(errorContext, out var idError))
            {
                return Result<TEntity>.ValidationFailure([idError], errorContext.DescriptionSuffix!);
            }

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id! }, ct);
                if (entity is null)
                {
                    Error notFoundError = Error.NotFound(errorContext);

                    return Result<TEntity>.Failure(notFoundError, $"{typeof(TEntity).Name} not found");
                }
                return Result<TEntity>.Success(entity);
            }
            catch (Exception ex)
            {
                errorContext.DescriptionSuffix = $"An error occurred while retrieving the {errorContext.EntityName}.";

                return Result<TEntity>.Failure(
                    Error.DbGetFailure(errorContext, ex),
                    $"An error occurred while retrieving the {typeof(TEntity).Name}.");
            }

        }

        public virtual async Task<Result<IReadOnlyList<TEntity>>> GetCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            // Define error handling context

            var errorContext = DefineErrorContext(nameof(GetCollectionAsync), OperationType.GetCollection);

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
                errorContext.DescriptionSuffix = $"An error occurred while retrieving the {errorContext.EntityName} collection.";

                return Result<IReadOnlyList<TEntity>>.Failure(
                    Error.DbGetCollectionFailure(errorContext, ex),
                    errorContext.DescriptionSuffix);
            }
        }

        public virtual async Task<Result> DeleteAsync(TKey id, CancellationToken ct = default)
        {
            // Define error handling context
            var errorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            if (!id.IsValidId(errorContext, out var idError))
                return Result.ValidationFailure([idError], errorContext.DescriptionSuffix!);

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id! }, ct);

                if (entity is null)
                {
                    return Result.Failure(
                        Error.NotFound(errorContext),
                        $"{typeof(TEntity).Name} not found");
                }

                _dbSet.Remove(entity);
                await _appDbContext.SaveChangesAsync(ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                errorContext.DescriptionSuffix = $"An error occurred while deleting the {errorContext.EntityName}.";

                return Result.Failure(
                    Error.DbDeleteFailure(errorContext, ex),
                    errorContext.DescriptionSuffix);
            }
        }

        public virtual async Task<Result> UpdateAsync(
            TEntity updatedEntity,
            CancellationToken ct = default)
        {

            // Define error handling context
            var errorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            if (updatedEntity.IsNull(errorContext, out var nullValueError))
                return Result.ValidationFailure([nullValueError], errorContext.DescriptionSuffix!);

            if (!updatedEntity.Id.IsValidId(errorContext, out var idError))
                return Result.ValidationFailure([idError], errorContext.DescriptionSuffix!);

            try
            {

                var oldEntity = await _dbSet.FindAsync(new object[] { updatedEntity.Id! }, ct);

                if (oldEntity is null)
                {
                    return Result.Failure(
                        Error.NotFound(errorContext),
                        $"{errorContext.EntityName} not found");
                }

                _appDbContext.Entry(oldEntity).CurrentValues.SetValues(updatedEntity);
                await _appDbContext.SaveChangesAsync(ct);

                return Result.Success();

            }
            catch (Exception ex)
            {
                errorContext.DescriptionSuffix = $"An error occurred while updating the {errorContext.EntityName}.";

                return Result.Failure(
                    Error.DbUpdateFailure(errorContext, ex),
                    errorContext.DescriptionSuffix);
            }

        }
        protected virtual ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null)
        {
            return new ErrorContext(
                layer: "Infrastructure",
                serviceName: this.GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: typeof(TEntity).Name,
                fieldName: fieldName);
        }
    }
}
