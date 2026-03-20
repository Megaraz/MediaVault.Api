using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;

namespace media_vault_app.Infrastructure.Repos
{
    public class MediaEntryRepo : GenericRepoEFCore<MediaEntry, Guid>, IMediaEntryRepo
    {
        public MediaEntryRepo(AppDbContext appDbContext) : base(appDbContext)
        {
        }
    }
}
