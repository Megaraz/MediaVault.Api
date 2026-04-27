using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.MediaEntry.Request;

public sealed record GameEntryUpdateDto : MediaEntryUpdateDto
{
    public string? DevStudioName { get; init; }
    public int HoursPlayed { get; init; }
    public override MediaType MediaType => MediaType.Game;
}
