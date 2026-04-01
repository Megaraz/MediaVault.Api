using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.GoogleBooks
{
    public sealed record GoogleBooksVolumeResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("volumeInfo")] GoogleBooksVolumeInfo? VolumeInfo);

    public sealed record GoogleBooksVolumeInfo(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("authors")] IReadOnlyList<string>? Authors,
        [property: JsonPropertyName("imageLinks")] GoogleBooksImageLinks? ImageLinks);

    public sealed record GoogleBooksImageLinks(
        [property: JsonPropertyName("small")] string? Small);
}
