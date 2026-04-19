using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IOwnableEntity<TKeyOwner, TKeyOwned> 
        : IWriteableEntity<TKeyOwned>
            where TKeyOwner : notnull, IEquatable<TKeyOwner>
            where TKeyOwned : notnull, IEquatable<TKeyOwned>
    {
        TKeyOwner OwnerId { get; set; }
    }
}
