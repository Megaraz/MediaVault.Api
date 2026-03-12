using Rasmus.SharedKernel.Interfaces;

namespace media_vault_app.Domain.Entities
{
    public class User : IEntityId<Guid>, ICreatedAtUtc
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public ICollection<MediaEntry> MediaEntries { get; set; } = new List<MediaEntry>();
        public DateTime CreatedAtUtc { get; set; }
    }
}
