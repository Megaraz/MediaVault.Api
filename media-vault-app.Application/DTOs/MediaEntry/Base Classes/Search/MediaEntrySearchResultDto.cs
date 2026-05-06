using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;


namespace media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search
{
    public record MediaEntrySearchResultDto(
        string IdExternal,
        string Title,
        string? CoverImageUrl,
        MediaType MediaType
        );
}