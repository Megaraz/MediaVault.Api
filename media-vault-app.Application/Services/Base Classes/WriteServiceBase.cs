using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.Interfaces.Validators;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services
{
    public class WriteServiceBase<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto>
        : IWriteService<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto>
            where TEntity : class, IEntityId<TKey>
            where TDetailedDto : IDtoID<TKey>
    {
        private readonly IGenericRepo<TEntity, TKey> _repo;

        private readonly IMapEntityToDetailedDto<TEntity, TDetailedDto> _entityToDtoMapper;
        private readonly IMapDtoToEntity<TEntity, TDetailedDto, TCreateDto, TKey, TUpdateDto> _dtoToEntityMapper;
        private readonly IDtoValidator<TKey, TCreateDto, TUpdateDto> _dtoValidator;

        public WriteServiceBase(
            IGenericRepo<TEntity, TKey> repo,
            IMapEntityToDetailedDto<TEntity, TDetailedDto> entityToDtoMapper,
            IMapDtoToEntity<TEntity, TDetailedDto, TCreateDto, TKey, TUpdateDto> dtoToEntityMapper,
            IDtoValidator<TKey, TCreateDto, TUpdateDto> dtoValidator)
        {
            _repo = repo;
            _entityToDtoMapper = entityToDtoMapper;
            _dtoToEntityMapper = dtoToEntityMapper;
            _dtoValidator = dtoValidator;
        }

        public virtual async Task<Result<TDetailedDto>> CreateAsync(TCreateDto createDto, CancellationToken ct)
        {
            var errorContext = CreateErrorContext(nameof(CreateAsync), OperationType.Create);

            if (!_dtoValidator.IsValidCreateDto(createDto, errorContext, out var validationErrors))
            {
                return Result<TDetailedDto>.ValidationFailure(validationErrors, errorContext.DescriptionPrefix);
            }

            var entity = _dtoToEntityMapper.ToEntity(createDto);

            var repoResult = await _repo.CreateAsync(entity, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);

        }

        public async Task<Result> DeleteAsync(TKey id, CancellationToken ct)
        {
            var errorContext = CreateErrorContext(nameof(DeleteAsync), OperationType.Delete);

            if (!Validator.IsValidId(id))
            {
                errorContext.DescriptionSuffix = $"A valid Id is required and cannot be null or empty.";
                errorContext.FieldName = nameof(id);

                var nullValueError = ValidationError.Required(errorContext);

                return Result.ValidationFailure([nullValueError], errorContext.DescriptionSuffix);
            }

            return await _repo.DeleteAsync(id, ct);
        }

        public Task<Result> UpdateAsync(TKey id, TUpdateDto updateDto, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        private ErrorContext CreateErrorContext(string methodName, OperationType operation)
        {
            return new ErrorContext(
                layer: "Service",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: typeof(TEntity).Name);
        }
    }
}
