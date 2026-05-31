using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.Services;
using Microsoft.Extensions.Logging;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;
using Rasmus.SharedKernel.Interfaces.Validators;
using Rasmus.SharedKernel.ResultPattern;

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
                _logger.LogDebug("CreateAsync validation failed: {ValidationErrors}", ServiceValidationLogging.FormatValidationErrors(validationErrors));
                return Result<TDetailedDto>.ValidationFailure(validationErrors);
            }

            var entity = _dtoToEntityMapper.ToEntity(createDto);

            var repoResult = await _repo.CreateAsync(entity, ct);

            var mappedRepoResult = repoResult.Map(_entityToDtoMapper.ToDetailedDto);
            if (mappedRepoResult.IsFailure)
                _logger.LogDebug("CreateAsync failed: {Code} — {Description}", mappedRepoResult.PrimaryError.Code, mappedRepoResult.PrimaryError.Description);
            return mappedRepoResult;

        }

        public async Task<Result> DeleteAsync(TKey id, CancellationToken ct)
        {
            var baseErrorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            if (id.IsNotValidId(baseErrorContext, out var idNotValidError))
            {
                _logger.LogDebug("DeleteAsync validation failed: {ValidationErrors}", ServiceValidationLogging.FormatValidationErrors([idNotValidError]));
                return Result.ValidationFailure([idNotValidError]);
            }

            var mappedRepoResult = await _repo.DeleteAsync(id, ct);
            if (mappedRepoResult.IsFailure)
                _logger.LogDebug("DeleteAsync failed: {Code} — {Description}", mappedRepoResult.PrimaryError.Code, mappedRepoResult.PrimaryError.Description);
            return mappedRepoResult;
        }

        public async Task<Result> UpdateAsync(TKey id, TUpdateDto updateDto, CancellationToken ct)
        {
            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            List<ValidationError> validationErrors = new();

            if (id.IsNotValidId(baseErrorContext with { FieldName = nameof(id) }, out var idError))
                validationErrors.Add(idError);

            if (!_dtoValidator.IsValidUpdateDto(updateDto, baseErrorContext, out var updateValidationErrors))
                validationErrors.AddRange(updateValidationErrors);

            if (validationErrors.Count > 0)
            {
                _logger.LogDebug("UpdateAsync validation failed: {ValidationErrors}", ServiceValidationLogging.FormatValidationErrors(validationErrors));
                return Result.ValidationFailure(validationErrors);
            }

            var entity = _dtoToEntityMapper.ToEntity(id, updateDto);

            var mappedRepoResult = await _repo.UpdateAsync(entity, ct);
            if (mappedRepoResult.IsFailure)
                _logger.LogDebug("UpdateAsync failed: {Code} — {Description}", mappedRepoResult.PrimaryError.Code, mappedRepoResult.PrimaryError.Description);
            return mappedRepoResult;

        }

        protected virtual ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null)
        {
            return new ErrorContext(
                Layer: "Service",
                ServiceName: GetType().Name,
                MethodName: methodName,
                Operation: operation,
                EntityName: typeof(TEntity).Name,
                FieldName: fieldName);
        }
    }
}
