using media_vault_app.Domain.Enums;
using media_vault_app.Domain.Interfaces;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Domain.Entities
{
    public sealed class TvSeriesEntry : MediaEntry, IOwnerEntity<Guid>, IHasSeasons
    {
        public string? BackdropImageUrl { get; set; }
        public DateOnly? LastAirDate { get; set; }
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
