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

        public async Task<Result<IReadOnlyList<MediaEntry>>> SearchMediaEntriesAsync(Guid ownerId, string query, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            try
            {
                var mediaEntries = await _dbSet
                    .AsNoTracking()
                    .Where(mediaEntry => mediaEntry.UserId == ownerId && mediaEntry.Title.Contains(query))
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
