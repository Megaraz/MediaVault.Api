using media_vault_app.Domain.Enums;

namespace media_vault_app.Domain.Entities
{
    public sealed record BookEntry : AuthoredEntry
    {
        public BookEntry()
        {
            MediaType = MediaType.Book;
        }
    }
}
