using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.Tmdb
{
    public sealed record TmdbSearchResultDto : SearchResultDto
    {
        public TmdbSearchResultDto(
            string ExternalId, 
            string Title, 
            string? CoverImageUrl, 
            MediaType MediaType
            ) : base(ExternalId, Title, CoverImageUrl, MediaType)
        {
        }
    }
}
