using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Domain.Entities
{
    public sealed record GameEntry : MediaEntry
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

    public sealed record GamePcRequirements
    {
        public Guid Id { get; set; }
        public Guid GameEntryId { get; set; }
        public string? Minimum { get; set; }
        public string? Recommended { get; set; }
        public string? High { get; set; }
        public string? VeryHigh { get; set; }
        public string? Ultra { get; set; }
    }
}
