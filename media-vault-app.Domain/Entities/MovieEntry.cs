using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Domain.Entities
{
    public class MovieEntry : MediaEntry
    {
        public int RuntimeMinutes { get; set; }
    }
}
