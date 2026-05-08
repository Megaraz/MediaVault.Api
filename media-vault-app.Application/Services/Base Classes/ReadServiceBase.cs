using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;
using Rasmus.SharedKernel.ResultPattern;

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

        protected ReadServiceBase(
            IRepo<TEntity, TKey> repo,
            IMapEntityToDto<TEntity, TKey, TDetailedDto, TMinimalDto> entityToDtoMapper)
        {
            _repo = repo;
            _entityToDtoMapper = entityToDtoMapper;
        }

        public async Task<Result<TDetailedDto>> GetByIdAsync(TKey id, CancellationToken ct)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            if (id.IsNotValidId(baseErrorContext, out var idNotValidError))
                return Result<TDetailedDto>.ValidationFailure([idNotValidError]);

            var repoResult = await _repo.GetByIdAsync(id, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDto);

        }

        public async Task<Result<IReadOnlyList<TDetailedDto>>> GetDetailedCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            Validator.ValidateAndAdjustPaginationParameters(ref pageNumber, ref pageSize);

            var repoResult = await _repo.GetCollectionAsync(pageNumber, pageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDtoCollection);

        }

        public async Task<Result<IReadOnlyList<TMinimalDto>>> GetMinimalCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            Validator.ValidateAndAdjustPaginationParameters(ref pageNumber, ref pageSize);

            var repoResult = await _repo.GetCollectionAsync(pageNumber, pageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToMinimalDtoCollection);
        }

        protected virtual ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null)
        {
            return new ErrorContext(
                Layer: "Service",
                ServiceName: this.GetType().Name,
                MethodName: methodName,
                Operation: operation,
                EntityName: typeof(TEntity).Name,
                FieldName: fieldName);
        }
    }
}
