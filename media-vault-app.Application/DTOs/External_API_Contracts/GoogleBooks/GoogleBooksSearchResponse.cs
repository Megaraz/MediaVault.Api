using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.External_API_Contracts.GoogleBooks
{
    public sealed record GoogleBooksSearchResponse(
        [property: JsonPropertyName("totalItems")] int TotalItems,
        [property: JsonPropertyName("items")] IReadOnlyList<GoogleBooksVolumeResponse>? Items);
}
