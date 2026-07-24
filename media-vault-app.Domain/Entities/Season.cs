using media_vault_app.Domain.Enums;
using media_vault_app.Domain.Value_Objects;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Domain.Entities
{
    public sealed class Season : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid TvSeriesEntryId { get; set; }
        public TvSeriesEntry TvSeriesEntry { get; set; } = null!;
        public string? IdExternal { get; set; }
        public string? Name { get; set; }
        public string? Overview { get; set; }
        public string? ImageUrl { get; set; }
        public int SeasonNumber { get; set; }
        public DateOnly? AirDate { get; set; }
        public int WatchedEpisodes { get; set; }
        public int Episodes { get; set; }

        public Status Status { get; set; }
        public Rating Rating { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
