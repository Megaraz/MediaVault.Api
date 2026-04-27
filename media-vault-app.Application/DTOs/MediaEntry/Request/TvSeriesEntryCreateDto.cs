using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.MediaEntry.Request
{

    public sealed record TvSeriesEntryCreateDto : MediaEntryCreateDto
    {
        public int TotalEpisodes { get; init; }
        public int TotalWatchedEpisodes { get; init; }
        public override MediaType MediaType => MediaType.TvSeries;
    }
}
