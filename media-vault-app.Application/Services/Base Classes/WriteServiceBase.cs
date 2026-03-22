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

namespace media_vault_app.Application.Services
{
    public class WriteServiceBase<TEntity, TKey, TCreateDto, TDetailedDto>
        : IWriteService<TEntity, TKey, TCreateDto, TDetailedDto>
            where TEntity : class, IEntityId<TKey>
            where TDetailedDto : IDtoID<TKey>
    {
        private readonly IGenericRepo<TEntity, TKey> _repo;

        private readonly IMapEntityToDetailedDto<TEntity, TDetailedDto> _entityToDtoMapper;
        private readonly IMapDtoToEntity<TEntity, TDetailedDto, TCreateDto, TKey> _dtoToEntityMapper;
        private readonly IDtoValidator<TKey, TCreateDto> _dtoValidator;

        public WriteServiceBase(
            IGenericRepo<TEntity, TKey> repo,
            IMapEntityToDetailedDto<TEntity, TDetailedDto> entityToDtoMapper,
            IMapDtoToEntity<TEntity, TDetailedDto, TCreateDto, TKey> dtoToEntityMapper,
            IDtoValidator<TKey, TCreateDto> dtoValidator)
        {
            _repo = repo;
            _entityToDtoMapper = entityToDtoMapper;
            _dtoToEntityMapper = dtoToEntityMapper;
            _dtoValidator = dtoValidator;
        }

        public virtual async Task<Result<TDetailedDto>> CreateAsync(TCreateDto createDto, CancellationToken ct)
        {
            var errorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);

            if (!_dtoValidator.IsValidCreateDto(createDto, errorContext, out var validationErrors))
            {
                return Result<TDetailedDto>.ValidationFailure(validationErrors, errorContext.DescriptionPrefix);
            }

            var entity = _dtoToEntityMapper.ToEntity(createDto);

            var repoResult = await _repo.CreateAsync(entity, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);

        }

        public async Task<Result> DeleteAsync(TKey id, CancellationToken ct)
        {
            var errorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

            if (!id.IsValidId(errorContext, out var idNotValidError))
                return Result.ValidationFailure([idNotValidError], errorContext.DescriptionSuffix!);

            return await _repo.DeleteAsync(id, ct);
        }

        //public async Task<Result> UpdateAsync(TKey id, TUpdateDto updateDto, Func<TEntity, TEntity, bool> shouldUpdate, CancellationToken ct)
        //{
        //    var errorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

        //    if (!id.IsValidId(errorContext, out var idError))
        //        return Result.Failure(idError, errorContext.DescriptionSuffix!);

        //    if (!_dtoValidator.IsValidUpdateDto(id, updateDto, errorContext, out var validationErrors))
        //        return Result.ValidationFailure(validationErrors, errorContext.DescriptionSuffix!);

        //    var entity = _dtoToEntityMapper.ToEntity(updateDto);


        //}

        private ErrorContext DefineErrorContext(string methodName, OperationType operation)
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
