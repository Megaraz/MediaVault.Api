using Microsoft.Extensions.Logging;
using media_vault_app.Application.Interfaces.Repos;
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
        protected readonly IEntityExistsRepo<TKeyOwner> _ownerRepo;
        protected readonly IMapEntityToDetailedDto<TEntityDependent, TDetailedDto> _entityToDtoMapper;
        protected readonly IMapDtoToEntity<TEntityDependent, TDetailedDto, TCreateDto, TUpdateDto, TKeyDependent> _dtoToEntityMapper;
        protected readonly IDtoValidator<TKeyDependent, TCreateDto, TUpdateDto> _dtoValidator;
        protected readonly ILogger _logger;

        protected DependentEntityWriteServiceBase(
            IDependentEntityRepo<TEntityDependent, TKeyOwner, TKeyDependent> dependentEntityRepo,
            IEntityExistsRepo<TKeyOwner> ownerRepo,
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

            if (ownerId.IsNotValidMediaVaultId(errorContext with { FieldName = nameof(ownerId) }, out var ownerIdNotValidError))
                errors.Add(ownerIdNotValidError);

            if (!_dtoValidator.IsValidCreateDto(createDto, errorContext, out var validationErrors))
                errors.AddRange(validationErrors);

            if (errors.Count > 0)
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, errors, GetType().Name, nameof(CreateAsync), errorContext);

                return Result<TDetailedDto>.ValidationFailure(errors, MediaVaultResultMessages.ValidationFailure);
            }

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult.ToResult<TDetailedDto>();
            }

            var entity = _dtoToEntityMapper.ToEntity(createDto);
            entity.OwnerId = ownerId;

            var repoResult = await _dependentEntityRepo.CreateAsync(entity, ct);

            var mappedRepoResult = repoResult.Map(_entityToDtoMapper.ToDetailedDto);

            return mappedRepoResult;

        }

        public async Task<Result> UpdateAsync(TKeyOwner ownerId, TKeyDependent id, TUpdateDto updateDto, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            List<ValidationError> errors = new();

            if (ownerId.IsNotValidMediaVaultId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdNotValidError))
                errors.Add(ownerIdNotValidError);

            if (id.IsNotValidMediaVaultId(baseErrorContext with { FieldName = nameof(id) }, out var idNotValidError))
                errors.Add(idNotValidError);

            if (!_dtoValidator.IsValidUpdateDto(updateDto, baseErrorContext, out var validationErrors))
                errors.AddRange(validationErrors);

            if (errors.Count > 0)
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, errors, GetType().Name, nameof(UpdateAsync), baseErrorContext);
                return Result.ValidationFailure(errors, "Validation Errors occurred, see validationErrors for details.");
            }

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult;
            }


            var updatedEntity = _dtoToEntityMapper.ToEntity(id, updateDto);
            updatedEntity.OwnerId = ownerId;

            var mappedRepoResult = await _dependentEntityRepo.UpdateAsync(
                ownerId,
                updatedEntity,
                ct);

            return mappedRepoResult;
        }

        public async Task<Result> DeleteAsync(
            TKeyOwner ownerId,
            TKeyDependent dependentId,
            int expectedVersion,
            CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            List<ValidationError> errors = new();

            if (ownerId.IsNotValidMediaVaultId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdNotValidError))
                errors.Add(ownerIdNotValidError);

            if (dependentId.IsNotValidMediaVaultId(baseErrorContext with { FieldName = nameof(dependentId) }, out var dependentIdNotValidError))
                errors.Add(dependentIdNotValidError);

            if (expectedVersion < 1)
            {
                errors.Add(MediaVaultValidationError.OutOfRange(
                    baseErrorContext with { FieldName = nameof(expectedVersion) },
                    $"1 to {int.MaxValue}"));
            }

            if (errors.Count > 0)
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, errors, GetType().Name, nameof(DeleteAsync), baseErrorContext);
                return Result.ValidationFailure(errors, "Validation Errors occurred, see validationErrors for details.");
            }

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult;
            }

            var mappedRepoResult = await _dependentEntityRepo.DeleteAsync(
                ownerId,
                dependentId,
                expectedVersion,
                ct);
            return mappedRepoResult;
        }

        protected async Task<Result<bool>> EnsureOwnerExistsAsync(TKeyOwner ownerId, CancellationToken ct)
        {
            return await _ownerRepo.ExistsAsync(ownerId, ct);
        }

        protected virtual ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null)
        {
            return new ErrorContext(
                operation: operation,
                entityName: typeof(TEntityDependent).Name,
                fieldName: fieldName);
        }
    }
}
