using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.GoogleBooks
{
    public sealed record GoogleBooksSearchResponse(
        [property: JsonPropertyName("totalItems")] int TotalItems,
        [property: JsonPropertyName("items")] IReadOnlyList<GoogleBooksVolumeResponse>? Items);
}
