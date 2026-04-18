using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Application.DTOs.MediaEntry.Response
{
    public record MediaEntryDetailedDto : IDtoIdentifiable<Guid>
    {
        public Guid Id { get; init; }
        public string? IdExternal { get; init; }
        public Guid UserId { get; init; }
        public Status Status { get; init; }
        public string? Title { get; init; } = null;
        public decimal Rating { get; init; }

        public string? Review { get; init; }
        public ICollection<string>? Genres { get; init; }
        public int ReleaseYear { get; init; }
        public string? ImageUrl { get; init; }
        public MediaEntryType MediaType { get; init; }
        public DateTime CreatedAtUtc { get; init; }

    }
}
