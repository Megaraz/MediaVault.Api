using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;


namespace media_vault_app.Application.DTOs.ExternalAPIs
{
    public sealed record SearchResultDto(
        int ExternalId,
        string Title,
        string? CoverImageUrl,
        MediaEntryType MediaType
        );
}