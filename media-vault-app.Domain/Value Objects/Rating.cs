using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Domain.Value_Objects
{
    public readonly record struct Rating
    {
        public decimal Value { get; }

        public Rating(decimal value)
        {
            var clamped = Math.Clamp(value, 0m, 5m);
            Value = Math.Round(clamped * 2, MidpointRounding.AwayFromZero) / 2;
        }

        // Allows implicit conversion from decimal for convenience
        public static implicit operator Rating(decimal value) => new(value);

        // Allows implicit conversion back to decimal when needed
        public static implicit operator decimal(Rating rating) => rating.Value;
    }
}
