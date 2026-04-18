using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Domain.Entities
{
    public sealed record Season : IOwnableEntity<TvSeriesEntry, Guid, Season, Guid>
    {
        public Guid Id { get; set; }
        public required TvSeriesEntry TvSeriesEntry { get; set; }
        public Guid OwnerId { get; set; }

        public int WatchedEpisodes { get; set; }
        public int Episodes { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
