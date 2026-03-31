using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Application.DTOs.Rawg
{
    public sealed record GameSearchResultDto(
        int ExternalId,
        string Title,
        string? CoverImageUrl,
        string Slug);
}
