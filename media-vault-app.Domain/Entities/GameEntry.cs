using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Domain.Entities
{
    public sealed record GameEntry : MediaEntry
    {
        public string? DevStudioName { get; set; }
        public int HoursPlayed { get; set; }

        public GameEntry()
        {
            MediaType = MediaType.Game;
        }
    }
}
