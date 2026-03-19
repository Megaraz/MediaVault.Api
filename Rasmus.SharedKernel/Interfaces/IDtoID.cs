using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces
{
    public interface IDtoID<TKey>
    {
        TKey Id { get; init; }
    }
}
