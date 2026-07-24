using media_vault_app.Domain.Entities;

namespace media_vault_app.Domain.Interfaces
{
    public interface IHasSeasons
    {
        public ICollection<Season> Seasons { get; set; }
    }
}
