using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.External_API_Contracts.Rawg
{
    public class RawgGameDetailedResponse
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("slug")]
        public string? Slug { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("metacritic")]
        public int Metacritic { get; set; }

        [JsonPropertyName("released")]
        public string? Released { get; set; }


        [JsonPropertyName("background_image")]
        public string? BackgroundImage { get; set; }


        [JsonPropertyName("website")]
        public string? Website { get; set; }

        [JsonPropertyName("suggestions_count")]
        public int SuggestionsCount { get; set; }

        [JsonPropertyName("platforms")]
        public Platform[]? Platforms { get; set; }
    }

    public class Platform
    {
        [JsonPropertyName("platform")]
        public Platform1? Platform1 { get; set; }

        [JsonPropertyName("released_at")]
        public string? ReleasedAt { get; set; }

        [JsonPropertyName("requirements")]
        public Requirements? Requirements { get; set; }
    }

    public class Platform1
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("slug")]
        public string? Slug { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class Requirements
    {
        [JsonPropertyName("minimum")]
        public string? Minimum { get; set; }

        [JsonPropertyName("recommended")]
        public string? Recommended { get; set; }
    }
}
