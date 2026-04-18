using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IOwnerEntity<TEntityOwner, TKeyOwner> 
        : IWriteableEntity<TKeyOwner>
            where TEntityOwner : IWriteableEntity<TKeyOwner>
            where TKeyOwner : notnull, IEquatable<TKeyOwner>
    {
    }
}
