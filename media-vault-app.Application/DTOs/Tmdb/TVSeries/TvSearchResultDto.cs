using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Application.DTOs.Tmdb.TVSeries
{
    public sealed record TvSearchResultDto(
        int ExternalId,
        string Title,
        string? CoverImageUrl);
}
