using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Domain.Entities
{
    public class User : IOwnerEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public ICollection<MediaEntry> MediaEntries { get; set; } = new List<MediaEntry>();
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public int Version { get; set; } = 1;
    }
}
