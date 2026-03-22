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
    public class ReadServiceBase<TEntity, TKey, TDetailedDto, TMinimalDto>
        : IReadService<TEntity, TKey, TDetailedDto, TMinimalDto>
            where TEntity : class, IEntityId<TKey>
            where TDetailedDto : IDtoID<TKey>
            where TMinimalDto : IDtoID<TKey>
    {

        private readonly IGenericRepo<TEntity, TKey> _repo;
        private readonly IMapEntityToDto<TEntity, TKey, TDetailedDto, TMinimalDto> _entityToDtoMapper;

        public ReadServiceBase(IGenericRepo<TEntity, TKey> repo, IMapEntityToDto<TEntity, TKey, TDetailedDto, TMinimalDto> entityToDtoMapper)
        {
            _repo = repo;
            _entityToDtoMapper = entityToDtoMapper;
        }

        public async Task<Result<TDetailedDto>> GetByIdAsync(TKey id, CancellationToken ct)
        {
            var errorContext = CreateErrorContext(nameof(GetByIdAsync), OperationType.Get);

            if (!id.IsValidId(errorContext, out var idNotValidError))
                return Result<TDetailedDto>.ValidationFailure([idNotValidError], errorContext.DescriptionSuffix!);

            var repoResult = await _repo.GetByIdAsync(id, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);

        }

        public async Task<Result<IEnumerable<TDetailedDto>>> GetDetailedCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var errorContext = CreateErrorContext(nameof(GetDetailedCollectionAsync), OperationType.GetCollection);

            IEnumerable<ValidationError> validationErrors = new List<ValidationError>();

            if (pageNumber < 1)
            {
                errorContext.DescriptionSuffix = $"Page number must be greater than 0.";
                errorContext.FieldName = nameof(pageNumber);

                var pageNumberError = ValidationError.OutOfRange(errorContext, "Greater than 0");
                validationErrors = validationErrors.Append(pageNumberError);

            }
            if (pageSize < 1)
            {
                errorContext.DescriptionSuffix = $"Page size must be greater than 0.";
                errorContext.FieldName = nameof(pageSize);

                var pageSizeError = ValidationError.OutOfRange(errorContext, "Greater than 0");
                validationErrors = validationErrors.Append(pageSizeError);
            }

            if (validationErrors.Any())
            {
                return Result<IEnumerable<TDetailedDto>>.ValidationFailure(validationErrors, "Validation errors occurred.");
            }

            var repoResult = await _repo.GetCollectionAsync(pageNumber, pageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDtoCollection);

        }

        public async Task<Result<IEnumerable<TMinimalDto>>> GetMinimalCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var errorContext = CreateErrorContext(nameof(GetMinimalCollectionAsync), OperationType.GetCollection);

            IEnumerable<ValidationError> validationErrors = new List<ValidationError>();

            if (pageNumber < 1)
            {
                errorContext.DescriptionSuffix = $"Page number must be greater than 0.";
                errorContext.FieldName = nameof(pageNumber);

                var pageNumberError = ValidationError.OutOfRange(errorContext, "Greater than 0");
                validationErrors = validationErrors.Append(pageNumberError);

            }
            if (pageSize < 1)
            {
                errorContext.DescriptionSuffix = $"Page size must be greater than 0.";
                errorContext.FieldName = nameof(pageSize);

                var pageSizeError = ValidationError.OutOfRange(errorContext, "Greater than 0");
                validationErrors = validationErrors.Append(pageSizeError);
            }

            if (validationErrors.Any())
            {
                return Result<IEnumerable<TMinimalDto>>.ValidationFailure(validationErrors, "Validation errors occurred.");
            }

            var repoResult = await _repo.GetCollectionAsync(pageNumber, pageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToMinimalDtoCollection);
        }

        private ErrorContext CreateErrorContext(string methodName, OperationType operation)
        {
            return new ErrorContext(
                layer: "Service",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: typeof(TEntity).Name);
        }
    }
}
