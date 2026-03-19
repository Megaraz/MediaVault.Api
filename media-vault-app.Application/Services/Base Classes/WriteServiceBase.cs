using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services
{
    public class WriteServiceBase<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto>
        : IWriteService<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto>
            where TEntity : class, IEntityId<TKey>, new()
            where TDetailedDto : IDtoID<TKey>
    {
        private readonly IGenericRepo<TEntity, TKey> _repo;

        private readonly IMapEntityToDetailedDto<TEntity, TDetailedDto> _entityToDtoMapper;
        private readonly IMapDtoToEntity<TEntity, TDetailedDto, TCreateDto, TKey, TUpdateDto> _dtoToEntityMapper;

        public WriteServiceBase(
            IGenericRepo<TEntity, TKey> repo,
            IMapEntityToDetailedDto<TEntity, TDetailedDto> entityToDtoMapper,
            IMapDtoToEntity<TEntity, TDetailedDto, TCreateDto, TKey, TUpdateDto> dtoToEntityMapper)
        {
            _repo = repo;
            _entityToDtoMapper = entityToDtoMapper;
            _dtoToEntityMapper = dtoToEntityMapper;
        }

        public virtual async Task<Result<TDetailedDto>> CreateAsync(TCreateDto createDto, CancellationToken ct)
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

            var entity = _dtoToEntityMapper.ToEntity(createDto);

            var repoResult = await _repo.CreateAsync(entity, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);

        }

        public Task<Result> DeleteAsync(TKey Id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<Result> UpdateAsync(TKey id, TUpdateDto updateDto, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
