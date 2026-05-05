using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Application.DTOs.Tmdb
{
    public sealed record TmdbSeasonDto
    (
         string? TmdbAirDate,
         int TmdbEpisodeCount,
         string? TmdbName,
         string? TmdbOverview,
         string? TmdbPosterPath,
         int TmdbSeasonNumber
    );
}
