using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Application.DTOs.MediaEntry.Response;

public abstract record MediaEntryMinimalDto : IDtoIdentifiable<Guid>
{
    public Guid Id { get; init; }
    public string? Title { get; init; }
    public abstract MediaType MediaType { get; }
    public string? ImageUrl { get; init; }
}
