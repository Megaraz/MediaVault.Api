using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Shared
{
    public sealed record TmdbGenre
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }
}
