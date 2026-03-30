using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.Rawg
{
    public sealed record RawgGameResponse(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("slug")] string? Slug,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("background_image")] string? BackgroundImage);
}
