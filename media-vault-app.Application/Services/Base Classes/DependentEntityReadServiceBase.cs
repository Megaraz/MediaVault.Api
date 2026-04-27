using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services.Base_Classes
{

    public abstract class DependentEntityReadServiceBase<
        TEntityOwner,
        TEntityDependent,
        TKeyOwner,
        TKeyDependent,
        TDetailedDto,
        TMinimalDto>
        : IDependentEntityReadService<TKeyOwner, TKeyDependent, TDetailedDto, TMinimalDto>
            where TEntityOwner : class, IEntity<TKeyOwner>
            where TEntityDependent : class, IDependentEntity<TKeyOwner, TKeyDependent>
            where TDetailedDto : IDtoIdentifiable<TKeyDependent>
            where TMinimalDto : IDtoIdentifiable<TKeyDependent>
            where TKeyOwner : notnull, IEquatable<TKeyOwner>
            where TKeyDependent : notnull, IEquatable<TKeyDependent>
    {

        protected readonly IDependentEntityRepo<TEntityDependent, TKeyOwner, TKeyDependent> _dependentEntityRepo;
        protected readonly IRepo<TEntityOwner, TKeyOwner> _ownerRepo;
        protected readonly IMapEntityToDto<TEntityDependent, TKeyDependent, TDetailedDto, TMinimalDto> _entityToDtoMapper;

        protected DependentEntityReadServiceBase(
            IDependentEntityRepo<TEntityDependent, TKeyOwner, TKeyDependent> dependentEntityRepo,
            IMapEntityToDto<TEntityDependent, TKeyDependent, TDetailedDto, TMinimalDto> entityToDtoMapper,
            IRepo<TEntityOwner, TKeyOwner> ownerRepo)
        {
            _dependentEntityRepo = dependentEntityRepo;
            _entityToDtoMapper = entityToDtoMapper;
            _ownerRepo = ownerRepo;
        }

        public async Task<Result<TDetailedDto>> GetByIdAsync(TKeyOwner ownerId, TKeyDependent id, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            var validationErrors = new List<ValidationError>();

            if (!ownerId.IsValidId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdError))
                validationErrors.Add(ownerIdError);

            if (!id.IsValidId(baseErrorContext with { FieldName = nameof(id) }, out var idError))
                validationErrors.Add(idError);

            if (validationErrors.Count > 0)
                return Result<TDetailedDto>.ValidationFailure(validationErrors, "Validation errors occurred, see validationErrors for details.");

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult.From<bool, TDetailedDto>();
            }

            var repoResult = await _dependentEntityRepo.GetByIdAsync(ownerId, id, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);

        }


        public async Task<Result<IEnumerable<TDetailedDto>>> GetDetailedCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetDetailedCollectionByOwnerIdAsync), OperationType.GetCollection);

            if (!ownerId.IsValidId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdError))
            {
                return Result<IEnumerable<TDetailedDto>>.ValidationFailure(
                    [ownerIdError],
                    "Validation errors occurred, see validationErrors for details.");
            }

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult.From<bool, IEnumerable<TDetailedDto>>();
            }

            ValidateAndAdjustPaginationParameters(ref pageNumber, ref pageSize);

            var repoResult = await _dependentEntityRepo.GetCollectionByOwnerIdAsync(ownerId, pageNumber, pageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDtoCollection);

        }

        public async Task<Result<IEnumerable<TMinimalDto>>> GetMinimalCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetMinimalCollectionByOwnerIdAsync), OperationType.GetCollection);

            if (!ownerId.IsValidId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdError))
            {
                return Result<IEnumerable<TMinimalDto>>.ValidationFailure(
                    [ownerIdError],
                    "Validation errors occurred, see validationErrors for details.");
            }

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult.From<bool, IEnumerable<TMinimalDto>>();
            }

            ValidateAndAdjustPaginationParameters(ref pageNumber, ref pageSize);

            var repoResult = await _dependentEntityRepo.GetCollectionByOwnerIdAsync(ownerId, pageNumber, pageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToMinimalDtoCollection);
        }

        protected async Task<Result<bool>> EnsureOwnerExistsAsync(TKeyOwner ownerId, CancellationToken ct)
        {
            return await _ownerRepo.ExistsAsync(ownerId, ct);
        }

        protected virtual void ValidateAndAdjustPaginationParameters(ref int pageNumber, ref int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1; // Default to page 1 if the provided page number is too low
            if (pageSize < 1)
                pageSize = 1; // Default to a minimum page size of 1 if the provided page size is too low
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
