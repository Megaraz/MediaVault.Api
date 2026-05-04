using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.MediaEntry.Request;

public sealed record GameEntryCreateDto : MediaEntryCreateDto
{
    //public string? DevStudioName { get; init; }
    public int HoursPlayed { get; init; }
    public override MediaType MediaType => MediaType.Game;
    public int MetacriticRating { get; init; }
    public string? Website { get; init; }
    public ICollection<string> Platforms { get; init; } = new List<string>();
    public GamePcRequirementsDto? PcRequirements { get; init; }
}
