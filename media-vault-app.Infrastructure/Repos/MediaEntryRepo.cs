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

        // Override to replace the Seasons collection when updating a TvSeriesEntry.
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

                var createdAt = existing.CreatedAtUtc;
                _appDbContext.Entry(existing).CurrentValues.SetValues(updatedTvSeries);
                existing.CreatedAtUtc = createdAt;

                // Replace seasons: clear existing (EF will delete via cascade) then add new ones.
                existing.Seasons.Clear();
                foreach (var season in updatedTvSeries.Seasons)
                {
                    season.OwnerId = existing.Id;
                    existing.Seasons.Add(season);
                }

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
