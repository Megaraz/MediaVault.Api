using media_vault_app.Domain.Enums;
using media_vault_app.Domain.Value_Objects;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Domain.Entities
{
    public abstract record MediaEntry : IDependentEntity<Guid, Guid>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        // Satisfies the interface, but routes to UserId
        Guid IDependentEntity<Guid, Guid>.OwnerId
        {
            get => UserId;
            set => UserId = value;
        }
        
        public string? IdExternal { get; set; }
        public Status Status { get; set; }
        public required string Title { get; set; }
        public Rating Rating { get; set; }
        public string? Review { get; set; }
        public ICollection<string>? Genres { get; set; }

        private int _releaseYear;
        public int ReleaseYear
        {
            get => _releaseYear;
            set => _releaseYear = Math.Clamp(value, 0, DateTime.UtcNow.Year);
        }

        public string? ImageUrl { get; set; }
        public MediaType MediaType { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        protected MediaEntry()
        {

        }
    }
}
