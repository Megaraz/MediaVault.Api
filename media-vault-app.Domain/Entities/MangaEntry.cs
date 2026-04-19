using media_vault_app.Domain.Enums;

namespace media_vault_app.Domain.Entities
{
    public sealed record MangaEntry : AuthoredEntry
    {
        public MangaEntry()
        {
            MediaType = MediaType.Manga;
        }
    }
}
