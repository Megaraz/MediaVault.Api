using media_vault_app.Domain.Enums;


namespace media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search
{
    public record MediaEntryExternalSearchResultDto(
        string IdExternal,
        string Title,
        string? CoverImageUrl,
        MediaType MediaType
        );
}