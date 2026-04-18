using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Domain.Entities
{
    public sealed record TvSeriesEntry : MediaEntry, IOwnerEntity<TvSeriesEntry, Guid>
    {
        public ICollection<Season>? Seasons { get; set; }
        public int TotalEpisodes { get; set; }
        public int TotalWatchedEpisodes { get; set; }


    }
}
