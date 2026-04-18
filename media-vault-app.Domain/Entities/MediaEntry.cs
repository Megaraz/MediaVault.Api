using media_vault_app.Domain.Enums;
using media_vault_app.Domain.Value_Objects;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Domain.Entities
{
    public abstract class MediaEntry : IOwnedEntity<User, Guid, MediaEntry, Guid>
    {
        public Guid Id { get; set; }
        public string? IdExternal { get; set; }
        public Guid OwnerId { get; set; }
        public Status Status { get; private set; }
        public string? Title { get; set; }
        public Rating Rating { get; set; }
        public string? Review { get; set; }
        public string? Genre { get; set; }
        public int ReleaseYear { get; set; }
        public string? ImageUrl { get; set; }
        public MediaEntryType MediaType { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        protected MediaEntry()
        {

        }

        public void SetStatus(Status newStatus) => Status = newStatus;

        public void SetReleaseYear(int releaseYear)
        {
            var clamped = Math.Clamp(releaseYear, 0, DateTime.UtcNow.Year);
            ReleaseYear = clamped;
        }


    }
}
