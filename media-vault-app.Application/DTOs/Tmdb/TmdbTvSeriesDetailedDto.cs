using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Application.DTOs.Tmdb
{
    public sealed record TmdbTvSeriesDetailedDto
    (
        string? TmdbBackdropPath,
        string? TmdbFirstAirDate,
        TmdbGenreDto[]? TmdbGenres,
        int TmdbTvSeriesId,
        string? TmdbLastAirDate,
        string? TmdbName,
        int TmdbNumberOfEpisodes,
        int TmdbNumberOfSeasons,
        string? TmdbOverview,
        string? TmdbPosterPath,
        TmdbSeasonDto[]? TmdbSeasons,
        string? TmdbStatus
    );

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
