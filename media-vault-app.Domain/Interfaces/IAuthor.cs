using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Domain.Interfaces
{
    public interface IAuthor : IOwnerEntity<Author, Guid>
    {
        
        public ICollection<IAuthorable> AuthoredEntries { get; set; } 

    }
}
