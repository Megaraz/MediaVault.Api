using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.MediaEntry.Request;

public sealed record MovieEntryCreateDto : MediaEntryCreateDto
{
    public int RuntimeMinutes { get; init; }
    public override MediaType MediaType => MediaType.Movie;
}
