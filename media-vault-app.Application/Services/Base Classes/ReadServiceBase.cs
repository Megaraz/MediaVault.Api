using Microsoft.Extensions.Logging;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;
using Rasmus.SharedKernel.Pagination;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Results;
using Rasmus.SharedKernel.Validation;

namespace media_vault_app.Application.Services.Base_Classes
{
    public abstract class ReadServiceBase<TEntity, TKey, TDetailedDto, TMinimalDto>
        : IReadService<TEntity, TKey, TDetailedDto, TMinimalDto>
            where TEntity : class, IEntity<TKey>
            where TDetailedDto : IDtoIdentifiable<TKey>
            where TMinimalDto : IDtoIdentifiable<TKey>
            where TKey : notnull, IEquatable<TKey>
    {

        protected readonly IRepo<TEntity, TKey> _repo;
        protected readonly IMapEntityToDto<TEntity, TKey, TDetailedDto, TMinimalDto> _entityToDtoMapper;
        protected readonly ILogger _logger;

        protected ReadServiceBase(
            IRepo<TEntity, TKey> repo,
            IMapEntityToDto<TEntity, TKey, TDetailedDto, TMinimalDto> entityToDtoMapper,
            ILogger logger)
        {
            _repo = repo;
            _entityToDtoMapper = entityToDtoMapper;
            _logger = logger;
        }

        public async Task<Result<TDetailedDto>> GetByIdAsync(TKey id, CancellationToken ct)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            if (id.IsNotValidMediaVaultId(baseErrorContext, out var idNotValidError))
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, [idNotValidError], GetType().Name, nameof(GetByIdAsync), baseErrorContext);
                return Result<TDetailedDto>.ValidationFailure([idNotValidError], MediaVaultResultMessages.ValidationFailure);
            }

            var repoResult = await _repo.GetByIdAsync(id, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDto);

        }

        public async Task<Result<IReadOnlyList<TDetailedDto>>> GetDetailedCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var pagination = PaginationParameters.Normalize(pageNumber, pageSize);

            var repoResult = await _repo.GetCollectionAsync(pagination.PageNumber, pagination.PageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDtoCollection);

        }

        public async Task<Result<IReadOnlyList<TMinimalDto>>> GetMinimalCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var pagination = PaginationParameters.Normalize(pageNumber, pageSize);

            var repoResult = await _repo.GetCollectionAsync(pagination.PageNumber, pagination.PageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToMinimalDtoCollection);
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
