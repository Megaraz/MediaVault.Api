using Microsoft.Extensions.Logging;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;
using Rasmus.SharedKernel.Interfaces.Validators;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Results;
using Rasmus.SharedKernel.Validation;

namespace media_vault_app.Application.Services.Base_Classes
{
    public abstract class WriteServiceBase<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto>
        : IWriteService<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto>
            where TEntity : class, IEntity<TKey>
            where TDetailedDto : IDtoIdentifiable<TKey>
            where TKey : notnull, IEquatable<TKey>
    {
        protected readonly IRepo<TEntity, TKey> _repo;
        protected readonly IMapEntityToDetailedDto<TEntity, TDetailedDto> _entityToDtoMapper;
        protected readonly IMapDtoToEntity<TEntity, TDetailedDto, TCreateDto, TUpdateDto, TKey> _dtoToEntityMapper;
        protected readonly IDtoValidator<TKey, TCreateDto, TUpdateDto> _dtoValidator;
        protected readonly ILogger _logger;

        protected WriteServiceBase(
            IRepo<TEntity, TKey> repo,
            IMapEntityToDetailedDto<TEntity, TDetailedDto> entityToDtoMapper,
            IMapDtoToEntity<TEntity, TDetailedDto, TCreateDto, TUpdateDto, TKey> dtoToEntityMapper,
            IDtoValidator<TKey, TCreateDto, TUpdateDto> dtoValidator,
            ILogger logger)
        {
            _repo = repo;
            _entityToDtoMapper = entityToDtoMapper;
            _dtoToEntityMapper = dtoToEntityMapper;
            _dtoValidator = dtoValidator;
            _logger = logger;
        }

        public virtual async Task<Result<TDetailedDto>> CreateAsync(TCreateDto createDto, CancellationToken ct)
        {
            var baseErrorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);

            if (!_dtoValidator.IsValidCreateDto(createDto, baseErrorContext, out var validationErrors))
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, validationErrors, GetType().Name, nameof(CreateAsync), baseErrorContext);
                return Result<TDetailedDto>.ValidationFailure(validationErrors, MediaVaultResultMessages.ValidationFailure);
            }

            var entity = _dtoToEntityMapper.ToEntity(createDto);

            var repoResult = await _repo.CreateAsync(entity, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDto);

        }

        public async Task<Result> DeleteAsync(TKey id, CancellationToken ct)
        {
            var baseErrorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            if (id.IsNotValidMediaVaultId(baseErrorContext, out var idNotValidError))
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, [idNotValidError], GetType().Name, nameof(DeleteAsync), baseErrorContext);
                return Result.ValidationFailure([idNotValidError], MediaVaultResultMessages.ValidationFailure);
            }

            return await _repo.DeleteAsync(id, ct);
        }

        public async Task<Result> UpdateAsync(TKey id, TUpdateDto updateDto, CancellationToken ct)
        {
            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            List<ValidationError> validationErrors = new();

            if (id.IsNotValidMediaVaultId(baseErrorContext with { FieldName = nameof(id) }, out var idError))
                validationErrors.Add(idError);

            if (!_dtoValidator.IsValidUpdateDto(updateDto, baseErrorContext, out var updateValidationErrors))
                validationErrors.AddRange(updateValidationErrors);

            if (validationErrors.Count > 0)
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, validationErrors, GetType().Name, nameof(UpdateAsync), baseErrorContext);
                return Result.ValidationFailure(validationErrors, MediaVaultResultMessages.ValidationFailure);
            }

            var entity = _dtoToEntityMapper.ToEntity(id, updateDto);

            return await _repo.UpdateAsync(entity, ct);

        }

        protected virtual ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null)
        {
            return new ErrorContext(
                operation: operation,
                entityName: typeof(TEntity).Name,
                fieldName: fieldName);
        }
    }
}
