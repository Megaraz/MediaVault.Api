using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Interfaces.Validators;
using media_vault_app.Application.Mappers.MediaEntry;
using media_vault_app.Domain.Entities;
using Microsoft.Extensions.Logging;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Results;
using Rasmus.SharedKernel.Validation;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;

namespace media_vault_app.Application.Services.MediaEntry;

public sealed class MediaEntryWriteService : IMediaEntryWriteService
{
    private readonly IMediaEntryRepo _mediaEntryRepo;
    private readonly IUserRepo _userRepo;
    private readonly IMediaEntryDtoValidator _dtoValidator;
    private readonly ILogger<MediaEntryWriteService> _logger;

    public MediaEntryWriteService(
        IMediaEntryRepo mediaEntryRepo,
        IUserRepo userRepo,
        IMediaEntryDtoValidator dtoValidator,
        ILogger<MediaEntryWriteService> logger)
    {
        _mediaEntryRepo = mediaEntryRepo;
        _userRepo = userRepo;
        _dtoValidator = dtoValidator;
        _logger = logger;
    }

    public async Task<Result<MovieEntryDetailedDto>> CreateMovieAsync(
        Guid ownerId,
        MovieEntryCreateDto createDto,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(CreateMovieAsync), OperationType.Create);
        var validationErrors = ValidateCreate(ownerId, createDto, errorContext);
        if (validationErrors.Count > 0)
            return LogValidationFailure<MovieEntryDetailedDto>(validationErrors, nameof(CreateMovieAsync), errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult.ToResult<MovieEntryDetailedDto>();

        var entity = MediaEntryMapping.ToMovie(createDto);
        entity.OwnerId = ownerId;
        var repoResult = await _mediaEntryRepo.CreateAsync(entity, ct);
        return repoResult.Map(entity => MediaEntryMapping.ToMovie((MovieEntry)entity));
    }

    public async Task<Result<TvSeriesEntryDetailedDto>> CreateTvSeriesAsync(
        Guid ownerId,
        TvSeriesEntryCreateDto createDto,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(CreateTvSeriesAsync), OperationType.Create);
        var validationErrors = ValidateCreate(ownerId, createDto, errorContext);
        if (validationErrors.Count > 0)
            return LogValidationFailure<TvSeriesEntryDetailedDto>(validationErrors, nameof(CreateTvSeriesAsync), errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult.ToResult<TvSeriesEntryDetailedDto>();

        var entity = MediaEntryMapping.ToTvSeries(createDto);
        entity.OwnerId = ownerId;
        var repoResult = await _mediaEntryRepo.CreateAsync(entity, ct);
        return repoResult.Map(entity => MediaEntryMapping.ToTvSeries((TvSeriesEntry)entity));
    }

    public async Task<Result<GameEntryDetailedDto>> CreateGameAsync(
        Guid ownerId,
        GameEntryCreateDto createDto,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(CreateGameAsync), OperationType.Create);
        var validationErrors = ValidateCreate(ownerId, createDto, errorContext);
        if (validationErrors.Count > 0)
            return LogValidationFailure<GameEntryDetailedDto>(validationErrors, nameof(CreateGameAsync), errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult.ToResult<GameEntryDetailedDto>();

        var entity = MediaEntryMapping.ToGame(createDto);
        entity.OwnerId = ownerId;
        var repoResult = await _mediaEntryRepo.CreateAsync(entity, ct);
        return repoResult.Map(entity => MediaEntryMapping.ToGame((GameEntry)entity));
    }

    public async Task<Result<BookEntryDetailedDto>> CreateBookAsync(
        Guid ownerId,
        BookEntryCreateDto createDto,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(CreateBookAsync), OperationType.Create);
        var validationErrors = ValidateCreate(ownerId, createDto, errorContext);
        if (validationErrors.Count > 0)
            return LogValidationFailure<BookEntryDetailedDto>(validationErrors, nameof(CreateBookAsync), errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult.ToResult<BookEntryDetailedDto>();

        var entity = MediaEntryMapping.ToBook(createDto);
        entity.OwnerId = ownerId;
        var repoResult = await _mediaEntryRepo.CreateAsync(entity, ct);
        return repoResult.Map(entity => MediaEntryMapping.ToBook((BookEntry)entity));
    }

    public async Task<Result<MangaEntryDetailedDto>> CreateMangaAsync(
        Guid ownerId,
        MangaEntryCreateDto createDto,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(CreateMangaAsync), OperationType.Create);
        var validationErrors = ValidateCreate(ownerId, createDto, errorContext);
        if (validationErrors.Count > 0)
            return LogValidationFailure<MangaEntryDetailedDto>(validationErrors, nameof(CreateMangaAsync), errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult.ToResult<MangaEntryDetailedDto>();

        var entity = MediaEntryMapping.ToManga(createDto);
        entity.OwnerId = ownerId;
        var repoResult = await _mediaEntryRepo.CreateAsync(entity, ct);
        return repoResult.Map(entity => MediaEntryMapping.ToManga((MangaEntry)entity));
    }

