using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.MediaEntry.Request
{
    public record MediaEntryCreateDto
    (
        string? IdExternal,
        Status Status,
        string Title,
        decimal Rating,
        string? Review,
        ICollection<string>? Genres,
        int? ReleaseYear,
        string? ImageUrl,
        MediaType MediaType
    );
}
