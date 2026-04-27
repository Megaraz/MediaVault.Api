using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;
using media_vault_app.Domain.Value_Objects;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Domain.Entities
{
    public sealed record Season : IDependentEntity<Guid, Guid>
    {
        public Guid Id { get; set; }
        public Guid TvSeriesId { get; set; }

        // Satisfies the interface, but routes to TvSeriesEntry
        Guid IDependentEntity<Guid, Guid>.OwnerId
        {
            get => TvSeriesId;
            set => TvSeriesId = value;
        }

        public int? ReleaseYear { get; set; }

        public int WatchedEpisodes { get; set; }
        public int Episodes { get; set; }
        public Status Status { get; set; }
        public Rating Rating { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
