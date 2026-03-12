using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.Interfaces;

namespace media_vault_app.Domain.Entities
{
    public abstract class MediaEntry : IEntityId<Guid>, ICreatedAtUtc
    {
        public Guid Id { get; set; }
        public string? IdExternal { get; set; }
        public Guid UserId { get; set; }
        public Status Status { get; set; }
        public string? Title { get; set; }

        private decimal _rating;

        public decimal Rating
        {
            get => _rating;
            set
            {
                // Clamp the value between 0.5 and 10
                var clamped = Math.Clamp(value, 0.5m, 10m);

                // Round to the nearest 0.5
                _rating = Math.Round(clamped * 2, MidpointRounding.AwayFromZero) / 2;
            }
        }

        public string? Review { get; set; }
        public string? Genre { get; set; }
        public int ReleaseYear { get; set; }
        public string? ImageUrl { get; set; }
        public MediaEntryType MediaType { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        protected MediaEntry()
        {

        }


    }
}
