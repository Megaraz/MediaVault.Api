using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.Rawg
{
    public sealed record RawgSearchResultDto : MediaEntrySearchResultDto
    {
        public RawgSearchResultDto(
            string externalId, 
            string title, 
            string? coverImageUrl
            ) : base(externalId, title, coverImageUrl, MediaType.Game)
        {
        }
    }
}
