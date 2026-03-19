using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Mappers;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services
{
    public class GenericService<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto, TMinimalDto>
        : IGenericService<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto, TMinimalDto>
        where TEntity : class, IEntityId<TKey>, new()
        where TDetailedDto : IEntityId<TKey>
    {

        private readonly IGenericRepo<TEntity, TKey> _repo;
        private readonly IMapper<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto, TCollectionDto, TMinimalDto> _mapper;

        public GenericService(
            IGenericRepo<TEntity, TKey> repo,
            IMapper<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto, TCollectionDto, TMinimalDto> mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }


        public async Task<Result<TDetailedDto>> CreateAsync(TCreateDto createDto, CancellationToken ct)
        {
            string methodName = nameof(CreateAsync);
            string errorDescriptionPrefix = $"An error occurred when trying to create the entity in Service Layer: {this.GetType().Name}.{methodName}()";
            string entityName = typeof(TCreateDto).Name;

            if (createDto is null)
            {
                string errorMessageReason = $"A value for the entity '{entityName}' is required and cannot be null or empty.";

                ValidationError nullValueError = ValidationError.Required<TCreateDto>(
                    OperationType.Create,
                    errorDescriptionPrefix,
                    entityName,
                    errorMessageReason);

                return Result<TDetailedDto>.ValidationFailure([nullValueError], errorMessageReason);
            }

            var entity = _mapper.ToEntity(createDto);

            var repoResult = await _repo.CreateAsync(entity, ct);

            return repoResult.Map(_mapper.ToDetailedDTO);

        }

        public Task<Result> DeleteAsync(TKey Id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<Result<TDetailedDto>> GetByIdAsync(TKey id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<TDetailedDto>>> GetDetailedCollectionAsync(int pageNumber, int pageSize, CancellationToken ct = default)
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
