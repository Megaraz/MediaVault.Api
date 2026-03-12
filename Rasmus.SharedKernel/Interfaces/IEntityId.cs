using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Domain.Interfaces
{
    public interface IEntityId<TKey>
    {
        TKey Id { get; set; }
    }
}
