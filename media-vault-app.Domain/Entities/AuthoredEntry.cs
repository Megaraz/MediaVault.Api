using media_vault_app.Domain.Interfaces;

namespace media_vault_app.Domain.Entities
{
    public abstract record AuthoredEntry : MediaEntry, IHasAuthor
    {
        public Guid AuthorId { get; set; }
        public Author Author { get; set; } = null!;
    }
}
