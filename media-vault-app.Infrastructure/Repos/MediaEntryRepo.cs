using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Data.Common;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Infrastructure.Timestamps;
using Microsoft.EntityFrameworkCore;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Infrastructure.Repos;

public sealed class MediaEntryRepo : IMediaEntryRepo
{
    private static readonly Expression<Func<MediaEntry, MediaEntryMinimalDto>> MinimalProjection =
        mediaEntry => new MediaEntryMinimalDto
        {
            Id = mediaEntry.Id,
            Title = mediaEntry.Title,
            ImageUrl = mediaEntry.ImageUrl,
            Rating = mediaEntry.Rating,
            ReleaseDate = mediaEntry.ReleaseDate ?? DateOnly.MinValue,
            Genres = mediaEntry.Genres,
            MediaType = mediaEntry.MediaType,
            Status = mediaEntry.Status,
            CreatedAtUtc = mediaEntry.CreatedAtUtc,
            UpdatedAtUtc = mediaEntry.UpdatedAtUtc,
            Version = mediaEntry.Version
        };

    private readonly AppDbContext _appDbContext;
    private readonly DbSet<MediaEntry> _mediaEntries;
    private readonly ErrorEventLogger<MediaEntryRepo> _errorEventLogger;
    private readonly ServerTimestampPolicy _timestampPolicy;

    public MediaEntryRepo(
        AppDbContext appDbContext,
        ErrorEventLogger<MediaEntryRepo> errorEventLogger,
        ServerTimestampPolicy? timestampPolicy = null)
    {
        _appDbContext = appDbContext;
        _mediaEntries = appDbContext.MediaEntries;
        _errorEventLogger = errorEventLogger;
        _timestampPolicy = timestampPolicy ?? new ServerTimestampPolicy(TimeProvider.System);
    }

    public async Task<Result<MediaEntry>> GetDetailedByIdAsync(
        Guid ownerId,
        Guid id,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(GetDetailedByIdAsync), OperationType.Get);

