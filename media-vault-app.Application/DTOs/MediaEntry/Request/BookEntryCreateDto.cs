using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.MediaEntry.Request;

public sealed record BookEntryCreateDto : MediaEntryCreateDto
{
    public string? Author { get; init; }
    public override MediaType MediaType => MediaType.Book;
}
