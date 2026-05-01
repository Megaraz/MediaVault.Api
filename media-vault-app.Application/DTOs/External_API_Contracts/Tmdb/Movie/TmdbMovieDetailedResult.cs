using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Shared;

namespace media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Movie
{
    public sealed record TmdbMovieDetailedResult
    {

        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("genres")]
        public IReadOnlyList<TmdbGenre> Genres { get; init; } = new List<TmdbGenre>();

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; init; }

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("overview")]
        public string? Overview { get; init; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; init; }
    }
}
