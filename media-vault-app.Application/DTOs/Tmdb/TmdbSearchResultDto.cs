using System;
using System.Text;
using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.Tmdb
{
    public sealed record TmdbSearchResultDto : MediaEntryExternalSearchResultDto
    {
        public string? Name { get; init; }
        public string? Overview { get; init; }
        public string? ReleaseDate { get; init; }
        public IReadOnlyList<int> GenreIds { get; init; } = new List<int>();

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
