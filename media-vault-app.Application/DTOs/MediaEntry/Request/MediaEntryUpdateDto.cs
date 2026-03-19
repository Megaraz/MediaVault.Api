using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.MediaEntry.Request
{
    public record MediaEntryUpdateDto
    (
        string? IdExternal,
        Status Status,
        string Title,
        decimal? Rating,
        string? Review,
        string? Genre,
        int? ReleaseYear,
        string? ImageUrl,
        MediaEntryType MediaType
    );
}
