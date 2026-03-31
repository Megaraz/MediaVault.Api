using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using media_vault_app.Application.DTOs.Tmdb.Movie;

namespace media_vault_app.Application.DTOs.Tmdb.TVSeries
{
    public sealed class TmdbTvSearchResponse
    {

        [JsonPropertyName("page")]
        public int? Page { get; set; }

        [JsonPropertyName("total_pages")]
        public int? TotalPages { get; set; }

        [JsonPropertyName("total_results")]
        public int? TotalResults { get; set; }


        [JsonPropertyName("results")]
        public IReadOnlyList<TmdbTvResult>? Results { get; set; } 
    }
}
