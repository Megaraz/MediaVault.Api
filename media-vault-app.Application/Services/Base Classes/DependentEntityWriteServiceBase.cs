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
            where TEntityOwner : class, IWriteableEntity<TKeyOwner>
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

        protected DependentEntityWriteServiceBase(
            IDependentEntityRepo<TEntityDependent, TKeyOwner, TKeyDependent> dependentEntityRepo,
            IRepo<TEntityOwner, TKeyOwner> ownerRepo,
            IMapEntityToDetailedDto<TEntityDependent, TDetailedDto> entityToDtoMapper,
            IMapDtoToEntity<TEntityDependent, TDetailedDto, TCreateDto, TUpdateDto, TKeyDependent> dtoToEntityMapper,
            IDtoValidator<TKeyDependent, TCreateDto, TUpdateDto> dtoValidator)
        {
            _dependentEntityRepo = dependentEntityRepo;
            _ownerRepo = ownerRepo;
            _dtoToEntityMapper = dtoToEntityMapper;
            _dtoValidator = dtoValidator;
            _entityToDtoMapper = entityToDtoMapper;
        }

        public virtual async Task<Result<TDetailedDto>> CreateAsync(TKeyOwner ownerId, TCreateDto createDto, CancellationToken ct)
        {
            var errorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);

            List<ValidationError> errors = new();

            if (!ownerId.IsValidId(errorContext with { FieldName = nameof(ownerId) }, out var ownerIdNotValidError))
                errors.Add(ownerIdNotValidError);

            if (!_dtoValidator.IsValidCreateDto(createDto, errorContext, out var validationErrors))
                errors.AddRange(validationErrors);

            if (errors.Count > 0)
                return Result<TDetailedDto>.ValidationFailure(errors);

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult.From<bool, TDetailedDto>();
            }

            var entity = _dtoToEntityMapper.ToEntity(createDto);
            entity.OwnerId = ownerId;

            var repoResult = await _dependentEntityRepo.CreateAsync(entity, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);

        }

        public async Task<Result> UpdateAsync(TKeyOwner ownerId, TKeyDependent id, TUpdateDto updateDto, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            List<ValidationError> errors = new();

            if (!ownerId.IsValidId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdNotValidError))
                errors.Add(ownerIdNotValidError);

            if (!id.IsValidId(baseErrorContext with { FieldName = nameof(id) }, out var idNotValidError))
                errors.Add(idNotValidError);

            if (!_dtoValidator.IsValidUpdateDto(updateDto, baseErrorContext, out var validationErrors))
                errors.AddRange(validationErrors);

            if (errors.Count > 0)
            {
                return Result.ValidationFailure(errors, "Validation Errors occurred, see validationErrors for details.");
            }

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult.From();
            }


            var updatedEntity = _dtoToEntityMapper.ToEntity(id, updateDto);
            updatedEntity.OwnerId = ownerId;

            return await _dependentEntityRepo.UpdateAsync(ownerId, updatedEntity, ct);
        }

        public async Task<Result> DeleteAsync(TKeyOwner ownerId, TKeyDependent dependentId, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            List<ValidationError> errors = new();

            if (!ownerId.IsValidId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdNotValidError))
                errors.Add(ownerIdNotValidError);

            if (!dependentId.IsValidId(baseErrorContext with { FieldName = nameof(dependentId) }, out var dependentIdNotValidError))
                errors.Add(dependentIdNotValidError);

            if (errors.Count > 0)
                return Result.ValidationFailure(errors, "Validation Errors occurred, see validationErrors for details.");

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult.From();
            }

            return await _dependentEntityRepo.DeleteAsync(ownerId, dependentId, ct);
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
