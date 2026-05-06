using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.Season
{
    public sealed record SeasonCreateDto
    {
        public string? IdExternal { get; set; }
        public string? Name { get; set; }
        public string? Overview { get; set; }
        public string? ImageUrl { get; set; }
        public int SeasonNumber { get; set; }
        public DateOnly? AirDate { get; set; }
        public int WatchedEpisodes { get; set; }
        public int Episodes { get; set; }

        public Status Status { get; set; }
        public decimal Rating { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
