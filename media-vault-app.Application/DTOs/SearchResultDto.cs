using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;


namespace media_vault_app.Application.DTOs
{
    public record SearchResultDto(
        string ExternalId,
        string Title,
        string? CoverImageUrl,
        MediaType MediaType
        );
}