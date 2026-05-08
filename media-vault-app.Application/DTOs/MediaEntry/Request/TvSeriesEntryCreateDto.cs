using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.Season;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.MediaEntry.Request
{

    public sealed record TvSeriesEntryCreateDto : MediaEntryCreateDto
    {
        public string? BackdropImageUrl { get; init; }
        public DateOnly? LastAirDate { get; init; }
        public int NumberOfSeasons { get; init; }
        public int NumberOfEpisodes { get; init; }
        public string? AiringStatus { get; init; }
        public int TotalWatchedEpisodes { get; init; }
        public required SeasonCreateDto[] Seasons { get; init; }
        public override MediaType MediaType => MediaType.TvSeries;
    }
}
