using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.TvSeries
{
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
