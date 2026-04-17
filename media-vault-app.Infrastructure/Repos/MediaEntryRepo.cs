using System;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{
    public class MediaEntryRepo : OwnedEntityGenericRepoBase<User, Guid, MediaEntry, Guid>, IMediaEntryRepo
    {
        public MediaEntryRepo(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<Result<IReadOnlyList<MediaEntry>>> SearchMediaEntriesAsync(Guid ownerId, string query, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {

            var baseErrorContext = DefineErrorContext(nameof(SearchMediaEntriesAsync), OperationType.GetCollection);

            try
            {
                var mediaEntries = await _dbSet
                    .AsNoTracking()
                    .Where(mediaEntry => mediaEntry.OwnerId == ownerId && !string.IsNullOrWhiteSpace(mediaEntry.Title) && mediaEntry.Title.Contains(query))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                return Result<IReadOnlyList<MediaEntry>>.Success(mediaEntries);
            }
            catch (Exception ex)
            {
                var dbExceptionErrorContext = baseErrorContext with
                {
                    DescriptionSuffix = $"An error occurred while searching for MediaEntries with query '{query}'."
                };

                return Result<IReadOnlyList<MediaEntry>>.Failure(
                    Error.DbGetCollectionFailure(dbExceptionErrorContext, ex),
                    dbExceptionErrorContext.DescriptionSuffix);
            }
        }

    }
}