    public async Task<Result> UpdateMovieAsync(
        Guid ownerId,
        Guid id,
        MovieEntryUpdateDto updateDto,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(UpdateMovieAsync), OperationType.Update);
        var validationErrors = ValidateUpdate(ownerId, id, updateDto, errorContext);
        if (validationErrors.Count > 0)
            return LogValidationFailure(validationErrors, nameof(UpdateMovieAsync), errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult;

        var entity = MediaEntryMapping.ToMovie(id, updateDto);
        entity.OwnerId = ownerId;
        return await _mediaEntryRepo.UpdateMovieAsync(ownerId, entity, ct);
    }

    public async Task<Result> UpdateTvSeriesAsync(
        Guid ownerId,
        Guid id,
        TvSeriesEntryUpdateDto updateDto,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(UpdateTvSeriesAsync), OperationType.Update);
        var validationErrors = ValidateUpdate(ownerId, id, updateDto, errorContext);
        if (validationErrors.Count > 0)
            return LogValidationFailure(validationErrors, nameof(UpdateTvSeriesAsync), errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult;

        var entity = MediaEntryMapping.ToTvSeries(id, updateDto);
        entity.OwnerId = ownerId;
        return await _mediaEntryRepo.UpdateTvSeriesAsync(ownerId, entity, ct);
    }

    public async Task<Result> UpdateGameAsync(
        Guid ownerId,
        Guid id,
        GameEntryUpdateDto updateDto,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(UpdateGameAsync), OperationType.Update);
        var validationErrors = ValidateUpdate(ownerId, id, updateDto, errorContext);
        if (validationErrors.Count > 0)
            return LogValidationFailure(validationErrors, nameof(UpdateGameAsync), errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult;

        var entity = MediaEntryMapping.ToGame(id, updateDto);
        entity.OwnerId = ownerId;
        return await _mediaEntryRepo.UpdateGameAsync(ownerId, entity, ct);
    }

    public async Task<Result> UpdateBookAsync(
        Guid ownerId,
        Guid id,
        BookEntryUpdateDto updateDto,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(UpdateBookAsync), OperationType.Update);
        var validationErrors = ValidateUpdate(ownerId, id, updateDto, errorContext);
        if (validationErrors.Count > 0)
            return LogValidationFailure(validationErrors, nameof(UpdateBookAsync), errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult;

        var entity = MediaEntryMapping.ToBook(id, updateDto);
        entity.OwnerId = ownerId;
        return await _mediaEntryRepo.UpdateBookAsync(ownerId, entity, ct);
    }

    public async Task<Result> UpdateMangaAsync(
        Guid ownerId,
        Guid id,
        MangaEntryUpdateDto updateDto,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(UpdateMangaAsync), OperationType.Update);
        var validationErrors = ValidateUpdate(ownerId, id, updateDto, errorContext);
        if (validationErrors.Count > 0)
            return LogValidationFailure(validationErrors, nameof(UpdateMangaAsync), errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult;

        var entity = MediaEntryMapping.ToManga(id, updateDto);
        entity.OwnerId = ownerId;
        return await _mediaEntryRepo.UpdateMangaAsync(ownerId, entity, ct);
    }

    public async Task<Result> DeleteAsync(
        Guid ownerId,
        Guid id,
        int expectedVersion,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);
        var validationErrors = new List<ValidationError>();
        validationErrors.AddRange(ValidateIds(ownerId, id, errorContext));

        if (expectedVersion < 1)
        {
            validationErrors.Add(MediaVaultValidationError.OutOfRange(
                errorContext with { FieldName = nameof(expectedVersion) },
                $"1 to {int.MaxValue}"));
        }

        if (validationErrors.Count > 0)
            return LogValidationFailure(validationErrors, nameof(DeleteAsync), errorContext);

        var ownerResult = await EnsureOwnerExistsAsync(ownerId, ct);
        if (ownerResult.IsFailure)
            return ownerResult;

        return await _mediaEntryRepo.DeleteAsync(ownerId, id, expectedVersion, ct);
    }

    private List<ValidationError> ValidateCreate(
        Guid ownerId,
        MediaEntryCreateDto createDto,
        ErrorContext errorContext)
    {
        var validationErrors = ValidateOwnerId(ownerId, errorContext);
        if (!_dtoValidator.IsValidCreateDto(createDto!, errorContext, out var dtoErrors))
            validationErrors.AddRange(dtoErrors);

        return validationErrors;
    }

    private List<ValidationError> ValidateUpdate(
        Guid ownerId,
        Guid id,
        MediaEntryUpdateDto updateDto,
        ErrorContext errorContext)
    {
        var validationErrors = ValidateIds(ownerId, id, errorContext);
        if (!_dtoValidator.IsValidUpdateDto(updateDto!, errorContext, out var dtoErrors))
            validationErrors.AddRange(dtoErrors);

        return validationErrors;
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

    private Result LogValidationFailure(
        IReadOnlyList<ValidationError> validationErrors,
        string methodName,
        ErrorContext errorContext)
    {
        ServiceValidationLogging.LogValidationFailure(
            _logger,
            validationErrors,
            GetType().Name,
            methodName,
            errorContext);

        return Result.ValidationFailure(
            validationErrors,
            MediaVaultResultMessages.ValidationFailure);
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
