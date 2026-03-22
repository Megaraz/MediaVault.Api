using System;
using System.Collections.Generic;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Mappers.MediaEntry;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.ResultPattern;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Services.MediaEntry
{
    public class MediaEntryReadService : IMediaEntryReadService
    {
        private readonly IMediaEntryRepo _mediaEntryRepo;
        private readonly IUserRepo _userRepo;
        private readonly IMapEntityToDto<MediaEntryEntity, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto> _entityToDtoMapper;

        public MediaEntryReadService(
            IMediaEntryRepo mediaEntryRepo,
            IUserRepo userRepo,
            IMapEntityToDto<MediaEntryEntity, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto> entityToDtoMapper)
        {
            _mediaEntryRepo = mediaEntryRepo;
            _userRepo = userRepo;
            _entityToDtoMapper = entityToDtoMapper;
        }

        public async Task<Result<MediaEntryDetailedDto>> GetByIdAsync(Guid userId, Guid mediaEntryId, CancellationToken ct = default)
        {
            var userIdValidationResult = ValidateUserId(userId, nameof(GetByIdAsync), OperationType.Get);
            if (userIdValidationResult is not null)
            {
                return Result<MediaEntryDetailedDto>.ValidationFailure(userIdValidationResult.ValidationErrors, userIdValidationResult.Message);
            }

            var mediaEntryIdValidationResult = ValidateMediaEntryId(mediaEntryId, nameof(GetByIdAsync), OperationType.Get);
            if (mediaEntryIdValidationResult is not null)
            {
                return Result<MediaEntryDetailedDto>.ValidationFailure(mediaEntryIdValidationResult.ValidationErrors, mediaEntryIdValidationResult.Message);
            }

            var userResult = await EnsureUserExistsAsync(userId, ct);
            if (userResult.IsFailure)
            {
                return userResult.From<UserEntity, MediaEntryDetailedDto>();
            }

            var repoResult = await _mediaEntryRepo.GetByIdAsync(userId, mediaEntryId, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);
        }

        public async Task<Result<IEnumerable<MediaEntryDetailedDto>>> GetDetailedCollectionAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var validationResult = ValidateCollectionRequest(userId, pageNumber, pageSize, nameof(GetDetailedCollectionAsync));
            if (validationResult is not null)
            {
                return Result<IEnumerable<MediaEntryDetailedDto>>.ValidationFailure(validationResult.ValidationErrors, validationResult.Message);
            }

            var userResult = await EnsureUserExistsAsync(userId, ct);
            if (userResult.IsFailure)
            {
                return userResult.From<UserEntity, IEnumerable<MediaEntryDetailedDto>>();
            }

            var repoResult = await _mediaEntryRepo.GetCollectionByUserIdAsync(userId, pageNumber, pageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDtoCollection);
        }

        public async Task<Result<IEnumerable<MediaEntryMinimalDto>>> GetMinimalCollectionAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var validationResult = ValidateCollectionRequest(userId, pageNumber, pageSize, nameof(GetMinimalCollectionAsync));
            if (validationResult is not null)
            {
                return Result<IEnumerable<MediaEntryMinimalDto>>.ValidationFailure(validationResult.ValidationErrors, validationResult.Message);
            }

            var userResult = await EnsureUserExistsAsync(userId, ct);
            if (userResult.IsFailure)
            {
                return userResult.From<UserEntity, IEnumerable<MediaEntryMinimalDto>>();
            }

            var repoResult = await _mediaEntryRepo.GetCollectionByUserIdAsync(userId, pageNumber, pageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToMinimalDtoCollection);
        }

        private async Task<Result<UserEntity>> EnsureUserExistsAsync(Guid userId, CancellationToken ct)
        {
            return await _userRepo.GetByIdAsync(userId, ct);
        }

        private Result? ValidateCollectionRequest(Guid userId, int pageNumber, int pageSize, string methodName)
        {
            var validationErrors = new List<ValidationError>();
            var errorContext = DefineErrorContext(methodName, OperationType.GetCollection);

            if (!Validator.IsValidId(userId))
            {
                errorContext.DescriptionSuffix = "A valid UserId is required and cannot be null or empty.";
                errorContext.FieldName = nameof(userId);

                validationErrors.Add(ValidationError.Required(errorContext));
            }

            if (pageNumber < 1)
            {
                errorContext.DescriptionSuffix = "Page number must be greater than 0.";
                errorContext.FieldName = nameof(pageNumber);

                validationErrors.Add(ValidationError.OutOfRange(errorContext, "Greater than 0"));
            }

            if (pageSize < 1)
            {
                errorContext.DescriptionSuffix = "Page size must be greater than 0.";
                errorContext.FieldName = nameof(pageSize);

                validationErrors.Add(ValidationError.OutOfRange(errorContext, "Greater than 0"));
            }

            return validationErrors.Any()
                ? Result.ValidationFailure(validationErrors, "Validation errors occurred.")
                : null;
        }

        private Result? ValidateUserId(Guid userId, string methodName, OperationType operation)
        {
            if (Validator.IsValidId(userId))
            {
                return null;
            }

            var errorContext = DefineErrorContext(methodName, operation);
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

            var errorContext = DefineErrorContext(methodName, operation);
            errorContext.DescriptionSuffix = "A valid MediaEntry Id is required and cannot be null or empty.";
            errorContext.FieldName = nameof(mediaEntryId);

            var validationError = ValidationError.Required(errorContext);
            return Result.ValidationFailure([validationError], errorContext.DescriptionSuffix);
        }

        private ErrorContext DefineErrorContext(string methodName, OperationType operation)
        {
            return new ErrorContext(
                layer: "Service",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: typeof(MediaEntryEntity).Name);
        }

    }
}
