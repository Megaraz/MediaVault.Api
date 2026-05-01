using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Application.DTOs.Tmdb
{
    public class TmdbTvSeriesDetailedDto
    {

        public string? TmdbBackdropPath { get; set; }
        public string? TmdbFirstAirDate { get; set; }
        public TmdbGenreDto[]? TmdbGenres { get; set; }
        public int TmdbTvSeriesId { get; set; }
        public string? TmdbLastAirDate { get; init; }
        public string? TmdbName { get; set; }

        public int TmdbNumberOfEpisodes { get; set; }

        public int TmdbNumberOfSeasons { get; set; }

        public string? TmdbOverview { get; set; } = null;

        public string? TmdbPosterPath { get; set; }

        public TmdbSeasonDto[]? TmdbSeasons { get; set; }
        public string? TmdbStatus { get; set; }
    }

    public class TmdbSeasonDto
    {
        public int TmdbSeasonId { get; set; }
        public string? TmdbAirDate { get; set; }
        public int TmdbEpisodeCount { get; set; }
        public string? TmdbName { get; set; }
        public string? TmdbOverview { get; set; }
        public string? TmdbPosterPath { get; set; }
        public string? PosterPath { get; set; }
        public int TmdbSeasonNumber { get; set; }
    }
}
