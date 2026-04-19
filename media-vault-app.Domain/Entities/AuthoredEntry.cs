using media_vault_app.Domain.Interfaces;

namespace media_vault_app.Domain.Entities
{
    public abstract record AuthoredEntry : MediaEntry, IHasAuthor
    {
        public Guid AuthorId { get; set; }
        public required Author Author { get; set; }
    }
}
