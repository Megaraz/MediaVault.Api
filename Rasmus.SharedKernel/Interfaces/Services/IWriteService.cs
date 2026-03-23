using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface IWriteService<TEntity, TKey, TCreateDto, TDetailedDto> :
        ICreateService<TCreateDto, TDetailedDto>,
        IDeleteService<TKey>
        where TEntity : class, IEntityId<TKey>
        where TDetailedDto : IDtoID<TKey>

    {
    }
}
