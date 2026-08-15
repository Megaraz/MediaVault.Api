using System.Text.Json.Serialization;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Shared;

namespace media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Movie
{
    public sealed record TmdbMovieDetailedResponse
    {

        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("genres")]
        public IReadOnlyList<TmdbGenre> Genres { get; init; } = new List<TmdbGenre>();

        [JsonPropertyName("backdrop_path")]
        public string? BackdropPath { get; init; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; init; }

        [JsonPropertyName("runtime")]
        public int RunTime { get; init; }

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("overview")]
        public string? Overview { get; init; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; init; }
    }
}
