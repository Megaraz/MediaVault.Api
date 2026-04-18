using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IDtoIdentifiable<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        TKey Id { get; init; }
    }
}
