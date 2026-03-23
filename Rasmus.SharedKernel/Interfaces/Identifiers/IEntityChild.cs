using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IEntityChild<TEntityParent, TKeyParent, TEntityChild, TKeyChild> : IEntityId<TKeyChild>
        where TEntityParent : class, IEntityId<TKeyParent>
        where TEntityChild : class, IEntityChild<TEntityParent, TKeyParent, TEntityChild, TKeyChild>
    {
    }
}
