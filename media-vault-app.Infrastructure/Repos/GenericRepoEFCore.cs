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

        public GenericRepoEFCore(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _dbSet = _appDbContext.Set<TEntity>();
        }
        public virtual async Task<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken ct = default)
        {

            if (entity is null)
                return new ValidationErrorResult<TEntity>(
                    "Adding of Entity to database did not proceed, reason: Entity cannot be null",
                    new List<Error>()
                    {
                        new Error("Entity.Create", "Entity cannot not be null")
                    });

            try
            {

                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct);
                return new SuccessResult<TEntity>(entity);
            }
            catch (Exception ex)
            {
                return new ErrorResult<TEntity>("An error occurred while creating the entity.", new List<Error> { new Error("Entity.Create", ex.Message) });
            }

        }


        public virtual async Task<Result<TEntity>> GetByIdAsync(TKey id, CancellationToken ct = default)
        {
            if (id is null || id.Equals(default(TKey)))
            {
                return new ValidationErrorResult<TEntity>(
                    "Invalid value for Id",
                    new List<Error> { new Error("Entity.GetById", "Id cannot be null or default") });
            }

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id }, ct);
                if (entity is null)
                {
                    return new ErrorResult<TEntity>("Entity not found", new List<Error> { new Error("Entity.GetById", "Entity not found") });
                }
                return new SuccessResult<TEntity>(entity);
            }
            catch (Exception ex)
            {
                return new ErrorResult<TEntity>("An error occurred while retrieving the entity.", new List<Error> { new Error("Entity.GetById", ex.Message) });
            }

        }

        public virtual async Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(CancellationToken ct = default)
        {

            try
            {
                var entities = await _dbSet
                    .AsNoTracking()
                    .ToListAsync(ct);

                return new SuccessResult<IReadOnlyList<TEntity>>(entities);
            }
            catch (Exception ex)
            {
                return new ErrorResult<IReadOnlyList<TEntity>>("An error occurred while retrieving the entities.", new List<Error> { new Error("Entity.GetAll", ex.Message) });
            }
        }

        public virtual async Task<Result> DeleteAsync(TKey id, CancellationToken ct = default)
        {
            if (id is null || id.Equals(default(TKey)))
            {
                return new ValidationErrorResult<TEntity>(
                    "Invalid value for Id",
                    new List<Error> { new Error("Entity.GetById", "Id cannot be null or default") });
            }

            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id }, ct);

                if (entity is null)
                {
                    return new ErrorResult<TEntity>("Entity not found", new List<Error> { new Error("Entity.Delete", "Entity not found") });
                }

                _dbSet.Remove(entity);
                await _appDbContext.SaveChangesAsync(ct);
                return new SuccessResult();
            }
            catch (Exception ex)
            {
                return new ErrorResult<TEntity>("An error occurred while deleting the entity.", new List<Error> { new Error("Entity.Delete", ex.Message) });

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
