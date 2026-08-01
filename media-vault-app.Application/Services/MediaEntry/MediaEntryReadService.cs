using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Mappers;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Services.Base_Classes;
using Microsoft.Extensions.Logging;
using Rasmus.SharedKernel.Pagination;
using Rasmus.SharedKernel.ResultPattern;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Services.MediaEntry
{
    public class MediaEntryReadService
        : DependentEntityReadServiceBase<UserEntity, MediaEntryEntity, Guid, Guid, MediaEntryDetailedDto, MediaEntryMinimalDto>,
        IMediaEntryReadService
    {
        private IMediaEntryRepo MediaEntryRepo => (IMediaEntryRepo)base._dependentEntityRepo;

        public MediaEntryReadService(
            IMediaEntryRepo mediaEntryRepo,
            IUserRepo ownerRepo,
            IMediaEntryEntityMapper entityMapper,
            ILogger<MediaEntryReadService> logger
            ) : base(mediaEntryRepo, entityMapper, ownerRepo, logger)
        {
        }

        public async Task<Result<MovieEntryDetailedDto>> GetMovieByIdAsync(
            Guid ownerId,
            Guid id,
            CancellationToken ct = default) =>
            await GetTypedByIdAsync<MovieEntryDetailedDto>(ownerId, id, nameof(GetMovieByIdAsync), "movie", ct);

        public async Task<Result<TvSeriesEntryDetailedDto>> GetTvSeriesByIdAsync(
            Guid ownerId,
            Guid id,
            CancellationToken ct = default) =>
            await GetTypedByIdAsync<TvSeriesEntryDetailedDto>(ownerId, id, nameof(GetTvSeriesByIdAsync), "TV series", ct);

        public async Task<Result<GameEntryDetailedDto>> GetGameByIdAsync(
            Guid ownerId,
            Guid id,
            CancellationToken ct = default) =>
            await GetTypedByIdAsync<GameEntryDetailedDto>(ownerId, id, nameof(GetGameByIdAsync), "game", ct);

        public async Task<Result<BookEntryDetailedDto>> GetBookByIdAsync(
            Guid ownerId,
            Guid id,
            CancellationToken ct = default) =>
            await GetTypedByIdAsync<BookEntryDetailedDto>(ownerId, id, nameof(GetBookByIdAsync), "book", ct);

        public async Task<Result<MangaEntryDetailedDto>> GetMangaByIdAsync(
            Guid ownerId,
            Guid id,
            CancellationToken ct = default) =>
            await GetTypedByIdAsync<MangaEntryDetailedDto>(ownerId, id, nameof(GetMangaByIdAsync), "manga", ct);

        public async Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> SearchMediaEntriesAsync(
            Guid ownerId,
            SearchRequestDto request,
            int pageNumber = 1,
            int pageSize = 10, CancellationToken ct = default)
        {

            var baseErrorContext = DefineErrorContext(nameof(SearchMediaEntriesAsync), OperationType.GetCollection);

            var validationErrors = new List<ValidationError>();

            if (ownerId.IsNotValidId(baseErrorContext with { FieldName = nameof(ownerId) }, out var ownerIdError))
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
                _logger.LogDebug("SearchMediaEntriesAsync validation failed: {ValidationErrors}", ServiceValidationLogging.FormatValidationErrors(validationErrors));
                return Result<IReadOnlyList<MediaEntryMinimalDto>>.ValidationFailure(validationErrors, "Validation errors occurred.");
            }

            // Ensure the owner exists before attempting to search for media entries
            var ownerExistsResult = await EnsureOwnerExistsAsync(ownerId, ct);
            if (ownerExistsResult.IsFailure)
            {
                _logger.LogDebug("SearchMediaEntriesAsync owner check failed: {Code} — {Description}",
                    ownerExistsResult.PrimaryError.Code, ownerExistsResult.PrimaryError.Description);

                return ownerExistsResult.From<bool, IReadOnlyList<MediaEntryMinimalDto>>();
            }

            var pagination = PaginationParameters.Normalize(pageNumber, pageSize);
            pageNumber = pagination.PageNumber;
            pageSize = pagination.PageSize;

            var repoResult = await MediaEntryRepo.SearchMediaEntriesAsync(ownerId, request.Query, pageNumber, pageSize, ct);

            // Maps the result internally  
            var mappedRepoResult = repoResult.Map(_entityToDtoMapper.ToMinimalDtoCollection);
            if (mappedRepoResult.IsFailure)
                _logger.LogDebug("SearchMediaEntriesAsync failed: {Code} — {Description}", mappedRepoResult.PrimaryError.Code, mappedRepoResult.PrimaryError.Description);
            return mappedRepoResult;

        }

        private async Task<Result<TDetailedSubtype>> GetTypedByIdAsync<TDetailedSubtype>(
            Guid ownerId,
            Guid id,
            string methodName,
            string subtypeDisplayName,
            CancellationToken ct = default)
            where TDetailedSubtype : MediaEntryDetailedDto
        {
            var baseResult = await GetDetailedByIdAsync(ownerId, id, ct);

            if (baseResult.IsFailure)
            {
                _logger.LogDebug("GetTypedByIdAsync ({Subtype}) failed: {Code} — {Description}",
                    subtypeDisplayName, baseResult.PrimaryError.Code, baseResult.PrimaryError.Description);

                return baseResult.From<MediaEntryDetailedDto, TDetailedSubtype>();
            }

            if (baseResult.Value is TDetailedSubtype typedDetailedDto)
            {
                return Result<TDetailedSubtype>.Success(typedDetailedDto);
            }

            var mismatchErrorContext = DefineErrorContext(methodName, OperationType.Get);

            return Result<TDetailedSubtype>.Failure(Error.NotFound(mismatchErrorContext));
        }

    }
}
