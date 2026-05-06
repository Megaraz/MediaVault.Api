using media_vault_app.Domain.Enums;
using media_vault_app.Domain.Interfaces;

namespace media_vault_app.Domain.Entities
{
    public sealed class MangaEntry : MediaEntry, IHasAuthor
    {
        public string? Author { get; set; }
        public MangaEntry()
        {
            MediaType = MediaType.Manga;
        }
    }
}
