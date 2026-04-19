using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IOwnerEntity<TKeyOwner> 
        : IWriteableEntity<TKeyOwner>
            where TKeyOwner : notnull, IEquatable<TKeyOwner>
    {
    }
}
