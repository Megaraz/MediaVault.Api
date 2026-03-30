using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.Rawg
{
    public sealed record RawgSearchResponse(
        [property: JsonPropertyName("results")] IReadOnlyList<RawgGameResponse>? Results);
}
