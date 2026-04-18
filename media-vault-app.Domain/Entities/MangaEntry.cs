using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Interfaces;

namespace media_vault_app.Domain.Entities
{
    public sealed record MangaEntry : MediaEntry, IAuthorable
    {
        public Guid AuthorId { get; set; }
        public required Author Author { get; set; }
    }
}