        try
        {
            var entity = await _mediaEntries
                .AsNoTracking()
                .Include(mediaEntry => (mediaEntry as TvSeriesEntry)!.Seasons)
                .FirstOrDefaultAsync(
                    mediaEntry => mediaEntry.Id == id && mediaEntry.OwnerId == ownerId,
                    ct)
                .ConfigureAwait(false);

            return entity is null
                ? Result<MediaEntry>.Failure(MediaVaultErrors.NotFound(errorContext))
                : Result<MediaEntry>.Success(entity);
        }
        catch (DbException ex)
        {
            return LogAndFail<MediaEntry>(
                DatabaseFailurePolicy.QueryFailure(errorContext, ex),
                errorContext);
        }
    }

    public async Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> GetMinimalCollectionByOwnerIdAsync(
        Guid ownerId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(
            nameof(GetMinimalCollectionByOwnerIdAsync),
            OperationType.GetCollection);

        try
        {
            var entries = await _mediaEntries
                .AsNoTracking()
                .Where(mediaEntry => mediaEntry.OwnerId == ownerId)
                .OrderByDescending(mediaEntry => mediaEntry.CreatedAtUtc)
                .ThenBy(mediaEntry => mediaEntry.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MinimalProjection)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return Result<IReadOnlyList<MediaEntryMinimalDto>>.Success(entries);
        }
        catch (DbException ex)
        {
            return LogAndFail<IReadOnlyList<MediaEntryMinimalDto>>(
                DatabaseFailurePolicy.QueryFailure(errorContext, ex),
                errorContext);
        }
    }

    public async Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> SearchMediaEntriesAsync(
        Guid ownerId,
        string query,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(SearchMediaEntriesAsync), OperationType.GetCollection);

        try
        {
            var entries = await _mediaEntries
                .AsNoTracking()
                .Where(mediaEntry => mediaEntry.OwnerId == ownerId && mediaEntry.Title.Contains(query))
                .OrderByDescending(mediaEntry => mediaEntry.CreatedAtUtc)
                .ThenBy(mediaEntry => mediaEntry.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MinimalProjection)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return Result<IReadOnlyList<MediaEntryMinimalDto>>.Success(entries);
        }
        catch (DbException ex)
        {
            return LogAndFail<IReadOnlyList<MediaEntryMinimalDto>>(
                DatabaseFailurePolicy.QueryFailure(errorContext, ex),
                errorContext);
        }
    }

    public async Task<Result<MediaEntry>> CreateAsync(
        MediaEntry entity,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create);

        try
        {
            _timestampPolicy.Initialize(entity);
            if (entity is TvSeriesEntry tvSeries)
            {
                foreach (var season in tvSeries.Seasons)
                    _timestampPolicy.Initialize(season);
            }

            _mediaEntries.Add(entity);
            await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result<MediaEntry>.Success(entity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return LogAndFail<MediaEntry>(
                DatabaseFailurePolicy.ConcurrencyFailure(errorContext, ex),
                errorContext);
        }
        catch (DbUpdateException ex)
        {
            return LogAndFail<MediaEntry>(
                DatabaseFailurePolicy.SaveChangesFailure(errorContext, ex),
                errorContext);
        }
    }

    public async Task<Result> UpdateMovieAsync(
        Guid ownerId,
        MovieEntry entity,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(UpdateMovieAsync), OperationType.Update);

        try
        {
            var existing = await _appDbContext.MovieEntries
                .FirstOrDefaultAsync(
                    movie => movie.Id == entity.Id && movie.OwnerId == ownerId,
                    ct)
                .ConfigureAwait(false);

            if (existing is null)
                return Result.Failure(MediaVaultErrors.NotFound(errorContext));

            if (existing.Version != entity.Version)
                return LogAndFail(DatabaseFailurePolicy.ConcurrencyFailure(errorContext), errorContext);

            var createdAt = existing.CreatedAtUtc;
            var updatedAt = existing.UpdatedAtUtc;
            ApplyMediaProperties(existing, entity);
            existing.RuntimeMinutes = entity.RuntimeMinutes;
            var hasMeaningfulChanges = ApplyUpdateTimestamp(existing, createdAt, updatedAt);
            ApplyConcurrencyVersion(existing, entity.Version, hasMeaningfulChanges);

            await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.ConcurrencyFailure(errorContext, ex),
                errorContext);
        }
        catch (DbUpdateException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.SaveChangesFailure(errorContext, ex),
                errorContext);
        }
        catch (DbException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.QueryFailure(errorContext, ex),
                errorContext);
        }
    }

    public async Task<Result> UpdateTvSeriesAsync(
        Guid ownerId,
        TvSeriesEntry entity,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(UpdateTvSeriesAsync), OperationType.Update);

        try
        {
            var existing = await _appDbContext.TvSeriesEntries
                .Include(tvSeries => tvSeries.Seasons)
                .FirstOrDefaultAsync(
                    tvSeries => tvSeries.Id == entity.Id && tvSeries.OwnerId == ownerId,
                    ct)
                .ConfigureAwait(false);

            if (existing is null)
                return Result.Failure(MediaVaultErrors.NotFound(errorContext));

            if (existing.Version != entity.Version)
                return LogAndFail(DatabaseFailurePolicy.ConcurrencyFailure(errorContext), errorContext);

            var createdAt = existing.CreatedAtUtc;
            var updatedAt = existing.UpdatedAtUtc;
            ApplyMediaProperties(existing, entity);
            existing.BackdropImageUrl = entity.BackdropImageUrl;
            existing.LastAirDate = entity.LastAirDate;
            existing.NumberOfSeasons = entity.NumberOfSeasons;
            existing.NumberOfEpisodes = entity.NumberOfEpisodes;
            existing.AiringStatus = entity.AiringStatus;
            existing.TotalWatchedEpisodes = entity.TotalWatchedEpisodes;
            var seasonsChanged = MergeSeasons(existing, entity.Seasons);
            var hasMeaningfulChanges = ApplyUpdateTimestamp(
                existing,
                createdAt,
                updatedAt,
                seasonsChanged);
            ApplyConcurrencyVersion(existing, entity.Version, hasMeaningfulChanges);

            await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.ConcurrencyFailure(errorContext, ex),
                errorContext);
        }
        catch (DbUpdateException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.SaveChangesFailure(errorContext, ex),
                errorContext);
        }
        catch (DbException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.QueryFailure(errorContext, ex),
                errorContext);
        }
    }

    public async Task<Result> UpdateGameAsync(
        Guid ownerId,
        GameEntry entity,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(UpdateGameAsync), OperationType.Update);

        try
        {
            var existing = await _appDbContext.GameEntries
                .FirstOrDefaultAsync(
                    game => game.Id == entity.Id && game.OwnerId == ownerId,
                    ct)
                .ConfigureAwait(false);

            if (existing is null)
                return Result.Failure(MediaVaultErrors.NotFound(errorContext));

            if (existing.Version != entity.Version)
                return LogAndFail(DatabaseFailurePolicy.ConcurrencyFailure(errorContext), errorContext);

            var createdAt = existing.CreatedAtUtc;
            var updatedAt = existing.UpdatedAtUtc;
            ApplyMediaProperties(existing, entity);
            existing.HoursPlayed = entity.HoursPlayed;
            existing.MetacriticRating = entity.MetacriticRating;
            existing.Platforms = entity.Platforms;
            existing.Website = entity.Website;
            existing.PcRequirements = entity.PcRequirements;
            var hasMeaningfulChanges = ApplyUpdateTimestamp(existing, createdAt, updatedAt);
            ApplyConcurrencyVersion(existing, entity.Version, hasMeaningfulChanges);

            await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.ConcurrencyFailure(errorContext, ex),
                errorContext);
        }
        catch (DbUpdateException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.SaveChangesFailure(errorContext, ex),
                errorContext);
        }
        catch (DbException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.QueryFailure(errorContext, ex),
                errorContext);
        }
    }

    public async Task<Result> UpdateBookAsync(
        Guid ownerId,
        BookEntry entity,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(UpdateBookAsync), OperationType.Update);

        try
        {
            var existing = await _appDbContext.BookEntries
                .FirstOrDefaultAsync(
                    book => book.Id == entity.Id && book.OwnerId == ownerId,
                    ct)
                .ConfigureAwait(false);

            if (existing is null)
                return Result.Failure(MediaVaultErrors.NotFound(errorContext));

            if (existing.Version != entity.Version)
                return LogAndFail(DatabaseFailurePolicy.ConcurrencyFailure(errorContext), errorContext);

            var createdAt = existing.CreatedAtUtc;
            var updatedAt = existing.UpdatedAtUtc;
            ApplyMediaProperties(existing, entity);
            existing.Author = entity.Author;
            var hasMeaningfulChanges = ApplyUpdateTimestamp(existing, createdAt, updatedAt);
            ApplyConcurrencyVersion(existing, entity.Version, hasMeaningfulChanges);

            await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.ConcurrencyFailure(errorContext, ex),
                errorContext);
        }
        catch (DbUpdateException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.SaveChangesFailure(errorContext, ex),
                errorContext);
        }
        catch (DbException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.QueryFailure(errorContext, ex),
                errorContext);
        }
    }

    public async Task<Result> UpdateMangaAsync(
        Guid ownerId,
        MangaEntry entity,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(UpdateMangaAsync), OperationType.Update);

        try
        {
            var existing = await _appDbContext.MangaEntries
                .FirstOrDefaultAsync(
                    manga => manga.Id == entity.Id && manga.OwnerId == ownerId,
                    ct)
                .ConfigureAwait(false);

            if (existing is null)
                return Result.Failure(MediaVaultErrors.NotFound(errorContext));

            if (existing.Version != entity.Version)
                return LogAndFail(DatabaseFailurePolicy.ConcurrencyFailure(errorContext), errorContext);

            var createdAt = existing.CreatedAtUtc;
            var updatedAt = existing.UpdatedAtUtc;
            ApplyMediaProperties(existing, entity);
            existing.Author = entity.Author;
            var hasMeaningfulChanges = ApplyUpdateTimestamp(existing, createdAt, updatedAt);
            ApplyConcurrencyVersion(existing, entity.Version, hasMeaningfulChanges);

            await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.ConcurrencyFailure(errorContext, ex),
                errorContext);
        }
        catch (DbUpdateException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.SaveChangesFailure(errorContext, ex),
                errorContext);
        }
        catch (DbException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.QueryFailure(errorContext, ex),
                errorContext);
        }
    }

    public async Task<Result> DeleteAsync(
        Guid ownerId,
        Guid id,
        int expectedVersion,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(DeleteAsync), OperationType.Delete);

        try
        {
            var entity = await _mediaEntries
                .FirstOrDefaultAsync(
                    mediaEntry => mediaEntry.Id == id && mediaEntry.OwnerId == ownerId,
                    ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Failure(MediaVaultErrors.NotFound(errorContext));

            if (entity.Version != expectedVersion)
                return LogAndFail(DatabaseFailurePolicy.ConcurrencyFailure(errorContext), errorContext);

            _appDbContext.Entry(entity)
                .Property(nameof(IConcurrencyVersion.Version))
                .OriginalValue = expectedVersion;
            _mediaEntries.Remove(entity);
            await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.ConcurrencyFailure(errorContext, ex),
                errorContext);
        }
        catch (DbUpdateException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.SaveChangesFailure(errorContext, ex),
                errorContext);
        }
        catch (DbException ex)
        {
            return LogAndFail(
                DatabaseFailurePolicy.QueryFailure(errorContext, ex),
                errorContext);
        }
    }

    private static void ApplyMediaProperties(MediaEntry existing, MediaEntry updated)
    {
        existing.IdExternal = updated.IdExternal;
        existing.Title = updated.Title;
        existing.Status = updated.Status;
        existing.Rating = updated.Rating;
        existing.Review = updated.Review;
        existing.Overview = updated.Overview;
        existing.Genres = updated.Genres;
        existing.ReleaseDate = updated.ReleaseDate;
        existing.ImageUrl = updated.ImageUrl;
    }

    private bool MergeSeasons(TvSeriesEntry existing, ICollection<Season> updatedSeasons)
    {
        var changes = false;
        var toRemove = existing.Seasons
            .Where(current => !updatedSeasons.Any(updated => updated.SeasonNumber == current.SeasonNumber))
            .ToList();

        foreach (var season in toRemove)
        {
            existing.Seasons.Remove(season);
            changes = true;
        }

        foreach (var updated in updatedSeasons)
        {
            var match = existing.Seasons.FirstOrDefault(
                current => current.SeasonNumber == updated.SeasonNumber);

            if (match is not null)
            {
                changes |= ApplySeasonProperties(match, updated);
                continue;
            }

            updated.TvSeriesEntryId = existing.Id;
            updated.Id = Guid.NewGuid();
            _timestampPolicy.Initialize(updated);
            existing.Seasons.Add(updated);
            changes = true;
        }

        return changes;
    }

    private bool ApplySeasonProperties(Season existing, Season updated)
    {
        var createdAt = existing.CreatedAtUtc;
        var updatedAt = existing.UpdatedAtUtc;
        existing.IdExternal = updated.IdExternal;
        existing.Name = updated.Name;
        existing.Overview = updated.Overview;
        existing.ImageUrl = updated.ImageUrl;
        existing.AirDate = updated.AirDate;
        existing.Episodes = updated.Episodes;
        existing.WatchedEpisodes = updated.WatchedEpisodes;
        existing.Status = updated.Status;
        existing.Rating = updated.Rating;
        return ApplyUpdateTimestamp(existing, createdAt, updatedAt);
    }

    private bool ApplyUpdateTimestamp(
        IEntity<Guid> entity,
        DateTime originalCreatedAtUtc,
        DateTime originalUpdatedAtUtc,
        bool relatedEntityChanged = false)
    {
        _appDbContext.ChangeTracker.DetectChanges();
        var entry = _appDbContext.Entry(entity);
        var hasMeaningfulChanges = relatedEntityChanged || entry.Properties.Any(
            property => property.IsModified &&
                property.Metadata.Name is not nameof(ICreatedAtUtc.CreatedAtUtc) and
                not nameof(IUpdatedAtUtc.UpdatedAtUtc));

        _timestampPolicy.ApplyUpdate(
            entity,
            originalCreatedAtUtc,
            originalUpdatedAtUtc,
            hasMeaningfulChanges);

        entry.Property(nameof(ICreatedAtUtc.CreatedAtUtc)).IsModified = false;
        entry.Property(nameof(IUpdatedAtUtc.UpdatedAtUtc)).IsModified = hasMeaningfulChanges;
        return hasMeaningfulChanges;
    }

    private void ApplyConcurrencyVersion(
        MediaEntry entity,
        int expectedVersion,
        bool hasMeaningfulChanges)
    {
        var versionProperty = _appDbContext.Entry(entity)
            .Property(nameof(IConcurrencyVersion.Version));
        versionProperty.OriginalValue = expectedVersion;
        versionProperty.CurrentValue = hasMeaningfulChanges
            ? checked(expectedVersion + 1)
            : expectedVersion;
        versionProperty.IsModified = hasMeaningfulChanges;
    }

    private Result LogAndFail(
        Error error,
        ErrorContext errorContext,
        [CallerMemberName] string methodName = "")
    {
        _errorEventLogger.Log(
            error,
            new ErrorEventContext("Infrastructure", GetType().Name, methodName, errorContext));
        return Result.Failure(error);
    }

    private Result<T> LogAndFail<T>(
        Error error,
        ErrorContext errorContext,
        [CallerMemberName] string methodName = "")
        where T : notnull
    {
        _errorEventLogger.Log(
            error,
            new ErrorEventContext("Infrastructure", GetType().Name, methodName, errorContext));
        return Result<T>.Failure(error);
    }

    private static ErrorContext DefineErrorContext(
        string methodName,
        OperationType operation,
        string? fieldName = null) =>
        new(operation: operation, entityName: nameof(MediaEntry), fieldName: fieldName);
}
