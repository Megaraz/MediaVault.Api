using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.MediaEntry.Response;

public sealed record GameEntryDetailedDto : MediaEntryDetailedDto
{
    //public string? DevStudioName { get; init; }
    public int HoursPlayed { get; init; }
    public override MediaType MediaType => MediaType.Game;
    public int MetacriticRating { get; init; }
    public string? Website { get; init; }
    public ICollection<string> Platforms { get; init; } = new List<string>();
    public GamePcRequirementsDto? PcRequirements { get; init; }
}

public sealed record GamePcRequirementsDto(string? Minimum, string? Recommended, string? High, string? VeryHigh, string? Ultra);
