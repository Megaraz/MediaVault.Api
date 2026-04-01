using System;
using System.Collections.Generic;
using media_vault_app.Application.DTOs.ExternalAPIs;
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
        private readonly MediaEntryEntityMapper _entityToDtoMapper;

        public MediaEntryReadService(
            IMediaEntryRepo mediaEntryRepo,
            IUserRepo userRepo)

        {
            _mediaEntryRepo = mediaEntryRepo;
            _userRepo = userRepo;
            _entityToDtoMapper = new();
        }

        public async Task<Result<MediaEntryDetailedDto>> GetByIdAsync(Guid userId, Guid mediaEntryId, CancellationToken ct = default)
        {

            var validationErrors = new List<ValidationError>();

            var userIdErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get, "User ID");

            if (!userId.IsValidId(userIdErrorContext, out var userIdValidationError))
                validationErrors.Add(userIdValidationError);

            var mediaEntryIdErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get, "Media Entry ID");

            if (!mediaEntryId.IsValidId(mediaEntryIdErrorContext, out var mediaEntryIdValidationError))
                validationErrors.Add(mediaEntryIdValidationError);

            if (validationErrors.Any())
                return Result<MediaEntryDetailedDto>.ValidationFailure(validationErrors, "Validation errors occurred.");

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
            if (HasCollectionValidationErrors(userId, pageNumber, pageSize, nameof(GetDetailedCollectionAsync), out var validationErrors))
            {
                return Result<IEnumerable<MediaEntryDetailedDto>>.ValidationFailure(validationErrors, "Validation errors occurred.");
            }

            var userResult = await EnsureUserExistsAsync(userId, ct);
            if (userResult.IsFailure)
            {
                return userResult.From<UserEntity, IEnumerable<MediaEntryDetailedDto>>();
            }

            var repoResult = await _mediaEntryRepo.GetCollectionByUserIdAsync(userId, pageNumber, pageSize, ct);

            return repoResult.Map(_entityToDtoMapper.ToDetailedDtoCollection);
        }


        public async Task<Result<IEnumerable<MediaEntryMinimalDto>>> SearchMediaEntriesAsync(
            Guid userId,
            SearchRequestDto request,
            int pageNumber = 1,
            int pageSize = 10, CancellationToken ct = default)
        {

            var validationErrors = new List<ValidationError>();

            // Validate userId, pageNumber and pageSize using the existing collection validation method
            if (HasCollectionValidationErrors(userId, pageNumber, pageSize, nameof(SearchMediaEntriesAsync), out var collectionValidationErrors))
            {
                validationErrors.AddRange(collectionValidationErrors);
            }

            // Validate search query
            var queryErrorContext = DefineErrorContext(nameof(SearchMediaEntriesAsync), OperationType.GetCollection, fieldName: nameof(request.Query));
            if (request.Query.IsNullOrWhiteSpace(queryErrorContext, out var nullOrEmptyError))
            {
                queryErrorContext.DescriptionSuffix = "Search query cannot be null or empty.";
                validationErrors.Add(ValidationError.Required(queryErrorContext));
            }

            // If there are any validation errors, return them in a single Result response
            if (validationErrors.Any())
            {
                return Result<IEnumerable<MediaEntryMinimalDto>>.ValidationFailure(validationErrors, "Validation errors occurred.");
            }

            // Ensure the user exists before attempting to search for media entries
            var userResult = await EnsureUserExistsAsync(userId, ct);
            if (userResult.IsFailure)
            {
                return userResult.From<UserEntity, IEnumerable<MediaEntryMinimalDto>>();
            }

            var repoResult = await _mediaEntryRepo.SearchMediaEntriesAsync(userId, request.Query, pageNumber, pageSize, ct);

            // Maps the result internally  
            return repoResult.Map(_entityToDtoMapper.ToMinimalDtoCollection);

        }

        public async Task<Result<IEnumerable<MediaEntryMinimalDto>>> GetMinimalCollectionAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            if (HasCollectionValidationErrors(userId, pageNumber, pageSize, nameof(GetMinimalCollectionAsync), out var validationErrors))
            {
                return Result<IEnumerable<MediaEntryMinimalDto>>.ValidationFailure(validationErrors, "Validation errors occurred.");
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


        private bool HasCollectionValidationErrors(Guid userId, int pageNumber, int pageSize, string methodName, out IEnumerable<ValidationError> validationErrors)
        {
            var internalErrors = new List<ValidationError>();

            var userIdErrorContext = DefineErrorContext(methodName, OperationType.GetCollection, "User ID");

            if (!userId.IsValidId(userIdErrorContext, out var userIdValidationError))
                internalErrors.Add(userIdValidationError);

            var pageNumberErrorContext = DefineErrorContext(methodName, OperationType.GetCollection, "Page Number");
            int minPageNumber = 1;
            if (pageNumber.IsToLow(minPageNumber, pageNumberErrorContext, out var pageNumberValidationError))
            {
                internalErrors.Add(pageNumberValidationError);
            }

            var pageSizeErrorContext = DefineErrorContext(methodName, OperationType.GetCollection, "Page Size");
            int minPageSize = 1;
            if (pageSize.IsToLow(minPageSize, pageSizeErrorContext, out var pageSizeValidationError))
            {
                internalErrors.Add(pageSizeValidationError);
            }

            validationErrors = internalErrors;

            return !validationErrors.Any();
        }

        //private Result? ValidateCollectionRequest(Guid userId, int pageNumber, int pageSize, string methodName)
        //{
        //    var validationErrors = new List<ValidationError>();

        //    var userIdErrorContext = DefineErrorContext(methodName, OperationType.GetCollection, "User ID");

        //    if (!userId.IsValidId(userIdErrorContext, out var userIdValidationError))
        //        validationErrors.Add(userIdValidationError);

        //    var pageNumberErrorContext = DefineErrorContext(methodName, OperationType.GetCollection, "Page Number");
        //    int minPageNumber = 1;
        //    if (pageNumber.IsToLow(minPageNumber, pageNumberErrorContext, out var pageNumberValidationError))
        //    {
        //        validationErrors.Add(pageNumberValidationError);
        //    }

        //    var pageSizeErrorContext = DefineErrorContext(methodName, OperationType.GetCollection, "Page Size");
        //    int minPageSize = 1;
        //    if (pageSize.IsToLow(minPageSize, pageSizeErrorContext, out var pageSizeValidationError))
        //    {
        //        validationErrors.Add(pageSizeValidationError);
        //    }

        //    return validationErrors.Any()
        //        ? Result.ValidationFailure(validationErrors, "Validation errors occurred.")
        //        : null;
        //}


        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null, string? confirmFieldName = null)
        {
            return new ErrorContext(
                layer: "Application",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: typeof(MediaEntryEntity).Name,
                fieldName: fieldName,
                confirmFieldName: confirmFieldName
                );
        }

    }
}
