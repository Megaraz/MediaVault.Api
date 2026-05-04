using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.MediaEntry.Request;

public abstract record MediaEntryUpdateDto
{
    public string? IdExternal { get; init; }
    public Status Status { get; init; }
    public required string Title { get; init; }
    public decimal Rating { get; init; }
    public string? Overview { get; init; }
    public string? Review { get; init; }
    public ICollection<string>? Genres { get; init; }
    public int? ReleaseYear { get; init; }
    public string? ImageUrl { get; init; }
    public abstract MediaType MediaType { get; }
}
