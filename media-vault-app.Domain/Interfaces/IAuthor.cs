using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Domain.Interfaces
{
    public interface IAuthor : IOwnerEntity<Guid>
    {
        ICollection<AuthoredEntry> AuthoredEntries { get; set; }
    }
}
