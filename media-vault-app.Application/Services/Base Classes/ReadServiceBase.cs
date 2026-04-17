using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services
{
    public abstract class ReadServiceBase<TEntity, TKey, TDetailedDto, TMinimalDto>
        : IReadService<TEntity, TKey, TDetailedDto, TMinimalDto>
            where TEntity : class, IEntityId<TKey>
            where TDetailedDto : IDtoID<TKey>
            where TMinimalDto : IDtoID<TKey>
            where TKey : notnull, IEquatable<TKey>
    {

        private protected readonly IGenericRepo<TEntity, TKey> _repo;
        private protected readonly IMapEntityToDto<TEntity, TKey, TDetailedDto, TMinimalDto> _entityToDtoMapper;

        protected ReadServiceBase(
            IGenericRepo<TEntity, TKey> repo, 
            IMapEntityToDto<TEntity, TKey, TDetailedDto, TMinimalDto> entityToDtoMapper)
        {
            _repo = repo;
            _entityToDtoMapper = entityToDtoMapper;
        }

        public async Task<Result<TDetailedDto>> GetByIdAsync(TKey id, CancellationToken ct)
        {
            var errorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            if (!id.IsValidId(errorContext, out var idNotValidError))
                return Result<TDetailedDto>.ValidationFailure([idNotValidError], errorContext.DescriptionSuffix!);

            var repoResult = await _repo.GetByIdAsync(id, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);

        }

        public async Task<Result<IEnumerable<TDetailedDto>>> GetDetailedCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            ValidateAndAdjustPaginationParameters(ref pageNumber, ref pageSize);

            var repoResult = await _repo.GetCollectionAsync(pageNumber, pageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDtoCollection);

        }

        public async Task<Result<IEnumerable<TMinimalDto>>> GetMinimalCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            ValidateAndAdjustPaginationParameters(ref pageNumber, ref pageSize);

            var repoResult = await _repo.GetCollectionAsync(pageNumber, pageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToMinimalDtoCollection);
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
                layer: "Service",
                serviceName: this.GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: typeof(TEntity).Name,
                fieldName: fieldName);
        }
    }
}
