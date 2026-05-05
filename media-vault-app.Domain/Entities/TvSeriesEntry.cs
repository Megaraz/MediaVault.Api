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
        public string? BackdropImageUrl { get; set; }
        public DateTime? LastAirDate { get; set; }
        public int NumberOfSeasons { get; set; }
        public int NumberOfEpisodes { get; set; }
        public string? AiringStatus { get; set; }
        public int TotalWatchedEpisodes { get; set; }
        public ICollection<Season> Seasons { get; set; } = new List<Season>();

        public TvSeriesEntry()
        {
            MediaType = MediaType.TvSeries;
        }

    }
}
