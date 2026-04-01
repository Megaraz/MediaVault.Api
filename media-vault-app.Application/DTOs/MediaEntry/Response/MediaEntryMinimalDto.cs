using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Application.DTOs.MediaEntry.Response
{
    public record MediaEntryMinimalDto
    (
        Guid Id,
        string? Title,
        MediaEntryType MediaType,
        string? ImageUrl


    ) : IDtoID<Guid>;

}