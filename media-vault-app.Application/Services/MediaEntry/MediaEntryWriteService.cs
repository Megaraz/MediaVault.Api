using System;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Mappers.MediaEntry;
using media_vault_app.Application.Validators.MediaEntry;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Validators;
using Rasmus.SharedKernel.ResultPattern;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Services.MediaEntry
{
    public class MediaEntryWriteService : IMediaEntryWriteService
    {
        private readonly IMediaEntryRepo _mediaEntryRepo;
        private readonly IUserRepo _userRepo;
        private readonly MediaEntryDtoValidator _dtoValidator;
        private readonly MediaEntryDtoMapper _dtoToEntityMapper;
        private readonly MediaEntryEntityMapper _entityToDtoMapper;


        public MediaEntryWriteService(
            IMediaEntryRepo mediaEntryRepo,
            IUserRepo userRepo)
        {
            _mediaEntryRepo = mediaEntryRepo;
            _userRepo = userRepo;
            _entityToDtoMapper = new MediaEntryEntityMapper();
            _dtoToEntityMapper = new MediaEntryDtoMapper();
            _dtoValidator = new MediaEntryDtoValidator();
        }

        public async Task<Result<MediaEntryDetailedDto>> CreateAsync(Guid userId, MediaEntryCreateDto createDto, CancellationToken ct = default)
        {
            var userIdValidationResult = ValidateUserId(userId, nameof(CreateAsync), OperationType.Create);
            if (userIdValidationResult is not null)
            {
                return ToFailureResult<MediaEntryDetailedDto>(userIdValidationResult);
            }

            var userResult = await EnsureUserExistsAsync(userId, ct);
            if (userResult.IsFailure)
            {
                return ToFailureResult<MediaEntryDetailedDto>(userResult);
            }

            var errorContext = CreateErrorContext(nameof(CreateAsync), OperationType.Create, typeof(MediaEntryCreateDto).Name);

            if (!_dtoValidator.IsValidRegisterDto(createDto, errorContext, out var validationErrors))
            {
                return Result<MediaEntryDetailedDto>.ValidationFailure(validationErrors, "MediaEntry creation validation failed.");
            }

            var entity = _dtoToEntityMapper.ToEntity(createDto);
            entity.OwnerId = userId;

            var repoResult = await _mediaEntryRepo.CreateAsync(entity, ct);
            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);
        }

        public async Task<Result> UpdateAsync(Guid userId, Guid mediaEntryId, MediaEntryUpdateDto updateDto, CancellationToken ct = default)
        {
            var userIdValidationResult = ValidateUserId(userId, nameof(UpdateAsync), OperationType.Update);
            if (userIdValidationResult is not null)
            {
                return ToFailureResult(userIdValidationResult);
            }

            var mediaEntryIdValidationResult = ValidateMediaEntryId(mediaEntryId, nameof(UpdateAsync), OperationType.Update);
            if (mediaEntryIdValidationResult is not null)
            {
                return ToFailureResult(mediaEntryIdValidationResult);
            }

            var userResult = await EnsureUserExistsAsync(userId, ct);
            if (userResult.IsFailure)
            {
                return ToFailureResult(userResult);
            }

            var errorContext = CreateErrorContext(nameof(UpdateAsync), OperationType.Update, typeof(MediaEntryUpdateDto).Name);

            if (!_dtoValidator.IsValidUpdateDto(updateDto, errorContext, out var validationErrors))
            {
                return Result.ValidationFailure(validationErrors, "MediaEntry update validation failed.");
            }

            var updatedEntity = _dtoToEntityMapper.MapToEntity(mediaEntryId, updateDto);
            updatedEntity.OwnerId = userId;

            return await _mediaEntryRepo.UpdateAsync(userId, updatedEntity, ct);
        }

        public async Task<Result> DeleteAsync(Guid userId, Guid mediaEntryId, CancellationToken ct = default)
        {
            var userIdValidationResult = ValidateUserId(userId, nameof(DeleteAsync), OperationType.Delete);
            if (userIdValidationResult is not null)
            {
                return ToFailureResult(userIdValidationResult);
            }

            var mediaEntryIdValidationResult = ValidateMediaEntryId(mediaEntryId, nameof(DeleteAsync), OperationType.Delete);
            if (mediaEntryIdValidationResult is not null)
            {
                return ToFailureResult(mediaEntryIdValidationResult);
            }

            var userResult = await EnsureUserExistsAsync(userId, ct);
            if (userResult.IsFailure)
            {
                return ToFailureResult(userResult);
            }

            return await _mediaEntryRepo.DeleteAsync(userId, mediaEntryId, ct);
        }

        private async Task<Result<UserEntity>> EnsureUserExistsAsync(Guid userId, CancellationToken ct)
        {
            return await _userRepo.GetByIdAsync(userId, ct);
        }

        private Result? ValidateUserId(Guid userId, string methodName, OperationType operation)
        {
            if (Validator.IsValidId(userId))
            {
                return null;
            }

            var errorContext = CreateErrorContext(methodName, operation);
            errorContext.DescriptionSuffix = "A valid UserId is required and cannot be null or empty.";
            errorContext.FieldName = nameof(userId);

            var validationError = ValidationError.Required(errorContext);
            return Result.ValidationFailure([validationError], errorContext.DescriptionSuffix);
        }

        private Result? ValidateMediaEntryId(Guid mediaEntryId, string methodName, OperationType operation)
        {
            if (Validator.IsValidId(mediaEntryId))
            {
                return null;
            }

            var errorContext = CreateErrorContext(methodName, operation);
            errorContext.DescriptionSuffix = "A valid MediaEntry Id is required and cannot be null or empty.";
            errorContext.FieldName = nameof(mediaEntryId);

            var validationError = ValidationError.Required(errorContext);
            return Result.ValidationFailure([validationError], errorContext.DescriptionSuffix);
        }

        private ErrorContext CreateErrorContext(string methodName, OperationType operation, string? entityName = null)
        {
            return new ErrorContext(
                layer: "Service",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: entityName ?? typeof(MediaEntryEntity).Name);
        }

        private static Result ToFailureResult(Result result)
        {
            return result.PrimaryError.Type == ErrorType.Validation
                ? Result.ValidationFailure(result.ValidationErrors, result.Message)
                : Result.Failure(result.PrimaryError, result.Message);
        }

        private static Result<TValue> ToFailureResult<TValue>(Result result)
        {
            return result.PrimaryError.Type == ErrorType.Validation
                ? Result<TValue>.ValidationFailure(result.ValidationErrors, result.Message)
                : Result<TValue>.Failure(result.PrimaryError, result.Message);
        }

    }
}
