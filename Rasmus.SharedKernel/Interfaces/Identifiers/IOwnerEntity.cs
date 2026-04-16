using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IOwnerEntity<TEntityOwner, TKeyOwner> : IEntityId<TKeyOwner>
        where TEntityOwner : class, IEntityId<TKeyOwner>
        where TKeyOwner : notnull, IEquatable<TKeyOwner>
    {
    }
}
