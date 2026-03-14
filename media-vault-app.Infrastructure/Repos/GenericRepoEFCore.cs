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
        private readonly string _entityName;

        public GenericRepoEFCore(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntity>();
            _entityName = typeof(TEntity).Name;
        }
        public virtual async Task<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken ct = default)
        {
            // Define error handling context
            string currentOperation = ErrorCodes.Operation.Create;
            string errorDescriptionPrefix = "An error occurred when trying to create the entity in Infrastructure layer, CreateAsync";
            string errorMessageReason = string.Empty;

            if (entity is null)
            {
                var errorCode = new ErrorCode(
                    currentOperation,
                    _entityName,
                    ErrorCodes.ValidationError.Required);

                errorMessageReason = "Entity cannot be null";

                return Result<TEntity>.Failure(
                    new Error(errorCode,
                        $"{errorDescriptionPrefix}: {errorMessageReason}",
                        ErrorType.Validation),
                     errorMessageReason);
            }

            try
            {

                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct);
                return Result<TEntity>.Success(entity);
            }
            catch (Exception ex)
            {
                var errorCode = new ErrorCode(
                    currentOperation,
                    _entityName,
                    ErrorCodes.DatabaseError.DbException);

                return Result<TEntity>.Failure(
                    new Error(errorCode,
                        $"{errorDescriptionPrefix}: {ex.Message}",
                        ErrorType.Failure),
                    "An error occurred while creating the entity.");
            }

        }


        public virtual async Task<Result<TEntity>> GetByIdAsync(TKey id, CancellationToken ct = default)
        {
            // Define error handling context
            string currentOperation = ErrorCodes.Operation.Get;
            string errorDescriptionPrefix = "An error occurred when trying to get the entity by Id in Infrastructure layer, GetByIdAsync";
            string errorMessageReason = string.Empty;

            if (id is null || id.Equals(default(TKey)))
            {
                var errorCode = new ErrorCode(
                    currentOperation,
                    _entityName,
                    ErrorCodes.ValidationError.Required);

                errorMessageReason = "Id cannot be null or default";

                return Result<TEntity>.Failure(
                    new Error(errorCode,
                    $"{errorDescriptionPrefix}: {errorMessageReason}",
                        ErrorType.Validation),
                    errorMessageReason);
            }

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id }, ct);
                if (entity is null)
                {
                    var errorCode = new ErrorCode(
                        currentOperation,
                        _entityName,
                        ErrorCodes.GeneralError.NotFound);

                    errorMessageReason = "Entity not found";

                    return Result<TEntity>.Failure(
                        new Error(errorCode,
                                $"{errorDescriptionPrefix}: {errorMessageReason}",
                            ErrorType.NotFound),
                        errorMessageReason);
                }
                return Result<TEntity>.Success(entity);
            }
            catch (Exception ex)
            {
                var errorCode = new ErrorCode(
                    currentOperation,
                    _entityName,
                    ErrorCodes.DatabaseError.DbException);

                return Result<TEntity>.Failure(
                    new Error(errorCode,
                        $"{errorDescriptionPrefix}: {ex.Message}",
                        ErrorType.Failure),
                    "An error occurred while retrieving the entity.");
            }

        }

        public virtual async Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(CancellationToken ct = default)
        {

            // Define error handling context
            string currentOperation = ErrorCodes.Operation.List;
            string errorDescriptionPrefix = "An error occurred when trying to get all entities in Infrastructure layer, GetAllAsync";
            string errorMessageReason = string.Empty;

            try
            {
                var entities = await _dbSet
                    .AsNoTracking()
                    .ToListAsync(ct);

                return Result<IReadOnlyList<TEntity>>.Success(entities);
            }
            catch (Exception ex)
            {
                var errorCode = new ErrorCode(
                    operation,
                    _entityName,
                    ErrorCodes.DatabaseError.DbException);

                return Result<IReadOnlyList<TEntity>>.Failure(
                    new Error(errorCode,
                        $"An error occurred when trying to get all entities in Infrastructure layer, GetAllAsync: {ex.Message}",
                        ErrorType.Failure),
                    "An error occurred while retrieving the entities.");
            }
        }

        public virtual async Task<Result> DeleteAsync(TKey id, CancellationToken ct = default)
        {

            // Define error handling context
            string currentOperation = ErrorCodes.Operation.Delete;
            string errorDescriptionPrefix = "An error occurred when trying to delete the entity in Infrastructure layer, DeleteAsync";
            string errorMessageReason = string.Empty;

            if (id is null || id.Equals(default(TKey)))
            {
                var errorCode = new ErrorCode(
                    currentOperation,
                    _entityName,
                    ErrorCodes.ValidationError.Required);

                errorMessageReason = "Id cannot be null or default";

                return Result.Failure(
                    new Error(errorCode,
                        $"{errorDescriptionPrefix}: {errorMessageReason}",
                        ErrorType.Validation),
                    errorMessageReason);
            }

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id }, ct);

                if (entity is null)
                {
                    var errorCode = new ErrorCode(
                        currentOperation,
                        _entityName,
                        ErrorCodes.GeneralError.NotFound);

                    errorMessageReason = "Entity not found";

                    return Result.Failure(
                        new Error(errorCode,
                            $"{errorDescriptionPrefix}: {errorMessageReason}",
                            ErrorType.NotFound),
                        errorMessageReason);
                }

                _dbSet.Remove(entity);
                await _appDbContext.SaveChangesAsync(ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                var errorCode = new ErrorCode(
                    currentOperation,
                    _entityName,
                    ErrorCodes.DatabaseError.DbException);

                return Result.Failure(
                    new Error(errorCode,
                        $"{errorDescriptionPrefix}: {ex.Message}",
                        ErrorType.Failure),
                    "An error occurred while deleting the entity.");
            }
        }

        public virtual async Task<Result> UpdateAsync(
            TEntity updatedEntity,
            Func<TEntity, TEntity, bool> shouldUpdate,
            CancellationToken ct = default)
        {

            if (updatedEntity.Id is null || updatedEntity.Id.Equals(default(TKey)))
            {
                return new ValidationErrorResult<TEntity>(
                    "Invalid value for Id",
                    new List<Error> { new Error("Entity.Update", "Id cannot be null or default") });
            }
            try
            {

                var oldEntity = await _dbSet.FindAsync(new object[] { updatedEntity.Id }, ct);

                if (oldEntity is null)
                {
                    return new ErrorResult<TEntity>("Entity not found", new List<Error> { new Error("Entity.Update", "Entity not found") });
                }

                if (!shouldUpdate(oldEntity, updatedEntity))
                {
                    return new SuccessResult();
                }

                _appDbContext.Entry(oldEntity).CurrentValues.SetValues(updatedEntity);
                await _appDbContext.SaveChangesAsync(ct);

                return new SuccessResult();

            }
            catch (Exception ex)
            {
                return new ErrorResult<TEntity>("An error occurred while updating the entity.", new List<Error> { new Error("Entity.Update", ex.Message) });

            }

        }
    }
}
