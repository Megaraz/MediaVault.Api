using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.Interfaces.Services;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services
{
    public class GenericService<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto> : IGenericService<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto>
        where TEntity : class, IEntityId<TKey>, new()
        where TDetailedDto : IEntityId<TKey>
    {

        private readonly IGenericRepoEFCore<TEntity, TKey> _repo;

        public GenericService(IGenericRepoEFCore<TEntity, TKey> repo)
        {
            _repo = repo;
        }


        public async Task<Result<TDetailedDto>> CreateAsync(TCreateDto createDto, CancellationToken ct)
        {
            string methodName = nameof(CreateAsync);
            string errorDescriptionPrefix = $"An error occurred when trying to create the entity in Service Layer: {this.GetType().Name}.{methodName}()";
            string entityName = typeof(TCreateDto).Name;

            if (createDto is null || createDto.Equals(default(TCreateDto)))
            {
                string errorMessageReason = $"A value for the entity '{entityName}' is required and cannot be null or empty.";

                ValidationError nullValueError = ValidationError.Required<TCreateDto>(
                    OperationType.Create,
                    errorDescriptionPrefix,
                    entityName,
                    errorMessageReason);

                return Result<TDetailedDto>.ValidationFailure([nullValueError], errorMessageReason);
            }

            // TODO : Add a mapper to the project and inject it here, then use it to map the create DTO to the entity
            // Map the create DTO to the entity

        }

        public Task<Result> DeleteAsync(TKey Id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<Result<TDetailedDto>> GetByIdAsync(TKey id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<TDetailedDto>>> GetCollectionAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<TDetailedDto>>> SearchAsync(string searchTerm, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result> UpdateAsync(TKey id, TUpdateDto entity, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
