using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IOwnableEntity<TEntityOwner, TKeyOwner, TOwnedEntity, TKeyOwned> 
        : IWriteableEntity<TKeyOwned>
            where TEntityOwner : IOwnerEntity<TEntityOwner, TKeyOwner>
            where TOwnedEntity : IOwnableEntity<TEntityOwner, TKeyOwner, TOwnedEntity, TKeyOwned>
            where TKeyOwner: notnull, IEquatable<TKeyOwner>
            where TKeyOwned: notnull, IEquatable<TKeyOwned>
    {
        TKeyOwner OwnerId { get; set; }

    }
}
