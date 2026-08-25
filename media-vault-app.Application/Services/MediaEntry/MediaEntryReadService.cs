using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Mappers.MediaEntry;
using Microsoft.Extensions.Logging;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using media_vault_app.Application.Pagination;
using media_vault_app.Application.Results;
using media_vault_app.Application.Validation;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;

namespace media_vault_app.Application.Services.MediaEntry;

public sealed class MediaEntryReadService : IMediaEntryReadService
{
    private readonly IMediaEntryRepo _mediaEntryRepo;
    private readonly IUserRepo _userRepo;
    private readonly ILogger<MediaEntryReadService> _logger;

    public MediaEntryReadService(
        IMediaEntryRepo mediaEntryRepo,
        IUserRepo userRepo,
        ILogger<MediaEntryReadService> logger)
    {
        _mediaEntryRepo = mediaEntryRepo;
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<Result<MediaEntryDetailedDto>> GetDetailedByIdAsync(
        Guid ownerId,
        Guid id,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(GetDetailedByIdAsync), OperationType.Get);
        var validationErrors = ValidateIds(ownerId, id, errorContext);

        if (validationErrors.Count > 0)
            return LogValidationFailure<MediaEntryDetailedDto>(validationErrors, nameof(GetDetailedByIdAsync), errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult.ToResult<MediaEntryDetailedDto>();

        var repoResult = await _mediaEntryRepo.GetDetailedByIdAsync(ownerId, id, ct);
        return repoResult.Map(MediaEntryMapping.ToDetailed);
    }

    public async Task<Result<MovieEntryDetailedDto>> GetMovieByIdAsync(
        Guid ownerId,
        Guid id,
        CancellationToken ct = default)
    {
        var result = await GetDetailedByIdAsync(ownerId, id, ct);
        if (result.IsFailure)
            return result.ToResult<MovieEntryDetailedDto>();

        if (result.Value is MovieEntryDetailedDto movie)
            return Result<MovieEntryDetailedDto>.Success(movie);

        return Result<MovieEntryDetailedDto>.Failure(
            MediaVaultErrors.NotFound(DefineErrorContext(nameof(GetMovieByIdAsync), OperationType.Get)));
    }

    public async Task<Result<TvSeriesEntryDetailedDto>> GetTvSeriesByIdAsync(
        Guid ownerId,
        Guid id,
        CancellationToken ct = default)
    {
        var result = await GetDetailedByIdAsync(ownerId, id, ct);
        if (result.IsFailure)
            return result.ToResult<TvSeriesEntryDetailedDto>();

        if (result.Value is TvSeriesEntryDetailedDto tvSeries)
            return Result<TvSeriesEntryDetailedDto>.Success(tvSeries);

        return Result<TvSeriesEntryDetailedDto>.Failure(
            MediaVaultErrors.NotFound(DefineErrorContext(nameof(GetTvSeriesByIdAsync), OperationType.Get)));
    }

    public async Task<Result<GameEntryDetailedDto>> GetGameByIdAsync(
        Guid ownerId,
        Guid id,
        CancellationToken ct = default)
    {
        var result = await GetDetailedByIdAsync(ownerId, id, ct);
        if (result.IsFailure)
            return result.ToResult<GameEntryDetailedDto>();

        if (result.Value is GameEntryDetailedDto game)
            return Result<GameEntryDetailedDto>.Success(game);

        return Result<GameEntryDetailedDto>.Failure(
            MediaVaultErrors.NotFound(DefineErrorContext(nameof(GetGameByIdAsync), OperationType.Get)));
    }

    public async Task<Result<BookEntryDetailedDto>> GetBookByIdAsync(
        Guid ownerId,
        Guid id,
        CancellationToken ct = default)
    {
        var result = await GetDetailedByIdAsync(ownerId, id, ct);
        if (result.IsFailure)
            return result.ToResult<BookEntryDetailedDto>();

        if (result.Value is BookEntryDetailedDto book)
            return Result<BookEntryDetailedDto>.Success(book);

        return Result<BookEntryDetailedDto>.Failure(
            MediaVaultErrors.NotFound(DefineErrorContext(nameof(GetBookByIdAsync), OperationType.Get)));
    }

    public async Task<Result<MangaEntryDetailedDto>> GetMangaByIdAsync(
        Guid ownerId,
        Guid id,
        CancellationToken ct = default)
    {
        var result = await GetDetailedByIdAsync(ownerId, id, ct);
        if (result.IsFailure)
            return result.ToResult<MangaEntryDetailedDto>();

        if (result.Value is MangaEntryDetailedDto manga)
            return Result<MangaEntryDetailedDto>.Success(manga);

        return Result<MangaEntryDetailedDto>.Failure(
            MediaVaultErrors.NotFound(DefineErrorContext(nameof(GetMangaByIdAsync), OperationType.Get)));
    }

    public async Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> GetMinimalCollectionByOwnerIdAsync(
        Guid ownerId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(GetMinimalCollectionByOwnerIdAsync), OperationType.GetCollection);
        var validationErrors = ValidateOwnerId(ownerId, errorContext);

        if (validationErrors.Count > 0)
            return LogValidationFailure<IReadOnlyList<MediaEntryMinimalDto>>(
                validationErrors,
                nameof(GetMinimalCollectionByOwnerIdAsync),
                errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult.ToResult<IReadOnlyList<MediaEntryMinimalDto>>();

        var pagination = PaginationParameters.Normalize(pageNumber, pageSize);
        return await _mediaEntryRepo.GetMinimalCollectionByOwnerIdAsync(
            ownerId,
            pagination.PageNumber,
            pagination.PageSize,
            ct);
    }

    public async Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> SearchMediaEntriesAsync(
        Guid ownerId,
        SearchRequestDto request,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(SearchMediaEntriesAsync), OperationType.GetCollection);
        var validationErrors = ValidateOwnerId(ownerId, errorContext);

        var query = request?.Query;
        if (query.IsMissingMediaVaultValue(
                errorContext with { FieldName = "Query" },
                out var queryError))
        {
            validationErrors.Add(queryError);
        }

        if (validationErrors.Count > 0)
            return LogValidationFailure<IReadOnlyList<MediaEntryMinimalDto>>(
                validationErrors,
                nameof(SearchMediaEntriesAsync),
                errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult.ToResult<IReadOnlyList<MediaEntryMinimalDto>>();

        var pagination = PaginationParameters.Normalize(pageNumber, pageSize);
        return await _mediaEntryRepo.SearchMediaEntriesAsync(
            ownerId,
            request!.Query,
            pagination.PageNumber,
            pagination.PageSize,
            ct);
    }

    private async Task<Result<bool>> EnsureOwnerExistsAsync(Guid ownerId, CancellationToken ct) =>
        await _userRepo.ExistsAsync(ownerId, ct);

    private static List<ValidationError> ValidateOwnerId(Guid ownerId, ErrorContext errorContext)
    {
        var validationErrors = new List<ValidationError>();
        if (ownerId.IsNotValidMediaVaultId(
                errorContext with { FieldName = nameof(ownerId) },
                out var ownerIdError))
        {
            validationErrors.Add(ownerIdError);
        }

        return validationErrors;
    }

    private static List<ValidationError> ValidateIds(Guid ownerId, Guid id, ErrorContext errorContext)
    {
        var validationErrors = ValidateOwnerId(ownerId, errorContext);
        if (id.IsNotValidMediaVaultId(
                errorContext with { FieldName = nameof(id) },
                out var idError))
        {
            validationErrors.Add(idError);
        }

        return validationErrors;
    }

    private Result<T> LogValidationFailure<T>(
        IReadOnlyList<ValidationError> validationErrors,
        string methodName,
        ErrorContext errorContext)
        where T : notnull
    {
        ServiceValidationLogging.LogValidationFailure(
            _logger,
            validationErrors,
            GetType().Name,
            methodName,
            errorContext);

        return Result<T>.ValidationFailure(
            validationErrors,
            MediaVaultResultMessages.ValidationFailure);
    }

    private static ErrorContext DefineErrorContext(string methodName, OperationType operation) =>
        new(operation: operation, entityName: nameof(MediaEntryEntity));
}
