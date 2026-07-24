using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Shared
{
    public class TmdbSearchResponse
    {

        [JsonPropertyName("page")]
        public int? Page { get; set; }

        [JsonPropertyName("total_pages")]
        public int? TotalPages { get; set; }

        [JsonPropertyName("total_results")]
        public int? TotalResults { get; set; }

        [JsonPropertyName("results")]
        public IReadOnlyList<TmdbSearchResult>? Results { get; set; }
    }
}
