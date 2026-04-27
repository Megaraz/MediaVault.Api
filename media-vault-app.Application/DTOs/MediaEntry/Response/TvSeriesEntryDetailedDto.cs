using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.MediaEntry.Response;

public sealed record TvSeriesEntryDetailedDto : MediaEntryDetailedDto
{
    public int TotalEpisodes { get; init; }
    public int TotalWatchedEpisodes { get; init; }
    public override MediaType MediaType => MediaType.TvSeries;
}
