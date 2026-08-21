using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Infrastructure.Timestamps;

namespace media_vault_app.Infrastructure.Repos
{
    using media_vault_app.Application.DTOs.MediaEntry.Response;
    public class MediaEntryRepo : DependentEntityRepoBase<MediaEntry, Guid, Guid>, IMediaEntryRepo
    {
        public MediaEntryRepo(
            AppDbContext appDbContext,
            ErrorEventLogger<DependentEntityRepoBase<MediaEntry, Guid, Guid>> errorEventLogger,
            ServerTimestampPolicy? timestampPolicy = null)
            : base(appDbContext, errorEventLogger, timestampPolicy)
        {
        }

        public override Task<Result<MediaEntry>> CreateAsync(MediaEntry entity, CancellationToken ct = default)
        {
            if (entity is TvSeriesEntry tvSeries)
            {
                foreach (var season in tvSeries.Seasons)
                    _timestampPolicy.Initialize(season);
            }

            return base.CreateAsync(entity, ct);
        }

        // Override to include Seasons for TvSeries via eager loading.
        // PcRequirements is an EF complex property and is materialized with the owning row.
        public override Task<Result<MediaEntry>> GetByIdAsync(
            Guid ownerId,
            Guid entityId,
            Func<IQueryable<MediaEntry>, IQueryable<MediaEntry>>? include = null,
            CancellationToken ct = default)
        {
            return base.GetByIdAsync(
                ownerId,
                entityId,
                query =>
                {
                    var shapedQuery = include is null ? query : include(query);
                    return shapedQuery.Include(e => (e as TvSeriesEntry)!.Seasons);
                },
                ct);
        }


