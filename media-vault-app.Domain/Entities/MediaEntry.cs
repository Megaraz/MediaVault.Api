using media_vault_app.Domain.Enums;
using media_vault_app.Domain.Value_Objects;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Domain.Entities
{
    public abstract record MediaEntry : IDependentEntity<Guid, Guid>
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string? IdExternal { get; set; }
        public Status Status { get; set; }
        public string Title { get; set; } = string.Empty;
        public Rating Rating { get; set; }
        public string? Review { get; set; }
        public ICollection<string>? Genres { get; set; }

        public string? Overview { get; set; }


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
