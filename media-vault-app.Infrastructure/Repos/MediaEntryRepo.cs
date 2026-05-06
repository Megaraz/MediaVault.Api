using System;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{
    public class MediaEntryRepo : DependentEntityRepoBase<MediaEntry, Guid, Guid>, IMediaEntryRepo
    {
        public MediaEntryRepo(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        // Override to include Seasons when the entry is a TvSeriesEntry.
        public override async Task<Result<MediaEntry>> GetByIdAsync(Guid ownerId, Guid entityId, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            try
            {
                // Try TvSeries first so seasons are eagerly loaded.
                var tvSeries = await _appDbContext.TvSeriesEntries
                    .Include(tv => tv.Seasons)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(tv => tv.Id == entityId && tv.OwnerId == ownerId, ct)
                    .ConfigureAwait(false);

                if (tvSeries is not null)
                    return Result<MediaEntry>.Success(tvSeries);

                // Fall back to the generic base query for other media types.
                return await base.GetByIdAsync(ownerId, entityId, ct);
            }
            catch (OperationCanceledException)
            {
                return Result<MediaEntry>.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return Result<MediaEntry>.Failure(DatabaseError.GetFailure(baseErrorContext, ex));
            }
        }

        // Override to update a TvSeriesEntry in-place, including merging its Seasons.
        public override async Task<Result> UpdateAsync(Guid ownerId, MediaEntry updatedEntity, CancellationToken ct = default)
        {
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
                    return Result.Failure(Error.NotFound(baseErrorContext));

                ApplyTvSeriesProperties(existing, updatedTvSeries);
                MergeSeasons(existing, updatedTvSeries.Seasons);

                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Result.Failure(DatabaseError.ConcurrencyFailure(baseErrorContext, ex));
            }
            catch (DbUpdateException ex)
            {
                return Result.Failure(DatabaseError.UpdateFailure(baseErrorContext, ex));
            }
            catch (OperationCanceledException)
            {
                return Result.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return Result.Failure(DatabaseError.UpdateFailure(baseErrorContext, ex));
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
            existing.UpdatedAtUtc = DateTime.UtcNow;

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
        private static void MergeSeasons(TvSeriesEntry existing, ICollection<Season> updatedSeasons)
        {
            // Remove seasons that are no longer in the updated list.
            var toRemove = existing.Seasons
                .Where(e => !updatedSeasons.Any(u => u.SeasonNumber == e.SeasonNumber))
                .ToList();

            foreach (var season in toRemove)
                existing.Seasons.Remove(season);

            foreach (var updated in updatedSeasons)
            {
                var match = existing.Seasons.FirstOrDefault(e => e.SeasonNumber == updated.SeasonNumber);

                if (match is not null)
                {
                    // Update the existing tracked season in-place.
                    ApplySeasonProperties(match, updated);
                }
                else
                {
                    // New season — assign the correct owner and add it.
                    updated.OwnerId = existing.Id;
                    updated.Id = Guid.NewGuid();
                    updated.CreatedAtUtc = DateTime.UtcNow;
                    updated.UpdatedAtUtc = DateTime.UtcNow;
                    existing.Seasons.Add(updated);
                }
            }
        }

        // Applies all scalar properties from the incoming Season onto the tracked one.
        private static void ApplySeasonProperties(Season existing, Season updated)
        {
            existing.IdExternal = updated.IdExternal;
            existing.Name = updated.Name;
            existing.Overview = updated.Overview;
            existing.ImageUrl = updated.ImageUrl;
            existing.AirDate = updated.AirDate;
            existing.Episodes = updated.Episodes;
            existing.WatchedEpisodes = updated.WatchedEpisodes;
            existing.Status = updated.Status;
            existing.Rating = updated.Rating;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        public async Task<Result<IReadOnlyList<MediaEntry>>> SearchMediaEntriesAsync(Guid ownerId, string query, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            try
            {
                var mediaEntries = await _dbSet
                    .AsNoTracking()
                    .Where(mediaEntry => mediaEntry.OwnerId == ownerId && mediaEntry.Title.Contains(query))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct).ConfigureAwait(false);

                return Result<IReadOnlyList<MediaEntry>>.Success(mediaEntries);
            }
            catch (OperationCanceledException)
            {
                var baseErrorContext = DefineErrorContext(nameof(SearchMediaEntriesAsync), OperationType.GetCollection);
                return Result<IReadOnlyList<MediaEntry>>.Failure(DatabaseError.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(SearchMediaEntriesAsync), OperationType.GetCollection);
                return Result<IReadOnlyList<MediaEntry>>.Failure(DatabaseError.GetCollectionFailure(baseErrorContext, ex));
            }
        }
    }
}