        // Override to update a GameEntry in-place, including its PcRequirements value object.
        // The base SetValues() only copies scalars and silently ignores PcRequirements.
        private async Task<Result> UpdateGameAsync(
            Guid ownerId,
            GameEntry updatedGame,
            int expectedVersion,
            CancellationToken ct)
        {
            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            try
            {
                var existing = await _appDbContext.GameEntries
                    .FirstOrDefaultAsync(g => g.Id == updatedGame.Id && g.OwnerId == ownerId, ct)
                    .ConfigureAwait(false);

                if (existing is null)
                    return Result.Failure(MediaVaultErrors.NotFound(baseErrorContext));

                if (existing.Version != expectedVersion)
                    return LogAndFail(
                        DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext),
                        baseErrorContext,
                        nameof(UpdateAsync));

                var createdAt = existing.CreatedAtUtc;
                var updatedAt = existing.UpdatedAtUtc;
                ApplyGameProperties(existing, updatedGame);
                var hasMeaningfulChanges = ApplyUpdateTimestamp(existing, createdAt, updatedAt);
                ApplyConcurrencyVersion(existing, expectedVersion, hasMeaningfulChanges);

                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return LogAndFail(
                    DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext, ex),
                    baseErrorContext,
                    nameof(UpdateAsync));
            }
            catch (DbUpdateException ex)
            {
                return LogAndFail(
                    DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, ex),
                    baseErrorContext,
                    nameof(UpdateAsync));
            }
            catch (System.Data.Common.DbException ex)
            {
                return LogAndFail(
                    DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex),
                    baseErrorContext,
                    nameof(UpdateAsync));
            }
        }

        // Applies all scalar + PcRequirements properties from the incoming GameEntry.
        private static void ApplyGameProperties(GameEntry existing, GameEntry updated)
        {
            // Base MediaEntry properties
            existing.IdExternal = updated.IdExternal;
            existing.Title = updated.Title;
            existing.Status = updated.Status;
            existing.Rating = updated.Rating;
            existing.Review = updated.Review;
            existing.Overview = updated.Overview;
            existing.Genres = updated.Genres;
            existing.ReleaseDate = updated.ReleaseDate;
            existing.ImageUrl = updated.ImageUrl;
            // Game-specific scalar properties
            existing.MetacriticRating = updated.MetacriticRating;
            existing.Website = updated.Website;
            existing.Platforms = updated.Platforms;
            existing.HoursPlayed = updated.HoursPlayed;

            // PcRequirements is a value object — replace the whole value.
            existing.PcRequirements = updated.PcRequirements;
        }

        // Override to update a TvSeriesEntry in-place, including merging its Seasons.
        public override async Task<Result> UpdateAsync(
            Guid ownerId,
            MediaEntry updatedEntity,
            CancellationToken ct = default)
        {
            var expectedVersion = updatedEntity.Version;

            if (updatedEntity is GameEntry updatedGame)
                return await UpdateGameAsync(ownerId, updatedGame, expectedVersion, ct);

            if (updatedEntity is not TvSeriesEntry updatedTvSeries)
                return await base.UpdateAsync(ownerId, updatedEntity, ct);

            var baseErrorContext = DefineErrorContext(nameof(UpdateAsync), OperationType.Update);

            try
            {
                var existing = await _appDbContext.TvSeriesEntries
                    .Include(tv => tv.Seasons)
                    .FirstOrDefaultAsync(tv => tv.Id == updatedTvSeries.Id && tv.OwnerId == ownerId, ct)
                    .ConfigureAwait(false);

                if (existing is null)
                    return Result.Failure(MediaVaultErrors.NotFound(baseErrorContext));

                if (existing.Version != expectedVersion)
                    return LogAndFail(
                        DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext),
                        baseErrorContext);

                var createdAt = existing.CreatedAtUtc;
                var updatedAt = existing.UpdatedAtUtc;
                ApplyTvSeriesProperties(existing, updatedTvSeries);
                var seasonsChanged = MergeSeasons(existing, updatedTvSeries.Seasons);
                var hasMeaningfulChanges = ApplyUpdateTimestamp(
                    existing,
                    createdAt,
                    updatedAt,
                    seasonsChanged);
                ApplyConcurrencyVersion(existing, expectedVersion, hasMeaningfulChanges);

                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return LogAndFail(
                    DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext, ex),
                    baseErrorContext);
            }
            catch (DbUpdateException ex)
            {
                return LogAndFail(
                    DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, ex),
                    baseErrorContext);
            }
            catch (System.Data.Common.DbException ex)
            {
                return LogAndFail(
                    DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex),
                    baseErrorContext);
            }
        }

        // Applies all scalar properties from the incoming TvSeriesEntry onto the
        // tracked entity. CreatedAtUtc is intentionally left unchanged.
        private static void ApplyTvSeriesProperties(TvSeriesEntry existing, TvSeriesEntry updated)
        {
            // Base MediaEntry properties
            existing.IdExternal = updated.IdExternal;
            existing.Title = updated.Title;
            existing.Status = updated.Status;
            existing.Rating = updated.Rating;
            existing.Review = updated.Review;
            existing.Overview = updated.Overview;
            existing.Genres = updated.Genres;
            existing.ReleaseDate = updated.ReleaseDate;
            existing.ImageUrl = updated.ImageUrl;
            // TvSeries-specific properties
            existing.BackdropImageUrl = updated.BackdropImageUrl;
            existing.LastAirDate = updated.LastAirDate;
            existing.NumberOfSeasons = updated.NumberOfSeasons;
            existing.NumberOfEpisodes = updated.NumberOfEpisodes;
            existing.AiringStatus = updated.AiringStatus;
            existing.TotalWatchedEpisodes = updated.TotalWatchedEpisodes;
        }

        // Merges the incoming seasons into the tracked collection:
        //   - Seasons matched by SeasonNumber have their properties updated.
        //   - Seasons present in the update but not in existing are added.
        //   - Seasons in existing but absent from the update are removed.
        private bool MergeSeasons(TvSeriesEntry existing, ICollection<Season> updatedSeasons)
        {
            var changes = false;

            // Remove seasons that are no longer in the updated list.
            var toRemove = existing.Seasons
                .Where(e => !updatedSeasons.Any(u => u.SeasonNumber == e.SeasonNumber))
                .ToList();

            foreach (var season in toRemove)
            {
                existing.Seasons.Remove(season);
                changes = true;
            }

            foreach (var updated in updatedSeasons)
            {
                var match = existing.Seasons.FirstOrDefault(e => e.SeasonNumber == updated.SeasonNumber);

                if (match is not null)
                {
                    // Update the existing tracked season in-place.
                    changes |= ApplySeasonProperties(match, updated);
                }
                else
                {
                    // New season — assign the correct owner and add it.
                    updated.TvSeriesEntryId = existing.Id;
                    updated.Id = Guid.NewGuid();
                    _timestampPolicy.Initialize(updated);
                    existing.Seasons.Add(updated);
                    changes = true;
                }
            }

            return changes;
        }

        // Applies all scalar properties from the incoming Season onto the tracked one.
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

        public async Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> GetMinimalCollectionByOwnerIdAsync(
            Guid ownerId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            try
            {
                var minimalEntries = await _dbSet
                    .AsNoTracking()
                    .Where(mediaEntry => mediaEntry.OwnerId == ownerId)
                    .OrderByDescending(mediaEntry => mediaEntry.CreatedAtUtc)
                    .ThenBy(mediaEntry => mediaEntry.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ToMinimalDtoProjection)
                    .ToListAsync(ct)
                    .ConfigureAwait(false);

                return Result<IReadOnlyList<MediaEntryMinimalDto>>.Success(minimalEntries);
            }
            catch (System.Data.Common.DbException ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(GetMinimalCollectionByOwnerIdAsync), OperationType.GetCollection);
                return LogAndFail<IReadOnlyList<MediaEntryMinimalDto>>(
                    DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex),
                    baseErrorContext);
            }
        }

        public async Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> SearchMediaEntriesAsync(Guid ownerId, string query, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            try
            {
                var minimalEntries = await _dbSet
                    .AsNoTracking()
                    .Where(mediaEntry => mediaEntry.OwnerId == ownerId && mediaEntry.Title.Contains(query))
                    .OrderByDescending(mediaEntry => mediaEntry.CreatedAtUtc)
                    .ThenBy(mediaEntry => mediaEntry.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ToMinimalDtoProjection)
                    .ToListAsync(ct).ConfigureAwait(false);

                return Result<IReadOnlyList<MediaEntryMinimalDto>>.Success(minimalEntries);
            }
            catch (System.Data.Common.DbException ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(SearchMediaEntriesAsync), OperationType.GetCollection);
                return LogAndFail<IReadOnlyList<MediaEntryMinimalDto>>(
                    DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex),
                    baseErrorContext);
            }
        }

        private static readonly System.Linq.Expressions.Expression<Func<MediaEntry, MediaEntryMinimalDto>> ToMinimalDtoProjection =
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
    }
}
