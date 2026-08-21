using Microsoft.Extensions.Logging;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;
using Rasmus.SharedKernel.Pagination;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Validation;

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
        protected readonly ILogger _logger;

        protected DependentEntityReadServiceBase(
            IDependentEntityRepo<TEntityDependent, TKeyOwner, TKeyDependent> dependentEntityRepo,
            IMapEntityToDto<TEntityDependent, TKeyDependent, TDetailedDto, TMinimalDto> entityToDtoMapper,
            IRepo<TEntityOwner, TKeyOwner> ownerRepo,
            ILogger logger)
        {
            _dependentEntityRepo = dependentEntityRepo;
            _entityToDtoMapper = entityToDtoMapper;
            _ownerRepo = ownerRepo;
            _logger = logger;
        }

        public async Task<Result<TMinimalDto>> GetMinimalByIdAsync(TKeyOwner ownerId, TKeyDependent id, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetMinimalByIdAsync), OperationType.Get);

            var validationErrors = new List<ValidationError>();

            if (ownerId.IsNotValidMediaVaultId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdError))
                validationErrors.Add(ownerIdError);

            if (id.IsNotValidMediaVaultId(baseErrorContext with { FieldName = nameof(id) }, out var idError))
                validationErrors.Add(idError);

            if (validationErrors.Count > 0)
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, validationErrors, GetType().Name, nameof(GetMinimalByIdAsync), baseErrorContext);

                return Result<TMinimalDto>.ValidationFailure(validationErrors, "Validation errors occurred, see validationErrors for details.");
            }

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult.ToResult<TMinimalDto>();
            }

            var repoResult = await _dependentEntityRepo.GetByIdAsync(ownerId, id, ct: ct);

            var mappedRepoResult = repoResult.Map(_entityToDtoMapper.ToMinimalDto);

            return mappedRepoResult;

        }

        public async Task<Result<TDetailedDto>> GetDetailedByIdAsync(TKeyOwner ownerId, TKeyDependent id, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetDetailedByIdAsync), OperationType.Get);

            var validationErrors = new List<ValidationError>();

            if (ownerId.IsNotValidMediaVaultId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdError))
                validationErrors.Add(ownerIdError);

            if (id.IsNotValidMediaVaultId(baseErrorContext with { FieldName = nameof(id) }, out var idError))
                validationErrors.Add(idError);

            if (validationErrors.Count > 0)
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, validationErrors, GetType().Name, nameof(GetDetailedByIdAsync), baseErrorContext);
                return Result<TDetailedDto>.ValidationFailure(validationErrors, "Validation errors occurred, see validationErrors for details.");
            }

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult.ToResult<TDetailedDto>();
            }

            var repoResult = await _dependentEntityRepo.GetByIdAsync(ownerId, id, ct: ct);

            var mappedRepoResult = repoResult.Map(_entityToDtoMapper.ToDetailedDto);

            return mappedRepoResult;

        }


        public async Task<Result<IReadOnlyList<TDetailedDto>>> GetDetailedCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetDetailedCollectionByOwnerIdAsync), OperationType.GetCollection);

            if (ownerId.IsNotValidMediaVaultId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdError))
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger,
                    [ownerIdError],
                    GetType().Name,
                    nameof(GetDetailedCollectionByOwnerIdAsync),
                    baseErrorContext);
                return Result<IReadOnlyList<TDetailedDto>>.ValidationFailure(
                    [ownerIdError],
                    "Validation errors occurred, see validationErrors for details.");
            }

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult.ToResult<IReadOnlyList<TDetailedDto>>();
            }

            var pagination = PaginationParameters.Normalize(pageNumber, pageSize);

            var repoResult = await _dependentEntityRepo.GetCollectionByOwnerIdAsync(ownerId, pagination.PageNumber, pagination.PageSize, ct);

            var mappedRepoResult = repoResult.Map(_entityToDtoMapper.ToDetailedDtoCollection);

            return mappedRepoResult;

        }

        public virtual async Task<Result<IReadOnlyList<TMinimalDto>>> GetMinimalCollectionByOwnerIdAsync(TKeyOwner ownerId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetMinimalCollectionByOwnerIdAsync), OperationType.GetCollection);

            if (ownerId.IsNotValidMediaVaultId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdError))
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger,
                    [ownerIdError],
                    GetType().Name,
                    nameof(GetMinimalCollectionByOwnerIdAsync),
                    baseErrorContext);
                return Result<IReadOnlyList<TMinimalDto>>.ValidationFailure(
                    [ownerIdError],
                    "Validation errors occurred, see validationErrors for details.");
            }

            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);

            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult.ToResult<IReadOnlyList<TMinimalDto>>();
            }

            var pagination = PaginationParameters.Normalize(pageNumber, pageSize);

            var repoResult = await _dependentEntityRepo.GetCollectionByOwnerIdAsync(ownerId, pagination.PageNumber, pagination.PageSize, ct);

            var mappedRepoResult = repoResult.Map(_entityToDtoMapper.ToMinimalDtoCollection);

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
