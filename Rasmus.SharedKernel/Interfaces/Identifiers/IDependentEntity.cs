using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IDependentEntity<TKeyOwner, TKeyDependent>
        : IEntity<TKeyDependent>
            where TKeyOwner : notnull, IEquatable<TKeyOwner>
            where TKeyDependent : notnull, IEquatable<TKeyDependent>
    {
        TKeyOwner OwnerId { get; set; }
    }
}
