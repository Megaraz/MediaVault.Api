using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Application.DTOs.MediaEntry.Response
{
    public record MediaEntryDetailedDto : IDtoID<Guid>
    {
        public Guid Id { get; init; }
        public string? IdExternal { get; init; }
        public Guid UserId { get; init; }
        public Status Status { get; init; }
        public string? Title { get; init; } = null;
        private decimal _rating;

        public decimal Rating
        {
            get => _rating;
            init
            {
                // Clamp the value between 0.5 and 10
                var clamped = Math.Clamp(value, 0.5m, 10m);

                // Round to the nearest 0.5
                _rating = Math.Round(clamped * 2, MidpointRounding.AwayFromZero) / 2;
            }
        }

        public string? Review { get; init; }
        public string? Genre { get; init; }
        public int ReleaseYear { get; init; }
        public string? ImageUrl { get; init; }
        public MediaEntryType MediaType { get; init; }
        public DateTime CreatedAtUtc { get; init; }

    }
}
