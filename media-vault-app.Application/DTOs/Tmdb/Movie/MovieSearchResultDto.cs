using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Application.DTOs.Tmdb.Movie
{
    public sealed record MovieSearchResultDto(
        int ExternalId,
        string Title,
        string? CoverImageUrl);
}
