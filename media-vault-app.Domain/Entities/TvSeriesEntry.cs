using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Interfaces;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Domain.Entities
{
    public sealed record TvSeriesEntry : MediaEntry, IOwnerEntity<Guid>, IHasSeasons
    {
        public ICollection<Season>? Seasons { get; set; }
        public int TotalEpisodes { get; set; }
        public int TotalWatchedEpisodes { get; set; }

        public TvSeriesEntry()
        {
            MediaType = MediaType.TvSeries;
        }

    }
}
