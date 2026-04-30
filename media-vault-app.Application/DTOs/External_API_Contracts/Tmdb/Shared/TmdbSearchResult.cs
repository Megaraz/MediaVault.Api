using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Shared
{
    public sealed record TmdbSearchResult
    {

        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; init; }

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("overview")]
        public string? Overview { get; init; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; init; }

        [JsonPropertyName("genre_ids")]
        public IReadOnlyList<int> GenreIds { get; init; } = new List<int>();

        //public bool adult { get; set; }

    }

}
