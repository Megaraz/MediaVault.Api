using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.Season;
using media_vault_app.Application.DTOs.Tmdb;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.MediaEntry.Response;

public sealed record TvSeriesEntryDetailedDto : MediaEntryDetailedDto
{
    public string? BackdropImageUrl { get; init; }
    public DateTime? LastAirDate { get; init; }
    public int NumberOfSeasons { get; init; }
    public int NumberOfEpisodes { get; init; }
    public string? AiringStatus { get; init; }
    public int TotalWatchedEpisodes { get; init; }
    public SeasonMinimalDto[]? Seasons { get; init; }
    public override MediaType MediaType => MediaType.TvSeries;
}

//public sealed record SeasonDto
//{
//    public Guid Id { get; init; }
//    public Guid OwnerId { get; init; }
//    public string? IdExternal { get; init; }
//    public string? Name { get; init; }
//    public string? Overview { get; init; }
//    public string? ImageUrl { get; init; }
//    public int SeasonNumber { get; init; }
//    public DateTime? AirDate { get; init; }
//    public int WatchedEpisodes { get; init; }
//    public int Episodes { get; init; }

//    public Status Status { get; init; }
//    public decimal Rating { get; init; }

//    public DateTime CreatedAtUtc { get; init; }
//    public DateTime UpdatedAtUtc { get; init; }
//}
