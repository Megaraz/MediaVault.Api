using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Domain.Entities
{
    public sealed class GameEntry : MediaEntry
    {
        //public string? DevStudioName { get; set; }

        public int MetacriticRating { get; set; }
        public string? Website { get; set; }
        public ICollection<string> Platforms { get; set; } = new List<string>();
        public GamePcRequirements? PcRequirements { get; set; }
        public int HoursPlayed { get; set; }

        public GameEntry()
        {
            MediaType = MediaType.Game;
        }
    }

    public readonly record struct GamePcRequirements(
        string? Minimum,
        string? Recommended,
        string? High,
        string? VeryHigh,
        string? Ultra);
}
