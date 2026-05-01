using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Shared;

namespace media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.TvSeries
{
    public class TmdbTvSeriesDetailedResult
    {

        [JsonPropertyName("backdrop_path")]
        public string? BackdropPath { get; set; }


        [JsonPropertyName("first_air_date")]
        public string? FirstAirDate { get; set; }

        [JsonPropertyName("genres")]
        public TmdbGenre[]? Genres { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("last_air_date")]
        public string? LastAirDate { get; init; }


        [JsonPropertyName("name")]
        public string? Name { get; set; }


        [JsonPropertyName("number_of_episodes")]
        public int NumberOfEpisodes { get; set; }

        [JsonPropertyName("number_of_seasons")]
        public int NumberOfSeasons { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; } = null;

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("seasons")]
        public TmdbSeason[]? Seasons { get; set; }


        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    public class TmdbSeason
    {
        [JsonPropertyName("air_date")]
        public string? AirDate { get; set; }

        [JsonPropertyName("episode_count")]
        public int EpisodeCount { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("season_number")]
        public int SeasonNumber { get; set; }

    }

}
