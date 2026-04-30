using System;
using System.Collections.Generic;
using media_vault_app.Application.DTOs;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Services.Base_Classes;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.ResultPattern;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Services.MediaEntry
{
    public class MediaEntryReadService
        : DependentEntityReadServiceBase<UserEntity, MediaEntryEntity, Guid, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto>,
        IMediaEntryReadService
    {

        private readonly IMediaEntryRepo _mediaEntryRepo;

        public MediaEntryReadService(
            IMediaEntryRepo mediaEntryRepo,
            IUserRepo ownerRepo,
            IMapEntityToDto<MediaEntryEntity, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto> entityMapper
            ) : base(mediaEntryRepo, entityMapper, ownerRepo)
        {
            _mediaEntryRepo = mediaEntryRepo;
        }

        public async Task<Result<IEnumerable<MediaEntryMinimalDto>>> SearchMediaEntriesAsync(
            Guid ownerId,
            SearchRequestDto request,
            int pageNumber = 1,
            int pageSize = 10, CancellationToken ct = default)
        {

            var baseErrorContext = DefineErrorContext(nameof(SearchMediaEntriesAsync), OperationType.GetCollection);

            var validationErrors = new List<ValidationError>();

            if (!ownerId.IsValidId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdError))
            {
                validationErrors.Add(ownerIdError);
            }

            // Validate search query
            var queryErrorContext = baseErrorContext with { FieldName = nameof(request.Query) };

            if (request.Query.IsNullOrWhiteSpace(queryErrorContext, out var nullOrEmptyError))
            {
                validationErrors.Add(nullOrEmptyError);
            }

            // If there are any validation errors, return them in a single Result response
            if (validationErrors.Any())
            {
                return Result<IEnumerable<MediaEntryMinimalDto>>.ValidationFailure(validationErrors, "Validation errors occurred.");
            }

            // Ensure the owner exists before attempting to search for media entries
            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);
            if (ownerExistsResult.IsFailure)
            {
                return ownerExistsResult.From<bool, IEnumerable<MediaEntryMinimalDto>>();
            }

            ValidateAndAdjustPaginationParameters(ref pageNumber, ref pageSize);

            var repoResult = await _mediaEntryRepo.SearchMediaEntriesAsync(ownerId, request.Query, pageNumber, pageSize, ct);

            // Maps the result internally  
            return repoResult.Map(_entityToDtoMapper.ToMinimalDtoCollection);

        }

    }
}
