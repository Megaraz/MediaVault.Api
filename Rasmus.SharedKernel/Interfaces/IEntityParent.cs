using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces
{
    public interface IEntityParent<TEntityParent, TKeyParent> : IEntityId<TKeyParent>

        where TEntityParent : class, IEntityId<TKeyParent> 
    {
    }
}
