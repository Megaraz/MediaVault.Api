using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.External_API_Contracts.GoogleBooks
{
    public sealed record GoogleBooksVolumeResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("volumeInfo")] GoogleBooksVolumeInfo? VolumeInfo);

    public sealed record GoogleBooksVolumeInfo(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("authors")] IReadOnlyList<string>? Authors,
        [property: JsonPropertyName("imageLinks")] GoogleBooksImageLinks? ImageLinks);

    public sealed record GoogleBooksImageLinks(
        [property: JsonPropertyName("smallThumbnail")] string? SmallThumbnail,
        [property: JsonPropertyName("thumbnail")] string? Thumbnail,
        [property: JsonPropertyName("small")] string? Small,
        [property: JsonPropertyName("medium")] string? Medium,
        [property: JsonPropertyName("large")] string? Large,
        [property: JsonPropertyName("extraLarge")] string? ExtraLarge);
}