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

            ErrorContext errorContext = new(
                layer: "Service",
                serviceName: this.GetType().Name,
                methodName: nameof(GetByIdAsync),
                operation: OperationType.Get,
                entityName: typeof(TEntity).Name
                );

            if (!Validator.IsValidId(id))
            {
                errorContext.DescriptionSuffix = $"A valid Id is required and cannot be null or empty.";
                errorContext.EntityName = nameof(id);

                var nullValueError = ValidationError.Required<TKey>(errorContext);

                return Result<TDetailedDto>.ValidationFailure([nullValueError], errorContext.DescriptionSuffix);
            }

            var repoResult = await _repo.GetByIdAsync(id, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);

        }

        public async Task<Result<IEnumerable<TDetailedDto>>> GetDetailedCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            ErrorContext errorContext = new(
                layer: "Service",
                serviceName: this.GetType().Name,
                methodName: nameof(GetDetailedCollectionAsync),
                operation: OperationType.GetCollection,
                entityName: typeof(TEntity).Name
                );

            IEnumerable<ValidationError> validationErrors = new List<ValidationError>();

            if (pageNumber < 1)
            {
                errorContext.DescriptionSuffix = $"Page number must be greater than 0.";
                errorContext.FieldName = nameof(pageNumber);

                var pageNumberError = ValidationError.OutOfRange<int>(errorContext, "Greater than 0");
                validationErrors = validationErrors.Append(pageNumberError);

            }
            if (pageSize < 1)
            {
                errorContext.DescriptionSuffix = $"Page size must be greater than 0.";
                errorContext.FieldName = nameof(pageSize);

                var pageSizeError = ValidationError.OutOfRange<int>(errorContext, "Greater than 0");
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
            ErrorContext errorContext = new(
                layer: "Service",
                serviceName: this.GetType().Name,
                methodName: nameof(GetMinimalCollectionAsync),
                operation: OperationType.GetCollection,
                entityName: typeof(TEntity).Name
                );

            IEnumerable<ValidationError> validationErrors = new List<ValidationError>();

            if (pageNumber < 1)
            {
                errorContext.DescriptionSuffix = $"Page number must be greater than 0.";
                errorContext.FieldName = nameof(pageNumber);

                var pageNumberError = ValidationError.OutOfRange<int>(errorContext, "Greater than 0");
                validationErrors = validationErrors.Append(pageNumberError);

            }
            if (pageSize < 1)
            {
                errorContext.DescriptionSuffix = $"Page size must be greater than 0.";
                errorContext.FieldName = nameof(pageSize);

                var pageSizeError = ValidationError.OutOfRange<int>(errorContext, "Greater than 0");
                validationErrors = validationErrors.Append(pageSizeError);
            }

            if (validationErrors.Any())
            {
                return Result<IEnumerable<TMinimalDto>>.ValidationFailure(validationErrors, "Validation errors occurred.");
            }

            var repoResult = await _repo.GetCollectionAsync(pageNumber, pageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToMinimalDtoCollection);
        }
    }
}
