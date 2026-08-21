using media_vault_app.Domain.Enums;
using media_vault_app.Domain.Value_Objects;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Domain.Entities
{
    public abstract class MediaEntry : IDependentEntity<Guid, Guid>
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string? IdExternal { get; set; }
        public Status Status { get; set; }
        public string Title { get; set; } = string.Empty;
        public Rating Rating { get; set; }
        public string? Review { get; set; }
        public ICollection<string> Genres { get; set; } = new List<string>();

        public string? Overview { get; set; }

        public DateOnly? ReleaseDate { get; set; }

        public string? ImageUrl { get; set; }
        public MediaType MediaType { get; protected set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public int Version { get; set; } = 1;

        protected MediaEntry()
        {

        }
    }
}
