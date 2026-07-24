using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.Rawg
{
    public sealed record RawgSearchResultDto : MediaEntryExternalSearchResultDto
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
