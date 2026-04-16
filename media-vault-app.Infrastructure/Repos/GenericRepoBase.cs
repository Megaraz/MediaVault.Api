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
            var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);

            if (entity.IsNull(baseErrorContext, out var nullValueError))
                return Result<TEntity>.ValidationFailure([nullValueError], baseErrorContext.DescriptionSuffix!);

            try
            {
                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct);
                entity.CreatedAtUtc = DateTime.UtcNow; // Set CreatedAtUtc after saving to ensure it has a value
                return Result<TEntity>.Success(entity);
            }
            catch (Exception ex)
            {
                var dbExceptionErrorContext = baseErrorContext
                    with
                { DescriptionSuffix = $"An error occurred while creating the {baseErrorContext.EntityName}." };

                Error dbCreateFailure = Error.DbCreateFailure(dbExceptionErrorContext, ex);

                return Result<TEntity>.Failure(dbCreateFailure, dbExceptionErrorContext.DescriptionSuffix);
            }

        }


        public virtual async Task<Result<TEntity>> GetByIdAsync(TKey id, CancellationToken ct = default)
        {
            // Define error handling context
            var baseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            if (!id.IsValidId(baseErrorContext with { FieldName = nameof(id) }, out var idError))
            {
                return Result<TEntity>.ValidationFailure([idError], baseErrorContext.DescriptionSuffix!);
            }

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id! }, ct);
                if (entity is null)
                {
                    var notFoundErrorContext = baseErrorContext with { DescriptionSuffix = $"{baseErrorContext.EntityName} with the specified ID was not found." };

                    return Result<TEntity>.Failure(
                        Error.NotFound(notFoundErrorContext),
                        notFoundErrorContext.DescriptionSuffix);
                }
                return Result<TEntity>.Success(entity);
            }
            catch (Exception ex)
            {
                var dbExceptionErrorContext = baseErrorContext with { DescriptionSuffix = $"An error occurred while retrieving the {baseErrorContext.EntityName}." };

                return Result<TEntity>.Failure(
                    Error.DbGetFailure(dbExceptionErrorContext, ex),
                    dbExceptionErrorContext.DescriptionSuffix);
            }

        }

        public virtual async Task<Result<IReadOnlyList<TEntity>>> GetCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            // Define error handling context
            var baseErrorContext = DefineErrorContext(nameof(GetCollectionAsync), OperationType.GetCollection);

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
                var dbExceptionErrorContext = baseErrorContext with { DescriptionSuffix = $"An error occurred while retrieving the {baseErrorContext.EntityName} collection." };

                return Result<IReadOnlyList<TEntity>>.Failure(
                    Error.DbGetCollectionFailure(dbExceptionErrorContext, ex),
                    dbExceptionErrorContext.DescriptionSuffix);
            }
        }

        public virtual async Task<Result> DeleteAsync(TKey id, CancellationToken ct = default)
        {
            // Define error handling context
            var baseErrorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            if (!id.IsValidId(baseErrorContext with { FieldName = nameof(id) }, out var idError))
                return Result.ValidationFailure([idError], baseErrorContext.DescriptionSuffix!);

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id! }, ct);

                if (entity is null)
                {
                    var notFoundErrorContext = baseErrorContext with { DescriptionSuffix = $"{baseErrorContext.EntityName} with the specified ID was not found." };

                    return Result.Failure(
                        Error.NotFound(notFoundErrorContext),
                        notFoundErrorContext.DescriptionSuffix);
                }

                _dbSet.Remove(entity);
                await _appDbContext.SaveChangesAsync(ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                var dbExceptionErrorContext = baseErrorContext with { DescriptionSuffix = $"An error occurred while deleting the {baseErrorContext.EntityName}." };

                return Result.Failure(
                    Error.DbDeleteFailure(dbExceptionErrorContext, ex),
                    dbExceptionErrorContext.DescriptionSuffix);
            }
        }

        public virtual async Task<Result> UpdateAsync(
            TEntity updatedEntity,
            CancellationToken ct = default)
        {

            // Define error handling context
            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            if (updatedEntity.IsNull(baseErrorContext, out var nullValueError))
                return Result.ValidationFailure([nullValueError], baseErrorContext.DescriptionSuffix!);

            if (!updatedEntity.Id.IsValidId(baseErrorContext with { FieldName = nameof(updatedEntity.Id) }, out var idError))
                return Result.ValidationFailure([idError], baseErrorContext.DescriptionSuffix!);

            try
            {

                var oldEntity = await _dbSet.FindAsync(new object[] { updatedEntity.Id! }, ct);

                if (oldEntity is null)
                {
                    var notFoundErrorContext = baseErrorContext 
                        with { DescriptionSuffix = $"{baseErrorContext.EntityName} with the specified ID was not found." };

                    return Result.Failure(
                        Error.NotFound(notFoundErrorContext),
                        notFoundErrorContext.DescriptionSuffix);
                }

                _appDbContext.Entry(oldEntity).CurrentValues.SetValues(updatedEntity);
                await _appDbContext.SaveChangesAsync(ct);

                return Result.Success();

            }
            catch (Exception ex)
            {
                var dbExceptionErrorContext = baseErrorContext 
                    with { DescriptionSuffix = $"An error occurred while updating the {baseErrorContext.EntityName}." };

                return Result.Failure(
                    Error.DbUpdateFailure(dbExceptionErrorContext, ex),
                    dbExceptionErrorContext.DescriptionSuffix);
            }

        }

        public virtual async Task<Result<bool>> ExistsAsync(TKey id, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(ExistsAsync), OperationType.Get);

            if (!id.IsValidId(baseErrorContext with { FieldName = nameof(id) }, out var idError))
            {
                return Result<bool>.ValidationFailure([idError], baseErrorContext.DescriptionSuffix!);
            }

            try
            {
                var exists = await _dbSet.AnyAsync(entity => entity.Id.Equals(id), ct);

                if (!exists)
                {
                    var notFoundErrorContext = baseErrorContext with
                    {
                        DescriptionSuffix = $"{baseErrorContext.EntityName} with the specified ID was not found."
                    };

                    return Result<bool>.Failure(
                        Error.NotFound(notFoundErrorContext),
                        notFoundErrorContext.DescriptionSuffix);
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                var dbExceptionErrorContext = baseErrorContext with
                {
                    DescriptionSuffix = $"An error occurred while checking existence of {baseErrorContext.EntityName}."
                };

                return Result<bool>.Failure(
                    Error.DbGetFailure(dbExceptionErrorContext, ex),
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
                entityName: typeof(TEntity).Name,
                fieldName: fieldName);
        }
    }
}
