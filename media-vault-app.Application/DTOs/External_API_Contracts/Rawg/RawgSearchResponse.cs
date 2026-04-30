using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace media_vault_app.Application.DTOs.External_API_Contracts.Rawg
{
    public sealed record RawgSearchResponse(
        [property: JsonPropertyName("results")] IReadOnlyList<RawgGameResponse>? Results);
}
