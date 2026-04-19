using media_vault_app.Domain.Interfaces;

namespace media_vault_app.Domain.Entities
{
    public sealed record Author : IAuthor
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public required string LastName { get; set; }
        public string? HomeCountry { get; set; }
        public int? YearOfBirth { get; set; }
        public ICollection<AuthoredEntry> AuthoredEntries { get; set; } = new List<AuthoredEntry>();
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
