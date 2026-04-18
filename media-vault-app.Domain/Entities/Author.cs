using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Interfaces;

namespace media_vault_app.Domain.Entities
{
    public sealed record Author : IAuthor
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; } = null;
        public required string LastName { get; set; }
        public string? HomeCountry { get; set; }
        public int? YearOfBirth { get; set; } = null;
        public ICollection<IAuthorable> AuthoredEntries { get; set; } = new List<IAuthorable>();
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
