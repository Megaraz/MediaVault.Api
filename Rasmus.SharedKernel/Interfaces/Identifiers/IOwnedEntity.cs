using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IOwnedEntity<TEntityOwner, TKeyOwner, TOwnedEntity, TKeyOwned> : IEntityId<TKeyOwned>
        where TEntityOwner : class, IOwnerEntity<TEntityOwner, TKeyOwner>
        where TOwnedEntity : class, IOwnedEntity<TEntityOwner, TKeyOwner, TOwnedEntity, TKeyOwned>
        where TKeyOwner: notnull, IEquatable<TKeyOwner>
        where TKeyOwned: notnull, IEquatable<TKeyOwned>
    {
        TKeyOwner OwnerId { get; set; }

    }
}
