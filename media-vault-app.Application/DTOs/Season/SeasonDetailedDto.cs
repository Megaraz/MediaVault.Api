using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Application.DTOs.Season
{
    public sealed record SeasonDetailedDto : IDtoIdentifiable<Guid>
    {
        public Guid Id { get; init; }
        public Guid TvSeriesId { get; init; }
        public string? IdExternal { get; init; }
        public string? Name { get; init; }
        public string? Overview { get; init; }
        public string? ImageUrl { get; init; }
        public int SeasonNumber { get; init; }
        public DateOnly? AirDate { get; init; }
        public int WatchedEpisodes { get; init; }
        public int Episodes { get; init; }

        public Status Status { get; init; }
        public decimal Rating { get; init; }

        public DateTime CreatedAtUtc { get; init; }
        public DateTime UpdatedAtUtc { get; init; }
    }
}
