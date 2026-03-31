using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.Tmdb.Movie
{
    public class TmdbMovieSearchResponse
    {

        [JsonPropertyName("page")]
        public int? Page { get; set; }

        [JsonPropertyName("total_pages")]
        public int? TotalPages { get; set; }

        [JsonPropertyName("total_results")]
        public int? TotalResults { get; set; }

        [JsonPropertyName("results")]
        public IReadOnlyList<TmdbMovieResult>? Results { get; set; }
    }
}
