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

    public abstract class DependentEntityWriteServiceBase<
        TEntityOwner,
        TEntityDependent, 
        TKeyOwner, 
        TKeyDependent, 
        TCreateDto, 
        TUpdateDto, 
        TDetailedDto>
        : IDependentEntityWriteService<TKeyOwner, TKeyDependent, TCreateDto, TUpdateDto, TDetailedDto>
            where TEntityOwner : class, IEntity<TKeyOwner>
            where TEntityDependent : class, IDependentEntity<TKeyOwner, TKeyDependent>
            where TDetailedDto : IDtoIdentifiable<TKeyDependent>
            where TKeyOwner : notnull, IEquatable<TKeyOwner>
            where TKeyDependent : notnull, IEquatable<TKeyDependent>
    {

        protected readonly IDependentEntityRepo<TEntityDependent, TKeyOwner, TKeyDependent> _dependentEntityRepo;
        protected readonly IRepo<TEntityOwner, TKeyOwner> _ownerRepo;
        protected readonly IMapEntityToDetailedDto<TEntityDependent, TDetailedDto> _entityToDtoMapper;
        protected readonly IMapDtoToEntity<TEntityDependent, TDetailedDto, TCreateDto, TUpdateDto, TKeyDependent> _dtoToEntityMapper;
        protected readonly IDtoValidator<TKeyDependent, TCreateDto, TUpdateDto> _dtoValidator;
        protected readonly ILogger _logger;

        protected DependentEntityWriteServiceBase(
            IDependentEntityRepo<TEntityDependent, TKeyOwner, TKeyDependent> dependentEntityRepo,
            IRepo<TEntityOwner, TKeyOwner> ownerRepo,
            IMapEntityToDetailedDto<TEntityDependent, TDetailedDto> entityToDtoMapper,
            IMapDtoToEntity<TEntityDependent, TDetailedDto, TCreateDto, TUpdateDto, TKeyDependent> dtoToEntityMapper,
            IDtoValidator<TKeyDependent, TCreateDto, TUpdateDto> dtoValidator,
            ILogger logger)
        {
            _dependentEntityRepo = dependentEntityRepo;
            _ownerRepo = ownerRepo;
            _dtoToEntityMapper = dtoToEntityMapper;
            _dtoValidator = dtoValidator;
            _entityToDtoMapper = entityToDtoMapper;
            _logger = logger;
        }

        public virtual async Task<Result<TDetailedDto>> CreateAsync(TKeyOwner ownerId, TCreateDto createDto, CancellationToken ct)
        {
            var errorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);

            List<ValidationError> errors = new();

            if (ownerId.IsNotValidId(errorContext with { FieldName = nameof(ownerId) }, out var ownerIdNotValidError))
                errors.Add(ownerIdNotValidError);

            if (!_dtoValidator.IsValidCreateDto(createDto, errorContext, out var validationErrors))
                errors.AddRange(validationErrors);

            if (errors.Count > 0)
            {
                _logger.LogDebug("CreateAsync validation failed: {ValidationErrors}", ServiceValidationLogging.FormatValidationErrors(errors));
                return Result<TDetailedDto>.ValidationFailure(errors);
            }

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                _logger.LogDebug("CreateAsync owner check failed: {Code} — {Description}", 
                    ownerExistsResult.PrimaryError.Code, ownerExistsResult.PrimaryError.Description);

                return ownerExistsResult.From<bool, TDetailedDto>();
            }

            var entity = _dtoToEntityMapper.ToEntity(createDto);
            entity.OwnerId = ownerId;

            var repoResult = await _dependentEntityRepo.CreateAsync(entity, ct);

            var mappedRepoResult = repoResult.Map(_entityToDtoMapper.ToDetailedDto);

            if (mappedRepoResult.IsFailure)
            {
                _logger.LogDebug("CreateAsync failed: {Code} — {Description}", 
                    mappedRepoResult.PrimaryError.Code, mappedRepoResult.PrimaryError.Description);
            }

            return mappedRepoResult;

        }

        public async Task<Result> UpdateAsync(TKeyOwner ownerId, TKeyDependent id, TUpdateDto updateDto, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            List<ValidationError> errors = new();

            if (ownerId.IsNotValidId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdNotValidError))
                errors.Add(ownerIdNotValidError);

            if (id.IsNotValidId(baseErrorContext with { FieldName = nameof(id) }, out var idNotValidError))
                errors.Add(idNotValidError);

            if (!_dtoValidator.IsValidUpdateDto(updateDto, baseErrorContext, out var validationErrors))
                errors.AddRange(validationErrors);

            if (errors.Count > 0)
            {
                _logger.LogDebug("UpdateAsync validation failed: {ValidationErrors}", ServiceValidationLogging.FormatValidationErrors(errors));
                return Result.ValidationFailure(errors, "Validation Errors occurred, see validationErrors for details.");
            }

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                _logger.LogDebug("UpdateAsync owner check failed: {Code} — {Description}", 
                    ownerExistsResult.PrimaryError.Code, ownerExistsResult.PrimaryError.Description);

                return ownerExistsResult;
            }


            var updatedEntity = _dtoToEntityMapper.ToEntity(id, updateDto);
            updatedEntity.OwnerId = ownerId;

            var mappedRepoResult = await _dependentEntityRepo.UpdateAsync(ownerId, updatedEntity, ct);

            if (mappedRepoResult.IsFailure)
            {
                _logger.LogDebug("UpdateAsync failed: {Code} — {Description}", 
                    mappedRepoResult.PrimaryError.Code, mappedRepoResult.PrimaryError.Description);
            }

            return mappedRepoResult;
        }

        public async Task<Result> DeleteAsync(TKeyOwner ownerId, TKeyDependent dependentId, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            List<ValidationError> errors = new();

            if (ownerId.IsNotValidId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdNotValidError))
                errors.Add(ownerIdNotValidError);

            if (dependentId.IsNotValidId(baseErrorContext with { FieldName = nameof(dependentId) }, out var dependentIdNotValidError))
                errors.Add(dependentIdNotValidError);

            if (errors.Count > 0)
            {
                _logger.LogDebug("DeleteAsync validation failed: {ValidationErrors}", ServiceValidationLogging.FormatValidationErrors(errors));
                return Result.ValidationFailure(errors, "Validation Errors occurred, see validationErrors for details.");
            }

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                _logger.LogDebug("DeleteAsync owner check failed: {Code} — {Description}", 
                    ownerExistsResult.PrimaryError.Code, ownerExistsResult.PrimaryError.Description);

                return ownerExistsResult;
            }

            var mappedRepoResult = await _dependentEntityRepo.DeleteAsync(ownerId, dependentId, ct);
            if (mappedRepoResult.IsFailure)
            {
                _logger.LogDebug("DeleteAsync failed: {Code} — {Description}", 
                    mappedRepoResult.PrimaryError.Code, mappedRepoResult.PrimaryError.Description);
            }

            return mappedRepoResult;
        }

        protected async Task<Result<bool>> EnsureOwnerExistsAsync(TKeyOwner ownerId, CancellationToken ct)
        {
            return await _ownerRepo.ExistsAsync(ownerId, ct);
        }

        protected virtual ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null)
        {
            return new ErrorContext(
                Layer: "Service",
                ServiceName: this.GetType().Name,
                MethodName: methodName,
                Operation: operation,
                EntityName: typeof(TEntityDependent).Name,
                FieldName: fieldName);
        }
    }
}
