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
        where TKey : notnull, IEquatable<TKey>
    {
        private protected readonly IGenericRepo<TEntity, TKey> _repo;

        private protected readonly IMapEntityToDetailedDto<TEntity, TDetailedDto> _entityToDtoMapper;
        private protected readonly IMapDtoToEntity<TEntity, TDetailedDto, TCreateDto, TUpdateDto, TKey> _dtoToEntityMapper;
        private protected readonly IDtoValidator<TKey, TCreateDto, TUpdateDto> _dtoValidator;

        public WriteServiceBase(
            IGenericRepo<TEntity, TKey> repo,
            IMapEntityToDetailedDto<TEntity, TDetailedDto> entityToDtoMapper,
            IMapDtoToEntity<TEntity, TDetailedDto, TCreateDto, TUpdateDto, TKey> dtoToEntityMapper,
            IDtoValidator<TKey, TCreateDto, TUpdateDto> dtoValidator)
        {
            _repo = repo;
            _entityToDtoMapper = entityToDtoMapper;
            _dtoToEntityMapper = dtoToEntityMapper;
            _dtoValidator = dtoValidator;
        }

        public virtual async Task<Result<TDetailedDto>> CreateAsync(TCreateDto createDto, CancellationToken ct)
        {
            var errorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);

            if (!_dtoValidator.IsValidRegisterDto(createDto, errorContext, out var validationErrors))
            {
                return Result<TDetailedDto>.ValidationFailure(validationErrors, errorContext.DescriptionPrefix);
            }

            var entity = _dtoToEntityMapper.ToEntity(createDto);

            var repoResult = await _repo.CreateAsync(entity, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);

        }

        public async Task<Result> DeleteAsync(TKey id, CancellationToken ct)
        {
            var errorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            if (!id.IsValidId(errorContext, out var idNotValidError))
                return Result.ValidationFailure([idNotValidError], errorContext.DescriptionSuffix!);

            return await _repo.DeleteAsync(id, ct);
        }

        public async Task<Result> UpdateAsync(TKey id, TUpdateDto updateDto, CancellationToken ct)
        {
            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            List<ValidationError> validationErrors = new();

            if (!id.IsValidId(baseErrorContext with { FieldName = nameof(id) }, out var idError))
                validationErrors.Add(idError);

            if (!_dtoValidator.IsValidUpdateDto(updateDto, baseErrorContext, out var updateValidationErrors))
                validationErrors.AddRange(updateValidationErrors);

            if (validationErrors.Count > 0)
                return Result.ValidationFailure(validationErrors, "Validation Errors occurred, see validationErrors for details.");

            var entity = _dtoToEntityMapper.ToEntity(id, updateDto);

            return await _repo.UpdateAsync(entity, ct);

        }

        protected private ErrorContext DefineErrorContext(string methodName, OperationType operation)
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
