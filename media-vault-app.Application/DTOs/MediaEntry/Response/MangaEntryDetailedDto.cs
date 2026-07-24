using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.MediaEntry.Response;

public sealed record MangaEntryDetailedDto : MediaEntryDetailedDto
{
    public string? Author { get; init; }
    public override MediaType MediaType => MediaType.Manga;
}
